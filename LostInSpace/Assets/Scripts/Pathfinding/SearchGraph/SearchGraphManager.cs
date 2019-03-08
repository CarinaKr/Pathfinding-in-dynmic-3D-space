using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

public abstract class SearchGraphManager : MonoBehaviour {

    public delegate void SearchGraphDone();
    public static event SearchGraphDone OnSearchGraphDone;

    public delegate void ChangesDetected(List<Node> changedNodes);
    public static event ChangesDetected OnChangesDetected;

    public List<Node> nodes { get; set; }

    public abstract Node GetStartNode();
    public abstract Node GetGoalNode();
    public abstract Node GetClosestNode(Vector3 position);
    public abstract IEnumerator UpdateSearch();

    protected int maxFrames = 15;
    protected int maxFramesPart = 2; 
    protected KeyValuePair<Node,float>[] currentKeyValue=new KeyValuePair<Node,float>[10];

    protected Stopwatch updateTimer;

    private Node oldHeuristicGoal;
    private bool updatePlainHeuristic;
    private bool isPublishing,isTakingSnapshot;

    protected void GraphBuild()
    {
        OnSearchGraphDone();
    }

    protected void DetectedChanges(List<Node> changedNodes)
    {
        OnChangesDetected(changedNodes);
    }

    public void UpdateHeuristic(Node goal,float inflate)
    {
        if (oldHeuristicGoal != goal)
        {
            updatePlainHeuristic = true;
            oldHeuristicGoal = goal;
        }
        else
            updatePlainHeuristic = false;

        foreach(Node node in nodes)
        {
            node.UpdateHeuristic(goal.publicGlobalPosition,inflate,updatePlainHeuristic);
        }
    }

    public void ResetNodes()
    {
        foreach (Node node in nodes)
        {
            node.Reset();
        }
    }

    protected IEnumerator TakeScreenshot()
    {
        while(isPublishing)
        {
            UnityEngine.Debug.Log("is publising");
        }
        isTakingSnapshot = true;
        
        int counter = nodes.Count / maxFrames;
        foreach (Node node in nodes)
        {
            Profiler.BeginSample("take screenshot");

            if (node.snapshotNeighbours.Count == 0 || this is WaypointManagerInbetween)
            {
                node.snapshotGlobalPosition = node.globalPosition;
                node.snapshotIsFree = node.isFree;

                Profiler.BeginSample("neighbours");
                node.snapshotNeighbours.Clear();
                node.snapshotNeighbours.AddRange(node.neighbours);
                Profiler.EndSample();

                Profiler.BeginSample("pred");
                node.snapshotPredecessors.Clear();
                node.snapshotPredecessors.AddRange(node.predecessors);
                Profiler.EndSample();
            }

            Profiler.BeginSample("path costs");
            node.snapshotPathCosts = new Dictionary<Node, float>(node.pathCosts);
            Profiler.EndSample();

            Profiler.EndSample();
            counter--;
            if(counter<=0)
            {
                yield return new WaitForUpdate();
                counter = nodes.Count / maxFrames;
            }
        }
        isTakingSnapshot = false;
    }
    virtual protected IEnumerator TakeScreenshotPart(List<Node> changedNodes)
    {
        while (isPublishing) { UnityEngine.Debug.Log("is publising"); }
        isTakingSnapshot = true;
        
        int counter = changedNodes.Count / (maxFramesPart);
        foreach (Node node in changedNodes)
        {
            Profiler.BeginSample("take snapshot part");
            node.snapshotGlobalPosition = node.globalPosition;
            node.snapshotIsFree = node.isFree;

            if (node.snapshotNeighbours.Count == 0 || node.snapshotPredecessors.Count == 0 || this is WaypointManagerInbetween)
            {
                node.snapshotNeighbours.Clear();
                node.snapshotNeighbours.AddRange(node.neighbours);

                node.snapshotPredecessors.Clear();
                node.snapshotPredecessors.AddRange(node.predecessors);
            }
            
            node.snapshotPathCosts.Clear();
            
            node.pathCosts.CopyTo(currentKeyValue, 0);

            for (int i = 0; i < node.pathCosts.Count; i++)
            {
                node.snapshotPathCosts.Add(currentKeyValue[i]);
            }
            Profiler.EndSample();

            counter--;
            if (counter <= 0)
            {
                yield return new WaitForUpdate();
                counter = changedNodes.Count / (maxFramesPart);
            }
        }
        isTakingSnapshot = false;
    }

