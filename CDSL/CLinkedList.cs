using System.Collections;

namespace CDSL
{
    public class CLinkedList<T> : IEnumerable
    {
        private class Node
        {
            public T value;
            public Node? prev;
            public Node? next;
            public Node()
            {
                value = default!;
                prev = null;
                next = null;
            }
            public Node(T value)
            {
                if(value == null)
                    throw new ArgumentNullException("value");
                this.value = value;
                prev = null;
                next = null;
            }
        }

        private Node? header;
        public int Count { get; private set; }

        public CLinkedList()
        {
            header = null;
            Count = 0;
        }

        public void Add(T value)
        {
            Node newNode = new Node(value);
            if (header == null)
                header = newNode;
            else
            {
                header.prev = newNode;
                newNode.next = header;
                header = newNode;
            }
            Count++;
        }

        private Node? Find(T value)
        {
            Node? walkPoint = header;
            while(walkPoint != null)
            {
                if (walkPoint.value != null)
                    if (walkPoint.value.Equals(value))
                        break;
            }
            return walkPoint;
        }

        public void Insert(T value, T posValue)
        {
            Node? pos = Find(posValue);
            if (pos == null)
                return;
            Node newNode = new Node(value);
            if(pos.prev == null)
            {
                pos.prev = newNode;
                newNode.next = pos;
                header = newNode;
            }
            else
            {
                pos.prev.next = newNode;
                newNode.next = pos;
                newNode.prev = pos.prev;
                pos.prev = newNode;
            }
            Count++;
        }

        public void Remove(T value)
        {
            Node? pos = Find(value);
            if( pos == null)
                return;
            if(pos.next != null)
                pos.next.prev = pos.prev;
            if (pos.prev != null)
                pos.prev.next = pos.next;
            else
                header = pos.next;
            Count--;
        }

        public void Clear()
        {
            header = null;
            Count = 0;
        }

        public bool Contains(T value)
        {
            Node? pos = Find(value);
            if(pos == null)
                return false;
            return true;
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public class Enumerator : IEnumerator
        {
            private readonly CLinkedList<T> list;
            private Node? current;
            private bool isFirstTime;

            public Enumerator(CLinkedList<T> list)
            {
                this.list = list;
                current = list.header;
                isFirstTime = true;
            }

            public object? Current
            {
                get
                {
                    if(current == null || isFirstTime)
                        return null;
                    return current.value;
                }
            }

            public bool MoveNext()
            {
                if(current == null)
                    return false;
                if(isFirstTime)
                {
                    isFirstTime = false;
                    return true;
                }
                if(current.next == null)
                    return false;
                current = current.next;
                return true;
            }

            public void Reset()
            {
                current = list.header;
                isFirstTime = true;
            }

            public void InsertBefore(T value)
            {
                if(current == null || isFirstTime)
                    return;
                if(current.prev == null)
                    list.Add(value);
                else
                {
                    Node newNode = new Node(value);
                    current.prev.next = newNode;
                    newNode.prev = current.prev;
                    current.prev = newNode;
                    newNode.next = current;
                }
                list.Count++;
            }

            public void InsertAfter(T value)
            {
                if(current == null || isFirstTime)
                    return;
                Node newNode = new Node(value);
                if( current.next == null)
                {
                    current.next = newNode;
                    newNode.prev = current;
                }
                else
                {
                    newNode.next = current.next;
                    current.next.prev = newNode;
                    current.next = newNode;
                    newNode.prev = current;
                }
                list.Count++;
            }

            public void Remove()
            {
                if(current == null || isFirstTime) 
                    return;
                if( current.next == null && current.prev == null)
                {
                    current = null;
                    list.header = null;
                }
                else if(current.prev == null)
                {
                    current = current.next ?? throw new InvalidOperationException();
                    current.prev = null;
                    list.header = current;
                }
                else if(current.next == null)
                {
                    current = current.prev;
                    current.next = null;
                }
                else
                {
                    current.prev.next = current.next;
                    current.next.prev = current.prev;
                    current = current.next;
                }
                list.Count--;
            }

            public bool AtEnd()
            {
                return (current == null || current.next == null);
            }
        }
    }
}
