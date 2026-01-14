# CDSL_Learning_10

## BSTree类说明
树结构常用链式结构来实现（除了堆以外），每个树结构都有一个根节点作为链式结构的入口，每个节点都有若干个指向子节点的指针。二叉树（BinaryTree）是指每个节点都有最多两个指向子节点指针（称左孩子和右孩子）的树。在细分下去，二叉搜索树（BinarySearchTree）（下文都简称BSTree），是对树的构建规则有了更严格的限制，所有比父节点所储存数据更大的节点作为右节点，更小的节点作为左节点。通过这种排列方式，能够很自然的使用二分搜索法寻找所需元素，减少搜索的时间。

## BinaryTree类实现
因为后续的二叉搜索树，平衡搜索树，红黑树都是二叉树，都具有几乎相同的节点，因此可以构建一个抽象类作为基类，不用每个种类的二叉树都重新构建一次Node成员类。同时，还可以加上二叉树常见的操作作为抽象方法，规范后续类的方法。BinaryTree类较为简单，不做过多说明
```C#
public abstract class CBinaryTree<T>
{
    protected class Node
    {
        public T value;
        public Node? left;
        public Node? right;

        public Node()
        {
            value = default!;
            left = null;
            right = null;
        }
        public Node(T value)
        {
            this.value = value;
            left = null;
            right = null;
        }
    }

    public abstract void Insert(T value);
    public abstract void Delete(T value);
    public abstract bool Contains(T value);
}
```

## BSTree类实现
### 类头
BSTree派生自BinaryTree，但同时要注意到，BSTree中所储存的元素有比较大小的需求（构建BSTree时要根据数据大小来排列），因此要使用where关键字对T进行限制，通过查询文档可知，接口IComparable\<T>要求类对T类型实现CompareTo方法，满足比较大小的要求，因此将T限制为IComparable类
```C#
public class CBSTree<T> : CBinaryTree<T>
    where T : IComparable<T>
```

### 字段及属性
root字段作为整个链式结构的入口，Count是所储存元素的数量，MinValue和MaxValue分别返回BSTree中最小和最大的元素，因为BSTree的构建规则，从根节点开始，不断地取右孩子直到没有右孩子，此时，该节点的值为最大值，最小值取法可类比
```C#
private Node? root;
public int Count {  get; private set; }
public T MaxValue
{ 
    get
    {
        if (root == null)
            throw new InvalidOperationException();
        Node current = root;
        while(current.right != null)
            current = current.right;
        return current.value;
    }
}
public T MinValue
{
    get
    {
        if (root == null)
            throw new InvalidOperationException();
        Node current = root;
        while( current.left != null)
            current = current.left;
        return current.value;
    }
}
```

### 构造器
与链表同理
```C#
public CBSTree()
{
    root = null;
    Count = 0;
}
```

### Insert
如果树此时为空，则根节点为新建节点。如果非空，使用while循环从根节点开始向下，根据大则右孩子，小则左孩子的方式，寻找所要插入的位置，每次插入都是插入到叶节点的位置
```C#
public override void Insert(T value)
{
    Node newNode = new Node(value);
    if(root == null)
    {
        root = newNode;
        Count++;
        return;
    }
    Node walkPoint = root;
    while(true)
    {
        int res = value.CompareTo(walkPoint.value);
        if(res < 0)
        {
            if(walkPoint.left == null)
            {
                walkPoint.left = newNode;
                Count++;
                return;
            }
            else
                walkPoint = walkPoint.left;
        }
        else if(res > 0) 
        {

            if (walkPoint.right == null)
            {
                walkPoint.right = newNode;
                Count++;
                return;
            }
            else
                walkPoint = walkPoint.right;
        }
        else
            return;
    }
}
```

### Contains
使用while循环，从根节点开始寻找所需元素，同插入规则，比较简单，不做说明
```C#
public override bool Contains(T value)
{
    Node? walkPoint = root;
    while (walkPoint != null)
    {
        int res = value.CompareTo(walkPoint.value);
        if (res < 0)
            walkPoint = walkPoint.left;
        else if (res > 0)
            walkPoint = walkPoint.right;
        else
            return true;
    }
    return false;
}
```

