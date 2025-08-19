namespace CDSL
{
    public abstract class CBinaryTree<T>
    {
        protected class Node
        {
            public T value;
            public Node? left;
            public Node? right;

            public Node()
            {
                value = default!;
                left = null;
                right = null;
            }
            public Node(T value)
            {
                this.value = value;
                left = null;
                right = null;
            }
        }

        public abstract void Insert(T value);
        public abstract void Delete(T value);
        public abstract bool Contains(T value);
    }
}
