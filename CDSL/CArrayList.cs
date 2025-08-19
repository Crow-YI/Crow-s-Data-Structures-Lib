namespace CDSL
{
    public class CArrayList : CList<object>
    {
        public CArrayList() { }
        public CArrayList(int capacity) : base(capacity) { }
        public CArrayList(object[] array) : base(array) { }

        override public CArrayList GetRange(int start, int len = int.MaxValue)
        {
            if(start < 0 || start >= Count || len <= 0)
                throw new ArgumentOutOfRangeException();
            CArrayList array = new CArrayList();
            int count = 0;
            while(count < len && count + start < Count)
                array.Add(this.array[start + count]);
            return array;
        }        
    }
}