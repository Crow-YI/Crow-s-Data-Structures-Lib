using System.Collections;

namespace CDSL
{
    public class CStack<T> : IEnumerable
    {
        private readonly CList<T> list;
        private int size;
        public int Count
        {
            get { return size; }
        }

        public CStack()
        {
            list = new CList<T>();
            size = 0;
        }
        public CStack(T[] array)
        {
            list = new CList<T>(array);
            size = array.Length;
        }

        public void Push(T item)
        {
            list.Add(item);
            size++;
        }

        public T Pop()
        {
            if (size == 0)
                throw new InvalidOperationException();
            size--;
            T res = list[size];
            list.RemoveAt(size);
            return res;
        }

        public T Peek()
        {
            return list[size - 1];
        }

        public void Clear()
        {
            list.Clear();
            size = 0;
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public T[] ToArray()
        {
            T[] array = new T[size];
            for(int i = 0; i < size; i++)
                array[i] = list[size - 1 - i];
            return array;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int len = array.Length;
            if(arrayIndex < 0 || arrayIndex > len)
                throw new ArgumentOutOfRangeException();
            for(int i = 0; i < size && arrayIndex + i < len; i++)
                array[arrayIndex + i] = list[size - 1 - i];
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

            public Enumerator(CStack<T> stack)
            {
                array = stack.ToArray();
                arrSize = stack.Count;
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
    }
}
