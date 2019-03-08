using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Profiling;
using System.Diagnostics;

public class CellGridManager : SearchGraphManager
{

    public int searchWidth, searchHeight, searchLength;
    public float cellSize;
    public GameObject cellPrefab;
    public LayerMask obstacleLayer;

    private Cell[,,] cellArray;
    private Collider[] hitCollider;
    private Vector3 position;
    private Vector3 _cellSize;
    private int UFOLayer;
    private Node startNode, goalNode;
    private Vector3 transformPosition;
    private bool[,,] oldFree, newFree;

    
    
    // Use this for initialization
    void Start () {
        updateTimer = new Stopwatch();
        oldFree = new bool[searchLength, searchHeight, searchWidth];
        newFree=new bool[searchLength, searchHeight, searchWidth];

        transformPosition = transform.position;
        _cellSize = new Vector3(cellSize, cellSize, cellSize);
        UFOLayer = LayerMask.NameToLayer("UFO");
        StartCoroutine("CreateCellGrid");
    }
    

    public IEnumerator CreateCellGrid()
    {
        cellArray = new Cell[searchLength, searchHeight, searchWidth];
        for (int z = 0; z < searchWidth; z++)
        {
            for (int y = 0; y < searchHeight; y++)
            {
                for (int x = 0; x < searchLength; x++)
                {
                    cellArray[x, y, z] = CellObject.Create(cellPrefab, transform, new Vector3(x, y, z) * cellSize, cellSize);
                }
            }
        }

        for (int z = 0; z < searchWidth; z++)
        {
            for (int y = 0; y < searchHeight; y++)
            {
                for (int x = 0; x < searchLength; x++)
                {
                    cellArray[x, y, z].neighbours=Neighbours(x,y,z);
                    cellArray[x, y, z].predecessors = cellArray[x, y, z].neighbours;
                }
            }
        }

        yield return StartCoroutine("CheckOccupation");
        SetNodes();
        yield return StartCoroutine("UpdatePathCosts");
        yield return StartCoroutine("TakeScreenshot");
        //yield return StartCoroutine("PublishLastScreenshotComplete");
        base.GraphBuild();
        StopCoroutine("CheckOccupation");
        StopCoroutine("CreateCellGrid");
    }

    private List<Node> Neighbours(int x,int y,int z)
    {
        List<Node> neighbours =new List<Node>();
        cellArray[x, y, z].pathCosts = new Dictionary<Node, float>();

        if(x>0)
        {
            neighbours.Add(cellArray[x - 1, y, z]);
            cellArray[x, y, z].pathCosts.Add(cellArray[x - 1, y, z], cellSize);
        }
        if(y>0)
        {
            neighbours.Add(cellArray[x, y - 1, z]);
            cellArray[x, y, z].pathCosts.Add(cellArray[x, y - 1, z], cellSize);
        }
        if(z>0)
        {
            neighbours.Add(cellArray[x, y, z - 1]);
            cellArray[x, y, z].pathCosts.Add(cellArray[x, y, z - 1], cellSize);
        }

        if(x<searchLength-1)
        {
            neighbours.Add(cellArray[x + 1, y, z]);
            cellArray[x, y, z].pathCosts.Add(cellArray[x + 1, y, z], cellSize);
        }
        if(y<searchHeight-1)
        {
            neighbours.Add(cellArray[x, y + 1, z]);
            cellArray[x, y, z].pathCosts.Add(cellArray[x, y + 1, z], cellSize);
        }
        if(z<searchWidth-1)
        {
            neighbours.Add(cellArray[x, y, z + 1]);
            cellArray[x, y, z].pathCosts.Add(cellArray[x, y, z + 1], cellSize);
        }
        
        return neighbours;
    }

    public IEnumerator CheckOccupation()
    {

        for (int z = 0; z < searchWidth; z++)
        {
            Profiler.BeginSample("check occupation");
            for (int y = 0; y < searchHeight; y++)
            {
                for (int x = 0; x < searchLength; x++)
                {
                    //check if cell is obstructed
                    oldFree[x, y, z] = cellArray[x, y, z].isFree;
                    cellArray[x, y, z].isFree = Physics.OverlapBox(cellArray[x, y, z].globalPosition, _cellSize / 2, Quaternion.identity, obstacleLayer, QueryTriggerInteraction.Collide).Length > 0 ? false : true;                    
                    newFree[x, y, z] = cellArray[x, y, z].isFree;
                }
            }
            Profiler.EndSample();
            yield return new WaitForUpdate();
        }
        
    }

