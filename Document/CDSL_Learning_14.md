# CDSL_Learning_14

## BTree类说明
使用树结构进行搜索操作时，平衡二叉搜索树因其控制树高的特性，已经实现了较小的操作复杂度了。但这是将整个树结构都放进内存中进行分析的，在内存中，读取引用类型的数据速度快，在整个搜索操作中占比小，因此不是我们分析性能的重点，我们的关注点会放在比较大小这个操作上，争取实现更少的比较大小操作。在其他的场景中，比如大型的数据库中，内存大小不足以放入全部的树结构，极端情况下，每次要读取一个新的节点时，都要从硬盘上取数据。相比于数据间比较大小的操作，CPU从硬盘读取数据的效率是极低的，此时，我们分析树搜索操作的性能，重点在于读取新节点的次数。在这种场景中，B树的优势就显示出来了。B树的每个节点都可以存放最多若干个数据（这个数量是在实例化一个B树时确定的）和数据数量加一的子树指针（最大的指针数量称为B树的阶数），同时，B树也通过一些算法尽可能地减少树高（即减少读取节点的次数）。B树的数据存储规律同二叉搜索树，所有比一个A数据大的数据都在A数据的右边（同节点的右边或者右边所有的子树中的数据），反之在左边。（这里对B树的介绍较为简单，基本都是代码实现时需要使用的特性，B树具体的定义请读者自行学习）

## BTree类实现
### 类头
```C#
public class CBTree<T>
    where T : IComparable<T>
```

### Node成员类
此处只介绍Node成员类较为基本的内容，Node类的相关方法在后续使用时会单独说明。B树中Node需要存储当前B树阶数，因为不同的B树实例的阶数可能不同，但共用同一个BTree.Node类，因此，要从Node类的层面将阶数区分开。有了阶数n，就决定了存储数据和子树指针的数组的大小。由于在B树的插入操作中会让一个节点先超出存储范围在分裂成两个节点（上溢出），所以这两个数组都要多预留一位（即n位的数据数组和n+1位的指针数组）。因为B树的操作采用迭代的思想而不是递归（原因后文会解释），所以需要指向父节点的指针来定位父节点（在AVL树中这一需求由递归的调用顺序实现了）。最后，每个节点不一定都会放满数据，所以需要一个int类型的数据用于指示当前节点中存储的数据数量（指针数量始终比节点数多一位）。综上，我们可以实现Node节点的基本成员和构造器。
```C#
private class Node
{
    private readonly int level;
    public T[] values;
    public Node?[] children;
    public Node? parent;
    public int valueNum;

    public Node(int level)
    {
        this.level = level;
        values = new T[level];
        children = new Node?[level + 1];
        parent = null;
        valueNum = 0;
    }
}
```

### 字段，属性和构造器
B树和大部分树结构相同，需要有一个树结构的入口root，Height用于指示树高（相比于存储的数量，树高在B树中更具有意义），Level用于确定树的阶数（Level是只读的），midLevel字段是后续操作中常使用的数据，可以用于指示当发生上溢出时，中间数据的下标，也可以用来指示除根节点外，每个节点存储数据的最小量。注意，不存在阶数小于3的B树
```C#
private Node? root;
public int Level { get; }
private int midLevel;
public int Height { get; private set; }

public CBTree(int level)
{
    if (level < 3)
        throw new ArgumentException("level");
    Level = level;
    midLevel = (level - 1) / 2;
    root = null;
    Height = 0;
}
```

### Search和Contains方法
Search方法是使用B树存储数据的逻辑查找相关的数据。在我的实现中，节点中下标为n的数据对应的左子树的下标为n，右子树为n+1。查找的逻辑是从根节点开始依次比较数组中的数据，直到找到所对应的子树指针，再在子树节点中重复上述过程，一直找到叶节点（但不会到空节点）。若找到，返回true，node存储数据所在的节点，index返回数据所在的下标（用于Delete方法）。若未找到，node必定指向叶节点，此时，index标记了该数据插入时的位置（用于Insert方法）。Comtains方法直接使用Search方法的返回值
```C#
private bool Search(T value, out Node node, out int index)
{
    if (root == null)
        throw new InvalidOperationException();
    node = root;
    while (true)
    {
        index = 0;
        for(int i = 0; i < node.valueNum; i++)
        {
            int diff = value.CompareTo(node.values[i]);
            if (diff == 0)
                return true;
            else if (diff < 0)
                break;
            index++;
        }
        if (node.children[index] == null)
            return false;
        else
            node = node.children[index]!;
    }
}

public bool Contains(T value)
{
    if(root == null)
        return false;
    return Search(value, out Node node, out int index);
}
```

