# CDSL_Learning_06

## HashTable类说明
当我们使用一个无序的数组集合时，我们要查找数组中是否含有某个元素，我们需要遍历一遍数组，复杂度为O(n)。如果我们在数组排列是有序排列，我们可以使用二分查找的方法查找元素，复杂度为O(logn)，如果我们想要更快的查找速度，HashTable就要登场了，HashTable通过一种特殊的存储数据的方法，将查找的复杂度变为O(1)。如果这是你第一次了解哈希表，我建议你先自行了解以下概念：hash函数，哈希表中的数据存储方式，哈希表中的冲突处理策略。本文意在通过C#的方式实现HashTable类，对这些基础知识不会加以说明

## KVPair实现
哈希表内存储的数据称为键值对（Key-Value-Pair），每一个数据由一个Key和Value组成，Key相当于是数组中的下标，Value是实际存储的数据，我们可以通过Key访问到对应的Value。出于C++的习惯，我实现的KVPair和HashTable有着严格的类型限制，一个HashTable中所有的Key都是同一类型，所有的Value也都是同一类型。
```C#
public struct KVPair<KType, Vtype>
{
    public KType key;
    public Vtype value;
    public KVPair(KType key, Vtype value)
    {
        this.key = key;
        this.value = value;
    }
}
```
但实际上，C#中的所有类型都有统一的基类型：object，因此更符合C#风格的实现应该是将所有的KType和VType全部变为object（后续的HashTable也是同理），读者可自行修改，得到一个类型不受限的哈希表，我是认为在实践上我的这种方式更符合实际需求（当然可能是我一直都在C++的环境里，没见过C#的需求）。

## HashTable类实现
### 字段
通过查阅文档可知，C#官方使用桶式法解决哈希表中的冲突，模仿官方的解决方法，使用一个存储KVPair\<KType, Vtype>的List的数组来存储数据，当发生冲突时，将元素放入同一个List中，还有一个int字段来指示数组的大小，当然也可以使用数组的Length属性来代替，这是个人习惯
```C#
private CList<KVPair<KType, VType>>[] data;
private readonly int size;
```

### 属性
Count属性用于指示哈希表中键值对的数量，索引器使用Key用来读取对应的Value（Add方法见下文）
```C#
public int Count { get; private set; }
public VType this[KType key]
{
    get
    {
        if(key == null)
            throw new ArgumentNullException("key");
        int index = key.GetHashCode() % size;
        if (index < 0)
            index += size;
        for(int i = 0; i < data[index].Count; i++)
        {
            if (key.Equals(data[index][i].key))
                return data[index][i].value;
        }
        throw new InvalidOperationException();
    }
    set { Add(key, value); }
}
```
C#提供了一个相当便捷的方法来获取hash值：GetHashCode方法，它是object中的一个虚函数，C#中所有的基础类型都有重写这一方法，我们的自定义类型也可以重写这一方法。得到的hash值余除data的大小可以获得存储的位置（为避免出现负数，采用负数+size的方式）。

属性Keys和Values很相似，Keys属性得到能够遍历所有存储的key的迭代器，Values属性得到value的迭代器。这里我们先说明Keys和相关的成员类。
```C#
public KeyCollection Keys => new KeyCollection(this);
public ValueCollection Values => new ValueCollection(this);
```
因为Keys和Values所得到的迭代器的行为很相似，可以先实现一个父类AbsCollection。使用一个二维数组存储哈希表中所有的键值对（因为迭代器没有动态变化大小的需求，所以用不到List），size存储哈希表中的size，实现一个简单的构造器和一个抽象方法GetEnumerator()
```C#
public abstract class AbsCollection : IEnumerable
{
    public readonly KVPair<KType, VType>[][] pairs;
    public readonly int size;

    public AbsCollection(CHashTable<KType, VType> hashTable)
    {
        pairs = new KVPair<KType, VType>[hashTable.size][];
        for(int i = 0; i < hashTable.size; i++)
            pairs[i] = hashTable.data[i].ToArray();
        size = hashTable.size;
    }
    public abstract IEnumerator GetEnumerator(); 
}
```
KeyCollection派生自AbsCollection，只要实现GetEnumerator即可，而这个方法之前实现过很多次了，公式实现即可
```C#
public class KeyCollection : AbsCollection
{
    public KeyCollection(CHashTable<KType, VType> hashTable) : base(hashTable) { }
    public override IEnumerator GetEnumerator()
    { return new KeyEnumerator(this); }
}
```
接着，要实现一个实现了IEnumerator接口的KeyEnumerator类，这里，同样可以注意到KeyEnumerator和Values那边对应的ValueEnumerator的遍历行为是相同的，只是一个所有的Key值，一个返回所有的Value值。因此，可以先实现一个父类Enumerator，再从这个类进行派生
```C#
public abstract class Enumerator : IEnumerator
{
    protected readonly KVPair<KType, VType>[][] pairsGroup;
    protected readonly int groupNum;
    protected int groupCount;
    protected int setCount;

    public Enumerator(AbsCollection collection)
    {
        groupNum = collection.size;
        pairsGroup = collection.pairs;
        groupCount = 0;
        setCount = -1;
    }

    public abstract object Current { get; }

    public bool MoveNext()
    {
        while (groupCount < groupNum)
        {
            if (++setCount < pairsGroup[groupCount].Length)
                return true;
            groupCount++;
            setCount = -1;
        }
        return false;
    }

    public void Reset()
    {
        groupCount = 0;
        setCount = -1;
    }
}
```
这个类在遍历时，要注意不同的数组中元素数量不同，有的数组中没有元素，应该直接跳过，其他的和正常的迭代器是相同的，然后由此类派生出KeyEnumerator类
```C#
public class KeyEnumerator : Enumerator
{
    public KeyEnumerator(KeyCollection collection) : base(collection) { }
    public override object Current
    {
        get
        {
            if (groupCount < groupNum && setCount >= 0 && setCount < pairsGroup[groupCount].Length)
            {
                var pair = pairsGroup[groupCount][setCount];
                if (pair.key != null)
                    return pair.key;
                throw new InvalidOperationException();
            }
            throw new InvalidOperationException();
        }
    }
}
```
因为遍历的过程较为复杂，因此Current返回时要考虑很多边界条件，防止返回一个null值

