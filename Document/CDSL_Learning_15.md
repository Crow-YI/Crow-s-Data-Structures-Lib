# CDSL_Learning_15

## RBTree类说明
红黑树和AVL树一样都是平衡二叉搜索树，两者采用的平衡策略不同，但最终目的都是通过减少树高来减少搜索次数，实现快速检索。相比于AVLTree的规则只有一句话，红黑树的规则初看让人摸不着头脑：1.节点是红色或者黑色的，2.根节点和失败节点为黑色，3.从任意节点到失败节点的所有路径上的黑色节点数量相同，4.不存在连续的红节点。大部分教科书都会依据AVLTree和红黑树都是平衡二叉搜索树这个特性，把两者放在连续的章节，并机械的告诉读者各个种类的失衡情况适合调整。但实际上，红黑树的构建规则其实类似于4阶B树，这也是为什么我先实现了B树，再实现红黑树。本文也会从两者的联系的角度分析红黑树的具体实现。从红黑树的构建规则看，从根节点到叶节点，最长的路径最多只能是最短路径的两倍，这就是红黑树的平衡原理，相较于AVLTree最多1的高度差，红黑树比较不严格，因此搜索效率会低一些，但插入或删除操作后的调整也更少一些，各有各的使用场景

## RBTree和4阶B树的联系
这一部分要求你对4阶B树和红黑树有一定的了解，也是为后续的方法实现做铺垫，旨在帮助那些背红黑树调整规则背到头晕的读者。拿出任何一个红黑树，如果把红色节点和父节点画在同一行，并拿一个方框把它们框起来，视作一个复合节点。把每个复合节点看作4阶B树的一个节点，这样我们就由一个红黑树得到了一个4阶B树。原本红黑树中的黑色节点看作是B树节点的中间元素，红色节点看作两边的元素（黑色节点必定存在，红色节点可能存在），复合节点间所有的指针都指向复合节点的黑色元素，黑色元素一定在复合节点的中间。为什么根节点一定是黑色的，因为root指针指向的是B树根节点的中间元素；为什么从任意节点到失败节点的所有路径上的黑色节点数量相同，因为B树的叶节点都在同一行，从B树的任意节点出发到叶节点的路径长度相同；为什么不能出现连续的红色节点，因为4阶B树只能每个节点只能有3个元素，除去中间的黑色元素，两边都最多只能存在一个红色元素。保留对这副由红黑树产生的4阶B树的印象（下文称之为伪B树），后面会经常用到它

## RBTree类实现
### 类头
```C#
public class CRBTree<T>
    where T : IComparable<T>
```

### Node成员类
因为在红黑树的调整操作中需要经常访问父节点和兄弟节点，且不便于使用递归的思想，所以红黑树的节点需要包含父节点的指针，在后续的方法中也是采用迭代而非递归的方法实现。每个节点还需要存储该节点的颜色（在此颜色使用一个枚举类型），其余于普通的二叉树节点相同
```C#
private enum Color
{
    Red = 0, Black = 1
}

private class Node
{
    public T value;
    public Color color;
    public Node? left;
    public Node? right;
    public Node? parent;

    public Node(T value)
    {
        this.value = value;
        color = Color.Red;
        left = null;
        right = null;
        parent = null;
    }
}
```

### 字段，属性和构造器
和AVLTree基本相同，不做说明
```C#
private Node? root;
public int Count { get; private set; }

public CRBTree()
{
    root = null;
    Count = 0;
}
```

### GetColor方法
GetColor私有方法用于获取一个节点的颜色，功能上可以用一个??操作符取代，但因为我认为??操作符的可读性较差，我比较喜欢定义一个私有方法
```C#
private Color GetColor(Node? node)
{
    if (node == null)
        return Color.Black;
    return node.color;
}
```

### 左旋和右旋
左旋和右旋在逻辑上和AVLTree中的左旋和右旋类似，但因为要额外维护一个parent指针且不是使用递归的思想（也就是不直接将旋转后的根节点返回），所以看起来很繁琐，但画画图很容易理解的
```C#
private void R_Rotate(Node node)
{
    Node? parent = node.parent;
    Node temp = node.left!;
    Node? child = temp.right;
    temp.parent = parent;
    if (parent == null)
        root = temp;
    else if (parent.left == node)
        parent.left = temp;
    else
        parent.right = temp;
    node.left = child;
    node.parent = temp;
    temp.right = node;
    if (child != null)
        child.parent = node;
}
private void L_Rotate(Node node)
{
    Node? parent = node.parent;
    Node temp = node.right!;
    Node? child = temp.left;
    temp.parent = node.parent;
    if (parent == null)
        root = temp;
    else if (parent.left == node)
        parent.left = temp;
    else
        parent.right = temp;
    node.right = child;
    node.parent = temp;
    temp.left = node;
    if(child != null) 
        child.parent = node;
}
```
要注意手动更新旋转结构和外部树结构的连接（也就是代码中的parent变量和新根节点的连接），大部分人都会漏掉这个部分（说的就是DEBUG之前的我）

