using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Profiling;
using System.Diagnostics;

public class ARA_star : AStarBased
{
    
    public float decreaseStep;
    public float minEpsilon,minSubOpt;
    public int lookaheadSteps;

    private float subOptimalityBound;
    private HashSet<Node> inconsList;
    private float epsilonStart;
    private bool updatePlainHeuristic=false;
    private int resetCount;

    private long totalTime, totalExpanded;
    

    public override void StartPathfinding()
    {
        epsilonStart = epsilon;
        inconsList = new HashSet<Node>();
        base.StartPathfinding();
    }

    public override async Task FindPath()
    {
        FileHandler.self.WriteString("ARA* with epsilon " + epsilon + " and decrease factor " + decreaseStep +" and min epsilon "+minEpsilon+ "\n");

        await Task.Delay((int)delay).ConfigureAwait(false);
        //await new WaitForBackgroundThread();
        timer.Restart();
        algTimer.Restart();
        publishTimer.Restart();
        searchGraphManager.PublishLastScreenshot();
        publishTimer.Stop();
        goalNode = searchGraphManager.GetGoalNode();
        searchGraphManager.UpdateHeuristic(goalNode, epsilon);
        
        fastOpenList.Enqueue(goalNode, Mathf.Infinity);
        startNode = ufoController.GetLookAheadNode(lookahead);
        if (startNode == null)
            startNode = searchGraphManager.GetStartNode();
        AddToFastOpen(startNode, null, fastOpenList);
        //await new WaitForUpdate();

        ShortestPathLoop();
        UpdateSubOptimality();
        resetAlgorithm = false;
        algTimer.Stop();
        await new WaitForUpdate();
        ////FileHandler.self.WriteString(":epsilon: " + epsilon);     //MEASURE
        ////FileHandler.self.WriteString(":publishTime: " + publishTimer.ElapsedMilliseconds );
        ////FileHandler.self.WriteString(":algorithm time: " + algTimer.ElapsedMilliseconds );
        ////FileHandler.self.WriteString(":expanded: " + expanded + "\n");
        //totalTime += algTimer.ElapsedMilliseconds;
        //totalExpanded += expanded;

        await Task.Delay((int)delay).ConfigureAwait(false);
        while (isRunning)
        {
            //await new WaitForBackgroundThread();
            //algTimer.Restart();   //MEASURE
            algTimer.Start();
            startNode = ufoController.GetLookAheadNode(lookahead);

            if ((resetAlgorithm || changes.Count>0))
            {
                //MEASURE data for static AI and destination
                //FileHandler.self.WriteString(":epsilon: " + epsilon);
                //FileHandler.self.WriteString(":total time: " + totalTime);
                //FileHandler.self.WriteString(":total expanded: " + totalExpanded + "\n");
                //totalTime = 0;
                //totalExpanded = 0;

                publishTimer.Restart();

                Profiler.BeginSample("publish screenshot");
                if (resetAlgorithm)
                {
                    //MEASURE data for all-dynamic
                    FileHandler.self.WriteString(":expanded: " + expanded);
                    FileHandler.self.WriteString(":total algorithm time: " + algTimer.ElapsedMilliseconds + "\n");
                    expanded = 0;
                    algTimer.Reset();

                    startNode = searchGraphManager.GetStartNode();
                    searchGraphManager.PublishLastScreenshot();
                }
                else
                    searchGraphManager.PublishLastScreenshot(changes[0]);
                Profiler.EndSample();
                publishTimer.Stop();
                //FileHandler.self.WriteString("publishTime: " + publishTimer.ElapsedMilliseconds); //MEASURE
                timer.Restart();
                
                epsilon = epsilonStart;
                changes.Clear();
                searchGraphManager.ResetNodes();
                goalNode = searchGraphManager.GetGoalNode();
                inconsList.Clear();
                fastOpenList.Clear();
                fastOpenList.ResetNode(goalNode);
                fastOpenList.Enqueue(goalNode, Mathf.Infinity);
                
                resetAlgorithm = false;
            }
            else if (subOptimalityBound > 1 )
            {
                epsilon = Mathf.Max(minEpsilon, epsilon - decreaseStep);
            }
            closedList.Clear();
            AddInconsToOpen();
            if (startNode == null )
                startNode = searchGraphManager.GetStartNode();

            AddToFastOpen(startNode, null, fastOpenList);

            ResortOpenList();
            ShortestPathLoop();     //algorithm inner loop
            UpdateSubOptimality();
            algTimer.Stop();
            ////await new WaitForUpdate();

            //MEASURE data for static AI and destination
            //FileHandler.self.WriteString(": epsilon: " + epsilon);    
            //FileHandler.self.WriteString(": algorithm time: " + algTimer.Elapsed.TotalMilliseconds + "\n");
            //FileHandler.self.WriteString(": expanded: " + expanded + "\n");
            //totalTime += algTimer.ElapsedMilliseconds;
            //totalExpanded += expanded;

            if (subOptimalityBound <= minSubOpt || epsilon <= minEpsilon)
            {
                timer.Stop();

                while (changes.Count == 0 && isRunning)
                {
                    //UnityEngine.Debug.Log("wait");
                    await Task.Delay(TimeSpan.FromSeconds(0.01));
                }
                //await new WaitForBackgroundThread();
                await Task.Delay((int)delay).ConfigureAwait(false);
            }
        }
        
    }

