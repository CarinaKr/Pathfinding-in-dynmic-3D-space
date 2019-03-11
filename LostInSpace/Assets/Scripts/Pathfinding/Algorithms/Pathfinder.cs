using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

public abstract class Pathfinder:MonoBehaviour {
    
    public delegate void PublishCorridor();
    public static event PublishCorridor OnNewCorridor;

    public AIController ufoController;
    public bool useThetaStar=false;
    public float epsilon = 1;
    public bool visualize = false;
    public int lookahead;
    public PathfindingHelper pathfindingHelper;
    public List<Node> corridor { get; set; }

    protected float delay=1;
    public abstract  Task FindPath();
    protected abstract void PrintList();

    protected SearchGraphManager searchGraphManager;

    protected List<List<Node>> changes;
    protected Node startNode, goalNode;
    protected Node nextNode;
    protected float pathLength;
    protected float estimatedPathfindingTime;

    protected int corridorCounter;        
    protected bool isRunning;

    protected bool resetAlgorithm;
    protected int count;
    protected int expanded;

    protected bool isUsingWaypoints;
    protected bool pathFound;

    protected bool isUpdatingNodes;

    protected Stopwatch publishTimer,algTimer,timer;

    protected void OnEnable()
    {
        SearchGraphManager.OnSearchGraphDone += StartPathfinding;
        SearchGraphManager.OnChangesDetected += ChangesDetected;
        UFOController.OnResetEnemy += ResetAlgorithm;
        AIController.OnResetAI += ResetAlgorithm;
        isRunning = true;
    }
    protected void OnDisable()
    {
        SearchGraphManager.OnSearchGraphDone -= StartPathfinding;
        SearchGraphManager.OnChangesDetected -= ChangesDetected;
        UFOController.OnResetEnemy -= ResetAlgorithm;
        AIController.OnResetAI-= ResetAlgorithm;
        isRunning = false;
    }

    virtual public void StartPathfinding()
    {
        timer = new Stopwatch();
        algTimer = new Stopwatch();
        publishTimer = new Stopwatch();
        changes = new List<List<Node>>();
        corridor = new List<Node>();
        searchGraphManager = SearchGraphSelector.self.searchGraphManager;
        StartCoroutine("FindPath");
        SearchGraphSelector.self.ManageChanges();
        if (searchGraphManager is WaypointManagerInbetween)
            isUsingWaypoints = true;
    }

    protected void DrawCorridor()
    {
        pathLength = 0; //DEBUG
        for (int i = 0; i < corridor.Count - 1; i++)
        {
            pathLength += Vector3.Distance(corridor[i].publicGlobalPosition, corridor[i + 1].publicGlobalPosition);
        }
        //FileHandler.self.WriteString(": path length: " + pathLength); //MEASURE data for static AI and destination
        OnNewCorridor();
    }

    protected void FillCorridor(Node child, bool goalToStart)
    {
        if (goalToStart)
        {
            corridor.Insert(0, child);
        }  
        else
        {
            corridor.Add(child);
        }

        if (child.parentNode == null)
            return;
        FillCorridor(child.parentNode, goalToStart);
    }

    

    virtual public void ChangesDetected(List<Node> changedNodes)
    {
        //while (isUpdatingNodes)
        //    UnityEngine.Debug.Log("isUpdatingNodes");

        if (changes.Count == 0)
            changes.Add(changedNodes);
        else
        {
            if (changes.Count == 0)
                UnityEngine.Debug.Log("changes =0");
            changes[0].AddRange(changedNodes);
            if (changes.Count == 0)
                UnityEngine.Debug.Log("changes =0");
            if (changes[0].Distinct().ToList<Node>() == null)
                UnityEngine.Debug.Log("distinct is null");
            if (changes[0].Distinct().ToList<Node>().Count == 0)
                UnityEngine.Debug.Log("distinct count is null");
            changes[0] = changes[0].Distinct().ToList<Node>();
        }
    }

    public void ResetAlgorithm()
    {
        resetAlgorithm = true;
    }
}
