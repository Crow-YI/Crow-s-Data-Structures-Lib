namespace CDSL
{
    public class CSet<T>
        where T : class
    {
        private CDictionary<string, T> data;
        public int Count { get { return data.Count; } }

        public CSet()
        {
            data = new CDictionary<string, T>();
        }

        private string GetKey(T item)
        {
            if(item == null)
                throw new ArgumentNullException("item");
            char[] chars;
            string? s = item.ToString();
            if (s == null)
                throw new InvalidOperationException();
            chars = s.ToCharArray();
            int hashValue = 0;
            for(int i = 0; i < chars.Length; i++)
                hashValue += chars[i];
            return hashValue.ToString();
        }

        public void Add(T item)
        {
            data.Add(GetKey(item), item);
        }

        public void Remove(T item)
        {
            data.Remove(GetKey(item));
        }

        public CSet<T> Union(CSet<T> other)
        {
            if(other == null) 
                throw new ArgumentNullException("other");
            CSet<T> tempSet = new CSet<T>();
            foreach(var item in data)
            {
                var entry = item as CDictionary<string, T>.DictionaryEntry;
                if(entry == null)
                    throw new ArgumentException();
                tempSet.Add(entry.Value);
            }
            foreach (var item in other.data)
            {
                var entry = item as CDictionary<string, T>.DictionaryEntry;
                if (entry == null)
                    throw new ArgumentException();
                tempSet.Add(entry.Value);
            }
            return tempSet;
        }

        public CSet<T> Intersection(CSet<T> other)
        {
            CSet<T> tempSet = new CSet<T>();
            foreach(var item in data)
            {
                var entry = item as CDictionary<string, T>.DictionaryEntry;
                if (entry == null)
                    throw new ArgumentException();
                if (other.data.Contains(entry.Key))
                    tempSet.Add(entry.Value);
            }
            return tempSet;
        }

        public bool Subset(CSet<T> other)
        {
            if(Count > other.Count)
                return false;
            else
                foreach (var item in data)
                {
                    var entry = item as CDictionary<string, T>.DictionaryEntry;
                    if (entry == null)
                        throw new ArgumentException();
                    if(!other.data.Contains(entry.Key))
                        return false;
                }
            return true;
        }

        public CSet<T> Difference(CSet<T> other)
        {
            CSet<T> tempSet = new CSet<T>();
            foreach(var item in data)
            {
                var entry = item as CDictionary<string, T>.DictionaryEntry;
                if (entry == null)
                    throw new ArgumentException();
                if(!other.data.Contains(entry.Key))
                    tempSet.Add(entry.Value);
            }
            return tempSet;
        }
    }
}
