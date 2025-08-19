namespace CDSL
{
    public class CBTree<T>
        where T : IComparable<T>
    {
        private class Node
        {
            private readonly int level;
            public T[] values;
            public Node?[] children;
            public Node? parent;
            public int valueNum;

            public Node(int level)
            {
                this.level = level;
                values = new T[level];
                children = new Node?[level + 1];
                parent = null;
                valueNum = 0;
            }

            public bool AddValue(int index, T value, Node? left, Node? right)
            {
                if(index < 0 || index > valueNum)
                    throw new ArgumentOutOfRangeException("index");
                for(int i = valueNum; i > index; i--)
                {
                    values[i] = values[i - 1];
                    children[i + 1] = children[i];
                }
                values[index] = value;
                children[index] = left;
                children[index + 1] = right;
                if(left != null)
                    left.parent = this;
                if(right != null) 
                    right.parent = this;
                valueNum++;
                if(valueNum == level)
                    return true;
                return false;
            }

            public bool Remove(int index)
            {
                if(index < 0 || index >= valueNum)
                    throw new ArgumentOutOfRangeException("index");
                for(int i = index; i < valueNum; i++)
                {
                    values[i] = values[i + 1];
                    children[i] = children[i + 1];
                }
                children[valueNum] = null;
                valueNum--;
                if(valueNum < (level - 1) /  2)
                    return true;
                return false;
            }

            public bool RemoveEnd()
            {
                children[valueNum] = null;
                valueNum--;
                if (valueNum < (level - 1) / 2)
                    return true;
                return false;
            }

            public void Split(ref Node? left, ref Node? right)
            {
                if (valueNum != level)
                    throw new InvalidOperationException();
                int midLevel = (level - 1) / 2;
                left = this;
                right = new Node(level);
                left.valueNum = midLevel;
                int gap = midLevel + 1;
                for(int i = midLevel + 1; i < level; i++)
                {
                    right.values[i - gap] = values[i];
                    right.children[i -  gap] = children[i];
                    if( children[i] != null)
                        children[i]!.parent = right;
                    children[i] = null;
                }
                right.children[level - gap] = children[level];
                if(children[level] != null)
                    children[level]!.parent = right;
                children[level] = null;
                right.valueNum = level - gap;
            }

            public void Combine(T singleValue, Node other)
            {
                if (valueNum + other.valueNum + 1 >= level)
                    throw new InvalidOperationException();
                values[valueNum] = singleValue;
                for(int i = 0; i < other.valueNum; i++)
                {
                    values[valueNum + i + 1] = other.values[i];
                    Node? temp = other.children[i];
                    children[valueNum + i + 1] = temp;
                    if (temp != null)
                        temp.parent = this;
                }
                Node? node = other.children[other.valueNum];
                children[valueNum + other.valueNum + 1] = node;
                if (node != null)
                    node.parent = this;
                valueNum = valueNum + other.valueNum + 1;
            }
        }

        private Node? root;
        public int Level { get; }
        private int midLevel;
        public int Height { get; private set; }

        public CBTree(int level)
        {
            if (level < 3)
                throw new ArgumentException("level");
            Level = level;
            midLevel = (level - 1) / 2;
            root = null;
            Height = 0;
        }

        private bool Search(T value, out Node node, out int index)
        {
            if (root == null)
                throw new InvalidOperationException();
            node = root;
            while (true)
            {
                index = 0;
                for(int i = 0; i < node.valueNum; i++)
                {
                    int diff = value.CompareTo(node.values[i]);
                    if (diff == 0)
                        return true;
                    else if (diff < 0)
                        break;
                    index++;
                }
                if (node.children[index] == null)
                    return false;
                else
                    node = node.children[index]!;
            }

        }

        public void Insert(T value)
        {
            if(root == null)
            {
                root = new Node(Level);
                Height = 1;
                root.AddValue(0, value, null, null);
                return;
            }
            if (Search(value, out Node walkPoint, out int index))
                return;

            Node? left = null;
            Node? right = null;
            while(walkPoint.AddValue(index, value, left, right))
            {
                value = walkPoint.values[midLevel];
                walkPoint.Split(ref left, ref right);
                if(walkPoint.parent == null)
                {
                    root = new Node(Level);
                    Height++;
                    root.AddValue(0, value, left, right);
                    return;
                }
                walkPoint = walkPoint.parent;
                for (index = 0; index < walkPoint.valueNum; index++)
                {
                    if (value.CompareTo(walkPoint.values[index]) < 0)
                        break;
                }
            }
        }

        public bool Contains(T value)
        {
            if(root == null)
                return false;
            return Search(value, out Node node, out int index);
        }

        public void Delete(T value)
        {
            if(root == null)
                return;
            if(!Search(value, out Node walkPoint, out int index))
                return;
            if (walkPoint.children[index + 1] != null)
            {
                Node temp = walkPoint;
                walkPoint = walkPoint.children[index + 1]!;
                while (walkPoint.children[0] != null)
                    walkPoint = walkPoint.children[0]!;
                value = walkPoint.values[0];
                temp.values[index] = value;
                index = 0;
            }

            while(walkPoint.Remove(index))
            {
                Node? parent = walkPoint.parent;
                if (parent == null)
                {
                    if (walkPoint.valueNum == 0)
                    {
                        root = walkPoint.children[0];
                        if (root != null)
                            root.parent = null;
                        Height--;
                    }
                    return;
                }
                int pos;
                for(pos = 0; pos < parent.valueNum; pos++)
                {
                    if (value.CompareTo(parent.values[pos]) < 0)
                        break;
                }

                if(pos != 0 && parent.children[pos - 1]!.valueNum > midLevel)
                {
                    Node brother = parent.children[pos - 1]!;
                    value = parent.values[pos - 1];
                    parent.values[pos - 1] = brother.values[brother.valueNum - 1];
                    Node? left = brother.children[brother.valueNum];
                    Node? right = walkPoint.children[0];
                    walkPoint.AddValue(0, value, left, right);
                    brother.RemoveEnd();
                    return;
                }
                if(pos != parent.valueNum && parent.children[pos + 1]!.valueNum > midLevel)
                {
                    Node brother = parent.children[pos + 1]!;
                    value = parent.values[pos];
                    parent.values[pos] = brother.values[0];
                    Node? left = walkPoint.children[walkPoint.valueNum];
                    Node? right = brother.children[0];
                    walkPoint.AddValue(walkPoint.valueNum , value, left, right);
                    brother.Remove(0);
                    return;
                }

                if(pos == parent.valueNum)
                {
                    Node brother = parent.children[pos - 1]!;
                    brother.Combine(parent.values[pos - 1], walkPoint);
                    parent.children[pos] = brother;
                    walkPoint = parent;
                    index = pos - 1;
                }
                else
                {
                    Node brother = parent.children[pos + 1]!;
                    walkPoint.Combine(parent.values[pos], brother);
                    parent.children[pos + 1] = walkPoint;
                    walkPoint = parent;
                    index = pos;
                }
            }
        }
    }
}