### Insert方法
Insert方法是将数据插入到叶节点中，若插入节点的元素数量大于最大值，即大于阶数-1。则发生上溢出，取出节点的中间数据加入到父节点的正确位置上（这个位置由于事先没有储存，所以要通过比较的方式重新查找），原节点两端的数据作为形成两个新节点作为中间数据的左右子树加入到父节点的子树指针数组中。之后，要判断父节点在加入新元素后是否上溢出，上溢出可能连续发生。有读者可能会想，这场景和AVL树的回溯调整树高处理失衡很相似，为什么不使用递归的思想？因为B树中要尽可能减少对新节点的访问操作，使用递归思想，对新节点的访问次数会是树高的两倍减一次（相当于一个折返跑），但使用迭代，一旦哪一层不再上溢出，则立刻停止，减少访问新节点的次数，下面是Insert方法，其中Node类的AddValue方法和Split方法见后文
```C#
public void Insert(T value)
{
    if(root == null)
    {
        root = new Node(Level);
        Height = 1;
        root.AddValue(0, value, null, null);
        return;
    }
    if (Search(value, out Node walkPoint, out int index))
        return;

    Node? left = null;
    Node? right = null;
    while(walkPoint.AddValue(index, value, left, right))
    {
        value = walkPoint.values[midLevel];
        walkPoint.Split(ref left, ref right);
        if(walkPoint.parent == null)
        {
            root = new Node(Level);
            Height++;
            root.AddValue(0, value, left, right);
            return;
        }
        walkPoint = walkPoint.parent;
        for (index = 0; index < walkPoint.valueNum; index++)
        {
            if (value.CompareTo(walkPoint.values[index]) < 0)
                break;
        }
    }
}
```
Node成员类中的AddValue方法是将数据和左右子树插入到节点的指定下标n中，注意，插入节点的原子树指针数组中下标为n的指针会被弃用（因为这个指针所指向的节点会在发生上溢出后被弃用），注意要调整子树的父节点。加入该数据后，若数量足够发生上溢出，返回true
```C#
public bool AddValue(int index, T value, Node? left, Node? right)
{
    if(index < 0 || index > valueNum)
        throw new ArgumentOutOfRangeException("index");
    for(int i = valueNum; i > index; i--)
    {
        values[i] = values[i - 1];
        children[i + 1] = children[i];
    }
    values[index] = value;
    children[index] = left;
    children[index + 1] = right;
    if(left != null)
        left.parent = this;
    if(right != null) 
        right.parent = this;
    valueNum++;
    if(valueNum == level)
        return true;
    return false;
}
```

Node成员类中的Split方法是用来处理节点的上溢出。它不负责存储节点的中间数据，只会将两侧的数据划分给left和right两个节点（这里的数据既指T类型的数据，也指子树指针，没有子树指针会被弃用）。left可以直接使用原节点，不过要调整valueNum值和删除多余的子树指针，实现时要注意调整所有子树的父节点，使其指向新的节点，这会使整个代码看起来很臃肿，不过我没有什么好办法
```C#
public void Split(ref Node? left, ref Node? right)
{
    if (valueNum != level)
        throw new InvalidOperationException();
    int midLevel = (level - 1) / 2;
    left = this;
    right = new Node(level);
    left.valueNum = midLevel;
    int gap = midLevel + 1;
    for(int i = midLevel + 1; i < level; i++)
    {
        right.values[i - gap] = values[i];
        right.children[i -  gap] = children[i];
        if( children[i] != null)
            children[i]!.parent = right;
        children[i] = null;
    }
    right.children[level - gap] = children[level];
    if(children[level] != null)
        children[level]!.parent = right;
    children[level] = null;
    right.valueNum = level - gap;
}
```

