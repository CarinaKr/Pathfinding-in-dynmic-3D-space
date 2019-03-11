using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Profiling;

public class D_StarLite : D_starBased {
    
    public bool useMT;      //Moving Target D* Lite optimization

    protected float km;
    protected Node lastStart;
         
    public override void StartPathfinding()
    {
        base.StartPathfinding();
    }
    public override async Task FindPath()
    {
        FileHandler.self.WriteString("D* Lite with epsilon " + epsilon+" and MT="+useMT+"\n");

        await Task.Delay((int)delay).ConfigureAwait(false);
        //await new WaitForBackgroundThread();                  //should work as well, but is not reliable
        algTimer.Restart();
        publishTimer.Restart();
        searchGraphManager.PublishLastScreenshot();
        publishTimer.Stop();
        timer.Restart();
        startNode = searchGraphManager.GetStartNode();
        lastStart = startNode;
        searchGraphManager.UpdateHeuristic(startNode, epsilon);
        goalNode = searchGraphManager.GetGoalNode();
        goalNode.rhsValue = 0;
        AddToFastOpen(goalNode);
        ShortestPathLoop();

        if (changes.Count > 0 && visualize)  //DEBUG
        {
            foreach (Node node in changes[0])
                node.isChanged = false;
        }
        changes.Clear();
        resetAlgorithm = false;

        algTimer.Stop();
        timer.Stop();
        await new WaitForUpdate();

        //FileHandler.self.WriteString(":publishTime: " + publishTimer.Elapsed.Milliseconds);   //MEASURE data for static AI and destination
        //FileHandler.self.WriteString(":only alg time: " + timer.ElapsedMilliseconds );
        //FileHandler.self.WriteString(":algorithm time: " + algTimer.Elapsed.Milliseconds);
        //FileHandler.self.WriteString(":expanded: " + expanded + "\n");

        while (isRunning)
        {
            while (changes.Count == 0 && isRunning)
            {
                await Task.Delay(TimeSpan.FromSeconds(0.01));
            }

            await Task.Delay((int)delay).ConfigureAwait(false);
            //await new WaitForBackgroundThread();
            //algTimer.Restart();   //MEASURE data for static AI and destination
            algTimer.Start();
            startNode = ufoController.GetLookAheadNode(lookahead);

            if(resetAlgorithm)
            {
                //FileHandler.self.WriteString("reset algorithm");
                algTimer.Stop(); //MEASURE data in all-dynamic environment
                //MEASURE data in all-dynamic environment
                FileHandler.self.WriteString(":expanded: " + expanded);
                FileHandler.self.WriteString(":total algorithm time: " + algTimer.ElapsedMilliseconds + "\n");
                expanded = 0;
                algTimer.Restart();

                ufoController.ResetCorridorMovement();
                publishTimer.Restart();
                searchGraphManager.PublishLastScreenshot();
                publishTimer.Stop();
                //FileHandler.self.WriteString("publish time: " + publishTimer.ElapsedMilliseconds+" ");    //MEASURE
                if ( visualize)  //DEBUG
                {
                    foreach (Node node in changes[0])
                        node.isChanged = false;

                    foreach (Node node in fastOpenListPrim)
                        node.isInOpen = false;
                }
                changes.Clear();
                
                fastOpenListPrim.Clear();
                km = 0;
                searchGraphManager.ResetNodes();
                startNode = searchGraphManager.GetStartNode();
                searchGraphManager.UpdateHeuristic(startNode, epsilon);
                goalNode = searchGraphManager.GetGoalNode();
                goalNode.rhsValue = 0;
                AddToFastOpen(goalNode);

                

                resetAlgorithm = false;
            }
            else if(changes.Count>0)
            {
                
                if (visualize)  //DEBUG
                {
                    foreach (Node node in searchGraphManager.nodes)
                    { node.isChanged = false; node.isInOpenAfterChange = false; }
                }

                publishTimer.Restart();
                isUpdatingNodes = true;
                searchGraphManager.PublishLastScreenshot(changes[0]);
                publishTimer.Stop();
                //FileHandler.self.WriteString("publish time: " + publishTimer.ElapsedMilliseconds);    //MEASURE 

                if (startNode == null)
                    startNode = searchGraphManager.GetStartNode();
                else if(!startNode.publicIsFree)
                    startNode = searchGraphManager.GetStartNode();
                
                km += (Vector3.Distance(lastStart.globalPosition, startNode.globalPosition) * epsilon);
                lastStart = startNode;
                
                searchGraphManager.UpdateHeuristic(startNode, epsilon);
                Node oldGoalNode = goalNode;
                goalNode = searchGraphManager.GetGoalNode();
                
                if ((searchGraphManager is WaypointManagerInbetween && changes[0].Count>0) || (oldGoalNode!=goalNode && !useMT))
                {
                    fastOpenListPrim.Clear();
                    km = 0;
                    searchGraphManager.ResetNodes();
                    goalNode.rhsValue = 0;
                    AddToFastOpen(goalNode);
                }
                else if(oldGoalNode != goalNode && useMT)
                {
                    goalNode.parentNode = null;
                    changes[0].Add(oldGoalNode);

                    UpdateChangedNodes(changes[0]);
                }
                else
                {
                    UpdateChangedNodes(changes[0]);
                }
                changes.Clear();
                isUpdatingNodes = false;
            }

            ShortestPathLoop();
            algTimer.Stop();
            await new WaitForUpdate();

            //MEASURE data for static AI and destination
            //FileHandler.self.WriteString("publishTime: " + publishTimer.Elapsed.Milliseconds);
            //FileHandler.self.WriteString(": algorithm time: " + algTimer.Elapsed.Milliseconds+ " : expanded: " + expanded + "\n");    //MEASURE
            //FileHandler.self.WriteString("expanded: " + expanded + "\n");
        }
    }

