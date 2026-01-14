# CDSL_Learning_13

## PriorytyQueue类说明
PriorityQueue是Queue的一种特殊类型，存入数据时，还要存入该数据的优先级（priority），但调用Dequeue取出队列元素时，会取出优先级最高的那个数据。最简单的PriorityQueue可以在取出元素时遍历整个队列，找出优先级最大的元素，但这一方式复杂度过大，为O(n)。我的实现使用一种称为堆的数据结构作为内部存储。堆是这样一种数据结构：它是线性排序的完全二叉树结构，每个节点的值都大于或小于（大于称为大根堆，反之为小根堆）它的左右孩子。使用这一种数据结构有助于我们取出迅速的找到所有数据中的最大值或最小值，很好地满足了PriorityQueue的需求。

## Heap类实现
### 字段和属性
我所实现的Heap是大根堆，存储实现了IComparable接口的T类型。由于Heap是线性存储的，因此使用List类型字段存储数据且属性Count用于指示Heap中的元素大小。当从下标0开始存储元素时，对应的完全二叉树满足以下性质，一个节点（索引为i）的左孩子的索引为2*i+1，右孩子为2*i+2，父节点的索引为（i-1）/2。
```C#
private CList<T> array;
public int Count { get; private set; }
```

### 构造器
第一类构造器是创造一个空的Heap，第二类构造器将一个数组转化为堆结构（熟悉推排列的读者可以意识到这就是建堆过程），不过这类构造器的算法在后文说明
```C#
public CHeap()
{
    array = new CList<T>();
    Count = 0;
}
public CHeap(T[] array)
{
    this.array = new CList<T>(array);
    Count = array.Length;
    for(int i = Count / 2 - 1; i >= 0; i--)
        Sink(i);
}
```

### 上浮和下沉
上浮（Floating）和下沉（Sink）是堆进行调整的基本操作。上浮是将一个节点元素沿着父节点的方向放置到正确的位置，即如果该元素大于它的父节点元素，两者交换位置，直到比父节点小或者到达索引0。下沉是将一个节点元素沿着孩子节点的方向放置在正确的位置，即如果该元素小于两个孩子（有时候只有一个孩子节点）节点中较大的元素，那么就和较大的孩子节点交换位置，直到没有孩子节点或者比所有的孩子节点都要大为止。这两个操作想要达到效果需要满足以下要求：节点移动方向的树结构已经满足堆结构要求。移动后，包括原节点所在位置在内的树结构都满足堆的要求
```C#
private void Floating(int index)
{
    while(index != 0 && (array[index].CompareTo(array[(index - 1) / 2]) > 0))
    {
        (array[(index - 1) / 2], array[index]) = (array[index], array[(index - 1) / 2]);
        index = (index - 1) / 2;
    }
}
private void Sink(int index)
{
    int child = index * 2 + 1;
    while(child < Count)
    {
        if(child + 1 < Count && (array[child].CompareTo(array[child + 1]) < 0))
            child++;
        if (array[index].CompareTo(array[child]) > 0)
            break;
        (array[index], array[child]) = (array[child], array[index]);
        index = child;
        child = index * 2 + 1;
    }
}
```
这里就可以解释第二类构造器的算法。先用原本的数组构建完全二叉树，此时所有的以叶节点为根节点的子树都是堆结构（因为它们都只有一个元素，没有左右孩子）。从最后一个有叶节点孩子的节点开始，向前不断地做Sink操作，从树上看就是从下层开始，一层一层地满足堆结构的要求，直到整个树都满足堆结构，键堆成功。这种方法比先建立一个空的堆结构，再依次插入数组元素的效率要高

