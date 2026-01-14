# CDSL_Learning_09

## 类补充说明
上次实现了LinkedList类之后，我发现书中的下一个章节Iterator类是一个服务于LinkedList的迭代器，迭代器是C++中常见的概念，但在C#的框架中，我认为它已经被IEnumerable和IEnumerator接口中的方法所代替，我们通过对这两个接口的实现和补充，也可以实现针对链表单个节点的枚举和插入删除操作。所以我决定不实现Iterator类，而是对原有的LinkedList类进行补充，通过修改原有的Enumerator成员类来实现Iterator类的功能

## LinkedList类补充
### Enumerator类修改
为了使Enumerator类具有更多的功能，即该类不再是只能静态的枚举链表元素，而是能动态的反作用于原链表，需要修改原类中的字段，将原本的header字段改为list字段，使得该成员类能够反作用于原来的链表，同时将原方法中涉及header的代码修改成list.header，保证原方法的功能不改变
```C#
private readonly CLinkedList<T> list;
private Node? current;
private bool isFirstTime;

public Enumerator(CLinkedList<T> list)
{
    this.list = list;
    current = list.header;
    isFirstTime = true;
}

public object? Current
{
    get
    {
        if(current == null || isFirstTime)
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
    current = list.header;
    isFirstTime = true;
}
```

### 增加方法
InsertBefore和InsertAfter是在原链表的当前元素（Enumerator中current所指向的元素）的前后插入新节点，因为是双向链表，所以实现起来很简单，要分别注意头元素和尾元素的情况。这里还要注意，当isFirstTime为真时，为了保证和原先逻辑一致，即使current指向链表的头节点，我们也要认为此时current指向头节点前的一个“虚空节点”，可以理解为其他数据结构枚举器count等于-1的情况，上一篇文章对此做过说明
```C#
public void InsertBefore(T value)
{
    if(current == null || isFirstTime)
        return;
    if(current.prev == null)
        list.Add(value);
    else
    {
        Node newNode = new Node(value);
        current.prev.next = newNode;
        newNode.prev = current.prev;
        current.prev = newNode;
        newNode.next = current;
    }
    list.Count++;
}

public void InsertAfter(T value)
{
    if(current == null || isFirstTime)
        return;
    Node newNode = new Node(value);
    if( current.next == null)
    {
        current.next = newNode;
        newNode.prev = current;
    }
    else
    {
        newNode.next = current.next;
        current.next.prev = newNode;
        current.next = newNode;
        newNode.prev = current;
    }
    list.Count++;
}
```
Remove方法虽说可以调用LinkedList中的Remove方法，但这个调用需要重新定位一次元素的位置，从性能的角度出发，我选择麻烦的写法，分类为唯一节点，首节点，尾节点，中间节点4种情况，不能像LinkedList中分为两种情况考虑的原因是需要保持对current的有效性，其中在首节点和中间节点时，current所指节点删除后current自动指向下一节点，尾节点的current要指向上一节点，唯一节点时，current要为null
```C#
public void Remove()
{
    if(current == null || isFirstTime) 
        return;
    if( current.next == null && current.prev == null)
    {
        current = null;
        list.header = null;
    }
    else if(current.prev == null)
    {
        current = current.next ?? throw new InvalidOperationException();
        current.prev = null;
        list.header = current;
    }
    else if(current.next == null)
    {
        current = current.prev;
        current.next = null;
    }
    else
    {
        current.prev.next = current.next;
        current.next.prev = current.prev;
        current = current.next;
    }
    list.Count--;
}
```
AtEnd用于判断当前current是否指向尾节点
```C#
public bool AtEnd()
{
    return (current == null || current.next == null);
}
```

## 总结
之前在C++中编写迭代器时特别麻烦，各种规则都不清楚，上网找资料也说不明白，还是C#的接口好用，使用一个好用的IDE可以直接把要实现的方法帮忙罗列出来，各种官方文档也比去找C++的标准方便很多，这次是吃到甜头了