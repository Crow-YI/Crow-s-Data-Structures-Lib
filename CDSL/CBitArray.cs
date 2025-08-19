using System.Collections;

namespace CDSL
{
    public class CBitArray : IEnumerable
    {
        private CList<int> array;
        public int Count { get; private set; }
        public bool this[int index]
        {
            get { return Get(index); }
            set { Set(index, value); } 
        }

        public CBitArray(CBitArray other)
        {
            array = new CList<int>(other.array.ToArray());
            Count = other.Count;
        }
        public CBitArray(bool[] bools)
        {
            int len = bools.Length;
            Count = len;
            array = new CList<int>(len / 32 + 1);

            int num1 = 0;
            int num2 = 0;
            for(int i = 0; i < len; i++)
            {
                if (bools[i])
                    array[num1] &= 1 << num2++;
                if(num2 == 32)
                {
                    num1++;
                    num2 = 0;
                }
            }
        }
        public CBitArray(int capacity)
        {
            Count = capacity;
            int len = capacity / 32 + 1;
            array = new CList<int>(len);
            for (int i = 0; i < len; i++)
                array[i] = 0;
        }
        public CBitArray(int capacity, bool defaultBoolen)
        {
            Count = capacity;
            int len = capacity / 32 + 1;
            array = new CList<int>(len);
            if (!defaultBoolen)
                for (int i = 0; i < len; i++)
                    array[i] = 0;
            else
                for (int i = 0; i < len; i++)
                    array[i] = -1;
        }
        public CBitArray(int[] ints)
        {
            Count = ints.Length * 32;
            array = new CList<int>(ints);
        }

        public bool Get(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");
            int num1 = index / 32;
            int num2 = index % 32;
            int res = array[num1] & 1 << num2;
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
                array[num1] |= 1 << num2;
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
                array[num1] |= 1 << num2;
            else
                array[num1] &= ~(1 << num2);
        }

        public static CBitArray And(CBitArray a, CBitArray b)
        {
            if (a.Count != b.Count)
                throw new ArgumentException();
            CBitArray res = new CBitArray(a.Count);
            int des = a.array.Count;
            for(int i = 0; i < des; i++)
                res.array[i] = a.array[i] & b.array[i];
            return res;
        }
        public static CBitArray operator &(CBitArray a, CBitArray b)
        { return And(a, b); }
        public static CBitArray Or(CBitArray a, CBitArray b)
        {
            if (a.Count != b.Count)
                throw new ArgumentException();
            CBitArray res = new CBitArray(a.Count);
            int des = a.array.Count;
            for (int i = 0; i < des; i++)
                res.array[i] = a.array[i] | b.array[i];
            return res;
        }
        public static CBitArray operator |(CBitArray a, CBitArray b)
        { return Or(a, b); }
        public static CBitArray Xor(CBitArray a, CBitArray b)
        {
            if (a.Count != b.Count)
                throw new ArgumentException();
            CBitArray res = new CBitArray(a.Count);
            int des = a.array.Count;
            for (int i = 0; i < des; i++)
                res.array[i] = a.array[i] ^ b.array[i];
            return res;
        }
        public static CBitArray operator ^(CBitArray a, CBitArray b)
        { return Xor(a, b); }
        public static CBitArray Not(CBitArray a)
        {
            CBitArray res = new CBitArray(a.Count);
            int des = a.array.Count;
            for (int i = 0; i < des; i++)
                res.array[i] = ~a.array[i];
            return res;
        }
        public static CBitArray operator !(CBitArray a)
        { return Not(a); }

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

        public void CopyTo(bool[] array, int index)
        {
            int len = array.Length;
            int num1 = 0;
            int num2 = 0;
            for(int i = 0; i < Count && index + i < len; i++)
            {
                if ((this.array[num1] >> num2 & 1) == 0)
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

            public Enumerator(CBitArray bitArray)
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
                    if ((array[num1] >> num2 & 1) == 0)
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
    }
}