    virtual public IEnumerator PublishLastScreenshotEnumerator()
    {
        float start = Time.realtimeSinceStartup;
        float delta = 0;
        int counter = nodes.Count / maxFrames;
        foreach (Node node in nodes)
        {
            node.publicGlobalPosition = node.snapshotGlobalPosition;
            node.publicIsFree = node.snapshotIsFree;

            if (node.publicNeighbours.Count == 0 || this is WaypointManagerInbetween)
            {
                node.publicNeighbours.Clear();
                foreach (Node screenshot in node.snapshotNeighbours)
                    node.publicNeighbours.Add(screenshot);
            }

            if (node.publicPredecessors.Count == 0 || this is WaypointManagerInbetween)
            {
                node.publicPredecessors.Clear();
                foreach (Node screenshot in node.snapshotPredecessors)
                    node.publicPredecessors.Add(screenshot);
            }

            start = Time.realtimeSinceStartup;
            node.publicPathCosts.Clear();
            
            node.snapshotPathCosts.CopyTo(currentKeyValue, 0);

            for (int i=0;i<node.snapshotPathCosts.Count;i++)
            {
                node.publicPathCosts.Add(currentKeyValue[i]);
            }
            delta = Time.realtimeSinceStartup - start;
            if (delta > 0.1)
            {
                UnityEngine.Debug.Log("too long");
                long size = 0;
                object o = new object();
                using (Stream s = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(s, o);
                    size = s.Length;
                }
            }
           
            counter--;
            if (counter <= 0)
            {
                start = Time.realtimeSinceStartup;
                yield return new WaitForUpdate();
                counter = nodes.Count / maxFrames;
            }
        }
    }
    public void PublishLastScreenshot()
    {
        while (isTakingSnapshot) { UnityEngine.Debug.Log("taking snapshot"); }
        isPublishing = true;
        Profiler.BeginSample("publish last screenshot");
        int counter = nodes.Count / maxFrames;
        foreach (Node node in nodes)
        {
            node.publicGlobalPosition = node.snapshotGlobalPosition;
            node.publicIsFree = node.snapshotIsFree;

            if (node.publicNeighbours.Count == 0 || this is WaypointManagerInbetween)
            {
                node.publicNeighbours.Clear();
                foreach (Node screenshot in node.snapshotNeighbours)
                    node.publicNeighbours.Add(screenshot);
            }

            if (node.publicPredecessors.Count == 0 || this is WaypointManagerInbetween)
            {
                node.publicPredecessors.Clear();
                foreach (Node screenshot in node.snapshotPredecessors)
                    node.publicPredecessors.Add(screenshot);
            }

            node.publicPathCosts.Clear();

            node.snapshotPathCosts.CopyTo(currentKeyValue, 0);

            for (int i = 0; i < node.snapshotPathCosts.Count; i++)
            {
                node.publicPathCosts.Add(currentKeyValue[i]);
            }
           
        }
        Profiler.EndSample();
        isPublishing = false;
    }

    public IEnumerator PublishLastScreenshotEnumerator(List<Node> changedNodes)
    {
        int counter = changedNodes.Count / (maxFramesPart);
        foreach (Node node in changedNodes)
        {

            node.publicGlobalPosition = node.snapshotGlobalPosition;
            node.publicIsFree = node.snapshotIsFree;

            if (node.snapshotNeighbours.Count == 0 || node.snapshotPredecessors.Count == 0 || this is WaypointManagerInbetween)
            {
                node.publicNeighbours.Clear();
                node.publicNeighbours.AddRange(node.snapshotNeighbours);

                node.publicPredecessors.Clear();
                node.publicPredecessors.AddRange(node.snapshotPredecessors);
            }
            
            node.publicPathCosts.Clear();
            foreach (KeyValuePair<Node, float> keyValue in node.snapshotPathCosts)
            {
                node.publicPathCosts.Add(keyValue);
            }

            counter--;
            if (counter <= 0)
            {
                yield return new WaitForUpdate();
                counter = changedNodes.Count / (maxFramesPart);
            }
        }

    }
    public void PublishLastScreenshot(List<Node> changedNodes)
    {
        while (isTakingSnapshot) { UnityEngine.Debug.Log("taking snapshot"); }
        isPublishing = true;
        foreach (Node node in changedNodes)
        {

            node.publicGlobalPosition = node.snapshotGlobalPosition;
            node.publicIsFree = node.snapshotIsFree;
            
            node.publicNeighbours.Clear();
            node.publicNeighbours.AddRange(node.snapshotNeighbours);

            node.publicPredecessors.Clear();
            node.publicPredecessors.AddRange(node.snapshotPredecessors);

            node.publicPathCosts.Clear();
            foreach (KeyValuePair<Node, float> keyValue in node.snapshotPathCosts)
            {
                node.publicPathCosts.Add(keyValue);
            }
            
        }
        isPublishing = false;
    }
}
