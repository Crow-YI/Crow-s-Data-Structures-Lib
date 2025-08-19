using System.Collections;

namespace CDSL
{
    public class CHashTable
    {
        private CList<KVPair<object, object>>[] data;
        private readonly int size;
        public int Count { get; private set; }
        public object this[object key]
        {
            get
            {
                if(key == null)
                    throw new ArgumentNullException("key");
                int index = key.GetHashCode() % size;
                if (index < 0)
                    index += size;
                for(int i = 0; i < data[index].Count; i++)
                {
                    if (key.Equals(data[index][i].key))
                        return data[index][i].value;
                }
                throw new InvalidOperationException();
            }
            set { Add(key, value); }
        }
        public KeyCollection Keys => new KeyCollection(this);
        public ValueCollection Values => new ValueCollection(this);

        public abstract class AbsCollection : IEnumerable
        {
            public readonly KVPair<object, object>[][] pairs;
            public readonly int size;

            public AbsCollection(CHashTable hashTable)
            {
                pairs = new KVPair<object, object>[hashTable.size][];
                for(int i = 0; i < hashTable.size; i++)
                    pairs[i] = hashTable.data[i].ToArray();
                size = hashTable.size;
            }
            public abstract IEnumerator GetEnumerator(); 
        }

        public class KeyCollection : AbsCollection
        {
            public KeyCollection(CHashTable hashTable) : base(hashTable) { }
            public override IEnumerator GetEnumerator()
            { return new KeyEnumerator(this); }
        }

        public class ValueCollection : AbsCollection
        {
            public ValueCollection(CHashTable hashTable) : base(hashTable) { }
            public override IEnumerator GetEnumerator()
            { return new ValueEnumerator(this); }
        }

        public abstract class Enumerator : IEnumerator
        {
            protected readonly KVPair<object, object>[][] pairsGroup;
            protected readonly int groupNum;
            protected int groupCount;
            protected int setCount;

            public Enumerator(AbsCollection collection)
            {
                groupNum = collection.size;
                pairsGroup = collection.pairs;
                groupCount = 0;
                setCount = -1;
            }

            public abstract object Current { get; }

            public bool MoveNext()
            {
                while (groupCount < groupNum)
                {
                    if (++setCount < pairsGroup[groupCount].Length)
                        return true;
                    groupCount++;
                    setCount = -1;
                }
                return false;
            }

            public void Reset()
            {
                groupCount = 0;
                setCount = -1;
            }
        }

        public class KeyEnumerator : Enumerator
        {
            public KeyEnumerator(KeyCollection collection) : base(collection) { }
            public override object Current
            {
                get
                {
                    if (groupCount < groupNum && setCount >= 0 && setCount < pairsGroup[groupCount].Length)
                    {
                        var pair = pairsGroup[groupCount][setCount];
                        if (pair.key != null)
                            return pair.key;
                        throw new InvalidOperationException();
                    }
                    throw new InvalidOperationException();
                }
            }
        }

        public class ValueEnumerator : Enumerator
        {
            public ValueEnumerator(ValueCollection collection) : base(collection) { }
            public override object Current
            {
                get
                {
                    if (groupCount < groupNum && setCount >= 0 && setCount < pairsGroup[groupCount].Length)
                    {
                        var pair = pairsGroup[groupCount][setCount];
                        if (pair.value != null)
                            return pair.value;
                        throw new InvalidOperationException();
                    }
                    throw new InvalidOperationException();
                }
            }
        }

        public CHashTable()
        {
            data = new CList<KVPair<object, object>>[101];
            for(int i = 0; i < 101; i++)
                data[i] = new CList<KVPair<object, object>>(1);
            size = 101;
            Count = 0;
        }
        public CHashTable(int size)
        {
            data = new CList<KVPair<object, object>>[size];
            for(int i = 0; i < size; i++)
                data[i] = new CList<KVPair<object, object>>(1);
            this.size = size;
            Count = 0;
        }
        public CHashTable(int size, int capacity)
        {
            data = new CList<KVPair<object, object>>[size];
            for (int i = 0; i < size; i++)
                data[i] = new CList<KVPair<object, object>>(capacity);
            this.size = size;
            Count = 0;
        }

        public void Add(object key, object value)
        {
            if (key == null || value == null)
                throw new ArgumentNullException();
            KVPair<object, object> newPair = new(key, value);
            int index = key.GetHashCode() % size;
            if (index < 0)
                index += size;
            for(int i = 0; i < data[index].Count; i++)
            {
                if(key.Equals(data[index][i].key))
                {
                    data[index][i] = newPair;
                    return;
                }
            }
            data[index].Add(newPair);
            Count++;
        }

        public void Remove(object key)
        {
            if(key ==  null) 
                throw new ArgumentNullException();
            int index = key.GetHashCode() % size;
            for(int i = 0; i < data[index].Count; i++)
            {
                if (key.Equals(data[index][i].key))
                {
                    data[index].RemoveAt(i);
                    Count--;
                    break;
                }
            }
        }

        public void Clear()
        {
            for(int i = 0; i < size; i++)
                data[i].Clear();
            Count = 0;
        }

        public bool ContainsKey(object key)
        {
            foreach(var currentKey in Keys)
            {
                if(currentKey.Equals(key))
                    return true;
            }
            return false;
        }

        public bool ContainsValue(object value)
        {
            foreach (var currentValue in Values)
            {
                if (currentValue.Equals(value))
                    return true;
            }
            return false;
        }
    }
}
