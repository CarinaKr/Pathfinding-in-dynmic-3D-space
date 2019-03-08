using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Priority_Queue;

public abstract class AStarBased : Pathfinder {

    protected FastPriorityQueue<Node> fastOpenList;
    protected HashSet<Node> closedList;

    override public void StartPathfinding()
    {
        closedList = new HashSet<Node>();
        fastOpenList = new FastPriorityQueue<Node>(SearchGraphSelector.self.searchGraphManager.nodes.Capacity);
        base.StartPathfinding();
    }

    protected double CalculateGValue(Node child, Node parent)
    {
        double gValue = 0;
        if (parent != null)
        {
            gValue = parent.gValue + parent.publicPathCosts[child];   
        }
        return gValue;
    }
    protected double CalculateGValue(Node child, Node parent, out Node newParent)
    {
        double gValue = 0;
        newParent = parent;
        if (parent != null && parent.parentNode!=null)
        {
            if (pathfindingHelper.LineOfSight(child.publicGlobalPosition, parent.parentNode.publicGlobalPosition,true) && newParent.publicPathCosts[child]!=Mathf.Infinity)
            {
                newParent = parent.parentNode;
                gValue = newParent.gValue + Vector3.Distance(newParent.publicGlobalPosition, child.publicGlobalPosition);
            }
            else
                gValue = newParent.gValue + newParent.publicPathCosts[child];
            
        }
        else if(parent!=null)
        {
            gValue = newParent.gValue + newParent.publicPathCosts[child];
        }
        return gValue;
    }

    protected double CalculateFValue(Node child,Node parent,out double gValue)
    {
        gValue = 0;
        double fValue = child.heuristic;                              //calculate f-value for sorting in the open list
        if(parent!=null)
        {
            gValue = CalculateGValue(child, parent);
            fValue += gValue; 
        }
        return fValue;
    }
    protected double CalculateFValue(Node child, Node parent, out double gValue, out Node newParent)
    {
        newParent = parent;
        gValue = 0;
        double fValue = child.heuristic;                              //calculate f-value for sorting in the open list
        if (parent != null)
        {
            gValue = CalculateGValue(child, parent,out newParent);
            fValue += gValue;
        }
        return fValue;
    }

    protected bool AddToInconsList(Node child, Node parent,/* IDictionary<Node, float>*/HashSet<Node> list)
    {
        double newFValue, gValue;
        if (!useThetaStar)
        {
            newFValue = CalculateFValue(child, parent, out gValue);
        }
        else
        {
            newFValue = CalculateFValue(child, parent, out gValue, out parent);
        }

        if (newFValue == Mathf.Infinity) return false;
        float oldFValue = 0;  
        SetValues(child, parent, gValue);//if child is not already in open list
        if (!list.Contains(child))
        {
            list.Add(child);
            return true;
        }
        return false;
    }

    protected void AddToFastOpen(Node child, Node parent, FastPriorityQueue<Node> list)
    {
        double newFValue, gValue;
        if (!useThetaStar)
        {
            newFValue = CalculateFValue(child, parent, out gValue);
        }
        else
        {
            newFValue = CalculateFValue(child, parent, out gValue, out parent);
        }

        if (newFValue == Mathf.Infinity) return;
        if (!list.Contains(child))                                          //if child is not already in open list
        {
            list.Enqueue(child, (float)newFValue);                            //add to open list
            SetValues(child, parent, gValue);
        }
        else                                                        //child is already in open list
        {
            float oldFValue = child.Priority;
            if (newFValue < oldFValue)                              //if f-value of child in open list is greather than current f-value
            {
                list.UpdatePriority(child, (float)newFValue);
                SetValues(child, parent, gValue);
            }
        }
    }

    protected void SetValues(Node child, Node parent, double gValue)
    {
        child.parentNode = parent;
        child.gValue = gValue;
    }

    protected void AddToClosed(Node node)
    {
        
        fastOpenList.Remove(node);
        closedList.Add(node);
    }

    protected Node GetNextNodeFast()
    {
        if (fastOpenList.Count == 0) return null;
        return fastOpenList.First();
    }

    protected override void PrintList()
    {
        foreach (Node node in searchGraphManager.nodes)
        {
            node.isInOpen = false;
            node.isInClosed = false;
        }
        foreach (Node node in fastOpenList)
        {
            node.isInOpen = true;
        }

        foreach (Node node in closedList)
        {
            node.isInOpen = false;
            node.isInClosed = true;
        }
    }

    protected void FillCorridorByPathCost(Node child)
    {
        if (child == null)
            Debug.Log("child is null");
        count++;
        
        corridor.Insert(0,child);

        if (child == startNode || count == 1000) return; 

        Node nextChild = child.publicPredecessors[0];
        double minValue = /*child.gValue*/Mathf.Infinity;
        for (int i = 0; i < child.publicPredecessors.Count; i++)
        {
            if (!child.publicPredecessors[i].publicPathCosts.Keys.Contains(child))
                Debug.Log("no path cost");
            double value = child.publicPredecessors[i].publicPathCosts[child] + child.publicPredecessors[i].gValue;
            if (value < minValue /*&& !corridor.Contains(nextChild)*/)
            {
                nextChild = child.publicPredecessors[i];
                minValue = value;
            }
        }
        FillCorridorByPathCost(nextChild);
    }
    
}
