# CDSL_Learning_04

## BitArray类说明
在我上学期所做的大作业中，有过这样一个需求：在一个上万的单词库中，我有若干个句子，我需要以类矩阵的形式存储每个句子中出现过单词库中哪些单词。我可以使用一个上万长度的数组，下标与单词库里的单词编号相对应，每个元素用0或1表示这个单词是否在单词库中出现过。这里就出现了一个问题，占用空间最小的数据类型char的大小是1字节，但我的元素可能的数值只有0和1，也就是说，每个char类型的元素浪费了7个bit的空间。可以想到，如果有一个数组，每个元素的大小是一个bit，最符合我的需求，BitArray就是这样的的数组。BitArray中的元素大小为1bit的List线性结构，且当你访问它的元素时，它返回一个bool值，你还可以对它进行一系列的位操作，如and，or和xor。（方便起见，下文中我们将这种大小为1bit的元素称为Bit）

## BitArray类实现
### 字段和属性
因为C#并没有提供大小为1bit的数据类型，所以我们使用int类型的数组array来存储元素，因为int类型的大小为4字节，所以可以将每个int看作是32个Bit的集合，我们可以通过位操作符来对每个Bit分别Get或Set（这两个方法的实现见下文），Count用于指示array中Bit的数量
```C#
private List<int> array;
public int Count { get; private set; }
public bool this[int index]
{
    get { return Get(index); }
    set { Set(index, value); } 
}
```

### 构造器
我实现了5种比较常见的构造器：使用其他的BitArray数组构造一个相同的BitArray（深拷贝，即两个BitArray中的List<int>指向两个不同的实例），使用bool数组构造（每个bool对应一个Bit起到压缩空间的作用），规定指定大小的构造，规定指定大小和默认值的构造，使用int数组的构造（每个int对应32个Bit）
```C#
public BitArray(BitArray other)
{
    array = new List<int>(other.array.ToArray());
    Count = other.Count;
}
public BitArray(bool[] bools)
{
    int len = bools.Length;
    Count = len;
    array = new List<int>(len / 32 + 1);

    int num1 = 0;
    int num2 = 0;
    for(int i = 0; i < len; i++)
    {
        if (bools[i])
            array[num1] &= (1 << num2++);
        if(num2 == 32)
        {
            num1++;
            num2 = 0;
        }
    }
}
public BitArray(int capacity)
{
    Count = capacity;
    int len = capacity / 32 + 1;
    array = new List<int>(len);
    for (int i = 0; i < len; i++)
        array[i] = 0;
}
public BitArray(int capacity, bool defaultBoolen)
{
    Count = capacity;
    int len = capacity / 32 + 1;
    array = new List<int>(len);
    if (!defaultBoolen)
        for (int i = 0; i < len; i++)
            array[i] = 0;
    else
        for (int i = 0; i < len; i++)
            array[i] = -1;
}
public BitArray(int[] ints)
{
    Count = ints.Length * 32;
    array = new List<int>(ints);
}
```
其中有两个构造器要进行说明：第二个构造器中我们使用了两个变量num1和num2进行迭代为ArrayBit赋值，可能有人会疑惑为什么不直接使用Set(i, bools[i])的方式进行赋值，更加方便，这是因为Get和Set的性能不高，在下面实现这两个方法时会详细说明；第四个构造器中最后一位int有可能之后部分在Count的范围内，但可以将int中的每一位赋值为-1（-1在内存中每一位都是1），因为超过Count的空间不会被使用，是什么值不影响使用

### 存取方法
存取方法也就是Get和Set方法，通过位操作符定位到每一个Bit进行存取
```C#
public bool Get(int index)
{
    if (index < 0 || index >= Count)
        throw new ArgumentOutOfRangeException("index");
    int num1 = index / 32;
    int num2 = index % 32;
    int res = array[num1] & (1 << num2);
    if (res == 0)
        return false;
    else
        return true;
}

public void Set(int index, bool value)
{
    if (index < 0 || index >= Count)
        throw new ArgumentOutOfRangeException("index");
    int num1 = index / 32;
    int num2 = index % 32;
    if (value)
        array[num1] |= (1 << num2);
    else
        array[num1] &= ~(1 << num2);
}
public void Set(int index,  int value)
{
    if (index < 0 || index >= Count)
        throw new ArgumentOutOfRangeException("index");
    int num1 = index / 32;
    int num2 = index % 32;
    if (value != 0)
        array[num1] |= (1 << num2);
    else
        array[num1] &= ~(1 << num2);
}
```
先说Set方法中的(1 << num2)和~(1 << num2)，这是非常经典的蒙版，可以改变指定位的Bit而不对其他位造成影响，接着是上文提到的性能问题，可以注意到我们每一次的存取都要进行一次/和%操作，而这两种操作的开销是很大的，因此对于BitArray中的每一个Bit进行枚举时（上文中的构造器和接下来要说的CopyTo方法和IEnumerable实现），使用迭代具有更高的性能。Set类型的两种重载也便于我们使用BitArray时直接用0和1位Bit赋值

