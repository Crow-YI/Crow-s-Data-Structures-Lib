# CDSL_Learning_12

## AVLTree类说明
AVLTree是平衡二叉搜索树，英文名来源于这种数据结构发明者的名字简写。二叉搜索树虽然在理想情况下的操作复杂度为O(logn)，但是当我们加入依次递增或递减的元素时，二叉搜索树会退化为链表，操作复杂度会提升至O(logn)。为解决这一缺陷，AVLTree通过一系列算法设计，最终能够达成这样一种效果：在整个树结构中，所有的子树的左右子树的高度差值绝对值小于2。通过这样是的树结构尽可能的平衡，不会退化为链表。同时在AVLTree中仍然满足某一节点的左子树中所有元素小于该节点元素，右子树反之

## AVLTree类实现
### Node成员类
在二叉搜索树中，我实现了BinaryTree抽象类，原设想之后的二叉树由此派生，但在实现AVLTree时，我发现到AVLTree的节点要维护一个int类型高度字段，与之前的节点成员类不相同，所以只能放弃之前的想法，不从BinaryTree派生，单独实现一个Node成员类
```C#
private class Node
{
    public T value;
    public int height;
    public Node? left;
    public Node? right;
    public Node()
    {
        value = default!;
        left = null;
        right = null;
        height = 1;
    }
    public Node(T value)
    {
        this.value = value;
        left = null;
        right = null;
        height = 1;
    }
}
```

### 字段和构造器
和之前的链式结构一样，类有一个链式结构头节点的字段和一个指示节点数量的Count属性，构造器初始化这个头节点和Count值。
```C#
private Node? root;
public int Count { get; private set; }

public CAVLtree()
{
    root = null;
    Count = 0;
}
```

### 高度及其维护
每个节点的height字段指示该节点的高度，维护该字段的方式是：任何新创造的节点的高度都为1，已经存在的节点在任何会影响节点高度的操作后（如左旋和右旋）都重新设置height值（取其左右子树高度的较大值再+1）。GetHeight方法在C#中可以用??操作符代替，但我不习惯使用??，主要是觉得可读性较差，又由于这一逻辑在类中多处使用，所以为此定义了GetHeight私有方法。
```C#
private static int GetHeight(Node? node)
{
    if (node == null)
        return 0;
    else 
        return node.height;
}
```

### 左旋和右旋
左旋和右旋操作是AVLTree的基本操作，但其实现效果用纯文字较难说明，请读者自行寻找相关的图解教程，此处展示具体的代码实现，注意要调整受影响的两个节点的高度以及返回调整后的子树根节点
```C#
private Node R_Rotate(Node node)
{
    Node temp = node.left!;
    node.left = temp.right;
    temp.right = node;

    node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));
    temp.height = 1 + Math.Max(GetHeight(temp.left), GetHeight(temp.right));
    return temp;
}
private Node L_Rotate(Node node)
{
    Node temp = node.right!;
    node.right = temp.left;
    temp.left = node;

    node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));
    temp.height = 1 + Math.Max(GetHeight(temp.left), GetHeight(temp.right));
    return temp;
}
```

### Insert
此处对Insert方法进行拆解说明，首先由于AVLTree在插入节点后要回溯调整，因此Insert方法要采用递归的思想实现，但向外部暴露时，要保证方法尽量的简单，我利用了C#的重载机制实现了两个Insert方法，一个公有的Insert只接受一个数据便于用户使用，私有的Insert方法使用递归的思想实现具体的操作（下面的Delete也使用了这一方式），此处展示公有的Insert方法和另一个Insert方法的签名。isAdd用于指示节点是否成功加入，若节点已存在，则无需加入和调整Count值
```C#
public void Insert(T value)
{
    bool isAdd = true;
    root = Insert(value, root, ref isAdd);
    if (isAdd)
        Count++;
}

private Node? Insert(T value, Node? node, ref bool isAdd)
```
AVLTree插入操作的前半部分的与二叉搜索树的插入操作的逻辑是相同的，不过是使用递归思想实现的，递归思想也是AVLTree的理解难点。当diff等于0时，说明要插入的元素和当前节点的元素是相同的，此时插入失败，isAdd要调整为false
```C#
if(node == null)
    return new Node(value);

int diff = value.CompareTo(node.value);
if(diff == 0)
{
    isAdd = false;
    return node;
}
else if(diff < 0)
    node.left = Insert(value, node.left, ref isAdd);
else
    node.right = Insert(value, node.right, ref isAdd);
```
插入元素后，在回溯时，每一个节点都要重新调整树高，并查看平衡因子检查是否失衡（失衡即左右子树的高度之差大于1，不符合AVLTree的定义），我定义了私有方法GetBanlance用于计算节点的平衡因子
```C#
private int GetBalance(Node? node)
{
    if(node == null)
        throw new ArgumentNullException(nameof(node));
    return GetHeight(node.left) - GetHeight(node.right);
}
```
AVLTree的失衡有LL，LR，RR，RL四种类型，具体的失衡类型和失衡解决方法也是建议从图解教程学习，此处只提供具体的代码实现。
```C#
node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));
int balance = GetBalance(node);
if (balance > 1 && GetBalance(node.left) > 0)
    return R_Rotate(node);
if(balance > 1 && GetBalance(node.left) < 0)
{
    node.left = L_Rotate(node.left!);
    return R_Rotate(node);
}
if (balance < -1 && GetBalance(node.right) < 0)
    return L_Rotate(node);
if(balance < -1 && GetBalance(node.right) > 0)
{
    node.right = R_Rotate(node.right!);
    return L_Rotate(node);
}
return node;
```
下附完整的Insert方法实现
```C#
public void Insert(T value)
{
    bool isAdd = true;
    root = Insert(value, root, ref isAdd);
    if (isAdd)
        Count++;
}
private Node? Insert(T value, Node? node, ref bool isAdd)
{
    if(node == null)
        return new Node(value);

    int diff = value.CompareTo(node.value);
    if(diff == 0)
    {
        isAdd = false;
        return node;
    }
    else if(diff < 0)
        node.left = Insert(value, node.left, ref isAdd);
    else
        node.right = Insert(value, node.right, ref isAdd);

    node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));
    int balance = GetBalance(node);
    if (balance > 1 && GetBalance(node.left) > 0)
        return R_Rotate(node);
    if(balance > 1 && GetBalance(node.left) < 0)
    {
        node.left = L_Rotate(node.left!);
        return R_Rotate(node);
    }
    if (balance < -1 && GetBalance(node.right) < 0)
        return L_Rotate(node);
    if(balance < -1 && GetBalance(node.right) > 0)
    {
        node.right = R_Rotate(node.right!);
        return L_Rotate(node);
    }
    return node;
}
```