    public IEnumerator UpdatePathCosts()
    {
        int counter = nodes.Count / maxFrames;
        foreach(Node node in nodes)
        {
            UpdatePathCosts(node);

            counter--;
            if(counter<=0)
            {
                yield return new WaitForUpdate();
                counter = nodes.Count / maxFrames;
            }
        }
            
    }
    public void UpdatePathCosts(Node node)
    {
        foreach (Node neighbour in node.neighbours)
        {
            if (!node.isFree || !neighbour.isFree)
            {
                node.pathCosts[neighbour] = Mathf.Infinity;
            }
            else 
            {
                node.pathCosts[neighbour] = cellSize;
            }

        }
    }

    private void SetNodes()
    {
        nodes = new List<Node>();
        for (int z = 0; z < searchWidth; z++)
        {
            for (int y = 0; y < searchHeight; y++)
            {
                for (int x = 0; x < searchLength; x++)
                {

                    nodes.Add(cellArray[x, y, z]);
                }
            }
        }
        int i = 0;
    }

    public override IEnumerator UpdateSearch()
    {
        updateTimer.Restart();
        StopCoroutine("CreateCellGrid");
        List<Node> changedNodes = new List<Node>();
        List<Node> freeBlockedChangedNodes = new List<Node>();
        yield return StartCoroutine("CheckOccupation");                     //check occupation of all nodes

        Profiler.BeginSample("update search");
        Profiler.BeginSample("free blocked");
        for (int z = 0; z < searchWidth; z++)                   //check which nodes have been changed
        {
            for (int y = 0; y < searchHeight; y++)
            {
                for (int x = 0; x < searchLength; x++)
                {
                    if (newFree[x, y, z] != oldFree[x, y, z])
                    {
                        freeBlockedChangedNodes.Add(cellArray[x, y, z]);
                    }
                }
            }
        }
        Profiler.EndSample();

        Profiler.BeginSample("update path costs");
        HashSet<Node> changed = new HashSet<Node>();
        foreach (Node node in freeBlockedChangedNodes)      //update path costs of all changed nodes
        {
            if(!changed.Contains(node))          //make sure to check each node only once
            {
                UpdatePathCosts(node);
                changed.Add(node);               //remove already checked nodes from copied list
            }
            foreach(Node pred in node.predecessors)  //also update path costs of all predecessors of changed nodes
            {
                if (!changed.Contains(pred))
                {
                    UpdatePathCosts(pred);
                    changed.Add(pred);
                    changedNodes.Add(pred);
                }
            }
        }
        Profiler.EndSample();

        changedNodes.AddRange(freeBlockedChangedNodes);
        changedNodes = changedNodes.Distinct().ToList<Node>();
        Profiler.EndSample();
        yield return StartCoroutine("TakeScreenshotPart", changedNodes);
        DetectedChanges(changedNodes);

        updateTimer.Stop();
        yield return null;
    }

    public override Node GetStartNode()
    {
        return GetClosestNode(SearchGraphSelector.self.startPosition);
    }
    public override Node GetGoalNode()
    {
        return GetClosestNode(SearchGraphSelector.self.goalPosition);
    }

    public override Node GetClosestNode(Vector3 position)
    {
        Node node=null;
        if (position.x > transformPosition.x - (cellSize / 2) && position.x < transformPosition.x + searchLength - (cellSize / 2)            //make sure start is inside of search space
            && position.y > transformPosition.y - (cellSize / 2) && position.y < transformPosition.y + searchHeight - (cellSize / 2)
            && position.z > transformPosition.z - (cellSize / 2) && position.z < transformPosition.z + searchWidth - (cellSize / 2))
        {
            int cellX, cellY, cellZ;
            cellX = Mathf.FloorToInt((position.x-transformPosition.x) / cellSize);
            cellY = Mathf.FloorToInt((position.y-transformPosition.y) / cellSize);
            cellZ = Mathf.FloorToInt((position.z-transformPosition.z) / cellSize);
            node = cellArray[cellX, cellY, cellZ];
        }
        if(!node.publicIsFree)
        {
            UnityEngine.Debug.Log("node is blocked: " + node.publicGlobalPosition);
            foreach(Node neighbour in node.neighbours)
            {
                if(neighbour.publicIsFree)
                {
                    UnityEngine.Debug.Log("gets replaced with: " + neighbour.publicGlobalPosition);
                    node = neighbour;
                    return node;
                }
            }
        }

        return node;
    }
    
}
