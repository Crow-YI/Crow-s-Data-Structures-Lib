# CDSL_Learning_11

## Set类说明
Set的中文翻译是集合，集合是数学意义上的那个集合，用于存储元素，不同集合间能够通过交集，并集等操作产生新的集合。集合常见的有两种类型，非数字元素的集合和数字元素的集合。非数字元素的集合从性能的角度出发采用哈希表作为内部存储（因为哈希表的存放和读取的复杂度都为O(1)）；数字元素的集合采用BitAray作为内部存储。因为我之前实现的BitArray已经实现了与或非等操作，稍加修改就可以实现集合的功能，因此不再实现。本文只实现非数字元素的集合。

## Set类实现
### 字段，属性，构造器
因为HashTable是弱类型的，我想实现的是一个强类型的集合，因此采用Dictionary作为内部存储的容器，所存储的数据是作为Value，Key的类型使用string（下文会说明）。Count属性是集合中的元素个数。构造器就是对字段的初始化
```C#
private CDictionary<string, T> data;
public int Count { get { return data.Count; } }

public CSet()
{
    data = new CDictionary<string, T>();
}
```

### GetKey方法
集合中存储的是单个值，也就是Value，而Dictionary中存储的是键值对，还缺少与值对应的键。GetKey方法就是从一个Value得到Key的方法，具体的算法实现如代码所示，使用该方法能够使每个Value产生独特的string类型Key。该算法的数学原理不做说明（我也不会，我是上网找的算法）
```C#
private string GetKey(T item)
{
    if(item == null)
        throw new ArgumentNullException("item");
    char[] chars;
    string? s = item.ToString();
    if (s == null)
        throw new InvalidOperationException();
    chars = s.ToCharArray();
    int hashValue = 0;
    for(int i = 0; i < chars.Length; i++)
        hashValue += chars[i];
    return hashValue.ToString();
}
```

### 单个集合内的方法
Add和Remove方法都是对单个集合进行修改，直接调用Dictionary的方法即可
```C#
public void Add(T item)
{
    data.Add(GetKey(item), item);
}

public void Remove(T item)
{
    data.Remove(GetKey(item));
}
```

### 集合间的方法
集合这种数据结构的最终目的是方便地实现集合间地交集，并集等操作。Union方法是集合的交集，通过两个foreach循环将两个集合的元素都放到新产生的集合中
```C#
public CSet<T> Union(CSet<T> other)
{
    if(other == null) 
        throw new ArgumentNullException("other");
    CSet<T> tempSet = new CSet<T>();
    foreach(var item in data)
    {
        var entry = item as CDictionary<string, T>.DictionaryEntry;
        if(entry == null)
            throw new ArgumentException();
        tempSet.Add(entry.Value);
    }
    foreach (var item in other.data)
    {
        var entry = item as CDictionary<string, T>.DictionaryEntry;
        if (entry == null)
            throw new ArgumentException();
        tempSet.Add(entry.Value);
    }
    return tempSet;
}
```
Intersection方法是集合的并集，通过foreach循环枚举一个集合中的元素，再使用另一个集合的Contains方法（在之前的实现中没有写出，就是调用Dictonary中的HashTable字段的ContainsValue方法，这里就不展示了）判断每个枚举的元素是否在该集合中，若存在。则加入到新产生的集合中
```C#
public CSet<T> Intersection(CSet<T> other)
{
    CSet<T> tempSet = new CSet<T>();
    foreach(var item in data)
    {
        var entry = item as CDictionary<string, T>.DictionaryEntry;
        if (entry == null)
            throw new ArgumentException();
        if (other.data.Contains(entry.Key))
            tempSet.Add(entry.Value);
    }
    return tempSet;
}
```
Subset方法是用来判断本集合是否是other集合的子集，通过foreach循环枚举本集合的所有元素，使用other的Contains方法，若该枚举元素不在other集合内，则返回false，若所有的元素都在other集合内，则返回true
```C#
public bool Subset(CSet<T> other)
{
    if(Count > other.Count)
        return false;
    else
        foreach (var item in data)
        {
            var entry = item as CDictionary<string, T>.DictionaryEntry;
            if (entry == null)
                throw new ArgumentException();
            if(!other.data.Contains(entry.Key))
                return false;
        }
    return true;
}
```
Difference方法产生这样一个集合，该集合内的元素是在本集合内且不在other集合内的元素，逻辑与Intersection相反，不做过多说明
```C#
public CSet<T> Difference(CSet<T> other)
{
    CSet<T> tempSet = new CSet<T>();
    foreach(var item in data)
    {
        var entry = item as CDictionary<string, T>.DictionaryEntry;
        if (entry == null)
            throw new ArgumentException();
        if(!other.data.Contains(entry.Key))
            tempSet.Add(entry.Value);
    }
    return tempSet;
}
```

## 总结
Set数据结构是这本书最后的温柔了，之后的AVLTree，红黑树，还有图，那些数据结构的算法个顶个的复杂，之后的更新速度也会明显下降，还是希望这个假期能够实现完