### 公有方法
利用上浮和下沉操作，可以很简单地实现其他方法。Push方法将元素插入堆中，堆中原本的完全二叉树满足堆结构，将新增元素放在完全二叉树的末尾，再将其上浮到正确的位置就能实现插入
```C#
public void Push(T value)
{
    array[Count] = value;
    Floating(Count);
    Count++;
}
```
Peek方法是查看堆中最大的元素，而最大的元素就是索引为0的元素，即整个完全二叉树的根节点元素
```C#
public T Peek()
{
    if(Count == 0)
        throw new InvalidOperationException();
    return array[0];
}
```
Pop方法弹出堆中最大的元素，同时将堆末尾的元素先放置在根节点的位置，此时除根节点外，其他节点满足堆结构，再将根节点元素下沉至正确位置
```C#
public T Pop()
{
    if (Count == 0)
        throw new InvalidOperationException();
    T res = array[0];
    Count--;
    array[0] = array[Count];
    Sink(0);
    return res;
}
```
ToArray方法将堆中的元素依次取出最大值，返回一个递减的数组（可以利用这一点实现排序功能，不过这样做的空间消耗大，效率也比较低，这里和正经的堆排序作比较），因为是依次Pop，所以在操作完成后，原堆中不存在元素，要使用一些方法还原原堆的树结构
```C#
public T[] ToArray()
{
    CList<T> list = new CList<T>(array.ToArray());
    int count = Count;
    T[] res = new T[Count];
    for (int i = 0; i < count; i++)
        res[i] = Pop();
    array = list;
    Count = count;
    return res;
}
```

## PriorityQueue类实现
### 类头
```C#
public class CPriorityQueue<PType, VType>
    where PType : IComparable<PType>
```

### 成员类
优先级队列使用堆进行存储，存储一种称为PVPair的数据对，value是待存储的数据，priority标记该数据的优先级，越大越优先取出。这里要强调：优先级队列与外界的通信始终以VType为主体，priority只是一种标记，因此，PVPair成员类只是作为Heap存储的元素类型，不应该被外界看见。后续的方法实现也要始终注意这一原则
```C#
private class PVPair : IComparable<PVPair>
{
    public PType priority;
    public VType value;

    public PVPair(PType priority, VType value)
    {
        this.priority = priority;
        this.value = value;
    }

    public int CompareTo(PVPair? other)
    {
        if (other == null)
            return 1;
        return priority.CompareTo(other.priority);
    }
}
```

### 字段，属性和构造器
使用Heap类型的字段存储数据，Count属性指示队列大小，构造器是创立一个空的优先级队列
```C#
private CHeap<PVPair> heap;
public int Count {  get { return heap.Count; } }

public CPriorityQueue()
{
    heap = new CHeap<PVPair>();
}
```

### 方法
优先级队列的方法大多可以直接依赖堆的方法，要做的只是PVPair的包装和去包装，就是将输入值转化为PVPair，并将从堆中取出的值转化为VType再返回给用户，Enqueue，Dequeue，Peek都是这样
```C#
public void Enqueue(VType value, PType priority)
{
    PVPair newPair = new PVPair(priority, value);
    heap.Push(newPair);
}

public VType Dequeue()
{
    if (Count == 0)
        throw new InvalidOperationException();
    return heap.Pop().value;
}

public VType Peek()
{
    if(Count == 0)
        throw new InvalidOperationException();
    return heap.Peek().value;
}
```
Clear方法是清空队列，可以直接创建新的空堆实现
```C#
public void Clear()
{
    heap = new CHeap<PVPair>();
}
```
ToArray是用优先级队列得到一个优先级递减的VType类型数组，注意对KVPair去包装即可
```C#
public VType[] ToArray()
{
    VType[] res = new VType[Count];
    PVPair[] array = heap.ToArray();
    for (int i = 0; i < Count; i++)
        res[i] = array[i].value;
    return res;
}
```
CopyTo是将优先级队列复制到指定数组的指定位置
```C#
public void CopyTo(VType[] array, int index)
{
    PVPair[] pairs = heap.ToArray();
    int len = pairs.Length;
    if(index < 0 || index >= len)
        throw new ArgumentOutOfRangeException("index");
    for(int i = 0; i < Count && (index + i) < len; i++)
        array[index + i] = pairs[i].value;
}
```

## 总结
实现优先级队列的基础是理解堆的算法，因为优先级队列的所有操作基本都是对堆的简单调用。而学习堆，重点是要熟悉数组和完全二叉树之间的联系，这也是教科书中唯一的线性结构的树