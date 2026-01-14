# CDSL_Learning_01

## ArrayList类说明
用我熟悉的c++的话来说，ArrayList类是一个储存object类的vector，也就是以数组的形式储存，在内存中连续排列，并且在元素溢出时能自动地扩展大小。这是一个很简单的类，并没有什么复杂的方法，相较于object[]多了一个自动调节大小的功能，更加灵活。不做过多说明，直接开始实现。

## ArrayList类实现
### 字段和属性
ArrayList的核心是一个object[]类型的字段用于储存元素，属性Count说明类中元素的个数，Capacity说明类中数组的大小，并且Count和Capacity应该不能被外界修改大小，只有在操作数组的元素时才可能改变，所以set要标记private

```C#
private object[] array;
public int Count { get; private set; }
public int Capacity { get; private set; }
```

### 构造器
我为ArrayList实现了3类构造器（微软提供的ArrayList类有远超3类的构造器，但我觉得有一些对于我这种初学者不常用），分别是
**默认构造器**
```C#
public ArrayList()
{
    array = new object[10];
    Capacity = 10;
    Count = 0;
}​
```
**决定数组初始大小的构造器**
```C#
public ArrayList(int capacity)
{
    array = new object[capacity];
    Capacity = capacity;
    Count = 0;
}
```
**使用现有的object数组的构造器**
数组初始大小设定为最接近所提供的数组大小的10的倍数，当然也可以直接设定为所提供的数组大小，我主要是考虑到留一定的空间用于后续的操作，毕竟扩展大小实在是太消耗性能了
```C#
public ArrayList(object[] array)
{
    int len = array.Length;
    Capacity = (len / 10 + 1) * 10;
    this.array = new object[Capacity];
    Count = 0;
    foreach (object item in array)
    {
        this.array[Count++] = item;
    }
}
```

### Add类方法
为ArrayList增加元素要考虑扩展空间的问题，一旦增加元素后空间不足，要及时的扩展空间，具体方法是创建一个新的更大的数组实例（我采用的是将原空间*2），然后将原数组的元素拷贝，然后修改array字段引用的实例，这个方法只给类中其他的方法使用，记得用private修饰符
```C#
private void AddCapacity(int num = 1)
{
    int newCapacity = Capacity * (int)Math.Pow(2, num);
    object[] newArray = new object[newCapacity];
    for (int i = 0; i < Count; i++)
        newArray[i] = array[i];
    Capacity = newCapacity;
    array = newArray;
}
```
可能单独看这个方法会觉得参数很奇怪，直接原空间\*2就可以了，为什么还要用一个Math.Pow方法计算扩展倍数，这是因为有时要增加一整个数组，可能原空间\*2后空间仍然不足，此时要有另一个方法用于计算扩展空间的倍数，用于提供num的值
```C#
private int CalcAddition(int len)
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
```
这两个方法要放在一起看才不会感觉奇怪

接着是增加元素的方法，有简单的add（将元素增加至数组末尾），insert（将元素插入至数组的中间，此时后面的元素要集体后移），还有它们的range版本，也就是插入一整个数组
```C#
public void Add(object item)
{
    if (Count == Capacity)
        AddCapacity();
    array[Count++] = item;
}

public void AddRange(object[] array)
{
    int len = array.Length;
    int count = CalcAddition(len);
    if(count != 0)
        AddCapacity(count);
    for(int i = 0; i < len; i++)
        this.array[Count + i] = array[i];
    Count += len;
}

public void Insert(int index, object item)
{
    if(index < 0 || index > Count)
        throw new ArgumentOutOfRangeException(nameof(index));
    if (Count == Capacity)
        AddCapacity();
    for(int i = Count - 1; i >= index; i--)
        array[i +  1] = array[i];
    array[index] = item;
    Count++;
}

public void InsertRange(int index, object[] array)
{
    if (index < 0 || index > Count)
        throw new ArgumentOutOfRangeException(nameof(index));
    int len = array.Length;
    int count = CalcAddition(len);
    if (count != 0) 
        AddCapacity(count);
    for(int i = Count - 1; i >= index; i--)
        this.array[i + len] = this.array[i];
    for(int i = 0; i < len; i++)
        this.array[index + i] = array[i];
    Count += len; 
}
```
其中，range版本中如果变量count结果是0，说明加上数组后ArrayList的大小仍未超过原空间，不用扩展空间

### Remove类方法
删除元素的方法有3种，删除全部元素，删除指定元素，删除指定位置的元素，要记得移动其余元素的位置
```C#
public void Clear()
{
    Count = 0;
}

public void Remove(object value)
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
    if(index < 0 || index >= Count)
        throw new ArgumentOutOfRangeException();
    for (int i = index; i < Count - 1; i++)
        array[i] = array[i + 1];
    Count--;
}
```
 因为所有的方法都是把Count之前的元素作为储存的元素，所以将Count清零就等效于清空类。Remove中使用的IndexOf方法见下文

### 索引器
类中的索引器模拟查找数组的方式查找类中的元素，ArrayList类可以直接索引字段array的元素
```C#
public object this[int index]
{
    get { return array[index]; }
    set { array[index] = value; }
}
```

### 其他方法
Contains用于查看类中是否有对应元素
```C#
public bool Contains(object value)
{
    if(IndexOf(value) < 0) 
        return false;
    else
        return true;
}
```
​IndexOf方法用于查看对应元素的下标，若没有该元素则返回-1，可以用于删除对应的元素（上文中的Remove）
```C#
public int IndexOf(object value)
{
    for(int i = 0; i < Count; i++)
    {
        if(this.array[i] == value)
            return i;
    }
    return -1;
}
```
Reserve方法用于将数组中的元素翻转
```C#
public void Reserve()
{
    int left = 0;
    int right = Count - 1;
    while (left < right)
    {
        object temp = array[left];
        array[left] = array[right];
        array[right] = temp;
        left++;
        right--;
    }
}
```
​ToArray方法用于将可变长度的ArrayList变为普通的数组
```C#
public object[] ToArray()
{
    object[] array = new object[Count];
    int count = 0;
    foreach(object o in this.array)
        array[count++] = o;
    return array;
}
```
TrimToSize方法用于将ArrayList类的容量调整为此时的元素大小，节省空间
```C#
 public void TrimToSize()
 {
     if(Count != Capacity)
     {
         object[] newArray = new object[Count];
         int count = 0;
         foreach(object o in array)
             newArray[count++] = o;
         Capacity = count;
         array = newArray;
     }
 }
```
接着是IEnumerable接口的实现，使得ArrayList类能够使用foreach遍历数组，相关的实现我也只是照猫画虎，没什么经验，看了一些源码，然后凭感觉写了一份，能用，但不是最优
```C#
public IEnumerator GetEnumerator()
{
    return new Enumeratotr(this);
}

public class Enumeratotr : IEnumerator
{
    private ArrayList array;
    private int count;

    public Enumeratotr(ArrayList array)
    {
        this.array = array;
        count = -1;
    }

    public object Current
    {
        get
        {
            return array[count];
        }
    }

    public bool MoveNext()
    {
        if(++count < array.Count)
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
以上是ArrayList的实现，说实话写完以后个人并不是很满意，但先做完，再求做好，后续有什么父类啊，接口啊之类的想法再说，从C++转过来写代码确实不习惯，慢慢来吧