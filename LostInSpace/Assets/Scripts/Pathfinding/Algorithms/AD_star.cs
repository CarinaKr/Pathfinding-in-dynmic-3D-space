using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System;
using UnityEngine.Profiling;

public class AD_star : D_starBased {

    
    public float decreaseStep;
    public float minEpsilon, minSubOpt;

    private HashSet<Node> closedList;
    
    private float subOptimalityBound;
    private HashSet<Node> inconsList;
    private float epsilonStart;
    private bool heuristicJustUpdated;
    private long totalTime, totalExpanded;

    override public void StartPathfinding()
    {
        epsilonStart = epsilon;
        inconsList = new HashSet<Node>();
        closedList = new HashSet<Node>();
        base.StartPathfinding();
    }
    

    public override async Task FindPath()
    {
        FileHandler.self.WriteString("AD* with epsilon " + epsilon + " and decrease step " + decreaseStep+"\n");

        await Task.Delay((int)delay).ConfigureAwait(false);
        //await new WaitForBackgroundThread();
        timer.Restart();
        algTimer.Restart(); //this was only changed later; might need to add publishTime to alg time
        publishTimer.Restart();
        searchGraphManager.PublishLastScreenshot();
        publishTimer.Stop();
        startNode = searchGraphManager.GetStartNode();
        goalNode = searchGraphManager.GetGoalNode();
        searchGraphManager.UpdateHeuristic(startNode, epsilon);
        goalNode.rhsValue = 0;
        AddToFastOpen(goalNode);
        ShortestPathLoop();
        
        UpdateSubOptimality();
        resetAlgorithm = false;
        algTimer.Stop();
        await new WaitForUpdate();
        //MEASURE
        ////FileHandler.self.WriteString(":epsilon: " + epsilon);
        //////FileHandler.self.WriteString(":publishTime: " + publishTimer.ElapsedMilliseconds);
        ////FileHandler.self.WriteString(":algorithm time: " + algTimer.ElapsedMilliseconds);
        ////FileHandler.self.WriteString(":expanded: " + expanded + "\n");
        //totalTime += algTimer.ElapsedMilliseconds;
        //totalExpanded += expanded;
        
        await Task.Delay((int)delay).ConfigureAwait(false);
        while (true)
        {
            //startTime = Time.realtimeSinceStartup;
            //await Task.Delay((int)delay).ConfigureAwait(false);
            //await new WaitForBackgroundThread();
            //algTimer.Restart();                           //MEASURE
            algTimer.Start();
            heuristicJustUpdated = false;
            startNode = ufoController.GetLookAheadNode(0);
            
            if (resetAlgorithm)
            {
                //FileHandler.self.WriteString("reset algorithm \n");
                //MEASURE
                algTimer.Stop();
                FileHandler.self.WriteString(":expanded: " + expanded);
                FileHandler.self.WriteString(":total algorithm time: " + algTimer.ElapsedMilliseconds + "\n");
                expanded = 0;
                algTimer.Restart();
                //FileHandler.self.WriteString(":epsilon: " + epsilon);
                //FileHandler.self.WriteString(":total time: " + totalTime);
                //FileHandler.self.WriteString(":total expanded: " + totalExpanded + "\n");
                //totalTime = 0;
                //totalExpanded = 0;

                ufoController.ResetCorridorMovement();
                epsilon = epsilonStart;
                publishTimer.Restart();
                searchGraphManager.PublishLastScreenshot();
                publishTimer.Stop();
                //FileHandler.self.WriteString("publishTime: " + publishTimer.ElapsedMilliseconds+" "); //MEASURE
                timer.Restart();
                if (changes.Count > 0 && visualize)  //DEBUG
                {
                    foreach (Node node in changes[0])
                        node.isChanged = false;

                    foreach (Node node in fastOpenListPrim)    //DEBUG
                        node.isInOpen = false;
                }
                changes.Clear();
                inconsList.Clear();
                fastOpenListPrim.Clear();
                searchGraphManager.ResetNodes();
                goalNode = searchGraphManager.GetGoalNode();
                goalNode.rhsValue = 0;
                AddToFastOpen(goalNode);
                startNode = searchGraphManager.GetStartNode();
                if (startNode == null)
                    Debug.Log("try updating heuristic with startnode=null");
                searchGraphManager.UpdateHeuristic(startNode, epsilon);
                heuristicJustUpdated = true;

               
                
                resetAlgorithm = false;
            }
            else if (changes.Count >= 1)            //changes were detected and are in the waitlist
            {
                //MEASURE data for static AI and destination
                //FileHandler.self.WriteString(":epsilon: " + epsilon);
                //FileHandler.self.WriteString(":total time: " + totalTime);
                //FileHandler.self.WriteString(":total expanded: " + totalExpanded + "\n");
                //totalTime = 0;
                //totalExpanded = 0;

                if (changes[0].Count>0)
                    epsilon = epsilonStart;
                
                if (visualize)  //DEBUG
                {
                    foreach (Node node in searchGraphManager.nodes)
                    { node.isChanged = false; node.isInOpenAfterChange = false; }
                }
                publishTimer.Restart();
                isUpdatingNodes = true;
                searchGraphManager.PublishLastScreenshot(changes[0]);
                publishTimer.Stop();
                //FileHandler.self.WriteString("changes: " + changes[0].Count);
                //FileHandler.self.WriteString("publishTime: " + publishTimer.ElapsedMilliseconds+"");  //MEASURE data in all-dynamic environment
                timer.Restart();
                Node oldGoal = goalNode;
                goalNode = searchGraphManager.GetGoalNode();
                if ((searchGraphManager is WaypointManagerInbetween && changes[0].Count>0) || goalNode!=oldGoal)
                {
                    inconsList.Clear();
                    fastOpenListPrim.Clear();
                    searchGraphManager.ResetNodes();
                    goalNode.rhsValue = 0;
                    AddToFastOpen(goalNode);
                }
                else
                {
                    UpdateChangedNodes(changes[0]);
                }

                
                if (startNode == null )
                {
                    startNode = searchGraphManager.GetStartNode();
                }
                else if(!startNode.publicIsFree)
                {
                    startNode = searchGraphManager.GetStartNode();
                }
                else if(searchGraphManager is WaypointManagerInbetween)
                {
                    if(!startNode.ToWaypoint().wpObstacle.shadowIsActive)
                    {
                        startNode = searchGraphManager.GetStartNode();
                    }
                }

                if (startNode == null)
                    Debug.Log("try updating heuristic with startnode=null");
                searchGraphManager.UpdateHeuristic(startNode, epsilon);
                heuristicJustUpdated = true;
                    //changes.RemoveAt(0);
                    //if (visualize)  //DEBUG
                    //{
                    //    foreach (Node node in changes[0])
                    //        node.isChanged = false;
                    //}
                changes.Clear();
                isUpdatingNodes = false;
            }
            else if (subOptimalityBound > 1)
            {
                epsilon = Mathf.Max(1, epsilon - decreaseStep);
            }
            closedList.Clear();
            AddInconsToOpen();
            if (startNode == null )
            {
                startNode = searchGraphManager.GetStartNode();
            }
            else if(!startNode.publicIsFree )
            {
                startNode = searchGraphManager.GetStartNode();
            }
            else if(searchGraphManager is WaypointManagerInbetween)
            {
                if(!startNode.ToWaypoint().wpObstacle.shadowIsActive)
                {
                    startNode = searchGraphManager.GetStartNode();
                }
            }
            
            ResortOpenList();
            
            ShortestPathLoop();

            UpdateSubOptimality();
            algTimer.Stop();
            //await new WaitForUpdate();
            //MEASURE data for static AI and destination
            ////FileHandler.self.WriteString(": epsilon: " + epsilon);
            ////FileHandler.self.WriteString(": algorithm time: " + algTimer.ElapsedMilliseconds);
            ////FileHandler.self.WriteString(": expanded: " + expanded + "\n");
            //totalTime += algTimer.ElapsedMilliseconds;
            //totalExpanded += expanded;

            if (subOptimalityBound <= minSubOpt || epsilon <= minEpsilon) 
            {
                epsilon = 1;
                timer.Stop();
                while (changes.Count==0 && !resetAlgorithm)
                {
                    //Debug.Log("waiting for change");
                    await Task.Delay(TimeSpan.FromSeconds(0.01));
                }
                await new WaitForBackgroundThread();
            }
        }
    }

