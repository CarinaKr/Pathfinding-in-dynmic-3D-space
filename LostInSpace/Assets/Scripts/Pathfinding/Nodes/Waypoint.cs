using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : Node {

    public WaypointObstacle wpObstacle;

    public List<Waypoint> neighbouringCorner { get; set; }
    public List<Node> indirectNeighbours { get; set; }
    public List<Node> directNeighbours { get; set; }
    public List<Node> directPredecessors { get; set; }
    public List<Node> indirectPredecessors { get; set; }
    public Vector3 vertexPosition { get; set; }

    public List<Waypoint> checkedBy { get; set; }

    public GameObject waypointObject { get; set; }

    public Waypoint(Vector3 localPosition,Vector3 globalPosition, Vector3 vertexPosition)
    {
        //do additional initialization steps here
        this.localPosition = localPosition;
        this.globalPosition = globalPosition;
        this.vertexPosition = vertexPosition;

        this.indirectNeighbours = new List<Node>();
        this.directNeighbours = new List<Node>();
        this.indirectPredecessors = new List<Node>();
        this.directPredecessors = new List<Node>();
        this.checkedBy = new List<Waypoint>();

        this.pathCosts = new Dictionary<Node, float>();
        this.gValue = Mathf.Infinity;
        this.rhsValue = Mathf.Infinity;

        snapshotNeighbours = new List<Node>();
        snapshotPredecessors = new List<Node>();
        snapshotPathCosts = new Dictionary<Node, float>();

        publicNeighbours = new List<Node>();
        publicPredecessors = new List<Node>();
        publicPathCosts = new Dictionary<Node, float>();

        priorityKey = new double[2];
        priorityKey[0] = Mathf.Infinity;
        priorityKey[1] = Mathf.Infinity;
    }

    public void AddNeighbour(Waypoint newNeighbour,bool indirect)
    {
        if (indirect)
        {
            if (indirectNeighbours.Contains(newNeighbour)) return;
            indirectNeighbours.Add(newNeighbour);
            newNeighbour.indirectPredecessors.Add(this);
        }
        else
        {
            if (directNeighbours.Contains(newNeighbour)) return;
            directNeighbours.Add(newNeighbour);
            newNeighbour.directPredecessors.Add(this);
        }
        pathCosts.Add(newNeighbour, Vector3.Distance(globalPosition, newNeighbour.globalPosition));
    }
    public void AddNeighbour(List<Waypoint> newNeighbour, bool indirect)
    {
        foreach(Waypoint wp in newNeighbour)
        {
            AddNeighbour(wp, indirect);
        }
        
    }

    override public List<Node> neighbours
    {
        get
        {
            List<Node> allNeighbours = new List<Node>();
            allNeighbours.AddRange(directNeighbours);
            allNeighbours.AddRange(indirectNeighbours);
            return allNeighbours;
        }
        set
        {
            neighbours = value;
        }
    }
    public override List<Node> predecessors
    {
        get
        {
            List<Node> allPredecessors = new List<Node>();
            allPredecessors.AddRange(directPredecessors);
            allPredecessors.AddRange(indirectPredecessors);
            return allPredecessors;
        }

        set
        {
            predecessors = value;
        }
    }
    
}
