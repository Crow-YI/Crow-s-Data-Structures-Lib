namespace CDSL
{
    public class CBSTree<T> : CBinaryTree<T>
        where T : IComparable<T>
    {
        private Node? root;
        public int Count {  get; private set; }
        public T MaxValue
        { 
            get
            {
                if (root == null)
                    throw new InvalidOperationException();
                Node current = root;
                while(current.right != null)
                    current = current.right;
                return current.value;
            }
        }
        public T MinValue
        {
            get
            {
                if (root == null)
                    throw new InvalidOperationException();
                Node current = root;
                while( current.left != null)
                    current = current.left;
                return current.value;
            }
        }

        public CBSTree()
        {
            root = null;
            Count = 0;
        }

        public override void Insert(T value)
        {
            Node newNode = new Node(value);
            if(root == null)
            {
                root = newNode;
                Count++;
                return;
            }
            Node walkPoint = root;
            while(true)
            {
                int res = value.CompareTo(walkPoint.value);
                if(res < 0)
                {
                    if(walkPoint.left == null)
                    {
                        walkPoint.left = newNode;
                        Count++;
                        return;
                    }
                    else
                        walkPoint = walkPoint.left;
                }
                else if(res > 0) 
                {

                    if (walkPoint.right == null)
                    {
                        walkPoint.right = newNode;
                        Count++;
                        return;
                    }
                    else
                        walkPoint = walkPoint.right;
                }
                else
                    return;
            }
        }

        public override bool Contains(T value)
        {
            Node? walkPoint = root;
            while (walkPoint != null)
            {
                int res = value.CompareTo(walkPoint.value);
                if (res < 0)
                    walkPoint = walkPoint.left;
                else if (res > 0)
                    walkPoint = walkPoint.right;
                else
                    return true;
            }
            return false;
        }

        public override void Delete(T value)
        {
            if (root == null)
                return;
            Node? current = root;
            Node parent = root;
            bool isLeftChild = false;
            //查找要删除的节点和它的父节点
            while (current != null)
            {
                int res = value.CompareTo(current.value);
                if (res == 0)
                    break;
                parent = current;
                if(res < 0)
                {
                    current = current.left;
                    isLeftChild = true;
                }
                else
                {
                    current = current.right;
                    isLeftChild = false;
                }
            }
            if (current == null)
                return;

            //待删除节点的三种情况
            if(current.left == null && current.right == null)
            {
                if (current == root)
                    root = null;
                else if (isLeftChild)
                    parent.left = null;
                else
                    parent.right = null;
            }
            else if(current.left == null)
            {
                if(current == root)
                    root = current.right;
                else if (isLeftChild)
                    parent.left = current.right;
                else
                    parent.right = current.right;
            }
            else if(current.right == null)
            {
                if (current == root)
                    root = current.left;
                else if (isLeftChild)
                    parent.left = current.left;
                else
                    parent.right = current.left;
            }
            else
            {
                Node successor = GetSuccessor(current);
                if (current == root)
                    root.value = successor.value;
                else if (isLeftChild)
                    parent.left!.value = successor.value;
                else
                    parent.right!.value = successor.value;
            }
            Count--;
        }

        private Node GetSuccessor(Node delNode)
        {
            Node node = delNode.left!;
            Node parent = delNode;
            while(node.right != null)
            {
                parent = node;
                node = node.right;
            }
            if(parent == delNode)
                parent.left = node.left;
            else
                parent.right = node.left;
            return node;
        }
    }
}