    private void ShortestPathLoop()
    {
        Profiler.BeginSample("AD");

        pathFound = true;
        nextNode = GetNextNodeFast();
        //expanded = 0; //MEASURE data for static AI and destination

        if (startNode.publicIsFree && nextNode!=null)
        {
            while ((startNode.gValue != startNode.rhsValue || IsLowerKey(nextNode, CalculateKeyValue(startNode))))
            {
                //FileHandler.self.WriteString("expand: " + nextNode.publicGlobalPosition);
                //yield return new WaitForSeconds(0.01f);
                expanded++;
                if (nextNode != null) nextNode.isNext = false;          //DEBUG
                else
                    Debug.Log("no next node: no shortest path found");

                if (nextNode.gValue > nextNode.rhsValue)   //overconsistent
                {
                    nextNode.gValue = nextNode.rhsValue;    //set g-value
                    closedList.Add(nextNode);
                    if (fastOpenListPrim.Contains(nextNode))
                        fastOpenListPrim.Remove(nextNode);

                    foreach (Node pred in nextNode.publicPredecessors)    //update rhs-value of all predecessors
                    {
                        UpdateNode(pred);   //possible optimization: rhs=min(oldRHS, g(nextNode)+c(pred,nextNode))
                    }
                }
                else                                    //underconsistent
                {
                    nextNode.gValue = Mathf.Infinity;       //set g-value to infinite
                    UpdateNode(nextNode);

                    foreach (Node pred in nextNode.publicPredecessors)    //update rhs-value of all predecessors
                    {
                        if (pred.parentNode == null || pred.parentNode == nextNode)
                            UpdateNode(pred);
                    }

                }
                //FileHandler.self.WriteString("next node g: " + nextNode.gValue + " rhs: " + nextNode.rhsValue);
                if (visualize)
                {
                    PrintList();
                    if (nextNode != null) nextNode.isNext = false;      //DEBUG
                }

                nextNode = GetNextNodeFast();
                if (nextNode == null)
                    pathFound=false;
                //await Task.Delay(TimeSpan.FromSeconds(0.5));
            }


            count = 0;
            corridor.Clear();
            if (pathFound)
            {
                FillCorridorByPathCost(startNode);
                DrawCorridor();
            }
        }
        else
        {
            Debug.Log("start node is blocked: no shortest path found");
        }
        Profiler.EndSample();
    }

