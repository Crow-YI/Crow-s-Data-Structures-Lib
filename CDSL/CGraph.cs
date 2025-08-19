using System.Collections;

namespace CDSL
{
    public class CGraph : IEnumerable
    {
        private readonly int[][] graph;
        public int VerticesNum { get; }
        public int NoEdge { get; }
        public bool IsDirected { get; }
        public CGraph(int verticesNum, int noEdge = 0, bool isDirected = false)
        {
            if (verticesNum < 0)
                throw new ArgumentException();
            VerticesNum = verticesNum;
            NoEdge = noEdge;
            IsDirected = isDirected;
            graph = new int[VerticesNum + 1][];
            for (int i = 1; i <= VerticesNum; i++)
            {
                graph[i] = new int[VerticesNum + 1];
                for (int j = 1; j <= VerticesNum; j++)
                    graph[i][j] = noEdge;
            }
        }

        private CGraph(CGraph other)
        {
            VerticesNum = other.VerticesNum;
            NoEdge = other.NoEdge;
            IsDirected = other.IsDirected;
            int[][] otherGraph = other.graph;
            graph = new int[VerticesNum + 1][];
            for (int i = 1; i <= VerticesNum; i++)
            {
                graph[i] = new int[VerticesNum + 1];
                for (int j = 1; j <= VerticesNum; j++)
                    graph[i][j] = otherGraph[i][j];
            }
        }
        public class Edge
        {
            public int StartVertice { get; set; }
            public int EndVertice { get; set; }
            public int Weight { get; set; }
            public Edge(int startVertice, int endVertice, int weight)
            {
                StartVertice = startVertice;
                EndVertice = endVertice;
                Weight = weight;
            }
        }

        public int WeightOfEdge(int startVertice,  int endVertice)
        {
            return graph[startVertice][endVertice];
        }

        public void InsertEdge(int startVertice, int endVertice, int weight)
        {
            graph[startVertice][endVertice] = weight;
            if(!IsDirected)
                graph[endVertice][startVertice] = weight;
        }
        public void InsertEdge(Edge edge)
        {
            graph[edge.StartVertice][edge.EndVertice] = edge.Weight;
            if (!IsDirected)
                graph[edge.EndVertice][edge.StartVertice] = edge.Weight;
        }

        public void DeleteEdge(int startVertice, int endVertice)
        {
            graph[startVertice][endVertice] = NoEdge;
            if (!IsDirected)
                graph[endVertice][startVertice] = NoEdge;
        }

        public bool CheckVertice(int verticeIndex)
        {
            if(verticeIndex > 0 && verticeIndex < VerticesNum)
                return true;
            return false;
        }

        public int OutDegree(int verticeIndex)
        {
            int sum = 0;
            for(int i = 1; i <= VerticesNum; i++)
            {
                if (graph[verticeIndex][i] != NoEdge)
                    sum += graph[verticeIndex][i];
            }
            return sum;
        }