### 位操作方法
C#中的位操作符有&，|，^，~四种，这四种操作符也要在BitArray中实现
```C#
public static BitArray And(BitArray a, BitArray b)
{
    if (a.Count != b.Count)
        throw new ArgumentException();
    BitArray res = new BitArray(a.Count);
    int des = a.array.Count;
    for(int i = 0; i < des; i++)
        res.array[i] = a.array[i] & b.array[i];
    return res;
}
public static BitArray operator &(BitArray a, BitArray b)
{ return And(a, b); }
public static BitArray Or(BitArray a, BitArray b)
{
    if (a.Count != b.Count)
        throw new ArgumentException();
    BitArray res = new BitArray(a.Count);
    int des = a.array.Count;
    for (int i = 0; i < des; i++)
        res.array[i] = a.array[i] | b.array[i];
    return res;
}
public static BitArray operator |(BitArray a, BitArray b)
{ return Or(a, b); }
public static BitArray Xor(BitArray a, BitArray b)
{
    if (a.Count != b.Count)
        throw new ArgumentException();
    BitArray res = new BitArray(a.Count);
    int des = a.array.Count;
    for (int i = 0; i < des; i++)
        res.array[i] = a.array[i] ^ b.array[i];
    return res;
}
public static BitArray operator ^(BitArray a, BitArray b)
{ return Xor(a, b); }
public static BitArray Not(BitArray a)
{
    BitArray res = new BitArray(a.Count);
    int des = a.array.Count;
    for (int i = 0; i < des; i++)
        res.array[i] = ~a.array[i];
    return res;
}
public static BitArray operator !(BitArray a)
{ return Not(a); }
```
每一个操作符都对应一个静态方法和一个操作符重载，每个方法中，都直接对int类型操作，本质上就是对32位Bit同时批量操作

### 其他方法
SetAll方法是将所有的Bit设置为指定的bool值
```C#
public void SetAll(bool boolen)
{
    int len = array.Count;
    if (boolen)
        for (int i = 0; i < len; i++)
            array[i] = -1;
    else
        for(int i = 0; i < len; i++)
            array[i] = 0;
}
```
CopyTo是将BitArray中的每一位Bit依次变为bool数组中的值，注意每次取出一位的Bit
```C#
public void CopyTo(bool[] array, int index)
{
    int len = array.Length;
    int num1 = 0;
    int num2 = 0;
    for(int i = 0; i < Count && (index + i) < len; i++)
    {
        if (((this.array[num1] >> num2) & 1) == 0)
            array[index + i] = false;
        else
            array[index + i] = true;
        num2++;
        if(num2 == 32)
        {
            num2 = 0;
            num1++;
        }
    }
}
```
IEnumerable的实现同样要注意，枚举的是Bit，而不是int
```C#
public IEnumerator GetEnumerator()
{
    throw new NotImplementedException();
}

public class Enumerator : IEnumerator
{
    private readonly int[] array;
    private readonly int length;
    private int count;
    private int num1;
    private int num2;

    public Enumerator(BitArray bitArray)
    {
        array = bitArray.array.ToArray();
        length = bitArray.Count;
        count = -1;
        num2 = -1;
        num1 = 0;
    }

    public object Current
    {
        get
        {
            if (((array[num1] >> num2) & 1) == 0)
                return false;
            else
                return true;
        }
    }

    public bool MoveNext()
    {
        if(++count < length)
        {
            num2++;
            if(num2 == 32)
            {
                num2 = 0;
                num1++;
            }
            return true;
        }
        return false;
    }

    public void Reset()
    {
        count = -1;
        num2 = -1;
        num1 = 0;
    }
}
```

## 总结
BitArray所说还是线性结构，但比起之前的那些结构复杂许多，而且还有许多东西要仔细考虑，就比如上文反复提及的用Set还是用迭代的方式枚举，还有List是使用int，uint还是byte，考虑到后续的强制类型转换的问题，我选择了int。总之磕磕绊绊，总算是做完了，实在累