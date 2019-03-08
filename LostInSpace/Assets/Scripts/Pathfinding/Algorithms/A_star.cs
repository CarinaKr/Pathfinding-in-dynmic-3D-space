using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Profiling;
using System.Diagnostics;

public class A_star : AStarBased
{
    private float loopTime,startTime,endTime;
    private List<Node> currentChanges;

    override public async Task FindPath()
    {
        FileHandler.self.WriteString("A* with epsilon " + epsilon+"\n");
        currentChanges = new List<Node>();
        //await new WaitForBackgroundThread();
        await Task.Delay((int)delay).ConfigureAwait(false);
        algTimer.Restart();
        publishTimer.Restart();
        searchGraphManager.PublishLastScreenshot();
        publishTimer.Stop();
        ShortestPathLoopTest();
        algTimer.Stop();
        timer.Stop();
        await new WaitForUpdate();
        //FileHandler.self.WriteString(": publishTime: " + publishTimer.Elapsed.Milliseconds);  //MEASURE
        //FileHandler.self.WriteString(": algorithm time: " + algTimer.Elapsed.Milliseconds);
        //FileHandler.self.WriteString(": expanded: " + expanded+"\n");

        while (isRunning)
        {
            while (changes.Count == 0 && isRunning)
            {
                await Task.Delay(TimeSpan.FromSeconds(0.01));
            }
            if(resetAlgorithm)  //MEASURE data in all-dynamic environment
            {
                FileHandler.self.WriteString(":expanded: " + expanded);
                FileHandler.self.WriteString(":total algorithm time: " + algTimer.ElapsedMilliseconds + "\n");
                expanded = 0;
                algTimer.Reset();
                resetAlgorithm = false;
            }
            
            await new WaitForBackgroundThread();
            //algTimer.Restart();   //MEASURE data for static AI and destination
            algTimer.Start();   //MEASURE data in all-dynamic environment
            publishTimer.Restart();
            searchGraphManager.PublishLastScreenshot(changes[0]);
            publishTimer.Stop();
            ShortestPathLoopTest();
            algTimer.Stop();
            await new WaitForUpdate();
            //FileHandler.self.WriteString(": publishTime: " + publishTimer.Elapsed.Milliseconds);  //MEASURE data for static AI and destination
            //FileHandler.self.WriteString(": algorithm time: " + algTimer.Elapsed.Milliseconds);
            //FileHandler.self.WriteString(": expanded: " + expanded + "\n");
            changes.RemoveAt(0);
        }
    }

    private void ShortestPathLoopTest()
    {
        //expanded = 0; //MEASURE data for static AI and destination
        fastOpenList.Clear();
        closedList.Clear();
        startNode = ufoController.GetLookAheadNode(lookahead);
        if (startNode == null )
            startNode = searchGraphManager.GetStartNode();
        else if(!startNode.publicIsFree)
            startNode = searchGraphManager.GetStartNode();
        goalNode = searchGraphManager.GetGoalNode();
        
        searchGraphManager.UpdateHeuristic(goalNode, epsilon);
        AddToFastOpen(startNode, null, fastOpenList);
        Profiler.BeginSample("A star");

        do
        {
            pathFound = true;
            expanded++;
            nextNode = GetNextNodeFast();
            if (nextNode != null)
            {
                AddToClosed(nextNode);
                foreach (Node node in nextNode.publicNeighbours)
                {
                    bool isInClosed = closedList.Contains(node);
                    if (!isInClosed)                                                    //node has not been expaned before
                    {
                        AddToFastOpen(node, nextNode, fastOpenList);
                    }
                }
                if(visualize)
                    PrintList();
            }
            else
            {
                pathFound = false;
                UnityEngine.Debug.Log("no shortest path found");
            }
        } while (nextNode != goalNode && nextNode != null);
        Profiler.EndSample();

        if (pathFound)
        {
            corridor.Clear();
            FillCorridor(goalNode, true);
            DrawCorridor();
        }
        //await new WaitForUpdate();
    }

    private async Task FrameDropLoop()
    {
        float count = 0;
        await Task.Delay(TimeSpan.FromSeconds(0.5));
        Profiler.BeginSample("frame drop");
        for(int i=0;i<1000000;i++)
        {
            count=Mathf.Log10(i);
            count = Mathf.Log10(i+1);
            count = Mathf.Log10(i+2);
            count = Mathf.Log10(i+3);
        }
        Profiler.EndSample();
        await Task.Delay(TimeSpan.FromSeconds(0.5));
    }
}