### Delete方法
Delete方法首先分为两类，删除叶节点上的数据和非叶节点上的数据。不过，可以采用寻找直接后继（这里寻找直接后继的代码实现更简单）做替代的方式统一为删除叶节点上的数据。当删除一个数据后，要判断该节点的数据数量是否小于最小值（midLevel）（除根节点外，根节点的数据数量无最小值）。如果小于，发生下溢出。此时要看左右兄弟的的数据数量够不够借（优先找左兄弟借），都不够借就要发生合并，优先与右兄弟合并（除非没有右兄弟）。合并是将父节点中两个子树中间的数据和两个节点的数据合并为一个新的节点，并删除父节点中被使用的数据。注意删除操作也可能连续发生。当删除到头节点的数据时，不会再发生下溢出。此时要判断删除后头节点是否为空，不为空就无事发生。如果为空，要降低树高并将root值调整为该节点的第一个子树，接着考虑树结构是否还有数据，如果有数据，则将现头节点的父节点设为空，如果没有数据，那么root值为null，不需要调整父节点。Delete方法的代码实现很复杂，但具体的逻辑分支判断上文已经完整列举，同时，Delete方法使用到了Node类中的Remove，RemoveEnd和Combine方法，在后文介绍
```C#
public void Delete(T value)
{
    if(root == null)
        return;
    if(!Search(value, out Node walkPoint, out int index))
        return;
    if (walkPoint.children[index + 1] != null)
    {
        Node temp = walkPoint;
        walkPoint = walkPoint.children[index + 1]!;
        while (walkPoint.children[0] != null)
            walkPoint = walkPoint.children[0]!;
        value = walkPoint.values[0];
        temp.values[index] = value;
        index = 0;
    }

    while(walkPoint.Remove(index))
    {
        Node? parent = walkPoint.parent;
        if (parent == null)
        {
            if (walkPoint.valueNum == 0)
            {
                root = walkPoint.children[0];
                if (root != null)
                    root.parent = null;
                Height--;
            }
            return;
        }
        int pos;
        for(pos = 0; pos < parent.valueNum; pos++)
        {
            if (value.CompareTo(parent.values[pos]) < 0)
                break;
        }

        if(pos != 0 && parent.children[pos - 1]!.valueNum > midLevel)
        {
            Node brother = parent.children[pos - 1]!;
            value = parent.values[pos - 1];
            parent.values[pos - 1] = brother.values[brother.valueNum - 1];
            Node? left = brother.children[brother.valueNum];
            Node? right = walkPoint.children[0];
            walkPoint.AddValue(0, value, left, right);
            brother.RemoveEnd();
            return;
        }
        if(pos != parent.valueNum && parent.children[pos + 1]!.valueNum > midLevel)
        {
            Node brother = parent.children[pos + 1]!;
            value = parent.values[pos];
            parent.values[pos] = brother.values[0];
            Node? left = walkPoint.children[walkPoint.valueNum];
            Node? right = brother.children[0];
            walkPoint.AddValue(walkPoint.valueNum , value, left, right);
            brother.Remove(0);
            return;
        }

        if(pos == parent.valueNum)
        {
            Node brother = parent.children[pos - 1]!;
            brother.Combine(parent.values[pos - 1], walkPoint);
            parent.children[pos] = brother;
            walkPoint = parent;
            index = pos - 1;
        }
        else
        {
            Node brother = parent.children[pos + 1]!;
            walkPoint.Combine(parent.values[pos], brother);
            parent.children[pos + 1] = walkPoint;
            walkPoint = parent;
            index = pos;
        }
    }
}
```
Node类中的Remove方法是删除当前节点中对应下标中的数据和子树指针，并在删除后，判断该节点是否要发生下溢出，需要则返回true。RemoveEnd方法是删除最后一个节点和最后一个子树指针，也会根据是否发生下溢出返回bool值。Remove(valueNum-1)和RemoveEnd()的区别在于删除的子树指针是不同的，前者删除指针数组中倒二个元素，后者删除最后一个元素
```C#
public bool Remove(int index)
{
    if(index < 0 || index >= valueNum)
        throw new ArgumentOutOfRangeException("index");
    for(int i = index; i < valueNum; i++)
    {
        values[i] = values[i + 1];
        children[i] = children[i + 1];
    }
    children[valueNum] = null;
    valueNum--;
    if(valueNum < (level - 1) /  2)
        return true;
    return false;
}

public bool RemoveEnd()
{
    children[valueNum] = null;
    valueNum--;
    if (valueNum < (level - 1) / 2)
        return true;
    return false;
}
```
介绍了Remove方法就可以解释为什么删除头节点数据并使其为空时要将root值调整为children[0]而不是其他指针。在删除前，头节点只有一个数据，如果整个树结构只有这一个数据，那么删除头节点后要将头节点设为null，children[0]为null，符合需求，并且不需要调整头节点的父节点指针（因为不能空解引用）。如果是因为头节点下一层的节点发生下溢出导致删除头节点中的数据，那么根据代码实现children[0]和children[1]的值都会是合并后的节点的引用，接着while循环的条件语句中删除了children[0]并使用children[1]的值为children[0]赋值，children[1]的值设为null，所以在这种情况下，新的root也是调整为children[0]，并且要调整新的root的父节点指针为null

Node类的Combine方法很像是Split方法的逆过程，它将原节点和singleValue和other中的所有数据合并产生新节点（这里的数据不仅指T类型的数据，还有子树指针），注意other的子树节点要重新设置父节点指针的值
```C#
public void Combine(T singleValue, Node other)
{
    if (valueNum + other.valueNum + 1 >= level)
        throw new InvalidOperationException();
    values[valueNum] = singleValue;
    for(int i = 0; i < other.valueNum; i++)
    {
        values[valueNum + i + 1] = other.values[i];
        Node? temp = other.children[i];
        children[valueNum + i + 1] = temp;
        if (temp != null)
            temp.parent = this;
    }
    Node? node = other.children[other.valueNum];
    children[valueNum + other.valueNum + 1] = node;
    if (node != null)
        node.parent = this;
    valueNum = valueNum + other.valueNum + 1;
}
```

## 总结
从文本量也可以看出，BTree是相当麻烦的，因此，再次建议先熟悉B树的所有操作后，再来看具体的代码实现。代码前前后后写了8个小时，情况太多了，我经常被绕晕，记得当时第一版代码过测试用例的时候，一堆问题，好歹是扛过来了。树结构快要写完了，接下来是红黑树，也是复杂的结构，争取3天内搞定吧