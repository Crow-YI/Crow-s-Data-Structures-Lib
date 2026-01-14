# CDSL_Learning_02

## List\<T>类说明
如果说上次的ArrayList类是一个存储object类的vector，那么List从性质上说基本上等同于vector，只不过微软为它新加了一点点点点的方法，加到打开官方文档东西多到看不过来的程度。既然List算是ArrayList的泛化，这一次的代码基本上照抄了ArrayList的代码，只是加入了泛型的概念，逻辑上不变

## List\<T>类的实现
正如之前所说，List的很多方法基本就是把ArrayList的方法里的object改成T，所以除非有较大改动，不然就不做说明了
### 字段和属性
```C#
protected T[] array;
public int Count { get; protected set; }
public int Capacity { get; protected set; }
public T this[int index]
{
    get
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException("index");
        return array[index];
    }
    set
    {
        if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException("index");
        else if (index == Count)
            Add(value);
        else
            array[index] = value;
    }
}
```
上次ArrayList做完之后我发现再C#官方文档中把索引器作为一种属性，那之后我也把索引器放在这里，关于索引器，我也进行了较大的改动，之前没有考虑到index越界的问题，这次加上了，同时也增加了set方法中index==Count的情况，把它视为一种Add方法的调用，这样就可以写出以下这种代码了
```C#
List<int> list = new();
for (int i = 0; i < 100; i++)
    list[i] = i;
```
​会有人这样写代码吗，不知道，反正写着

### 构造器
基本不变
```C#
public List()
{
    array = new T[10];
    Capacity = 10;
    Count = 0;
}
public List(int capacity)
{
    array = new T[capacity];
    Capacity = capacity;
    Count = 0;
}
public List(T[] array)
{
    int len = array.Length;
    Capacity = (len / 10 + 1) * 10;
    this.array = new T[Capacity];
    Count = 0;
    foreach (T item in array)
    {
        this.array[Count++] = item;
    }
}
```
​
### Add类方法
基本不变
```C#
protected void AddCapacity(int num = 1)
{
    int newCapacity = Capacity * (int)Math.Pow(2, num);
    T[] newArray = new T[newCapacity];
    for (int i = 0; i < Count; i++)
        newArray[i] = array[i];
    Capacity = newCapacity;
    array = newArray;
}

protected int CalcAddition(int len)
{
    int times = (Count + len) / Capacity;
    int count = 0;
    while (times > 0)
    {
        times >>= 1;
        count++;
    }
    return count;
}

public void Add(T item)
{
    if (Count == Capacity)
        AddCapacity();
    array[Count++] = item;
}

public void AddRange(T[] array)
{
    int len = array.Length;
    int count = CalcAddition(len);
    if (count != 0)
        AddCapacity(count);
    for (int i = 0; i < len; i++)
        this.array[Count + i] = array[i];
    Count += len;
}

public void Insert(int index, T item)
{
    if (index < 0 || index > Count)
        throw new ArgumentOutOfRangeException(nameof(index));
    if (Count == Capacity)
        AddCapacity();
    for (int i = Count - 1; i >= index; i--)
        array[i + 1] = array[i];
    array[index] = item;
    Count++;
}

public void InsertRange(int index, T[] array)
{
    if (index < 0 || index > Count)
        throw new ArgumentOutOfRangeException(nameof(index));
    int len = array.Length;
    int count = CalcAddition(len);
    if (count != 0)
        AddCapacity(count);
    for (int i = Count - 1; i >= index; i--)
        this.array[i + len] = this.array[i];
    for (int i = 0; i < len; i++)
        this.array[index + i] = array[i];
    Count += len;
}
```
​
### Remove类方法
基本不变
```C#
public void Clear()
{
    Count = 0;
}

public void Remove(T value)
{
    int index = IndexOf(value);
    if (index == -1)
        return;
    else
    {
        for (int i = index; i < Count - 1; i++)
            array[i] = array[i + 1];
        Count--;
    }
}

public void RemoveAt(int index)
{
    if (index < 0 || index >= Count)
        throw new ArgumentOutOfRangeException();
    for (int i = index; i < Count - 1; i++)
        array[i] = array[i + 1];
    Count--;
}
```
​
### 其他方法
IndexOf方法与之前变化较大，因为之前是object类型可以直接用array\[i] == value，但是换成了泛型T，无法保证T也实现了\==操作符的重载，不能直接写==，否则编译器会报错。这个报错是非常有帮助的，C++是没用这个报错的，大一我写自定义vector的时候就是直接用的==，然后写大作业的时候，泛型T也是一个很复杂的自定义的类型，这个T类型没有重载\==操作符（即使那时并未用到要使用==重载的方法），之后运行时报错，我为了抓这只虫子debug了一下午，如果能在写自定义vector类时注意到这一点会方便很多

C#的所有类都是由object派生的，且object类型有一个虚函数Equals，以下的代码意思是当T类型有重写这个方法时会调用重写的方法，否则调用object的方法，该函数有很好的兼容性
```C#
public int IndexOf(T value)
{
    var comparer = EqualityComparer<T>.Default;
    for (int i = 0; i < Count; i++)
    {
        if (comparer.Equals(array[i], value))
            return i;
    }
    return -1;
}
```
​
Reserve方法也进行了一定的修改，我发现了一个很好用的语法糖，能够方便的实现两个元素的交换
```C#
public void Reserve()
{
    int left = 0;
    int right = Count - 1;
    while (left < right)
    {
        (array[right], array[left]) = (array[left], array[right]);
        left++;
        right--;
    }
}
```
​
其余的方法基本不变
```C#
public bool Contains(T value)
{
    if (IndexOf(value) < 0)
        return false;
    else
        return true;
}

public T[] ToArray()
{
    T[] array = new T[Count];
    int count = 0;
    foreach (T item in this.array)
        array[count++] = item;
    return array;
}

public void TrimToSize()
{
    if (Count != Capacity)
    {
        T[] newArray = new T[Count];
        int count = 0;
        foreach (T item in array)
            newArray[count++] = item;
        Capacity = count;
        array = newArray;
    }
}

virtual public List<T> GetRange(int start, int len = Int32.MaxValue)
{
    if (start < 0 || start >= Count || len <= 0)
        throw new ArgumentOutOfRangeException();
    List<T> array = new List<T>();
    int count = 0;
    while (count < len && (count + start) < Count)
        array.Add(this.array[start + count]);
    return array;
}

public IEnumerator GetEnumerator()
{
    return new Enumerator(this);
}

public class Enumerator : IEnumerator
{
    private readonly T[] array;
    private readonly int arrSize;
    private int count;

    public Enumerator(List<T> list)
    {
        array = list.array;
        arrSize = list.Count;
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
​
## 总结
List类是对ArrayList类的泛化，同时我也修改了相关的逻辑，整体看起来更工整，后续可能不知道什么时候我会把ArrayList类再拿出来改一改。完成了List类后，可以用List派生ArrayList类
```C#
public class ArrayList : List<object>
{
    public ArrayList() { }
    public ArrayList(int capacity) : base(capacity) { }
    public ArrayList(object[] array) : base(array) { }

    override public ArrayList GetRange(int start, int len = Int32.MaxValue)
    {
        if(start < 0 || start >= Count || len <= 0)
            throw new ArgumentOutOfRangeException();
        ArrayList array = new ArrayList();
        int count = 0;
        while(count < len && (count + start) < Count)
            array.Add(this.array[start + count]);
        return array;
    }        
}
```
​这样再看ArrayList我就满意多了