### Contains
因为Contains不涉及对树结构的调整，不需要使用使用递归的方法（递归的效率是低于迭代的），具体的搜索方式与二叉搜索树一致
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

### Delete
Delete方法拆解说明。与Insert相同，使用重载实现两个Delete方法。此处展示公有的Insert方法和另一个Insert方法的签名，isRemove指示是否成功删除节点
```C#
public void Delete(T value)
{
    bool isRemove = true;
    root = Delete(value, root, ref isRemove);
    if(isRemove)
        Count--;
}
private Node? Delete(T value, Node? node, ref bool isRemove)
```
使用递归的方式寻找待删除的元素，若node为空，说明树结构中不存在该元素，删除失败，将isRemove设为false
```C#
if (node == null)
{
    isRemove = false;
    return null;
}
int diff = value.CompareTo(node.value);
if(diff < 0)
    node.left = Delete(value, node.left, ref isRemove);
else if (diff > 0)
    node.right = Delete(value, node.right, ref isRemove);
```
和二叉搜索树相似，搜索到待删除的元素时，要考虑3种情况：叶节点，有一个子树，有两个子树，不同情况的处理方法在二叉搜索树的实现中已经说明，此处不赘述，要做的只有从迭代思想到递归思想的转变，在递归中，父子节点的关系已经在方法的递归调用中展现，每一次的递归只需处理目前节点的问题
```C#
else
{
    if (node.left == null && node.right == null)
        node = null;
    else if (node.right == null && node.left != null)
        node = node.left;
    else if (node.left == null && node.right != null)
        node = node.right;
    else
    {
        Node walkPoint = node.left!;
        while (walkPoint.right != null)
            walkPoint = walkPoint.right;
        node.value = walkPoint.value;
        node.left = Delete(walkPoint.value, node.left, ref isRemove);
    }
}
```
删除指定节点后，在回溯时，要调整每个节点的高度和失衡问题，此处的处理和Insert中十分相似，唯一的区别在于LL和RR型失衡的判定条件不同
```C#
if (node == null)
    return null;
node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));
int balance = GetBalance(node);
if (balance > 1 && GetBalance(node.left) >= 0)
    return R_Rotate(node);
if (balance > 1 && GetBalance(node.left) < 0)
{
    node.left = L_Rotate(node.left!);
    return R_Rotate(node);
}
if (balance < -1 && GetBalance(node.right) <= 0)
    return L_Rotate(node);
if (balance < -1 && GetBalance(node.right) > 0)
{
    node.right = R_Rotate(node.right!);
    return L_Rotate(node);
}
return node;
```
下附完整的Delete方法
```C#
public void Delete(T value)
{
    bool isRemove = true;
    root = Delete(value, root, ref isRemove);
    if(isRemove)
        Count--;
}
private Node? Delete(T value, Node? node, ref bool isRemove)
{
    if (node == null)
    {
        isRemove = false;
        return null;
    }
    int diff = value.CompareTo(node.value);
    if(diff < 0)
        node.left = Delete(value, node.left, ref isRemove);
    else if (diff > 0)
        node.right = Delete(value, node.right, ref isRemove);
    else
    {
        if (node.left == null && node.right == null)
            node = null;
        else if (node.right == null && node.left != null)
            node = node.left;
        else if (node.left == null && node.right != null)
            node = node.right;
        else
        {
            Node walkPoint = node.left!;
            while (walkPoint.right != null)
                walkPoint = walkPoint.right;
            node.value = walkPoint.value;
            node.left = Delete(walkPoint.value, node.left, ref isRemove);
        }
    }

    if (node == null)
        return null;
    node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));
    int balance = GetBalance(node);
    if (balance > 1 && GetBalance(node.left) >= 0)
        return R_Rotate(node);
    if (balance > 1 && GetBalance(node.left) < 0)
    {
        node.left = L_Rotate(node.left!);
        return R_Rotate(node);
    }
    if (balance < -1 && GetBalance(node.right) <= 0)
        return L_Rotate(node);
    if (balance < -1 && GetBalance(node.right) > 0)
    {
        node.right = R_Rotate(node.right!);
        return L_Rotate(node);
    }
    return node;
}
```

## 总结
递归很难，所以AVLTree很难，但递归在编程中很常见。把握好递归与迭代的本质差别是学习递归的一个好方法。这几天生了场小病，好久没更新了，之后会加快进度的，暑假不多了