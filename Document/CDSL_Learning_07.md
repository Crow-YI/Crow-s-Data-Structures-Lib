# CDSL_Learning_07

## Dictionary类说明
Dictionary是一种用于存储和读取键值对的数据结构，一般有两种数据存储方式：使用哈希表存储和使用链表存储，但出于性能考虑，大部分的Dictionary都采用哈希表存储数据。但使用哈希表存储数据时，Dictionary和HashTable类的功能高度重合，这也是我在学校学习数据结构时困惑的地方，就算Dictionary只是对HashTable的包装，但这个包装显得没有意义。不过，当我结合C#的语言特性，我突然意识到：如果我将HashTable的键值对都设定为object类，然后在Dictionary的包装中，规定特定类型，就能让这个包装有意义。最终我实现的Dictionary的使用效果，和上次的HashTable很类似，同时我修改了HashTable类（修改方式是我在HashTable的博客中提到的将KType和VType都修改为object，修改后的代码也会放在本文末尾）

## Dictionary类实现
### 字段和属性
Dictionary使用一个HashTable字段用于存储数据，还有一个索引器用于查找数据，因为Dictionary是强数据类型的，所以从hashTable中取得的object类型的数据要进行数据转换，后续的方法也同理。类型转换时要考虑null的情况，此时要抛出异常
```C#
private CHashTable hashTable;
public VType this[KType key]
{
    get
    {
        VType? res = hashTable[key] as VType;
        return res ?? throw new InvalidOperationException();
    }
    set
    {
        hashTable[key] = value;
    }
}
```

### 构造器
Dictionary的构造器与上次实现的HashTable的构造器类型相同，默认构造器，指定data大小的构造器，指定data大小和List初始容量的构造器，不同的构造器用于得到不同的hashTable字段
```C#
public CDictionary()
{
    hashTable = new CHashTable();
}
public CDictionary(int size)
{
    hashTable = new CHashTable(size);
}
public CDictionary(int size, int capacity)
{
    hashTable = new CHashTable(size, capacity);
}
```

### 增加，删除和清空方法
这三个方法都是使用HashTable类的方法，不做过多说明
```C#
public void Add(KType key, VType value)
{
    if(key == null || value == null)
        throw new ArgumentNullException();
    hashTable.Add(key, value);
}

public void Remove(KType key)
{
    if(key == null)
        throw new ArgumentNullException();
    hashTable.Remove(key);
}

public void Clear()
{
    hashTable.Clear();
}
```

### 成员类
我们可以认为，Dictionary中的单个元素的类型是KVPair\<KType, VType>（也就是上次实现的HashTable的元素类型，不过我现在已经修改了HashTable的元素类型），但这一个类型与Dictionary类型之间的联系不够紧密，因此我在Dictionary中创造了一个成员类DictionaryEntry，它存储的数据类型就是KVPair\<KType, VType>，这样就成功建立了联系
```C#
public class DictionaryEntry
{
    private KVPair<KType, VType> pair;
    public KType Key => pair.key;
    public VType Value => pair.value;

    public DictionaryEntry(KType key, VType value)
    {
        pair = new KVPair<KType, VType>(key, value);
    }
}
```

### IEnumerable实现 
此处的实现大部分都与之前的数据结构的IEnumerable实现相同，唯一需要说明的是Enumerator的构造器，因为要从HashTable中同时枚举两个元素，因此不能使用foreach，而是分开使用方法来同时枚举。还要注意取得的数据要进行类型转化
```C#
public IEnumerator GetEnumerator()
{
    return new Enumerator(this);
}

public class Enumerator : IEnumerator
{
    private DictionaryEntry[] array;
    private int size;
    private int count;

    public Enumerator(CDictionary<KType, VType> dictionary)
    {
        size = dictionary.hashTable.Count;
        array = new DictionaryEntry[size];
        var k_arr = dictionary.hashTable.Keys.GetEnumerator();
        var v_arr = dictionary.hashTable.Values.GetEnumerator();
        for(int i = 0; i < size; i++)
        {
            k_arr.MoveNext();
            v_arr.MoveNext();
            KType? key = k_arr.Current as KType;
            VType? value = v_arr.Current as VType;
            if(key == null ||  value == null)
                throw new ArgumentNullException();
            array[i] = new DictionaryEntry(key, value);
        }
        count = -1;
    }
    public object Current => array[count];

    public bool MoveNext()
    {
        if (++count < size)
            return true;
        return false;
    }

    public void Reset()
    {
        count = -1;
    }
}
```

