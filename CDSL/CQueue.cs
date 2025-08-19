using System.Collections;

namespace CDSL
{
    public class CQueue<T> : IEnumerable
    {
        private CList<T> list;
        public int Count { get { return list.Count; } }
        public CQueue()
        {
            list = new CList<T>();
        }
        public CQueue(T[] array)
        {
            list = new CList<T>(array);
        }

        public void EnQueue(T item)
        {
            list.Add(item);
        }

        public T DeQueue()
        {
            T res = list[0];
            list.RemoveAt(0);
            return res;
        }

        public T Peek()
        {
            return list[0];
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public T[] ToArray()
        {
            return list.ToArray();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int size = list.Count;
            int len = array.Length;
            if(arrayIndex  < 0 || arrayIndex >= len)
                throw new IndexOutOfRangeException();
            for(int i = 0; i < size && arrayIndex + i < len; i++)
                array[arrayIndex + i] = list[i];
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public class Enumerator : IEnumerator
        {
            private readonly T[] array;
            private int arrSize;
            private int count;

            public Enumerator(CQueue<T> queue)
            {
                array = queue.ToArray();
                arrSize = queue.Count;
                count = -1;
            }
            public object Current => array[count]!;

            public bool MoveNext()
            {
                if(++count < arrSize)
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
