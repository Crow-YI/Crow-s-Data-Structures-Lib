namespace CDSL
{
    public class CHeap<T>
        where T : IComparable<T>
    {
        private CList<T> array;
        public int Count { get; private set; }
        
        public CHeap()
        {
            array = new CList<T>();
            Count = 0;
        }
        public CHeap(T[] array)
        {
            this.array = new CList<T>(array);
            Count = array.Length;
            for(int i = Count / 2 - 1; i >= 0; i--)
                Sink(i);
        }

        private void Floating(int index)
        {
            while(index != 0 && (array[index].CompareTo(array[(index - 1) / 2]) > 0))
            {
                (array[(index - 1) / 2], array[index]) = (array[index], array[(index - 1) / 2]);
                index = (index - 1) / 2;
            }
        }
        private void Sink(int index)
        {
            int child = index * 2 + 1;
            while(child < Count)
            {
                if(child + 1 < Count && (array[child].CompareTo(array[child + 1]) < 0))
                    child++;
                if (array[index].CompareTo(array[child]) > 0)
                    break;
                (array[index], array[child]) = (array[child], array[index]);
                index = child;
                child = index * 2 + 1;
            }
        }

        public void Push(T value)
        {
            array[Count] = value;
            Floating(Count);
            Count++;
        }

        public T Pop()
        {
            if (Count == 0)
                throw new InvalidOperationException();
            T res = array[0];
            Count--;
            array[0] = array[Count];
            Sink(0);
            return res;
        }

        public T Peek()
        {
            if(Count == 0)
                throw new InvalidOperationException();
            return array[0];
        }

        public T[] ToArray()
        {
            CList<T> list = new CList<T>(array.ToArray());
            int count = Count;
            T[] res = new T[Count];
            for (int i = 0; i < count; i++)
                res[i] = Pop();
            array = list;
            Count = count;
            return res;
        }
    }
}