        public int InDegree(int verticeIndex)
        {
            int sum = 0;
            for (int i = 1;i <= VerticesNum; i++)
            {
                if (graph[i][verticeIndex] != NoEdge)
                    sum += graph[i][verticeIndex];
            }
            return sum;
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public class Enumerator : IEnumerator
        {
            private readonly CGraph graph;
            private int startVertice;
            private int endVertice;
            public Enumerator(CGraph graph)
            {
                this.graph = graph;
                startVertice = 1;
                endVertice = 0;
            }
            public object Current
            {
                get 
                { 
                    return new Edge(startVertice, endVertice, 
                                    graph.WeightOfEdge(startVertice, endVertice)); 
                }
            }

            public bool MoveNext()
            {
                do
                {
                    endVertice++;
                    if (endVertice > graph.VerticesNum)
                    {
                        endVertice = 1;
                        startVertice++;
                    }
                    if (startVertice > graph.VerticesNum)
                        return false;
                }
                while (graph.WeightOfEdge(startVertice, endVertice) == graph.NoEdge);
                return true;
            }

            public void Reset()
            {
                startVertice = 1;
                endVertice = 0;
            }
        }

        public void BFS()
        {
            bool[] isTraversed = new bool[VerticesNum + 1];
            int count = 0;
            CQueue<int> queue = new CQueue<int>();
            while (++count <= VerticesNum && !isTraversed[count])
            {
                isTraversed[count] = true;
                queue.EnQueue(count);
                while (queue.Count > 0)
                {
                    int current = queue.DeQueue();
                    Console.WriteLine(current);
                    for(int i = 1; i <= VerticesNum; i++)
                    {
                        if(WeightOfEdge(current, i) != NoEdge && !isTraversed[i])
                        {
                            isTraversed[i] = true;
                            queue.EnQueue(i);
                        }
                    }
                }
            }
        }

        public void DFS()
        {
            bool[] isTraversed = new bool[VerticesNum + 1];
            int count = 0;
            while (++count <= VerticesNum && !isTraversed[count])
                DFS(count, isTraversed);
        }
        private void DFS(int current, bool[] isTraversed)
        {
            Console.WriteLine(current);
            for(int i = 1; i <= VerticesNum; i++)
            {
                if (WeightOfEdge(current, i) != NoEdge && !isTraversed[i])
                {
                    isTraversed[i] = true;
                    DFS(i, isTraversed);
                }
            }
        }

        private void ClearOut(int verticesIndex)
        {
            for (int i = 1; i <= VerticesNum; i++)
                graph[verticesIndex][i] = NoEdge;
        }

        public void DFTopologicalSort()
        {
            if (!IsDirected)
                throw new InvalidOperationException();
            CGraph temp = new CGraph(this);
            bool[] isTraversed = new bool[VerticesNum + 1];
            int count = 0;
            while (++count <= VerticesNum && !isTraversed[count] 
                   && temp.InDegree(count) == 0)
                DFTopologicalSort(count, isTraversed, temp);
        }
        private void DFTopologicalSort(int current, bool[] isTraversed, CGraph temp)
        {
            Console.WriteLine(current);
            temp.ClearOut(current);
            isTraversed[current] = true;
            for (int i = 1; i <= VerticesNum; i++)
            {
                if (!isTraversed[i] && WeightOfEdge(current, i) != NoEdge  
                    && temp.InDegree(i) == 0)
                    DFTopologicalSort(i, isTraversed, temp);
            }
        }

        public int[] ShortestPaths(int startVertice)
        {
            int[] res = new int[VerticesNum + 1];
            bool[] isChose = new bool[VerticesNum + 1];
            int[] shortPath = new int[VerticesNum + 1];
            for (int i = 1; i <= VerticesNum; i++)
                shortPath[i] = int.MaxValue;

            int current = startVertice;
            int distance = 0;
            res[current] = distance;
            for(int i = 1; i < VerticesNum; i++)
            {
                isChose[current] = true;
                int nextDistance = int.MaxValue;
                int nextVertice = 0;
                for(int j = 1; j <= VerticesNum; j++)
                {
                    if (!isChose[j])
                    {
                        if (WeightOfEdge(current, j) != NoEdge)
                        {
                            int tempDistance = distance + WeightOfEdge(current, j);
                            if (tempDistance < shortPath[j])
                                shortPath[j] = tempDistance;
                        }
                        if (shortPath[j] < nextDistance)
                        {
                            nextDistance = shortPath[j];
                            nextVertice = j;
                        }
                    }
                }
                current = nextVertice;
                distance = nextDistance;
                res[current] = distance;
            }
            return res;
        }

        public void MinimumSpanningTree()
        {
            if(IsDirected)
                throw new InvalidOperationException();
            int[] lowWeight = new int[VerticesNum + 1];
            int[] vertices = new int[VerticesNum + 1];
            bool[] isChose = new bool[VerticesNum + 1];
            for(int i = 1;  i <= VerticesNum; i++)
            {
                lowWeight[i] = int.MaxValue;
                vertices[i] = 1;
            }

            int current = 1;
            for(int i = 0; i < VerticesNum - 1; i++)
            {
                isChose[current] = true;
                int nextVertice = 0;
                int lowestWeight = int.MaxValue;
                for (int j = 2; j <= VerticesNum; j++)
                {
                    if (!isChose[j])
                    {
                        if(WeightOfEdge(current, j) != NoEdge)
                        {
                            int weight = WeightOfEdge(current, j);
                            if (weight < lowWeight[j])
                            {
                                lowWeight[j] = weight;
                                vertices[j] = current;
                            }
                        }
                        if(lowWeight[j] < lowestWeight)
                        {
                            lowestWeight = lowWeight[j];
                            nextVertice = j;
                        }
                    }
                }
                current = nextVertice;
            }
            for(int i = 2; i <= VerticesNum; i++)
                Console.WriteLine($"{vertices[i]} -> {i}");
        }
    }
}
