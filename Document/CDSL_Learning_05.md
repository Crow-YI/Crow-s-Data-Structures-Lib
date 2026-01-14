# CDSL_Learning_05

## String类说明
String类本质上就是一个char类型的数组，为String类编写方法，就可以看作对char数组编写方法。因为string和String都是微软System命名空间下的，所以我的类名使用CString

## String类实现
### 字段和属性
str用于存储数据，Length指示长度，索引器用于查看对应下标的元素
```C#
private char[] str;
public int Length { get; private set; }
public char this[int index]
{
    get { return str[index]; }
}
```

### 构造器
这里提供两种构造器的实现，使用字符数组构造和比较常用的string构造（C#中使用双引号就是构造了一个string类）。因为我认为使用=和双引号的方式（如CString str = "Hello"）作为类似string的类的构造器是十分符合直觉的（实际上我在构建C++的库的时候就是这样做的），但是因为C#出于安全考虑不允许对=操作符重载，只能放弃
```C#
public CString(char[] str)
{
    Length = str.Length;
    this.str = new char[Length];
    for(int i = 0; i < Length; i++) 
        this.str[i] = str[i];
}
public CString(string str)
{
    Length = str.Length;
    this.str = new char[Length];
    for (int i = 0; i < Length; i++)
        this.str[i] = str[i];
}
```

### 比较相关方法
比较有实例方法CompareTo，可以将对应的实例与其他实例比较，以及静态方法Compare和Equals（原本我想要重写object类的Equals方法，但是不知道如何重写GetHashCode方法，所以只写了一个非重写的版本）
```C#
public int CompareTo(CString other)
{
    int len = Length > other.Length ? other.Length : Length;
    for(int i = 0; i < len; i++)
    {
        int res = this[i] - other[i];
        if(res != 0)
        {
            if (res < 0)
                return -1;
            else
                return 1;
        }
    }
    if (Length == other.Length)
        return 0;
    else if (Length < other.Length)
        return -1;
    return 1;
}

public  bool Equals(CString other)
{
    if (other == null) 
        return false;
    return CompareTo(other) == 0;
}

public static int Compare(CString a, CString b)
    { return a.CompareTo(b); }
```
这里还由CompareTo引申出StartsWith和EndsWith方法，用于判断该实例是否由指定的字符串开头或结尾（SubString见下文）
```C#
public bool StartsWith(CString other)
{
    CString substr = SubString(0, other.Length);
    return substr.CompareTo(other) == 0;
}

public bool EndsWith(CString other)
{
    CString substr = SubString(Length -  other.Length, other.Length);
    return substr.CompareTo(other) == 0;
}
```

### 划分与合并方法
SubString用于得到索引开始的指定长度的字符串，Concat用于将多个字符串合并，并且对+操作符进行重载，使得Concat方法的调用更加简便
```C#
public CString SubString(int index, int length)
{
    char[] str = new char[length];
    for(int i = 0; i < length && (index + i) < Length; i++)
        str[i] = this[index + i];
    return new CString(str);
}

public static CString ConCat(CString s1, CString s2)
{
    int len1=  s1.Length;
    int len2= s2.Length;
    char[] newStr = new char[len1 + len2];
    for(int i = 0;i < len1; i++)
        newStr[i] = s1[i];
    for (int i = 0; i < len2; i++)
        newStr[len1 + i] = s2[i];
    return new CString(newStr);
}

public static CString operator +(CString s1, CString s2)
{ return ConCat(s1, s2); }
```

### 编辑字符串方法
在C#的官方定义中，一个string实例中存储的char数组是无法被修改的，只能通过重新赋值的方式对一个实例存储的值进行修改，但我在实践中又常常遇到以下这类代码（以ToLower举例）
```C#
string s1 = "Hello World";
s1 = s1.ToLower();
```
其实目的就是改变s1内的值，但因为官方定义中s1中的值无法直接修改，所以采用了这种重新赋值的方式，我认为这是反直觉的，所以我自己的实现中我对这类方法进行了修改，不完全实现官方方法的效果，直接对实例内部的char数组进行编辑

Insert是在指定位置插入给定的字符串，Remove是删去指定位置开始的指定长度的字符串，Replace是将原字符串内所有的source变为des，ToLower和ToUpper是改变字符串内的大小写，Trim和TrimEnd是删除字符串内指定的前缀或后缀。其中，Insert和Remove提供的Array.Copy方法为字符串的合并或拆分提供了一个便捷的方法，不需要通过迭代的方式进行相应的修改，具体的方法签名请自行上网查询
```C#
public void Insert(int index, CString value)
{
    int totalLen = Length + value.Length;
    char[] totalStr = new char[totalLen];
    Array.Copy(str, totalStr, index);
    Array.Copy(value.str, 0, totalStr, index, value.Length);
    Array.Copy(str, index, totalStr, index + value.Length, Length - index);
    str = totalStr;
    Length = totalLen;
}

public void Remove(int index,  int length)
{
    char[] totalStr;
    if(Length - index <= length)
    {
        Length = index;
        totalStr = new char[Length];
        Array.Copy(str, totalStr, Length);
    }
    else
    {
        Length -= length;
        totalStr = new char[Length];
        Array.Copy(str, totalStr, index);
        Array.Copy(str, index + length, totalStr, index, Length - index);
    }
    str = totalStr;
}

public void Replace(CString source, CString des)
{
    bool flag = true;
    int index;
    while(flag)
    {
        if((index = IndexOf(source)) != -1)
        {
            Remove(index, source.Length);
            Insert(index, des);
        }
        else
            flag = false;
    }
}

public void ToLower()
{
    for(int i = 0; i < Length; i++)
    {
        if (str[i] >= 'A' && str[i] <= 'Z')
            str[i] = (char)(str[i] - 'A' + 'a');
    }
}

public void ToUpper()
{
    for(int i = 0; i < Length;i++)
    {
        if (str[i] >= 'a' && str[i] <= 'z')
            str[i] = (char)(str[i] - 'a' + 'A');
    }
}

public void Trim(char[] chars)
{
    if(StartsWith(new CString(chars)))
        Remove(0, chars.Length);
}

public void TrimEnd(char[] chars)
{
    if(EndsWith(new CString(chars)))
        Remove(Length - chars.Length, chars.Length);
}
```

### 其他方法
IndexOf方法用于查找实例字符串中第一次出现指定字符串的下标，类似C++的strstr库函数，找不到时返回-1
```C#
public int IndexOf(CString value)
{
    char c = value[0];
    int len = value.Length;
    int des = Length - len + 1;
    for(int i = 0; i < des; i++)
    {
        if (this[i] == c)
        {
            int j;
            for(j = 1;  j < len; j++)
            {
                if (this[i + j] != value[j])
                    break;
            }
            if (j == len)
                return i;
        }
    }
    return -1;
}
```

## 总结
C#中的string的功能之完善，性能之高超出了我的想象（相比C++来说，C++中我们常会自定义一个string类来满足相关的需要），因此在C#中自定义一个string类并没有什么实际用处，本文也只是作为一篇学习笔记，通过实现相关的类来熟悉C#编程和对数据结构的认识