    override protected void UpdateNode(Node node)
    {
        
        base.UpdateNode(node);  //update rhs-value

        if (node.rhsValue != node.gValue)  //if node is inconsistent
        {
            if (!closedList.Contains(node))
            {

                AddToFastOpen(node);                //add to open list
            }
            else
            {

                AddToList(node, inconsList);
                if (fastOpenListPrim.Contains(node))
                    fastOpenListPrim.Remove(node);
            }
        }
        else if(fastOpenListPrim.Contains(node))                            //if node is consistent
        {
            fastOpenListPrim.Remove(node);          //remove from open list
        }
    }

    protected void AddToList(Node child, HashSet<Node> list)
    {
        double[] keyValue = CalculateKeyValue(child);
        if (keyValue[0] >= float.MaxValue) return;
        float[] oldKeyValue = new float[2];
         if(!list.Contains(child))
        {
            list.Add(child);
        }
       
    }
    private void AddInconsToOpen()
    {
        foreach(Node entry in inconsList)
        {
            fastOpenListPrim.Enqueue(entry, (float)entry.priorityKey[0]);
        }
        inconsList.Clear();
        
    }

    override protected double[] CalculateKeyValue(Node node)
    {
        double[] key = new double[2];
        if(node.gValue>node.rhsValue)
        {
            key[0] = node.rhsValue + node.heuristic;
            key[1] = node.rhsValue ;
        }
        else
        {
            key[0] = node.gValue + node.plainHeuristic;
            key[1] = node.gValue;
        }
        
        return key;
    }


    protected override void UpdateChangedNodes(List<Node> changedNodes)
    {
        base.UpdateChangedNodes(changedNodes);
    }

    private void ResortOpenList()
    {
        if (startNode == null)
            Debug.Log("try updating heuristic with startnode=null in resortOpenList");
        //if (!heuristicJustUpdated)
            searchGraphManager.UpdateHeuristic(startNode, epsilon);  //update the nodes' heuristics
        HashSet<Node> keys = new HashSet<Node>(fastOpenListPrim);
        foreach (Node node in keys)
        {
            node.priorityKey = CalculateKeyValue(node);
            fastOpenListPrim.UpdatePriority(node, (float)node.priorityKey[0]);
        }
    }

    private void UpdateSubOptimality()
    {
        double lowestKey = float.MaxValue;
        foreach (Node node in fastOpenListPrim)
        {
            double key = node.rhsValue + node.plainHeuristic;
            if (key < lowestKey)
            {
                lowestKey = key;
            }
        }
        foreach (Node node in inconsList)
        {
            double key = node.rhsValue + node.plainHeuristic;
            if (key < lowestKey)
            {
                lowestKey = key;
            }
        }

        double[] startKeyValue = CalculateKeyValue(startNode);
        subOptimalityBound = Mathf.Min(epsilon, (float)(CalculateKeyValue(startNode)[0] / lowestKey));
    }

    protected override void PrintList()
    {
        foreach(Node node in searchGraphManager.nodes)
        {
            node.isInIncons = false;
            node.isInOpen = false;
            node.isInClosed = false;
        }
        base.PrintList();
        foreach (Node node in closedList)
        {
            node.isInClosed = true;
        }
        foreach(Node node in inconsList)
        {
            node.isInIncons = true;
        }
    }
}
