using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Priority_Queue;
using System.Linq;

public abstract class D_starBased : Pathfinder {

    public Transform childObj, corridorObj;

    protected abstract double[] CalculateKeyValue(Node child);

    protected FastPriorityQueue<Node> fastOpenListPrim;
    protected SortedDictionary<Node, float[]> sortedOpenList;
    //protected bool isUpdatingNodes;
    protected double[] newKeyValue, oldKeyValue;
    protected Node bestParentNode, newParent;

    private double minRHS, rhs, calculateRHS;

    override public void StartPathfinding()
    {
        oldKeyValue = new double[2];
        corridor = new List<Node>();
        fastOpenListPrim = new FastPriorityQueue<Node>(SearchGraphSelector.self.searchGraphManager.nodes.Capacity);

        base.StartPathfinding();
    }

    protected Node GetNextNodeFast()
    {
        if (fastOpenListPrim.Count == 0)
        {
            Debug.Log("empty open list");
            Debug.Log("no shortest path found");
        }
        if (fastOpenListPrim.Count == 0) return null;
        

        List<Node> samePriority = GetSamePrimKeys();                                             //create List of all Nodes with same priority (check prim key)
        

        Node nextNode = samePriority[0];
        double minSecKey = nextNode.priorityKey[1];
        double currentSecKey = minSecKey;
        for(int i=1;i<samePriority.Count;i++)
        {
            currentSecKey= samePriority[i].priorityKey[1];
            if (currentSecKey<minSecKey)
            {
                minSecKey = currentSecKey;
                nextNode = samePriority[i];
            }
        }
        
        return nextNode;
    }

    protected void AddToFastOpen(Node child)
    {
        newKeyValue = CalculateKeyValue(child);
        if (newKeyValue[0] >= float.MaxValue) return;

        child.priorityKey = newKeyValue;
        if (!fastOpenListPrim.Contains(child))
        {
            fastOpenListPrim.Enqueue(child, (float)newKeyValue[0]);
            
        }
        else
        {
            fastOpenListPrim.UpdatePriority(child, (float)newKeyValue[0]);
            
        }
        if (isUpdatingNodes && visualize)
            child.isInOpenAfterChange = true;   //DEBUG
    }

    private List<Node> GetSamePrimKeys()
    {
        List<Node> samePrim = new List<Node>();
        float firstPriority = fastOpenListPrim.First().Priority;
        while (fastOpenListPrim.Count>0 )
        {
            if (fastOpenListPrim.First().Priority <= firstPriority)
            {
                samePrim.Add(fastOpenListPrim.Dequeue());
            }
            else
                break;
        }
        for(int i=0;i<samePrim.Count;i++)
        {
            fastOpenListPrim.Enqueue(samePrim[i], samePrim[i].Priority);
        }
        if (fastOpenListPrim.Count == 0)
            Debug.Log("empty open list");
        return samePrim;
    }
    
    virtual protected void UpdateNode(Node node)
    {
        if (node != goalNode)               
        {
            minRHS = Mathf.Infinity;                                        
            bestParentNode = null;   
            foreach (Node succ in node.publicNeighbours)
            {
                newParent = null;
                if (succ.gValue < minRHS)                           //rhs=g+c(n,n') only calculate rhs values based on nodes, which have a smaller g-value
                {
                    rhs = CalculateRHS(node, succ, out newParent);
                    if (rhs < minRHS)
                    {
                        minRHS = rhs;
                        bestParentNode = newParent;   
                    }
                }
        }
            node.rhsValue = minRHS;
            node.parentNode = bestParentNode;
        }
    }

    protected Node GetMinKey(Node nodeA, Node nodeB)
    {
        double[] keyA = CalculateKeyValue(nodeA);
        double[] keyB = CalculateKeyValue(nodeB);
        Node minNode = nodeB;
        if (keyA[0] < keyB[0])
        {
            minNode = nodeA;
        }
        else if (keyA[0] == keyB[0])
        {
            if (keyA[1] < keyB[1])
            {
                minNode = nodeA;
            }
        }
        return minNode;
    }

    protected bool IsLowerKey(Node keyNodeA,double[] keyB)
    {
        bool isLower = false;
        double[] keyA = new double[2];
        if (keyNodeA != null)
            keyA = keyNodeA.priorityKey;
        else
            return isLower;
        if (keyA[0] < keyB[0])
        {
            isLower = true;
        }
        else if (keyA[0] == keyB[0])
        {
            if (keyA[1] < keyB[1])
            {
                isLower = true;
            }
        }
        return isLower;
    }

    protected bool IsLowerKey(double[] keyA, double[] keyB)
    {
        bool isLower = false;
        if (keyA[0] < keyB[0])
        {
            isLower=true;
        }
        else if (keyA[0] == keyB[0])
        {
            if (keyA[1] < keyB[1])
            {
                isLower = true;
            }
        }
        return isLower;
    }

    protected double CalculateRHS(Node node, Node succ, out Node newParent)  //use for implmenenting theta*
    {
        calculateRHS = 0;
        newParent = succ;
        if (!useThetaStar)
        {
            calculateRHS = node.publicPathCosts[succ] + succ.gValue;       //calculate rhs-value based on successor
        }
        else
        {
            if (node != succ.parentNode && pathfindingHelper.LineOfSight(node, succ.parentNode, true))
            {
                newParent = succ.parentNode;
                calculateRHS = Vector3.Distance(node.publicGlobalPosition, newParent.publicGlobalPosition) + newParent.gValue;
            }
            else
                calculateRHS = node.publicPathCosts[succ] + succ.gValue;
        }
        return calculateRHS;
    }

    virtual protected void UpdateChangedNodes(List<Node> changedNodes)
    {
        //FileHandler.self.WriteString("start UpdateChangedNodes \n");
        foreach (Node node in changedNodes)
        { 
            UpdateNode(node);
            node.isChanged = true;
        }
        //FileHandler.self.WriteString("done UpdateChangedNodes");
    }

    protected override void PrintList()
    {
        foreach (Node node in fastOpenListPrim)
        {
            node.isInOpen = true;
        }
    }

    protected void FillCorridorByPathCost(Node child)
    {
        count++;
        if (corridor.Contains(child))
        {
            Debug.Log("corridor contains child");
            //Debug.Log("start node is free: " + startNode.isFree);
            //Debug.Log("child: "+child.publicGlobalPosition+" g: "+child.gValue+" path cost: "+ child.publicPathCosts[corridor[corridor.Count - 1]]);
            //Debug.Log("last corridor: " + corridor[corridor.Count - 1].publicGlobalPosition+" g: " + corridor[corridor.Count - 1].gValue+" path cost: "+ corridor[corridor.Count - 1].publicPathCosts[child]);
            return;
        }
        
        corridor.Add(child);

        if (child == goalNode || count == 1000)
        {
            return;     //DEBUG
        }
        
        Node nextChild = child.publicNeighbours[0];
        double minValue = child.publicPathCosts[child.publicNeighbours[0]] + child.publicNeighbours[0].gValue;
        for (int i = 1; i < child.publicNeighbours.Count; i++)
        {
            double value = child.publicPathCosts[child.publicNeighbours[i]] + child.publicNeighbours[i].gValue;
            if (value < minValue)
            {
                nextChild = child.publicNeighbours[i];
                minValue = value;
            }
        }
        FillCorridorByPathCost(nextChild);
    }


}