学会了Keys相关的成员类，Values相关的成员类就很简单了
```C#
public class ValueCollection : AbsCollection
{
    public ValueCollection(CHashTable<KType, VType> hashTable) : base(hashTable) { }
    public override IEnumerator GetEnumerator()
    { return new ValueEnumerator(this); }
}

public class ValueEnumerator : Enumerator
{
    public ValueEnumerator(ValueCollection collection) : base(collection) { }
    public override object Current
    {
        get
        {
            if (groupCount < groupNum && setCount >= 0 && setCount < pairsGroup[groupCount].Length)
            {
                var pair = pairsGroup[groupCount][setCount];
                if (pair.value != null)
                    return pair.value;
                throw new InvalidOperationException();
            }
            throw new InvalidOperationException();
        }
    }
}
```

### 构造器
HashTable有3种简单的构造器，默认构造器，指定data大小的构造器（从数学角度能解释，size最好是一个质数，能够保证元素在其中均匀分布），指定data大小和List初始容量的构造器（默认为1）
```C#
public CHashTable()
{
    data = new CList<KVPair<KType, VType>>[101];
    for(int i = 0; i < 101; i++)
        data[i] = new CList<KVPair<KType, VType>>(1);
    size = 101;
    Count = 0;
}
public CHashTable(int size)
{
    data = new CList<KVPair<KType, VType>>[size];
    for(int i = 0; i < size; i++)
        data[i] = new CList<KVPair<KType, VType>>(1);
    this.size = size;
    Count = 0;
}
public CHashTable(int size, int capacity)
{
    data = new CList<KVPair<KType, VType>>[size];
    for (int i = 0; i < size; i++)
        data[i] = new CList<KVPair<KType, VType>>(capacity);
    this.size = size;
    Count = 0;
}
```

### 方法
Add方法将一个键值对加入哈希表，如果键在表中已存在，就修改对应的值
```C#
public void Add(KType key, VType value)
{
    if (key == null || value == null)
        throw new ArgumentNullException();
    KVPair<KType, VType> newPair = new(key, value);
    int index = key.GetHashCode() % size;
    if (index < 0)
        index += size;
    for(int i = 0; i < data[index].Count; i++)
    {
        if(key.Equals(data[index][i].key))
        {
            data[index][i] = newPair;
            return;
        }
    }
    data[index].Add(newPair);
    Count++;
}
```
Remove方法将所给Key值的键值对删去
```C#
public void Remove(KType key)
{
    if(key ==  null) 
        throw new ArgumentNullException();
    int index = key.GetHashCode() % size;
    for(int i = 0; i < data[index].Count; i++)
    {
        if (key.Equals(data[index][i].key))
        {
            data[index].RemoveAt(i);
            Count--;
            break;
        }
    }
}
```
clear方法将哈希表清空
```C#
public void Clear()
{
    for(int i = 0; i < size; i++)
        data[i].Clear();
    Count = 0;
}
```
ContainsKey和ContainsValue方法判断哈希表内是否存在对应的Key和Value值，可以使用我们已经实现Keys和Values来简化代码
```C#
public bool ContainsKey(KType key)
{
    foreach(var currentKey in Keys)
    {
        if(currentKey.Equals(key))
            return true;
    }
    return false;
}

public bool ContainsValue(VType value)
{
    foreach (var currentValue in Values)
    {
        if (currentValue.Equals(value))
            return true;
    }
    return false;
}
```

## 总结
我可以猜到Keys和Values和那些成员类可以弄晕很多人，因为我在组织相关的类继承关系时也很晕，在写这篇博客时也很晕，不知道我的内容组织顺序清不清楚，只能说我已经尽力了。哈希表的实现真的很麻烦，花了我很多时间，希望读者能看得懂