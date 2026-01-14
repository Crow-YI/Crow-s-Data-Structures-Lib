# CDSL_Learning_03

## Stack和Queue类说明
从内存上看，Stack和Queue都是线性存储的，即在内存上都连续排列；从功能上看，Stack是LIFO（后进先出），像压盘子一样，Queue是LILO（后进后出），像排队一样。用户只能访问到最后（Stack）和最前（Queue）的元素，而不能直接操控其他元素。因此，我们用List来作为内部的存储，并将访问最后或最前的元素的方法暴露，其他的方法隐藏。实现上很简单，不做过多说明

## Stack类实现
### 字段和属性
list用于存储数据，size用于指示list内部的元素数量（我有考虑过使用list.Count的方法实时获取的方式代替size，但发现性能下降不少，不如直接加一个int字段），Count用于返回Stack元素数量，因为不能直接暴露出结尾元素外的其他元素，所以不添加索引器
```C#
private readonly List<T> list;
private int size;
public int Count
{
    get { return size; }
}
```

### 构造器
构造器有默认和使用数组构造两种方式
```C#
public Stack()
{
    list = new List<T>();
    size = 0;
}
public Stack(T[] array)
{
    list = new List<T>(array);
    size = array.Length;
}
```

### Stack基本操作
Stack要实现push（压栈），pop（弹出栈顶元素），peek（查看栈顶元素），clear（清空栈），contains（查看栈内是否还有某个元素），toArray（将栈内元素依次弹出，形成数组），copyTo（将栈内元素依次弹出，放置在数组的指定位置）
```C#
public void Push(T item)
{
    list.Add(item);
    size++;
}

public T Pop()
{
    if (size == 0)
        throw new InvalidOperationException();
    size--;
    T res = list[size];
    list.RemoveAt(size);
    return res;
}

public T Peek()
{
    return list[size - 1];
}

public void Clear()
{
    list.Clear();
    size = 0;
}

public bool Contains(T item)
{
    return list.Contains(item);
}

public T[] ToArray()
{
    T[] array = new T[size];
    for(int i = 0; i < size; i++)
        array[i] = list[size - 1 - i];
    return array;
}

public void CopyTo(T[] array, int arrayIndex)
{
    if(arrayIndex < 0 || arrayIndex > array.Length)
        throw new ArgumentOutOfRangeException();
    for(int i = 0; i < size; i++)
        array[arrayIndex + i] = list[size - 1 - i];
}
```

### IEnumerable实现
注意，栈的元素是从顶部依次弹出的，因此枚举时也是从顶部依次枚举的（也就是从list的尾部开始枚举）
```C#
public IEnumerator GetEnumerator()
{
    return new Enumerator(this);
}

public class Enumerator : IEnumerator
{
    private readonly T[] array;
    private readonly int arrSize;
    private int count;

    public Enumerator(Stack<T> stack)
    {
        array = stack.ToArray();
        arrSize = stack.Count;
        count = -1;
    }
    public object Current => array[count];

    public bool MoveNext()
    {
        if (++count < arrSize)
            return true;
        return false;
    }

    public void Reset()
    {
        count = -1;
    }
}
```

## Queue类实现
Queue的实现和Stack很像，只不过Queue是将List的最前一个元素暴露给用户，Stack将最后一个元素暴露给用户

### 字段和属性
因为Queue类方法中需要list大小的地方不多，因此不需要专门设置一个size来存放大小，而是在需要时调用list中的Count属性
```C#
private List<T> list;
public int Count { get { return list.Count; } }
```

### 方法
其中，注意toArray，copyTo和IEnumerable实现时，是从前往后枚举的，其他的逻辑都很简单，不做说明
```C#
public Queue()
{
    list = new List<T>();
}
public Queue(T[] array)
{
    list = new List<T>(array);
}

public void EnQueue(T item)
{
    list.Add(item);
}

public T DeQueue()
{
    T res = list[0];
    list.RemoveAt(0);
    return res;
}

public T Peek()
{
    return list[0];
}

public void Clear()
{
    list.Clear();
}

public bool Contains(T item)
{
    return list.Contains(item);
}

public T[] ToArray()
{
    return list.ToArray();
}

public void CopyTo(T[] array, int arrayIndex)
{
    int size = list.Count;
    int len = array.Length;
    if(arrayIndex  < 0 || arrayIndex >= len)
        throw new IndexOutOfRangeException();
    for(int i = 0; i < size && (arrayIndex + i) < len; i++)
        array[arrayIndex + i] = list[i];
}

public IEnumerator GetEnumerator()
{
    return new Enumerator(this);
}

public class Enumerator : IEnumerator
{
    private readonly T[] array;
    private int arrSize;
    private int count;

    public Enumerator(Queue<T> queue)
    {
        array = queue.ToArray();
        arrSize = queue.Count;
        count = -1;
    }
    public object Current => array[count];

    public bool MoveNext()
    {
        if(++count < arrSize)
            return true;
        return false;
    }

    public void Reset()
    {
        count = -1;
    }
}
```

## 总结
可以说，Stack和Queue是对List的一层封装，将需要被用户访问的元素暴露，其余的隐藏。到此，简单的线性结构应该就结束了，之后的数据结构大概就不会有这么多“不做说明”了