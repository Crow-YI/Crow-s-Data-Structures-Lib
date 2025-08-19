using System.Collections;

namespace CDSL
{
    public class CDictionary<KType, VType> : IEnumerable
        where KType : class
        where VType : class
    {
        private CHashTable hashTable;
        public int Count {  get { return hashTable.Count; } }
        public VType this[KType key]
        {
            get
            {
                VType? res = hashTable[key] as VType;
                return res ?? throw new InvalidOperationException();
            }
            set
            {
                hashTable[key] = value;
            }
        }

        public CDictionary()
        {
            hashTable = new CHashTable();
        }
        public CDictionary(int size)
        {
            hashTable = new CHashTable(size);
        }
        public CDictionary(int size, int capacity)
        {
            hashTable = new CHashTable(size, capacity);
        }
        
        public class DictionaryEntry
        {
            private KVPair<KType, VType> pair;
            public KType Key => pair.key;
            public VType Value => pair.value;

            public DictionaryEntry(KType key, VType value)
            {
                pair = new KVPair<KType, VType>(key, value);
            }
        }

        public void Add(KType key, VType value)
        {
            if(key == null || value == null)
                throw new ArgumentNullException();
            hashTable.Add(key, value);
        }

        public bool Contains(KType key)
        {
            return hashTable.ContainsKey(key);
        }

        public void Remove(KType key)
        {
            if(key == null)
                throw new ArgumentNullException();
            hashTable.Remove(key);
        }

        public void Clear()
        {
            hashTable.Clear();
        }
        
        public void CopyTo(DictionaryEntry[] array, int index)
        {
            var enumerator = GetEnumerator();
            for(int i = 0; i < hashTable.Count && index + i < array.Length; i++)
            {
                enumerator.MoveNext();
                array[index + i] = (DictionaryEntry)enumerator.Current;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public class Enumerator : IEnumerator
        {
            private DictionaryEntry[] array;
            private int size;
            private int count;

            public Enumerator(CDictionary<KType, VType> dictionary)
            {
                size = dictionary.hashTable.Count;
                array = new DictionaryEntry[size];
                var k_arr = dictionary.hashTable.Keys.GetEnumerator();
                var v_arr = dictionary.hashTable.Values.GetEnumerator();
                for(int i = 0; i < size; i++)
                {
                    k_arr.MoveNext();
                    v_arr.MoveNext();
                    KType? key = k_arr.Current as KType;
                    VType? value = v_arr.Current as VType;
                    if(key == null ||  value == null)
                        throw new ArgumentNullException();
                    array[i] = new DictionaryEntry(key, value);
                }
                count = -1;
            }
            public object Current => array[count];

            public bool MoveNext()
            {
                if (++count < size)
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