### Delete
相较于前两个方法，删除操作更加复杂，需要将方法分解说明。
```C#
public override void Delete(T value)
```
首先需要定位要删除的元素和它的父元素，正如链表一样，删除一个链式结构中的节点是要让指向它的所有指针指向空或其他节点。树中删除一个元素需要它的父节点，同时需要一个值用于指示删除元素是父节点的左孩子还是右孩子。其中，和Cantains逻辑相同的是，当current为null时，说明未找到要删除的元素
```C#
if (root == null)
    return;
Node? current = root;
Node parent = root;
bool isLeftChild = false;
//查找要删除的节点和它的父节点
while (current != null)
{
    int res = value.CompareTo(current.value);
    if (res == 0)
        break;
    parent = current;
    if(res < 0)
    {
        current = current.left;
        isLeftChild = true;
    }
    else
    {
        current = current.right;
        isLeftChild = false;
    }
}
if (current == null)
    return;
```
找到待删除的元素后，考虑三种情况：待删除元素是叶节点，此时可以让父节点的指针直接指向空来删除该节点
```C#
if(current.left == null && current.right == null)
{
    if (current == root)
        root = null;
    else if (isLeftChild)
        parent.left = null;
    else
        parent.right = null;
}
```
待删除节点有右孩子或者左孩子中的一个，此时将父节点指向该节点的指针指向它的孩子节点
```C#
else if(current.left == null)
{
    if(current == root)
        root = current.right;
    else if (isLeftChild)
        parent.left = current.right;
    else
        parent.right = current.right;
}
else if(current.right == null)
{
    if (current == root)
        root = current.left;
    else if (isLeftChild)
        parent.left = current.left;
    else
        parent.right = current.left;
}
```
待删除节点有两个孩子节点，此时为了不破坏树的结构，要找到该节点的直接后继或直接前驱（本文采用直接前驱），找到它的所有子元素中小于该元素的最大元素，即从它的左孩子出发，不断地寻找右孩子直到没有右孩子为止。将该元素从原有位置删除，因为没有右孩子，所以删除一定为前两种情况。然后将该元素放在待删除节点上。先实现一个私有方法，用来找到待删除节点的直接前驱并将该前驱从链式结构中删除
```C#
private Node GetSuccessor(Node delNode)
{
    Node node = delNode.left!;
    Node parent = delNode;
    while(node.right != null)
    {
        parent = node;
        node = node.right;
    }
    if(parent == delNode)
        parent.left = node.left;
    else
        parent.right = node.left;
    return node;
}
```
有了该方法，可以很简单的实现第三种情况的删除操作
```C#
else
{
    Node successor = GetSuccessor(current);
    if (current == root)
        root.value = successor.value;
    else if (isLeftChild)
        parent.left!.value = successor.value;
    else
        parent.right!.value = successor.value;
}
```
接下来是Delete方法的全代码
```C#
public override void Delete(T value)
{
    if (root == null)
        return;
    Node? current = root;
    Node parent = root;
    bool isLeftChild = false;
    //查找要删除的节点和它的父节点
    while (current != null)
    {
        int res = value.CompareTo(current.value);
        if (res == 0)
            break;
        parent = current;
        if(res < 0)
        {
            current = current.left;
            isLeftChild = true;
        }
        else
        {
            current = current.right;
            isLeftChild = false;
        }
    }
    if (current == null)
        return;

    //待删除节点的三种情况
    if(current.left == null && current.right == null)
    {
        if (current == root)
            root = null;
        else if (isLeftChild)
            parent.left = null;
        else
            parent.right = null;
    }
    else if(current.left == null)
    {
        if(current == root)
            root = current.right;
        else if (isLeftChild)
            parent.left = current.right;
        else
            parent.right = current.right;
    }
    else if(current.right == null)
    {
        if (current == root)
            root = current.left;
        else if (isLeftChild)
            parent.left = current.left;
        else
            parent.right = current.left;
    }
    else
    {
        Node successor = GetSuccessor(current);
        if (current == root)
            root.value = successor.value;
        else if (isLeftChild)
            parent.left!.value = successor.value;
        else
            parent.right!.value = successor.value;
    }
    Count--;
}
```

## 总结
从树结构开始，数据结构的复杂性会迅速提升，单单通过文字很难说明白，因此，本文意在简单说明相关算法和具体的代码实现，如果简单的文字说明让你感到相当困惑，推荐去看更形象的数据结构动画图解