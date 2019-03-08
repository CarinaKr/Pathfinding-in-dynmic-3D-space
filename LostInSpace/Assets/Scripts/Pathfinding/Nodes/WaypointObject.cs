using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointObject : MonoBehaviour {

    private Waypoint waypoint;

    public bool showAllNodes;
    public bool showDirectNeighbours, showDirectPredecessors;
    public bool showIndirectNeighbours, showIndirectPredecessors;
    public bool showParent;

    public double rhs, g,heuristic,plainHeuristic;
    public bool isInOpen, isInClosed, isInOpenAfterUpdate, isInIncons,isChanged;
    public List<float> pathCosts;

    public static Waypoint Create(GameObject prefab, Transform parent, Vector3 position, Vector3 vertexPosition)
    {
        GameObject newObject = Instantiate(prefab, parent);
        newObject.transform.localPosition = position;
        WaypointObject waypointObject = newObject.GetComponent<WaypointObject>();
        Waypoint waypoint = new Waypoint(position, newObject.transform.position, vertexPosition);
        waypointObject.waypoint = waypoint;
        waypoint.waypointObject = newObject;
        return waypoint;
    }

    private void OnDrawGizmosSelected()
    {
        rhs = waypoint.rhsValue;
        g = waypoint.gValue;
        heuristic = waypoint.heuristic;
        plainHeuristic = waypoint.plainHeuristic;

        isInOpen = waypoint.isInOpen;
        isInClosed = waypoint.isInClosed;
        isInOpenAfterUpdate = waypoint.isInOpenAfterChange;
        isInIncons = waypoint.isInIncons;
        isChanged = waypoint.isChanged;
        pathCosts.Clear();
        foreach (float value in waypoint.publicPathCosts.Values)
            pathCosts.Add(value);


        if (waypoint.isInOpenAfterChange)
            Gizmos.color = Color.yellow;
        else if (waypoint.isInOpen)
            Gizmos.color = Color.magenta;
        else if (waypoint.isChanged)
            Gizmos.color = Color.blue;
        else if (waypoint.isInClosed)
            Gizmos.color = Color.gray;
        else if (waypoint.isInIncons)
            Gizmos.color = Color.cyan;

        else if (waypoint.isFree)
        {
            Gizmos.color = Color.green;
        }
        else if (!waypoint.isFree)
        {
            Gizmos.color = Color.red;
        }

        //if(waypoint.isChanged)
        //    Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));

        if (!showAllNodes)
        {
            if (waypoint.isInOpen || waypoint.isInClosed || waypoint.isInIncons || waypoint.isChanged)
                Gizmos.DrawWireCube(waypoint.publicGlobalPosition, new Vector3(1, 1, 1));
        }
        else
            Gizmos.DrawWireCube(waypoint.globalPosition, new Vector3(1, 1, 1));

        if (showDirectNeighbours)
        {
            foreach (Waypoint neighbour in waypoint.directNeighbours)
            {
                //if(waypoint.pathCosts[neighbour]<10)
                    Debug.DrawLine(waypoint.globalPosition, neighbour.globalPosition, Color.yellow);
                //else
                    //Debug.DrawLine(waypoint.globalPosition, neighbour.globalPosition, Color.red);
            }
        }
        if (showIndirectNeighbours)
        {
            foreach (Waypoint neighbour in waypoint.indirectNeighbours)
            {
                //if (waypoint.pathCosts[neighbour] < 10)
                    Debug.DrawLine(waypoint.globalPosition, neighbour.globalPosition, Color.yellow);
                //else
                //    Debug.DrawLine(waypoint.globalPosition, neighbour.globalPosition, Color.magenta);
            }
        }
        if (showDirectPredecessors)
        {
            foreach (Waypoint neighbour in waypoint.directPredecessors)
            {
                Debug.DrawLine(waypoint.publicGlobalPosition, neighbour.publicGlobalPosition, Color.red);
            }
        }
        if (showIndirectPredecessors)
        {
            foreach (Waypoint neighbour in waypoint.indirectPredecessors)
            {
                Debug.DrawLine(waypoint.publicGlobalPosition, neighbour.publicGlobalPosition, Color.red);
            }
        }

        if(showParent)
        {
            if(waypoint.parentNode!=null)
            {
                Debug.DrawLine(waypoint.publicGlobalPosition, waypoint.parentNode.publicGlobalPosition, Color.blue);
            }
        }
    }
}
