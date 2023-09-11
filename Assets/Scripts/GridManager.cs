using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scenes;
using UnityEngine;

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
        EnemySpawn.GetComponent<Cell>().ChangeTypes(EnumCell.EnemySpawn);
        EnemyTarget.GetComponent<Cell>().ChangeTypes(EnumCell.Finish);

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
            if (path != null)
                prevSecurePath = new LinkedList<Node>(path);


            path = graph.EnemyPathFinding(EnemySpawn, EnemyTarget);

            if (path == null)
            {
                if (prevSecurePath != null)
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
                    j.GetValue().GetComponent<SpriteRenderer>().color = Color.magenta;
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
                GameObject cell = Instantiate(cellPrefab, new Vector3(transform.position.x + row, transform.position.y + col, 0), Quaternion.identity);
                cell.name = $"{row}x{col}";
                //TEMPORAL --------------------------------------------------
                if (cell.name == "0x0")
                {
                    //INICIO
                    EnemySpawn = cell;
                    // cell.GetComponent<Node>().SetUsed(false);
                }
                else if (cell.name == "19x19")
                {
                    //FINAL
                    // cell.GetComponent<SpriteRenderer>().color = Color.green;
                    EnemyTarget = cell;
                    // cell.GetComponent<Node>().SetUsed(false);
                }
                //TEMPORAL --------------------------------------------------
                cell.transform.SetParent(transform);
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
        transform.position = new Vector3(transform.position.x - (Width / 2) + 0.5f, transform.position.y - (Height / 2) + 0.5f, 0);
    }
}
