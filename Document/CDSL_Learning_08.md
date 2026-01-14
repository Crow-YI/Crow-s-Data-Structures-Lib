# CDSL_Learning_08

## LinkedList类说明
我之前实现的所有的数据结构都是线性结构，它们所存储的数据在计算机的内存中是连续排列的。而这次要实现的LinkedList不同，它是链式排列的，这种类型的数据结构大多存储一种节点式的数据，每个节点除了实际存储的内容外，还有若干个引用类型的变量用于存储与该节点相关联的其他节点的地址，可以通过这些变量访问其他的节点，节点在内存中不是连续存储的，不使用下标等方式进行访问，而是通过那些引用类型的变量寻找其他的数据。简单的链式结构有3中，单链表，双链表，循环链表，它们通过不同的数据组织形式模仿之前所说的List的功能。从链式存储和线性存储的差别来看，LinkedList在增加，插入，删除元素时更有优势，所需的操作更少，List在访问各个元素时可以直接访问，更有优势。在3种简单的链式结构中，双向链表在所需空间和使用性能之间的平衡得很好，在我的实践之中，大部分链表都是使用双向链表效果最优，因此，我的LinkedList是使用双向链表实现的

## LinkedList类实现
### Node成员类
不同的链式结构有着不同的节点类型，为了避免取名危机，可以将不同的Node类放在各自的链式结构中。双向链表中，每一个数据（除开首数据和尾数据外）都有两个引用类型的变量（下文中称为指针，有C语言背景的人都习惯这么说）分别指向自己的上一个节点和下一个节点，注意指针是可空的
```C#
public class Node
{
    public T value;
    public Node? prev;
    public Node? next;
    public Node()
    {
        value = default!;
        prev = null;
        next = null;
    }
    public Node(T value)
    {
        if(value == null)
            throw new ArgumentNullException("value");
        this.value = value;
        prev = null;
        next = null;
    }
    public Node(T value, Node prev, Node next)
    {
        if(value == null)
            throw new ArgumentNullException("value");
        this.value = value;
        this.prev = prev;
        this.next = next;
    }
}
```

### 字段和属性
每个LinkedList类都只需要保留一个头节点的指针作为整个链式结构的入口，还有一个Count属性用于记录链表中的节点数
```C#
private Node? header;
public int Count { get; private set; }
```

### 构造器
链表的构造器也很简单，因为初始时没有存储节点，所以header为空，Count为0
```C#
public CLinkedList()
{
    header = null;
    Count = 0;
}
```

### 方法
LinkedList是通过链式结构仿照List实现功能，首先是Add，向链表中添加元素，此处使用头插法，在没有尾节点入口的情况下，尾插法需要寻找到尾节点，开销太大，头插法只需要简单的几步操作，注意要考虑链表中没有元素的情况
```C#
public void Add(T value)
{
    Node newNode = new Node(value);
    if (header == null)
        header = newNode;
    else
    {
        header.prev = newNode;
        newNode.next = header;
        header = newNode;
    }
    Count++;
}
```
因为插入元素的操作和删除元素的操作都需要找到特定的节点，因此可以先实现一个私有方法Find用于查找链表内特定节点，并返回指针
```C#
private Node? Find(T value)
{
    Node? walkPoint = header;
    while(walkPoint != null)
    {
        if (walkPoint.value != null)
            if (walkPoint.value.Equals(value))
                break;
    }
    return walkPoint;
}
```
因为在链表中没有下标的概念（毕竟不能通过下标直接访问元素，下标的意义不大），所以链表的插入不是使用索引插入，而是将新的元素插入到指定元素的前后一位，不同的链表在这里的实现略有差异，有的是前，有的是后，但因为加入元素时采用头插法，我认为这里插入在指定元素前更符合直觉，注意考虑指定元素是头元素的情况
```C#
public void Insert(T value, T posValue)
{
    Node? pos = Find(posValue);
    if (pos == null)
        return;
    Node newNode = new Node(value);
    if(pos.prev == null)
    {
        pos.prev = newNode;
        newNode.next = pos;
        header = newNode;
    }
    else
    {
        pos.prev.next = newNode;
        newNode.next = pos;
        newNode.prev = pos.prev;
        pos.prev = newNode;
    }
    Count++;
}
```
Remove操作要考虑的情况更多，有节点在中间，节点是头元素，节点是尾元素3种情况，但此时可以反向考虑，考虑节点前后是否有元素两种情况，若节点前无元素，说明节点是头节点，记得要重新给header赋值
```C#
public void Remove(T value)
{
    Node? pos = Find(value);
    if( pos == null)
        return;
    if(pos.next != null)
        pos.next.prev = pos.prev;
    if (pos.prev != null)
        pos.prev.next = pos.next;
    else
        header = pos.next;
    Count--;
}
```
Clear和Contains都是老熟人了，实现上与之前的数据结构没太大变化，不做说明
```C#
public void Clear()
{
    header = null;
    Count = 0;
}

public bool Contains(T value)
{
    Node? pos = Find(value);
    if(pos == null)
        return false;
    return true;
}
```

### IEnumerable实现
链表的枚举器的实现与线性结构有所不同，差别在于MoveNext的实现，线性结构中++count < size的模式不管用了，毕竟0之下还有-1，而头节点前没有节点了，为处理第一次MoveNext的问题，我选用bool变量标记的方式解决
```C#
public IEnumerator GetEnumerator()
{
    return new Enumerator(this);
}

public class Enumerator : IEnumerator
{
    private readonly Node? header;
    private Node? current;
    private bool isFirstTime;

    public Enumerator(CLinkedList<T> list)
    {
        header = list.header;
        current = header;
        isFirstTime = true;
    }

    public object? Current
    {
        get
        {
            if(current == null )
                return null;
            return current.value;
        }
    }

    public bool MoveNext()
    {
        if(current == null)
            return false;
        if(isFirstTime)
        {
            isFirstTime = false;
            return true;
        }
        if(current.next == null)
            return false;
        current = current.next;
        return true;
    }

    public void Reset()
    {
        current = header;
        isFirstTime = true;
    }
}
```

## 总结
用惯了C的指针，再来看C#的引用类型，真是用得很不顺手，经常习惯性地开始解引用。除开这个问题，C#对空引用的限制极大，到处给我报可能空引用的警告，修那些警告加了好多if语句，看着非常臃肿，虽然安全也是真的安全