namespace CDSL
{
    public class CAVLtree<T>
        where T : IComparable<T>
    {
        private class Node
        {
            public T value;
            public int height;
            public Node? left;
            public Node? right;
            public Node()
            {
                value = default!;
                left = null;
                right = null;
                height = 1;
            }
            public Node(T value)
            {
                this.value = value;
                left = null;
                right = null;
                height = 1;
            }
        }

        private Node? root;
        public int Count { get; private set; }

        public CAVLtree()
        {
            root = null;
            Count = 0;
        }

        private static int GetHeight(Node? node)
        {
            if (node == null)
                return 0;
            else 
                return node.height;
        }

        private Node R_Rotate(Node node)
        {
            Node temp = node.left!;
            node.left = temp.right;
            temp.right = node;

            node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));
            temp.height = 1 + Math.Max(GetHeight(temp.left), GetHeight(temp.right));
            return temp;
        }
        private Node L_Rotate(Node node)
        {
            Node temp = node.right!;
            node.right = temp.left;
            temp.left = node;

            node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));
            temp.height = 1 + Math.Max(GetHeight(temp.left), GetHeight(temp.right));
            return temp;
        }

        private int GetBalance(Node? node)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));
            return GetHeight(node.left) - GetHeight(node.right);
        }

        public void Insert(T value)
        {
            bool isAdd = true;
            root = Insert(value, root, ref isAdd);
            if (isAdd)
                Count++;
        }
        private Node? Insert(T value, Node? node, ref bool isAdd)
        {
            if(node == null)
                return new Node(value);

            int diff = value.CompareTo(node.value);
            if(diff == 0)
            {
                isAdd = false;
                return node;
            }
            else if(diff < 0)
                node.left = Insert(value, node.left, ref isAdd);
            else
                node.right = Insert(value, node.right, ref isAdd);

            node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));
            int balance = GetBalance(node);
            if (balance > 1 && GetBalance(node.left) > 0)
                return R_Rotate(node);
            if(balance > 1 && GetBalance(node.left) < 0)
            {
                node.left = L_Rotate(node.left!);
                return R_Rotate(node);
            }
            if (balance < -1 && GetBalance(node.right) < 0)
                return L_Rotate(node);
            if(balance < -1 && GetBalance(node.right) > 0)
            {
                node.right = R_Rotate(node.right!);
                return L_Rotate(node);
            }
            return node;
        }

        public bool Contains(T value)
        {
            Node? walkPoint = root;
            while(walkPoint != null)
            {
                int diff = value.CompareTo(walkPoint.value);
                if (diff == 0) 
                    return true;
                if (diff > 0)
                    walkPoint = walkPoint.right;
                else
                    walkPoint = walkPoint.left;
            }
            return false;
        }

        public void Delete(T value)
        {
            bool isRemove = true;
            root = Delete(value, root, ref isRemove);
            if(isRemove)
                Count--;
        }
        private Node? Delete(T value, Node? node, ref bool isRemove)
        {
            if (node == null)
            {
                isRemove = false;
                return null;
            }
            int diff = value.CompareTo(node.value);
            if(diff < 0)
                node.left = Delete(value, node.left, ref isRemove);
            else if (diff > 0)
                node.right = Delete(value, node.right, ref isRemove);
            else
            {
                if (node.left == null && node.right == null)
                    node = null;
                else if (node.right == null && node.left != null)
                    node = node.left;
                else if (node.left == null && node.right != null)
                    node = node.right;
                else
                {
                    Node walkPoint = node.left!;
                    while (walkPoint.right != null)
                        walkPoint = walkPoint.right;
                    node.value = walkPoint.value;
                    node.left = Delete(walkPoint.value, node.left, ref isRemove);
                }
            }

            if (node == null)
                return null;
            node.height = 1 + Math.Max(GetHeight(node.left), GetHeight(node.right));
            int balance = GetBalance(node);
            if (balance > 1 && GetBalance(node.left) >= 0)
                return R_Rotate(node);
            if (balance > 1 && GetBalance(node.left) < 0)
            {
                node.left = L_Rotate(node.left!);
                return R_Rotate(node);
            }
            if (balance < -1 && GetBalance(node.right) <= 0)
                return L_Rotate(node);
            if (balance < -1 && GetBalance(node.right) > 0)
            {
                node.right = R_Rotate(node.right!);
                return L_Rotate(node);
            }
            return node;
        }
    }
}