    public void ShortestPathLoop()
    {
        Profiler.BeginSample("D_lite");
        //expanded = 0; //MEASURE data for static AI and destination
        pathFound = true;
        if (startNode == null)
            startNode = searchGraphManager.GetStartNode();
        

        nextNode = GetNextNodeFast();

        if (startNode.publicIsFree && nextNode != null)
        {
            while ((startNode.gValue != startNode.rhsValue || IsLowerKey(nextNode, CalculateKeyValue(startNode))) && isRunning)
            {
                expanded++;
                if (nextNode != null && visualize) nextNode.isNext = false;          //DEBUG
                if (fastOpenListPrim.Count == 0)
                    Debug.Log("empty open list");
                
                double[] oldKey = nextNode.priorityKey;
                double[] newKey = CalculateKeyValue(nextNode);
                if (oldKey[0] < newKey[0] || (oldKey[0] == newKey[0] && oldKey[1] < newKey[1]))
                {
                    nextNode.priorityKey = CalculateKeyValue(nextNode);
                    fastOpenListPrim.UpdatePriority(nextNode, (float)nextNode.priorityKey[0]);
                }
                else if (nextNode.gValue > nextNode.rhsValue)   //overconsistent
                {
                    nextNode.gValue = nextNode.rhsValue;    //set g-value
                                                            //openList.Remove(nextNode);
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
                    UpdateNode(nextNode);                    //update own rhs-value
                    foreach (Node pred in nextNode.publicPredecessors)    //update rhs-value of all predecessors
                    {
                        if (pred.parentNode == null || pred.parentNode == nextNode)
                            UpdateNode(pred);
                    }
                }

                if (visualize)
                {
                    PrintList();
                    if (nextNode != null) nextNode.isNext = false;      //DEBUG
                }

                nextNode = GetNextNodeFast();
                if (nextNode == null)
                    pathFound = false;
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
        //await new WaitForEndOfFrame();
    }

    protected void ResortOpenList()
    {
        HashSet<Node> keys = new HashSet<Node>(fastOpenListPrim);
        foreach (Node node in keys)
        {
            node.priorityKey = CalculateKeyValue(node);
            fastOpenListPrim.UpdatePriority(node,(float)node.priorityKey[0]);
        }
    }

    override protected void UpdateNode(Node node)
    {
        base.UpdateNode(node);  //update rhs-value
        
        if(node.rhsValue!=node.gValue)  //if node is inconsistent
        {
            AddToFastOpen(node);                //add to open list
        }
        else if(fastOpenListPrim.Contains(node))                           //if node is consistent
        {
            if (fastOpenListPrim.Count == 0)
                Debug.Log("empty open list");
            fastOpenListPrim.Remove(node);          //remove from open list
            if (visualize)
            {
                node.isInOpen = false;  //DEBUG
                node.isInClosed = true;
            }
        }
        else if (visualize)  //DEBUG
        {
            node.isInOpen = false;  //DEBUG
            node.isInClosed = true;
        }
    }

    override protected double[] CalculateKeyValue(Node child)
    {
        //keep for demonstrations
        //double[] key = new double[2];                        
        //key[1] = Mathf.Min((float)child.gValue, (float)child.rhsValue);
        //key[0] = key[1] + child.heuristic + km;
        //return key;

        double[] key = new double[2];
        if (child.gValue > child.rhsValue)
        {
            key[0] = child.rhsValue + child.heuristic + km;
            key[1] = child.rhsValue;
        }
        else
        {
            key[0] = child.gValue + child.plainHeuristic + km;
            key[1] = child.gValue;
        }

        return key;
    }

    

    

}
