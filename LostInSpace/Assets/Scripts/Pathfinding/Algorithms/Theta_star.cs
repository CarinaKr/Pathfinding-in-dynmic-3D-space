using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Profiling;

public class Theta_star : AStarBased
{

    public int maxNodesExpanded = 1;
    public int maxFrames=1;

    private float loopTime, startTime, endTime;
    private int maxCount;
    private int counter = 0;

    

    private async Task ShortestPathLoop()
    {
        
        maxCount = maxNodesExpanded / maxFrames;
        fastOpenList.Clear();
        closedList.Clear();
        startNode = ufoController.GetLookAheadNode(lookahead);
        if (startNode == null )
        {
            startNode = searchGraphManager.GetStartNode();
        }
        else if(!pathfindingHelper.LineOfSight(ufoController.transform.position, startNode.publicGlobalPosition, true))
        {
            Debug.Log("no line of sight; new start node");
            Vector3 dir = (startNode.publicGlobalPosition - ufoController.transform.position).normalized ;
            startNode = searchGraphManager.GetClosestNode(ufoController.transform.position + dir);
            Debug.DrawRay(ufoController.transform.position, dir, Color.red, 10f);
        }
        goalNode = searchGraphManager.GetGoalNode();

        searchGraphManager.UpdateHeuristic(goalNode, epsilon);
        AddToFastOpen(startNode, null, fastOpenList);
        float endTime = Time.realtimeSinceStartup;

        //expanded = 0; //MEASURE data for static AI and destination
        loopTime = 0;
        counter = 0;
        do
        {
            Profiler.BeginSample("Theta");
            pathFound = true;
            expanded++;
            nextNode = GetNextNodeFast();
            if (nextNode != null)
            {
                AddToClosed(nextNode);
                foreach (Node node in nextNode.publicNeighbours)
                {
                    if (!closedList.Contains(node))                             //node is in closed list -> node has not been expaned before
                    {
                        AddToFastOpen(node, nextNode, fastOpenList);            //add node to open list
                    }
                }
                if (visualize)
                    PrintList();
            }
            else
            {
                pathFound = false;
                Debug.Log("no shortest path found");
            }
            Profiler.EndSample();
            counter++;
            if (counter >= maxCount)
            {
                counter = 0;
                await new WaitForUpdate();
            }

        } while (nextNode != goalNode && nextNode != null);                 //while goal is not reached and open list is not empty
        

        if (pathFound)
        {
            corridor.Clear();
            FillCorridor(goalNode, true);
            DrawCorridor();
        }

        //yield return new WaitForSeconds(0);
    }

    override public async Task FindPath()
    {
        FileHandler.self.WriteString("Theta* with epsilon: " + epsilon+"\n");

        timer.Restart();
        await new WaitForBackgroundThread();
        publishTimer.Restart();
        searchGraphManager.PublishLastScreenshot();
        publishTimer.Stop();
        await new WaitForUpdate();
        algTimer.Restart();
        await ShortestPathLoop();
        algTimer.Stop();
        timer.Stop();

        //FileHandler.self.WriteString(":publishTime: " + publishTimer.Elapsed.Milliseconds );  //MEASURE for static AI and destination
        //FileHandler.self.WriteString(":algorithm time: " + algTimer.Elapsed.Milliseconds );
        //FileHandler.self.WriteString(":publish + path: " + timer.Elapsed.Milliseconds );
        //FileHandler.self.WriteString(":expanded: " + expanded + "\n");

        while (true)
        {
            while (changes.Count == 0)
            {
                //yield return new WaitForSeconds(0.01f);
                await Task.Delay(TimeSpan.FromSeconds(0.01));
            }
            if (resetAlgorithm)  //MEASURE data in all-dynamic environment
            {
                FileHandler.self.WriteString(":expanded: " + expanded);
                FileHandler.self.WriteString(":total algorithm time: " + timer.ElapsedMilliseconds + "\n");
                expanded = 0;
                timer.Reset();
                resetAlgorithm = false;
            }

            //timer.Restart();
            timer.Start();
            await new WaitForBackgroundThread();
            publishTimer.Restart();
            searchGraphManager.PublishLastScreenshot(changes[0]);
            publishTimer.Stop();
            await new WaitForUpdate();
            algTimer.Restart();
            await ShortestPathLoop();
            algTimer.Stop();
            timer.Stop();
            changes.RemoveAt(0);

            //FileHandler.self.WriteString(": publishTime: " + publishTimer.Elapsed.Milliseconds); //MEASURE for static AI and destination
            //FileHandler.self.WriteString(": algorithm time: " + algTimer.Elapsed.Milliseconds);
            //FileHandler.self.WriteString(": publish + path: " + timer.Elapsed.Milliseconds);
            //FileHandler.self.WriteString(": expanded: " + expanded + "\n");
        }
    }

    
}

