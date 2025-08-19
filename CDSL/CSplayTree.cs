namespace CDSL
{
    public class CSplayTree<T>
        where T : IComparable<T>
    {
        private class Node
        {
            public T value;
            public Node? parent;
            public Node? left;
            public Node? right;

            public Node(T value)
            {
                this.value = value;
                parent = null;
                left = null;
                right = null;
            }
        }

        private Node? root;
        public int Count { get; private set; }

        public CSplayTree()
        {
            root = null;
            Count = 0;
        }

        private void R_Rotate(Node node)
        {
            Node? parent = node.parent;
            Node temp = node.left!;
            Node? child = temp.right;
            temp.parent = parent;
            if (parent == null)
                root = temp;
            else if (parent.left == node)
                parent.left = temp;
            else
                parent.right = temp;
            node.left = child;
            node.parent = temp;
            temp.right = node;
            if (child != null)
                child.parent = node;
        }
        private void L_Rotate(Node node)
        {
            Node? parent = node.parent;
            Node temp = node.right!;
            Node? child = temp.left;
            temp.parent = node.parent;
            if (parent == null)
                root = temp;
            else if (parent.left == node)
                parent.left = temp;
            else
                parent.right = temp;
            node.right = child;
            node.parent = temp;
            temp.left = node;
            if (child != null)
                child.parent = node;
        }

        private void Rotate(Node node)
        {
            Node? parent = node.parent;
            if (parent == null)
                return;
            if (parent.left == node)
                R_Rotate(parent);
            else
                L_Rotate(parent);
        }

        private bool IsLeftChild(Node node)
        {
            Node? parent = node.parent;
            if (parent == null)
                throw new InvalidOperationException();
            if(parent.left == node) 
                return true;
            return false;

        }

        private void Splay(ref Node des, Node node)
        {
            Node? temp = des.parent;
            for(Node? parent; (parent = node.parent) != temp; Rotate(node))
            {
                if(parent!.parent != temp)
                {
                    if (IsLeftChild(node) == IsLeftChild(parent))
                        Rotate(parent);
                    else
                        Rotate(node);
                }
                des = node;
            }
        }

        public void Insert(T value)
        {
            Node? node = root;
            Node? parent = null;
            bool isLeftChild = true;

            while(node != null)
            {
                parent = node;
                int diff = value.CompareTo(node.value);
                if (diff == 0)
                    return;
                if(diff > 0)
                {
                    node = node.right;
                    isLeftChild = false;
                }
                else
                {
                    node = node.left;
                    isLeftChild = true;
                }
            }

            Node newNode = new Node(value);
            Count++;
            if(parent == null)
            {
                root = newNode;
                return;
            }
            if(isLeftChild)
                parent.left = newNode;
            else
                parent.right = newNode;
            newNode.parent = parent;
            Splay(ref root!, newNode);
        }

        private void Find(T value)
        {
            if (root == null)
                return;
            Node node = root;
            int diff;
            while ((diff = value.CompareTo(node.value)) != 0)
            {
                if(diff > 0)
                {
                    if (node.right == null)
                        break;
                    node = node.right;
                }
                else
                {
                    if(node.left == null)
                        break;
                    node = node.left;
                }
            }
            Splay(ref root, node);
        }

        public bool Contains(T value)
        {
            if(root == null)
                return false;
            Find(value);
            return (value.CompareTo(root.value) == 0);
        }

        public void Delete(T value)
        {
            if (root == null)
                return;
            Find(value);
            if (value.CompareTo(root.value) != 0)
                return;
            Count--;
            if (root.left == null)
            {
                root = root.right;
                if(root != null)
                    root.parent = null;
            }
            else if(root.right == null)
            {
                root = root.left;
                root.parent = null;
            }
            else
            {
                Node left = root.left;
                Node right = root.right;

                right.parent = null;
                Node walkPoint = right;
                while (walkPoint.left != null)
                    walkPoint = walkPoint.left;
                Splay(ref right, walkPoint);
                right.left = left;
                left.parent = right;
                root = right;
            }
        }
    }
}