### Contains方法
红黑树的数据存储规则和二叉搜索树一样，因此搜索的规则不变，不做说明
```C#
public bool Contains(T value)
{
    Node? walkPoint = root;
    while(walkPoint != null)
    {
        int diff = value.CompareTo(walkPoint.value);
        if (diff == 0)
            return true;
        if (diff > 0)
            walkPoint = walkPoint.right;
        else
            walkPoint = walkPoint.left;
    }
    return false;
}
```

Insert方法
上面还是很眉清目秀的，难的来了。所有插入的节点都先为红色节点，为什么，因为插入的元素必定在现存的伪B树的节点中，这意味着伪B树节点中黑色元素必定存在，我们先插入红色元素，再考虑是否会上溢出。先寻找插入的位置
```C#
Node? cur = root;
Node? parent = null;
bool isLeftChild = true;
while(cur != null)
{
    parent = cur;
    int diff = value.CompareTo(cur.value);
    if (diff == 0)
        return;
    if(diff > 0)
    {
        isLeftChild = false;
        cur = cur.right;
    }
    else
    {
        isLeftChild = true;
        cur = cur.left;
    }
}
```
接着是插入操作，要考虑插入的位置是否是根节点
```C#
Node newNode = new Node(value);
if(parent == null)
{
    root = newNode;
    root.color = Color.Black;
    Count++;
    return;
}

if(isLeftChild)
    parent.left = newNode;
else
    parent.right = newNode;
newNode.parent = parent;
Count++;
```
插入红色元素后，查看其父节点（红黑树中）的颜色，如果为黑色，那么说明在伪B树节点中，插入的这个节点一定没有满，那么不需要调整。如果是红色，那么要看父节点的兄弟节点的颜色，如果兄弟节点是黑色，说明这个伪B树节点可以放下这个元素，只是歪了（就是一个黑色元素的一边有两个红色元素，另一边没有元素），这时可以使用左旋或右旋操作进行调整。目的是把大小排序中间的元素调整到伪B树节点的中间，这里要分LL，LR，RL，RR四种情况（此处的代码以插入节点的父节点在祖父节点的右边为例，也就是LL和LR两种情况）
```C#
if(GetColor(grandpa.right) == Color.Black)
{
    if(isLeftChild)
    {
        R_Rotate(grandpa);
        grandpa.color = Color.Red;
        parent.color = Color.Black;
    }
    else
    {
        L_Rotate(parent);
        R_Rotate(grandpa);
        grandpa.color = Color.Red;
        newNode.color = Color.Black;
    }
    return;
}
```
如果兄弟节点是红色，说明这个伪B树节点已经满了，插入新元素后要上溢出。把黑色元素变红当作新插入的元素，原先存在的两个红色节点变黑形成两个新的伪B树节点，插入节点不变色，调整相关变量后，进行下一次迭代
```C#
else
{
    grandpa.color = Color.Red;
    parent.color = Color.Black;
    grandpa.right!.color = Color.Black;
    newNode = grandpa;
    parent = newNode.parent;
}
```
情况分析完毕，下面是插入的完整代码
```C#
public void Insert(T value)
{
    Node? cur = root;
    Node? parent = null;
    bool isLeftChild = true;
    while(cur != null)
    {
        parent = cur;
        int diff = value.CompareTo(cur.value);
        if (diff == 0)
            return;
        if(diff > 0)
        {
            isLeftChild = false;
            cur = cur.right;
        }
        else
        {
            isLeftChild = true;
            cur = cur.left;
        }
    }

    Node newNode = new Node(value);
    if(parent == null)
    {
        root = newNode;
        root.color = Color.Black;
        Count++;
        return;
    }
    
    if(isLeftChild)
        parent.left = newNode;
    else
        parent.right = newNode;
    newNode.parent = parent;
    Count++;
    
    while(GetColor(parent) == Color.Red)
    {
        Node grandpa = parent!.parent!;
        isLeftChild = (parent.left == newNode);
        if(parent == grandpa.left)
        {
            if(GetColor(grandpa.right) == Color.Black)
            {
                if(isLeftChild)
                {
                    R_Rotate(grandpa);
                    grandpa.color = Color.Red;
                    parent.color = Color.Black;
                }
                else
                {
                    L_Rotate(parent);
                    R_Rotate(grandpa);
                    grandpa.color = Color.Red;
                    newNode.color = Color.Black;
                }
                return;
            }
            else
            {
                grandpa.color = Color.Red;
                parent.color = Color.Black;
                grandpa.right!.color = Color.Black;
                newNode = grandpa;
                parent = newNode.parent;
            }
        }
        else
        {
            if(GetColor(grandpa.left) == Color.Black)
            {
                if(isLeftChild)
                {
                    R_Rotate(parent);
                    L_Rotate(grandpa);
                    grandpa.color = Color.Red;
                    newNode.color = Color.Black;
                }
                else
                {
                    L_Rotate(grandpa);
                    grandpa.color = Color.Red;
                    parent.color = Color.Black;
                }
                return;
            }
            else
            {
                grandpa.color = Color.Red;
                parent.color = Color.Black;
                grandpa.left!.color = Color.Black;
                newNode = grandpa;
                parent = newNode.parent;
            }
        }
    }
    if(parent == null)
    {
        root = newNode;
        newNode.color = Color.Black;
    }
}
```

