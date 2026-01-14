# CDSL_Learning_16

## SplayTree类说明
SplayTree（伸展树）也是一种二叉搜索树，它可以通过一种称为伸展的操作将新访问的元素放在根节点的位置。虽然它不具备平衡二叉搜索树那种减少树高的能力，但是它能减少常用元素的搜索次数，起到类似于缓存的功能，以此减少一系列操作所需的时间（单看一个操作，效率不如平衡二叉搜索树）。使用场景于平衡二叉搜索树有所不同

## SplayTree类实现
### 类头
```C#
public class CSplayTree<T>
    where T : IComparable<T>
```

### Node成员类
本次使用迭代的思想实现相关方法（当然理解原理之后也可以改为递归思想），因此需要父节点指针，此外，不需要任何其他的用于维持树平衡的变量
```C#
private class Node
{
    public T value;
    public Node? parent;
    public Node? left;
    public Node? right;

    public Node(T value)
    {
        this.value = value;
        parent = null;
        left = null;
        right = null;
    }
}
```

### 字段，属性和构造器
实现了这么多二叉树了，这些已经是手到擒来了
```C#
private Node? root;
public int Count { get; private set; }

public CSplayTree()
{
    root = null;
    Count = 0;
}
```

### 左旋和右旋
伸展树的基本操作是伸展，而伸展分解后不过也只是左旋和右旋。带有父节点指针的左旋和右旋在红黑树已经讲过了，这里懒得讲了，代码是一模一样的，看着有点繁琐，可以自己试着优化一下
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
    if (child != null)
        child.parent = node;
}
```

### 伸展操作
伸展（Splay）操作的最终目的是将一个节点换到根节点的位置，而这一过程是通过旋转实现的，当对一个节点进行左旋或者右旋时，它的右节点或者左节点在树结构中就会往上移一位。吸取了红黑树中因为左右旋问题导致代码量翻倍的教训，我这次实现了一个旋转（Rotate）方法。当对一个节点进行旋转时，如果它是父节点的左孩子，就对父节点右旋，反之则左旋，最终让这个节点上移一位
```C#
private void Rotate(Node node)
{
    Node? parent = node.parent;
    if (parent == null)
        return;
    if (parent.left == node)
        R_Rotate(parent);
    else
        L_Rotate(parent);
}
```
伸展操作并不是靠一位位移上去直到目的地的，而是有一套规则，具体的数学原理我看不懂，但是规则可以这样说明：1.如果上移的节点是目的地节点的左右孩子，就正常的进行的左右旋单步操作，2.如果不是左右孩子，我们把待上移的节点记为x，x的父节点为p，p的父节点为g（此时g可能是目的地节点，或者目的地节点在g的祖先序列中）。如果p是g的左孩子，x是p的左孩子，形成向左的一字型，先对g右旋，再对p右旋，得到向右的一字型。反之向右的一字型就反向操作。3.如果p是g的左孩子，x是p的右孩子，形成开口向右的之字形，先对p左旋，再对g右旋。如果是开口向左的之字形，旋转的方向要变。根据上述规则，可以得到下文代码（压缩的比较厉害，将就看看），IsLeftChild用于判断一个节点是否是父节点的左孩子，Splay将node节点上移到des位置
```C#
private bool IsLeftChild(Node node)
{
    Node? parent = node.parent;
    if (parent == null)
        throw new InvalidOperationException();
    if(parent.left == node) 
        return true;
    return false;

}

private void Splay(ref Node des, Node node)
{
    Node? temp = des.parent;
    for(Node? parent; (parent = node.parent) != temp; Rotate(node))
    {
        if(parent!.parent != temp)
        {
            if (IsLeftChild(node) == IsLeftChild(parent))
                Rotate(parent);
            else
                Rotate(node);
        }
        des = node;
    }
}
```

### Insert
讲了这么多Splay树的基础操作，下来就很简单了。插入操作就是按照二叉搜索树的方式将节点插入后，在把该节点上移到根节点的位置
```C#
public void Insert(T value)
{
    Node? node = root;
    Node? parent = null;
    bool isLeftChild = true;

    while(node != null)
    {
        parent = node;
        int diff = value.CompareTo(node.value);
        if (diff == 0)
            return;
        if(diff > 0)
        {
            node = node.right;
            isLeftChild = false;
        }
        else
        {
            node = node.left;
            isLeftChild = true;
        }
    }

    Node newNode = new Node(value);
    Count++;
    if(parent == null)
    {
        root = newNode;
        return;
    }
    if(isLeftChild)
        parent.left = newNode;
    else
        parent.right = newNode;
    newNode.parent = parent;
    Splay(ref root!, newNode);
}
```

### Contains
Contains和Delete方法都会用到一个Find私有方法，Find方法通过正常的二叉搜索树查找元素的方式查找元素，如果找到，就把找到的元素上移到根节点，如果没找到，就把查找失败之前的最后一个元素（这个元素是已存储的元素中大于待查找元素中最小的元素或小于待查找元素中最大的元素）上移到根节点
```C#
private void Find(T value)
{
    if (root == null)
        return;
    Node node = root;
    int diff;
    while ((diff = value.CompareTo(node.value)) != 0)
    {
        if(diff > 0)
        {
            if (node.right == null)
                break;
            node = node.right;
        }
        else
        {
            if(node.left == null)
                break;
            node = node.left;
        }
    }
    Splay(ref root, node);
}
```
此时的Contains方法只需调用Find后查看根节点是否是所找的元素来判断是否存储指定元素
```C#
public bool Contains(T value)
{
    if(root == null)
        return false;
    Find(value);
    return (value.CompareTo(root.value) == 0);
}
```

### Delete
Delete也是先调用FInd方法，如果根节点不是待删除元素，则不用删除。如果是，先判断根节点有几个子树，如果没有子树，那么root设为空，如果子树数量为1，root设为该子树，该子树的根节点的父节点设为空，如果有两个子树，首先把两个子树独立出来，此时可以得到右子树中所有的元素都大于左子树中的所有元素，将右子树最小的元素上移到右子树的根节点处，此时能够保证右子树根节点的左孩子为空，左孩子接上左子树，能够得到一个新的符合二叉搜索树性质的树，注意右子树的父节点要设为空，根节点要指向右子树的根节点，这样就删除了指定节点
```C#
public void Delete(T value)
{
    if (root == null)
        return;
    Find(value);
    if (value.CompareTo(root.value) != 0)
        return;
    Count--;
    if (root.left == null)
    {
        root = root.right;
        if(root != null)
            root.parent = null;
    }
    else if(root.right == null)
    {
        root = root.left;
        root.parent = null;
    }
    else
    {
        Node left = root.left;
        Node right = root.right;

        right.parent = null;
        Node walkPoint = right;
        while (walkPoint.left != null)
            walkPoint = walkPoint.left;
        Splay(ref right, walkPoint);
        right.left = left;
        left.parent = right;
        root = right;
    }
}
```

## 总结
到此，教科书上比较基础的树结构全部实现完毕，终于不用被这些树折磨了。在实现树时，我都是采用链式结构，其实有使用线性结构的实现方式，但因为原理是相同，我就不再实现一遍了，剩下的是图，这项工作就完成了