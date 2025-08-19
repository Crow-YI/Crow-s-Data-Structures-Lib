namespace CDSL
{
    public class CString
    {
        private char[] str;
        public int Length { get; private set; }
        public char this[int index]
        {
            get { return str[index]; }
        }

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
    }
}
