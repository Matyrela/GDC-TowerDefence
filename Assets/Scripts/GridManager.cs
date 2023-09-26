using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scenes;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    public int Width;
    public int Height;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject container;
    public Graph graph;
    public Node[,] nodes;
    private static GridManager instance;
    [SerializeField] private GameObject Enemy;
    public static GridManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GridManager>();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        GridPosition();
        nodes = new Node[Width, Height];
        GridCreate();
        CreateGraphConnections();
    }

    private GameObject EnemySpawn;
    private GameObject EnemyTarget;
    private static bool pathIsValid = false;
    private LinkedList<Node> path = null;
    private LinkedList<Node> prevSecurePath = null;
    
    public LinkedList<Node> GetPath()
    {
        if (!pathIsValid)
        {
            if(path != null)
                prevSecurePath = new LinkedList<Node>(path);
            
            path = graph.EnemyPathFinding(EnemySpawn, EnemyTarget);

            if (path == null)
            {
                if(prevSecurePath != null) 
                    path = new LinkedList<Node>(prevSecurePath);
            }

            pathIsValid = true;
        }

        return path;
    }

    public bool updatePath(Cell cell)
    {
        if (path != null)
        {
            foreach (Node j in path)
            {
                if (!j.GetUsed())
                {
                    j.GetValue().GetComponent<Cell>().RemoveColor();
                }
            }
        }
        
        pathIsValid = false;
        GetPath();
        previewPath();

        return isPartOf(cell);
    }
    
    private bool isPartOf(Cell cell)
    {
        LinkedListNode<Node> a = path.First;
        
        while (a != null)
        {
            if (a.Value.GetCell() == cell)
            {
                return true;
            }
            a = a.Next;
        }
        return false;
    }

    public void previewPath()
    {
        if (path != null)
        {
            foreach (Node j in path)
            {
                j.GetCell().ChangeColor(Color.yellow);
                j.SetUsed(false);
            }
        }
        else
        {
            updatePath(null);
            previewPath();
        }
    }
    
    private void GridCreate()
    {
        graph = new Graph();
        for (int row = 0; row < Width; row++)
        {
            for (int col = 0; col < Height; col++)
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(transform.position.x + row, transform.position.y + col, 0 ), Quaternion.identity);
                cell.name = $"{row}x{col}";
                //TEMPORAL --------------------------------------------------
                if (cell.name == "0x0")
                {
                    //INICIO
                    EnemySpawn = cell;
                }
                else if (cell.name == "19x19")
                {
                    //FINAL
                    EnemyTarget = cell;
                }
                //TEMPORAL --------------------------------------------------
                cell.transform.SetParent(container.transform);
                Node node = new Node(cell);
                nodes[row, col] = node; // Asignar el objeto a la matriz
                graph.AddNode(node);
                cell.GetComponent<Cell>().node = node;
            }
        }
    }

    private void CreateGraphConnections()
    {
        for (int row = 0; row < Width; row++)
        {
            for (int col = 0; col < Height; col++)
            {
                if (row > 0)
                {
                    graph.AddEdge(nodes[row, col], nodes[row - 1, col]);
                }

                if (row < Width - 1)
                {
                    graph.AddEdge(nodes[row, col], nodes[row + 1, col]);
                }

                if (col < Height - 1)
                {
                    graph.AddEdge(nodes[row, col], nodes[row, col + 1]);
                }

                if (col > 0)
                {
                    graph.AddEdge(nodes[row, col], nodes[row, col - 1]);
                }
            }
        }
    }
    private void PrintGrid()
    {
        for (int row = 0; row < Width; row++)
        {
            for (int col = 0; col < Height; col++)
            {
                Debug.Log(nodes[row, col].GetValue().name);
            }
        }
    }

    private void PrintEdges()
    {
        for (int row = 0; row < Width; row++)
        {
            for (int col = 0; col < Height; col++)
            {
                Node[] related = nodes[row, col].GetAdy().ToArray();
                if (related.Length > 2 && related.Length <= 3)
                {
                    Debug.Log(nodes[row, col].GetValue().name + " esta relacionado con " + related[0].GetValue().name);
                    Debug.Log(nodes[row, col].GetValue().name + " esta relacionado con " + related[1].GetValue().name);
                    Debug.Log(nodes[row, col].GetValue().name + " esta relacionado con " + related[2].GetValue().name);
                }
                else if (related.Length > 3)
                {
                    Debug.Log(nodes[row, col].GetValue().name + " esta relacionado con " + related[0].GetValue().name);
                    Debug.Log(nodes[row, col].GetValue().name + " esta relacionado con " + related[1].GetValue().name);
                    Debug.Log(nodes[row, col].GetValue().name + " esta relacionado con " + related[2].GetValue().name);
                    Debug.Log(nodes[row, col].GetValue().name + " esta relacionado con " + related[3].GetValue().name);
                }
                else
                {
                    Debug.Log(nodes[row, col].GetValue().name + " esta relacionado con " + related[0].GetValue().name);
                    Debug.Log(nodes[row, col].GetValue().name + " esta relacionado con " + related[1].GetValue().name);
                }
            }
        }
    }

    private void GridPosition()
    {
        transform.position = new Vector3(transform.position.x - (Width/2) + 0.5f, transform.position.y - (Height/2) + 0.5f, 0);
    }
    
    public int SetRandomNodeAndNeighborsToFalse()
    {
        Node randomNode;
        Node neighborNode;
        int randomRow;
        int randomCol;
        int destruccion = 0;    // contador de paredes destruidas
        do
        {
            // Select a random node
            randomRow = UnityEngine.Random.Range(0, Width);
            randomCol = UnityEngine.Random.Range(0, Height);

            randomNode = nodes[randomRow, randomCol];
        }
        while (!randomNode.GetUsed()); // Check if the random node is initially true (or another condition)
        
        randomNode.SetUsed(false);
        destruccion++;
        
        // Loop through the neighboring nodes and set them to false
        for (int row = Mathf.Max(0, randomRow - 1); row <= Mathf.Min(Width - 1, randomRow + 1); row++)
        {
            for (int col = Mathf.Max(0, randomCol - 1); col <= Mathf.Min(Height - 1, randomCol + 1); col++)
            {
                neighborNode = nodes[row, col];
                if (neighborNode.GetUsed())
                {
                    nodes[row, col].SetUsed(false);
                    destruccion++;
                }
            }
        }
        return destruccion;
    }

    
    
}