    private void ShortestPathLoop()
    {
        pathFound = true;
        //expanded = 0; //MEASURE data for static AI and destination
        Profiler.BeginSample("ARA");
        nextNode = GetNextNodeFast();
        if (nextNode != null && startNode.publicIsFree)
        {
            while (goalNode.Priority > nextNode.Priority && fastOpenList.Count>0)
            {
                expanded++;
                AddToClosed(nextNode);
                foreach (Node node in nextNode.publicNeighbours)
                {
                    if (node.gValue > CalculateGValue(node, nextNode))          //current gValue of neibour is greater than if routing via current node (nextNode)
                    {
                        if (closedList.Contains(node))
                        {
                            AddToInconsList(node, nextNode, inconsList);
                        }
                        else
                        {
                            AddToFastOpen(node, nextNode, fastOpenList);
                        }
                    }
                }
                if (visualize)
                {
                    PrintList();
                    if (nextNode != null) nextNode.isNext = false;      //DEBUG
                }


                nextNode = GetNextNodeFast();
                if (nextNode == null||nextNode.Priority==Mathf.Infinity)
                {
                    UnityEngine.Debug.Log("no shortest path found");
                    pathFound = false;
                }
            }
        }
        else
            pathFound = false;

        if (pathFound)
        {
            count = 0;
            corridor.Clear();
            FillCorridorByPathCost(goalNode);
            DrawCorridor();
        }
        Profiler.EndSample();
        //yield return new WaitForUpdate();
    }

    private void ResortOpenList()
    {
        searchGraphManager.UpdateHeuristic(goalNode, epsilon);  //update the nodes' heuristics
        HashSet<Node> keys = new HashSet<Node>(fastOpenList);
        foreach (Node node in keys)
        {
            fastOpenList.UpdatePriority(node, (float)(node.gValue + node.heuristic));
        }
    }

    private void UpdateSubOptimality()
    {
        double lowestFValue = float.MaxValue;
        foreach (Node node in fastOpenList)
        {
            double fValue = node.gValue + node.plainHeuristic;
            if (fValue < lowestFValue)
            {
                lowestFValue = fValue;
            }
        }
        foreach (Node node in inconsList)
        {
            double fValue = node.gValue + node.plainHeuristic;
            if (fValue < lowestFValue)
            {
                lowestFValue = fValue;
            }

        }
        subOptimalityBound = Mathf.Min(epsilon, (float)(goalNode.Priority / lowestFValue));
    }

    private void AddInconsToOpen()
    {
        foreach(Node entry in inconsList)
        {
            fastOpenList.Enqueue(entry, (float)entry.priorityKey[0]);
        }
        inconsList.Clear();
    }

}