### Delete方法
Delete方法也会分段说明，首先要搜索插入节点的位置，和搜索的逻辑是相同的
```C#
Node? cur = root;
Node? parent = null;
while (cur != null)
{
    int diff = value.CompareTo(cur.value);
    if (diff == 0)
        break;
    parent = cur;
    if(diff > 0)
        cur = cur.right;
    else 
        cur = cur.left;
}
if (cur == null)
    return;

Count--;
```
如果待删除的节点左右子树都不为空，那么要用它的直接前驱代替它，并删除它的直接前驱（直接后继也可以，本文使用的是直接前驱），注意这里的代替只是数值的取代，不包含颜色。这一步和大多数平衡二叉树是一样的，起到一个化繁为简的作用
```C#
if(cur.right != null && cur.left != null)
{
    Node temp= cur;
    parent = cur;
    cur = cur.left;
    while(cur.right != null)
    {
        parent = cur;
        cur = cur.right;
    }
    (cur.value, temp.value) = (temp.value, cur.value);
}
```
待删除元素如果是红色的，说明该伪B树节点至少有两个元素，删除其中一个元素，不发生下溢出（4阶B树最少可以有一个元素）同时它最多有一个子树，直接连接该子树和它的父节点。因此删除红色节点这是不会破坏伪B树的性质的（当然也不会破环红黑树的性质，不过这里不做说明）
```C#
if(cur.color == Color.Red)
{
    if(parent!.left == cur)
    {
        if (cur.left != null)
        {
            parent.left = cur.left;
            cur.left.parent = parent;
        }
        else if (cur.right != null)
        {
            parent.left = cur.right;
            cur.right.parent = parent;
        }
        else
            parent.left = null;
    }
    else
    {
        if(cur.left != null)
        {
            parent.right = cur.left;
            cur.left.parent = parent;
        }
        else if(cur.right != null)
        {
            parent.right= cur.right;
            cur.right.parent = parent;
        }
        else 
            parent.right = null;
    }
}
```
如果待删除节点是黑色。要判断该节点是否有红色子节点（最多只能有一个），如果有，说明伪B树节点有2个元素，删除黑色元素后不会发生下溢出，只是要把原本的红色元素变为黑色元素作为伪B树节点的中心，就是变色加调整和祖父节点之间的连接
```C#
bool isLeftChild = (parent.left == cur);
if (GetColor(cur.left) == Color.Red)
{
    Node temp = cur.left!;
    if (isLeftChild)
        parent.left = temp;
    else
        parent.right = temp;
    temp.parent = parent;
    temp.color = Color.Black;
    break;
}
else if (GetColor(cur.right) == Color.Red)
{
    Node temp = cur.right!;
    if (isLeftChild)
        parent.left = temp;
    else
        parent.right = temp;
    temp.parent = parent;
    temp.color = Color.Black;
    break;
}
```
如果该节点没有红色子节点。这时考虑B树的情况，说明该节点删除元素小于所需的最小元素的数量了，要先看兄弟节点的元素是否够借。但这里有一个问题，虽然我们使用B树的形式理解，在红黑树中，是元素之间使用指针相连，而不是伪B树节点之间使用指针相连吗，因此找兄弟节点可能找到的是红色节点。这说明父节点所在的伪B树节点有两个元素，为了找到于待删除节点所在的伪B树节点同层的伪B树兄弟节点，需要通过旋转的方式调整，分为待删除节点在父节点的左边或右边两种情况。通过画图可以很好的完成这个过程，这里举例的是待删除节点在父节点左孩子的情况
```C#
Node brother = parent.right!;
if(GetColor(brother) == Color.Red)
{
    L_Rotate(parent);
    brother.color = Color.Black;
    parent.color = Color.Red;
    brother = parent.right!;
}
```
现在能找到同层的伪B树节点的兄弟节点了，要判断兄弟节点是否够借，也就是兄弟节点是否有红色子节点。如果有，怎么借呢，就是通过旋转来借，这里同样有4种情况：待删除节点是父节点的左孩子或右孩子，兄弟节点有红色左孩子或右孩子节点。具体的操作同样通过画图的形式很好理解，以下是待删除节点在父节点左边的情况
```C#
Node? temp = cur.left;
if(GetColor(brother.right) == Color.Red)
{
    L_Rotate(parent);
    brother.color = parent.color;
    parent.color = Color.Black;
    brother.right!.color = Color.Black;
    parent.left = temp;
    if (temp != null)
        temp.parent = parent;
    break;
}
else if(GetColor(brother.left) == Color.Red)
{
    brother.left!.color = parent.color;
    R_Rotate(brother);
    L_Rotate(parent);
    parent.color = Color.Black;
    brother.color = Color.Black;
    parent.left = temp;
    if(temp != null)
        temp.parent = parent;
    break;
}
```
如果兄弟节点不够借。就要找父节点和兄弟节点合并了，并删除父节点。这里分两种情况，如果父节点是红色节点，说明父节点所在的伪B树节点在删除了父节点后是不会下溢出的，从红黑树的角度看此时只需要简单的染色就可以
```C#
if(GetColor(parent) == Color.Red)
{
    parent.color = Color.Black;
    brother.color = Color.Red;
    parent.left = temp;
    if(temp != null)
        temp.parent = parent;
    break;
}
```
如果父节点是黑色，说明在B树中要发生连续的下溢出。我这里的操作是，将父节点和兄弟节点合并后，放在父节点的左孩子处，并在下一次的迭代中删除父节点，同时也可以通过访问下一次的待删除节点（也就是这一次的parent）的左孩子（也就是上述代码中的temp变量）访问这次删除剩余的子树结构。调整相关的变量后就可以开始下一次的迭代了
```C#
else
{
    cur.value = parent.value;
    cur.right = brother;
    cur.left = temp;
    if (temp != null)
        temp.parent = cur;
    brother.parent = cur;
    brother.color = Color.Red;
    parent.right = null;
    cur = parent;
    parent = cur.parent;
}
```
以上的代码均只考虑了待删除节点在父节点左孩子的情况，加上右孩子的情况，就可以得到完整的方法实现，完整的方法实现如下
```C#
public void Delete(T value)
{
    Node? cur = root;
    Node? parent = null;
    while (cur != null)
    {
        int diff = value.CompareTo(cur.value);
        if (diff == 0)
            break;
        parent = cur;
        if(diff > 0)
            cur = cur.right;
        else 
            cur = cur.left;
    }
    if (cur == null)
        return;

    Count--;
    if(cur.right != null && cur.left != null)
    {
        Node temp= cur;
        parent = cur;
        cur = cur.left;
        while(cur.right != null)
        {
            parent = cur;
            cur = cur.right;
        }
        (cur.value, temp.value) = (temp.value, cur.value);
    }

    if(cur.color == Color.Red)
    {
        if(parent!.left == cur)
        {
            if (cur.left != null)
            {
                parent.left = cur.left;
                cur.left.parent = parent;
            }
            else if (cur.right != null)
            {
                parent.left = cur.right;
                cur.right.parent = parent;
            }
            else
                parent.left = null;
        }
        else
        {
            if(cur.left != null)
            {
                parent.right = cur.left;
                cur.left.parent = parent;
            }
            else if(cur.right != null)
            {
                parent.right= cur.right;
                cur.right.parent = parent;
            }
            else 
                parent.right = null;
        }
    }
    else
    {
        while (parent != null)
        {
            bool isLeftChild = (parent.left == cur);
            if (GetColor(cur.left) == Color.Red)
            {
                Node temp = cur.left!;
                if (isLeftChild)
                    parent.left = temp;
                else
                    parent.right = temp;
                temp.parent = parent;
                temp.color = Color.Black;
                break;
            }
            else if (GetColor(cur.right) == Color.Red)
            {
                Node temp = cur.right!;
                if (isLeftChild)
                    parent.left = temp;
                else
                    parent.right = temp;
                temp.parent = parent;
                temp.color = Color.Black;
                break;
            }
            else
            {
                if(isLeftChild)
                {
                    Node brother = parent.right!;
                    if(GetColor(brother) == Color.Red)
                    {
                        L_Rotate(parent);
                        brother.color = Color.Black;
                        parent.color = Color.Red;
                        brother = parent.right!;
                    }

                    Node? temp = cur.left;
                    if(GetColor(brother.right) == Color.Red)
                    {
                        L_Rotate(parent);
                        brother.color = parent.color;
                        parent.color = Color.Black;
                        brother.right!.color = Color.Black;
                        parent.left = temp;
                        if (temp != null)
                            temp.parent = parent;
                        break;
                    }
                    else if(GetColor(brother.left) == Color.Red)
                    {
                        brother.left!.color = parent.color;
                        R_Rotate(brother);
                        L_Rotate(parent);
                        parent.color = Color.Black;
                        brother.color = Color.Black;
                        parent.left = temp;
                        if(temp != null)
                            temp.parent = parent;
                        break;
                    }
                    else
                    {
                        if(GetColor(parent) == Color.Red)
                        {
                            parent.color = Color.Black;
                            brother.color = Color.Red;
                            parent.left = temp;
                            if(temp != null)
                                temp.parent = parent;
                            break;
                        }
                        else
                        {
                            cur.value = parent.value;
                            cur.right = brother;
                            cur.left = temp;
                            if (temp != null)
                                temp.parent = cur;
                            brother.parent = cur;
                            brother.color = Color.Red;
                            parent.right = null;
                            cur = parent;
                            parent = cur.parent;
                        }
                    }
                }
                else
                {
                    Node brother = parent.left!;
                    if (GetColor(brother) == Color.Red)
                    {
                        R_Rotate(parent);
                        brother.color = Color.Black;
                        parent.color = Color.Red;
                        brother = parent.left!;
                    }

                    Node? temp = cur.left;
                    if (GetColor(brother.right) == Color.Red)
                    {
                        R_Rotate(parent);
                        brother.color = parent.color;
                        parent.color = Color.Black;
                        brother.left!.color = Color.Black;
                        parent.right = temp;
                        if (temp != null)
                            temp.parent = parent;
                        break;
                    }
                    else if (GetColor(brother.left) == Color.Red)
                    {
                        brother.right!.color = parent.color;
                        L_Rotate(brother);
                        R_Rotate(parent);
                        parent.color = Color.Black;
                        brother.color = Color.Black;
                        parent.right = temp;
                        if (temp != null)
                            temp.parent = parent;
                        break;
                    }
                    else
                    {
                        if (GetColor(parent) == Color.Red)
                        {
                            parent.color = Color.Black;
                            brother.color = Color.Red;
                            parent.right= temp;
                            if (temp != null)
                                temp.parent = parent;
                            break;
                        }
                        else
                        {
                            cur.value = parent.value;
                            cur.left = brother;
                            cur.right = temp;
                            if (temp != null)
                                temp.parent = cur;
                            brother.parent = cur;
                            brother.color = Color.Red;
                            parent.left = parent.right;
                            parent.right = null;
                            cur = parent;
                            parent = cur.parent;
                        }
                    }
                }
            }
        }
        if(parent == null)
        {
            if(cur.left != null)
            {
                root = cur.left;
                root.parent = null;
                root.color = Color.Black;
            }
            else if(cur.right != null)
            {
                root = cur.right;
                root.parent = null;
                root.color = Color.Black;
            }
            else
                root = null;
        }
    }
}
```

## 总结
在写代码前，我从来没有意识到红黑树的代码量这么大，虽然情况很多，但是大部分都是左旋和右旋。但是在具体的实现中，我发现一个节点在父节点左右的问题经常会使得代码量直接翻倍，上述的代码就是最好的证明。这使得我的实现看起来极其的繁琐，但我没有好的优化思路。红黑树的很多操作其实画图是很好理解的，但用左旋右旋这种具体的语言会变得很麻烦，所以我还是建议读者多画画图，对于红黑树的学习很有帮助