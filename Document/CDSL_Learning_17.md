# CDSL_Learning_17

## Graph类说明
图（graph）是一种复杂的数据结构，用于描述多个数据间的关系：使用顶点（vertice）表示数据，顶点间的边（edge）表示数据间的关系。图的种类很多，是否有向，边是否有权重。图的数据存储方式也有多种，使用邻接矩阵，邻接链表，邻接数组都可以描述图。因此，图的实现方式多种多样，大多数语言的标准库都没有图这一数据结构，不同的使用场景对应的图的种类不同，本文实现的是邻接矩阵描述的有向或无向加权图，并使用这个图实现了图常见的5种算法：深度优先搜索，宽度优先搜索，拓扑排序，最短路径，最小生成树

## Graph类实现
### 字段和属性
因为在图中，每个顶点都可以和任意个其他顶点存在边，因此，很难想二叉树那样实现一个固定的节点类型，而是使用邻接矩阵来存储边。在创建一个图实例时要确定图中的顶点n，创建一个int[n+1][n+1]大小的矩阵，下标0不使用。行表示边的出发点，列表示边的到达点，矩阵中对应的数据是权重。只读属性VerticesNum存储顶点数，NoEdge存储一个用于代表边不存在的权重（一般都是0），IsDirected用于说明图是有向图还是无向图
```C#
private readonly int[][] graph;
public int VerticesNum { get; }
public int NoEdge { get; }
public bool IsDirected { get; }
```

### 构造器
用户使用的构造器要在实例化一个图时指定3个只读属性的值，还有一个私有的构造器能够创建一个图的副本（下文中的方法会使用到这个构造器），依据我的实践经验来看，用户一般没有创建图副本的需求，所以我就把这个构造器设为私有
```C#
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
```

### Edge成员类
Edge成员类的实现一目了然，用户可以使用Edge成员类来作为InsertEdge方法的参数，Graph的迭代器返回的也是Edge成员类
```C#
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
```

### 与边相关的方法
常见对边的操作有：插入，删除边，查询边的权重。在无向图中，当插入或删除边时，反方向的边也要插入或者删除（因为无向图中没有出发顶点和结束顶点的概念）
```C#
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
```

### 与顶点相关的操作
常见对顶点的操作有查询某一个编号是否是实例中存在的顶点编号，查询顶点的出入度。出度是指把该点作为出发点的边的数量，也就是矩阵中对应行上权重不为NoEdge的数量，入度就是列上权重不为NoEdge的数量
```C#
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
```

### IEnumerable实现
Graph的枚举器是行从小到大，列从小到大枚举所有的边，要注意权重为NoEdge的边是不存在的，枚举时要跳过这些边，除此之外，和之前实现的枚举器没有差别，枚举返回的类型是Edge成员类
```C#
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
```

### BFS
BFS，DFS和最小生成树本质都是遍历图的算法，只是遍历的顺序不同。宽度优先搜索需要借助队列存储遍历元素的顺序，每遍历一个元素时，都把与该元素有关的且未被遍历过的元素加入到队列中。遍历元素的顺序就是队列依次取出的顺序，同时考虑到整个图并不一定是整体连通的，所以要在外加一层while循环防止无法全部元素遍历
```C#
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
```

### DFS
深度优先遍历采用递归思想实现。每一层递归都代表对一个元素的遍历，由某一层往下的递归就是对与该元素有关的且未被遍历过的元素的遍历。为了防止因图整体不连通导致无法完全遍历，需要再加一层while遍历。和AVLTree的递归插入一样，需要一个对外的公有方法和实现具体递归的私有方法重载
```C#
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
```

### 拓扑排序
拓扑排序一定是在有向无环图中。拓扑排序同样有深度优先和宽度优先，但因为和BFS，DFS的思想是相同的，只是在遍历时要时刻考虑顶点的入度问题，所以在此只实现深度优先的拓扑排序。此方法与DFS的搜索下一层的遍历很像，不过在搜索时要寻找入度为0的元素。同时，每遍历一个元素后，要把所有以该元素为出发点的边删除，这里定义一个私有方法实现这一过程
```C#
private void ClearOut(int verticesIndex)
{
    for (int i = 1; i <= VerticesNum; i++)
        graph[verticesIndex][i] = NoEdge;
}
```
因为拓扑排序在进行排序后并没有改变原本类中的边，所以删除边这一操作不能在自身中进行，需要一个副本，这里用到了那个私有的构造器。又因为删除边的操作是在副本中进行，所以判断入度是否为0的操作也要在副本中进行，但是判断两个顶点之间是否存在关系不能在副本上进行，否则会出现逻辑错误（在我的实现中是这样的，可以修改，不过会比较麻烦，需要用到队列）
```C#
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
```

### 最短路径
最短路径问题的目的是求从指定顶点出发，到所有连通的顶点的最短路径。我这里使用的是Dijkstra算法，算法的具体原理不做解释。代码实现就是模拟填表格的过程，创建一个矩阵（table）表示表格，行表示各个顶点，列表示每一次的找点操作。每次找点都是从当前顶点出发，看是否要更新先前的路径（有无更短路径），然后把上一次找点时的最短的路径和顶点作为下一次找点的总路径和顶点，找过的点要做标记防止重复寻找（使用isChose做标记），记录点时要同时记录此时的路径（res）作为最短路径
```C#
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
```

### 最小生成树
最小生成树一般是在无向加权图中。可以这样理解，把每个顶点当作城市，边权作为城市间的距离，要修最短的路把所有的城市连通。我这里使用的是Prim算法，算法原理不做介绍。代码实现和最短路径相似，也是模拟画表格的过程，只不过不需要维护总路径的长度，要维护的是每个顶点的前驱顶点（vertices）。每次找点都是从当前顶点出发，看是否要更新先前的前驱顶点和最小权重（lowWeight）（有无更小权重），然后把上一次找点时的最小权重的顶点作为下一次找点的起始顶点，找过的点要做标记防止重复寻找（使用isChose做标记），记录点时也同时固定了此时起始顶点的前驱顶点
```C#
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
```

## 总结
正如上文所说，图这一数据结构是十分依赖具体场景的，基本没有通用的实现方法，本文的实现只是无数种实现之一，但基本的思想是不变的。至此，上一学期中所学的所有数据结构已经全部使用C#语言重写了一遍，成功在暑假内完成任务