### 其他方法
CopyTo方法可以将Dictionary中的DictionaryEntry元素放入数组的指定位置
```C#
public void CopyTo(DictionaryEntry[] array, int index)
{
    var enumerator = GetEnumerator();
    for(int i = 0; i < hashTable.Count && (index + i) < array.Length; i++)
    {
        enumerator.MoveNext();
        array[index + i] = (DictionaryEntry)enumerator.Current;
    }
}
```

## 总结
Dictionary是对HashTable做的一层强类型的包装，因为减少了object的使用，因此用户使用时可以减少类型转换，下文是修改过后的HashTable类实现（我实现的数据结构为了避免与标准库重名，会在类名前加一个C）
```C#
public class CHashTable
{
    private CList<KVPair<object, object>>[] data;
    private readonly int size;
    public int Count { get; private set; }
    public object this[object key]
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
    public KeyCollection Keys => new KeyCollection(this);
    public ValueCollection Values => new ValueCollection(this);

    public abstract class AbsCollection : IEnumerable
    {
        public readonly KVPair<object, object>[][] pairs;
        public readonly int size;

        public AbsCollection(CHashTable hashTable)
        {
            pairs = new KVPair<object, object>[hashTable.size][];
            for(int i = 0; i < hashTable.size; i++)
                pairs[i] = hashTable.data[i].ToArray();
            size = hashTable.size;
        }
        public abstract IEnumerator GetEnumerator(); 
    }

    public class KeyCollection : AbsCollection
    {
        public KeyCollection(CHashTable hashTable) : base(hashTable) { }
        public override IEnumerator GetEnumerator()
        { return new KeyEnumerator(this); }
    }

    public class ValueCollection : AbsCollection
    {
        public ValueCollection(CHashTable hashTable) : base(hashTable) { }
        public override IEnumerator GetEnumerator()
        { return new ValueEnumerator(this); }
    }

    public abstract class Enumerator : IEnumerator
    {
        protected readonly KVPair<object, object>[][] pairsGroup;
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

    public CHashTable()
    {
        data = new CList<KVPair<object, object>>[101];
        for(int i = 0; i < 101; i++)
            data[i] = new CList<KVPair<object, object>>(1);
        size = 101;
        Count = 0;
    }
    public CHashTable(int size)
    {
        data = new CList<KVPair<object, object>>[size];
        for(int i = 0; i < size; i++)
            data[i] = new CList<KVPair<object, object>>(1);
        this.size = size;
        Count = 0;
    }
    public CHashTable(int size, int capacity)
    {
        data = new CList<KVPair<object, object>>[size];
        for (int i = 0; i < size; i++)
            data[i] = new CList<KVPair<object, object>>(capacity);
        this.size = size;
        Count = 0;
    }

    public void Add(object key, object value)
    {
        if (key == null || value == null)
            throw new ArgumentNullException();
        KVPair<object, object> newPair = new(key, value);
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

    public void Remove(object key)
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

    public void Clear()
    {
        for(int i = 0; i < size; i++)
            data[i].Clear();
        Count = 0;
    }

    public bool ContainsKey(object key)
    {
        foreach(var currentKey in Keys)
        {
            if(currentKey.Equals(key))
                return true;
        }
        return false;
    }

    public bool ContainsValue(object value)
    {
        foreach (var currentValue in Values)
        {
            if (currentValue.Equals(value))
                return true;
        }
        return false;
    }
}
```