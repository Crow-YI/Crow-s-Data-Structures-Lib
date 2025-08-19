using System.Collections;

namespace CDSL
{
    public class CList<T> : IEnumerable
    {
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
        public CList()
        {
            array = new T[10];
            Capacity = 10;
            Count = 0;
        }
        public CList(int capacity)
        {
            array = new T[capacity];
            Capacity = capacity;
            Count = 0;
        }
        public CList(T[] array)
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

        public void Clear()
        {
            Count = 0;
        }

        public bool Contains(T value)
        {
            if (IndexOf(value) < 0)
                return false;
            else
                return true;
        }

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

        public T[] ToArray()
        {
            T[] array = new T[Count];
            for(int i = 0; i < Count; i++)
                array[i] = this[i];
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

        virtual public CList<T> GetRange(int start, int len = int.MaxValue)
        {
            if (start < 0 || start >= Count || len <= 0)
                throw new ArgumentOutOfRangeException();
            CList<T> array = new CList<T>();
            int count = 0;
            while (count < len && count + start < Count)
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

            public Enumerator(CList<T> list)
            {
                array = list.array;
                arrSize = list.Count;
                count = -1;
            }

            public object Current => array[count]!;

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

    }
}
