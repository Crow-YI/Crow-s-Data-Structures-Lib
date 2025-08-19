namespace CDSL
{
    public class CPriorityQueue<PType, VType>
        where PType : IComparable<PType>
    {
        private class PVPair : IComparable<PVPair>
        {
            public PType priority;
            public VType value;

            public PVPair(PType priority, VType value)
            {
                this.priority = priority;
                this.value = value;
            }

            public int CompareTo(PVPair? other)
            {
                if (other == null)
                    return 1;
                return priority.CompareTo(other.priority);
            }
        }

        private CHeap<PVPair> heap;
        public int Count {  get { return heap.Count; } }

        public CPriorityQueue()
        {
            heap = new CHeap<PVPair>();
        }
        
        public void Enqueue(VType value, PType priority)
        {
            PVPair newPair = new PVPair(priority, value);
            heap.Push(newPair);
        }

        public VType Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException();
            return heap.Pop().value;
        }

        public VType Peek()
        {
            if(Count == 0)
                throw new InvalidOperationException();
            return heap.Peek().value;
        }

        public void Clear()
        {
            heap = new CHeap<PVPair>();
        }

        public VType[] ToArray()
        {
            VType[] res = new VType[Count];
            PVPair[] array = heap.ToArray();
            for (int i = 0; i < Count; i++)
                res[i] = array[i].value;
            return res;
        }

        public void CopyTo(VType[] array, int index)
        {
            PVPair[] pairs = heap.ToArray();
            int len = pairs.Length;
            if(index < 0 || index >= len)
                throw new ArgumentOutOfRangeException("index");
            for(int i = 0; i < Count && (index + i) < len; i++)
                array[index + i] = pairs[i].value;
        }
    }
}
