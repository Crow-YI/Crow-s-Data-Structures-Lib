namespace CDSL
{
    public class CRBTree<T>
        where T : IComparable<T>
    {
        private enum Color
        {
            Red = 0, Black = 1
        }

        private class Node
        {
            public T value;
            public Color color;
            public Node? left;
            public Node? right;
            public Node? parent;

            public Node(T value)
            {
                this.value = value;
                color = Color.Red;
                left = null;
                right = null;
                parent = null;
            }
        }

        private Node? root;
        public int Count { get; private set; }

        public CRBTree()
        {
            root = null;
            Count = 0;
        }

        private Color GetColor(Node? node)
        {
            if (node == null)
                return Color.Black;
            return node.color;
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
            if(child != null) 
                child.parent = node;
        }

        public void Insert(T value)
        {
            Node? cur = root;
            Node? parent = null;
            bool isLeftChild = true;
            while(cur != null)
            {
                parent = cur;
                int diff = value.CompareTo(cur.value);
                if (diff == 0)
                    return;
                if(diff > 0)
                {
                    isLeftChild = false;
                    cur = cur.right;
                }
                else
                {
                    isLeftChild = true;
                    cur = cur.left;
                }
            }

            Node newNode = new Node(value);
            if(parent == null)
            {
                root = newNode;
                root.color = Color.Black;
                Count++;
                return;
            }
            
            if(isLeftChild)
                parent.left = newNode;
            else
                parent.right = newNode;
            newNode.parent = parent;
            Count++;
            
            while(GetColor(parent) == Color.Red)
            {
                Node grandpa = parent!.parent!;
                isLeftChild = (parent.left == newNode);
                if(parent == grandpa.left)
                {
                    if(GetColor(grandpa.right) == Color.Black)
                    {
                        if(isLeftChild)
                        {
                            R_Rotate(grandpa);
                            grandpa.color = Color.Red;
                            parent.color = Color.Black;
                        }
                        else
                        {
                            L_Rotate(parent);
                            R_Rotate(grandpa);
                            grandpa.color = Color.Red;
                            newNode.color = Color.Black;
                        }
                        return;
                    }
                    else
                    {
                        grandpa.color = Color.Red;
                        parent.color = Color.Black;
                        grandpa.right!.color = Color.Black;
                        newNode = grandpa;
                        parent = newNode.parent;
                    }
                }
                else
                {
                    if(GetColor(grandpa.left) == Color.Black)
                    {
                        if(isLeftChild)
                        {
                            R_Rotate(parent);
                            L_Rotate(grandpa);
                            grandpa.color = Color.Red;
                            newNode.color = Color.Black;
                        }
                        else
                        {
                            L_Rotate(grandpa);
                            grandpa.color = Color.Red;
                            parent.color = Color.Black;
                        }
                        return;
                    }
                    else
                    {
                        grandpa.color = Color.Red;
                        parent.color = Color.Black;
                        grandpa.left!.color = Color.Black;
                        newNode = grandpa;
                        parent = newNode.parent;
                    }
                }
            }
            if(parent == null)
            {
                root = newNode;
                newNode.color = Color.Black;
            }
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
            Node? cur = root;
            Node? parent = null;
            while (cur != null)
            {
                int diff = value.CompareTo(cur.value);
                if (diff == 0)
                    break;
                parent = cur;
                if(diff > 0)
                    cur = cur.right;
                else 
                    cur = cur.left;
            }
            if (cur == null)
                return;

            Count--;
            if(cur.right != null && cur.left != null)
            {
                Node temp= cur;
                parent = cur;
                cur = cur.left;
                while(cur.right != null)
                {
                    parent = cur;
                    cur = cur.right;
                }
                (cur.value, temp.value) = (temp.value, cur.value);
            }

            if(cur.color == Color.Red)
            {
                if(parent!.left == cur)
                {
                    if (cur.left != null)
                    {
                        parent.left = cur.left;
                        cur.left.parent = parent;
                    }
                    else if (cur.right != null)
                    {
                        parent.left = cur.right;
                        cur.right.parent = parent;
                    }
                    else
                        parent.left = null;
                }
                else
                {
                    if(cur.left != null)
                    {
                        parent.right = cur.left;
                        cur.left.parent = parent;
                    }
                    else if(cur.right != null)
                    {
                        parent.right= cur.right;
                        cur.right.parent = parent;
                    }
                    else 
                        parent.right = null;
                }
            }
            else
            {
                while (parent != null)
                {
                    bool isLeftChild = (parent.left == cur);
                    if (GetColor(cur.left) == Color.Red)
                    {
                        Node temp = cur.left!;
                        if (isLeftChild)
                            parent.left = temp;
                        else
                            parent.right = temp;
                        temp.parent = parent;
                        temp.color = Color.Black;
                        break;
                    }
                    else if (GetColor(cur.right) == Color.Red)
                    {
                        Node temp = cur.right!;
                        if (isLeftChild)
                            parent.left = temp;
                        else
                            parent.right = temp;
                        temp.parent = parent;
                        temp.color = Color.Black;
                        break;
                    }
                    else
                    {
                        if(isLeftChild)
                        {
                            Node brother = parent.right!;
                            if(GetColor(brother) == Color.Red)
                            {
                                L_Rotate(parent);
                                brother.color = Color.Black;
                                parent.color = Color.Red;
                                brother = parent.right!;
                            }

                            Node? temp = cur.left;
                            if(GetColor(brother.right) == Color.Red)
                            {
                                L_Rotate(parent);
                                brother.color = parent.color;
                                parent.color = Color.Black;
                                brother.right!.color = Color.Black;
                                parent.left = temp;
                                if (temp != null)
                                    temp.parent = parent;
                                break;
                            }
                            else if(GetColor(brother.left) == Color.Red)
                            {
                                brother.left!.color = parent.color;
                                R_Rotate(brother);
                                L_Rotate(parent);
                                parent.color = Color.Black;
                                brother.color = Color.Black;
                                parent.left = temp;
                                if(temp != null)
                                    temp.parent = parent;
                                break;
                            }
                            else
                            {
                                if(GetColor(parent) == Color.Red)
                                {
                                    parent.color = Color.Black;
                                    brother.color = Color.Red;
                                    parent.left = temp;
                                    if(temp != null)
                                        temp.parent = parent;
                                    break;
                                }
                                else
                                {
                                    cur.value = parent.value;
                                    cur.right = brother;
                                    cur.left = temp;
                                    if (temp != null)
                                        temp.parent = cur;
                                    brother.parent = cur;
                                    brother.color = Color.Red;
                                    parent.right = null;
                                    cur = parent;
                                    parent = cur.parent;
                                }
                            }
                        }
                        else
                        {
                            Node brother = parent.left!;
                            if (GetColor(brother) == Color.Red)
                            {
                                R_Rotate(parent);
                                brother.color = Color.Black;
                                parent.color = Color.Red;
                                brother = parent.left!;
                            }

                            Node? temp = cur.left;
                            if (GetColor(brother.right) == Color.Red)
                            {
                                R_Rotate(parent);
                                brother.color = parent.color;
                                parent.color = Color.Black;
                                brother.left!.color = Color.Black;
                                parent.right = temp;
                                if (temp != null)
                                    temp.parent = parent;
                                break;
                            }
                            else if (GetColor(brother.left) == Color.Red)
                            {
                                brother.right!.color = parent.color;
                                L_Rotate(brother);
                                R_Rotate(parent);
                                parent.color = Color.Black;
                                brother.color = Color.Black;
                                parent.right = temp;
                                if (temp != null)
                                    temp.parent = parent;
                                break;
                            }
                            else
                            {
                                if (GetColor(parent) == Color.Red)
                                {
                                    parent.color = Color.Black;
                                    brother.color = Color.Red;
                                    parent.right= temp;
                                    if (temp != null)
                                        temp.parent = parent;
                                    break;
                                }
                                else
                                {
                                    cur.value = parent.value;
                                    cur.left = brother;
                                    cur.right = temp;
                                    if (temp != null)
                                        temp.parent = cur;
                                    brother.parent = cur;
                                    brother.color = Color.Red;
                                    parent.left = parent.right;
                                    parent.right = null;
                                    cur = parent;
                                    parent = cur.parent;
                                }
                            }
                        }
                    }
                }
                if(parent == null)
                {
                    if(cur.left != null)
                    {
                        root = cur.left;
                        root.parent = null;
                        root.color = Color.Black;
                    }
                    else if(cur.right != null)
                    {
                        root = cur.right;
                        root.parent = null;
                        root.color = Color.Black;
                    }
                    else
                        root = null;
                }
            }
        }
    }
}
