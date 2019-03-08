//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Linq;

//public class WaypointManager : SearchGraphManager
//{

//    //public GameObject obj;
//    public GameObject waypointPrefab;
//    public float playerRadius;
//    public int marginPercent;
    
//    public bool initOnStart;

//    private List<Waypoint> currentWaypointsList;
//    //private List<Waypoint> neighbouringCorner;
//    //private List<Waypoint> directNeighbours;
//    private int[] triangles;           //int values point towards index in vertices
//    private Vector3[] vertices, directions, allVertices,allNormals ;
//    private MeshCollider meshCollider;
//    private Mesh mesh;
//    private Vector3 directionFromCentre;
//    private WaypointObstacle[] waypointObstacle;
//    private float localPlayerRadius;
//    private Waypoint[][] allWaypoints;      //array of all waypoints in the scene [obstacle_index][waypoint_index]

//    private Collider[] hitCollider;
//    private int layer;
//    private float marginedRadius;

//    // Use this for initialization
//    void Start () {
//        layer = LayerMask.NameToLayer("Obstacle");
//        marginedRadius = playerRadius + (playerRadius * ( (float)marginPercent)/100);
//        if (initOnStart)
//            InstantiateWaypoints();
//	}
	
//	public void InstantiateWaypoints()
//    {
//        float startTime = Time.realtimeSinceStartup;

//        waypointObstacle = GameObject.FindObjectsOfType<WaypointObstacle>();
//        allWaypoints = new Waypoint[waypointObstacle.Length][];

//        for (int i=0;i<waypointObstacle.Length;i++)
//        {
//            currentWaypointsList = new List<Waypoint>();

//            //set up all waypoints
//            mesh = waypointObstacle[i].mesh;                       //get mesh of current obstacle
//            allVertices = mesh.vertices;                           //create List of all vertices of the current mesh
//            vertices = allVertices.ToList().Distinct().ToArray();  //remove all duplicate elements
//            directions = GetAllDirections(i);                      //get offset direction for all vertices

//            CreateCornerWaypoints(waypointObstacle[i]);
            
//            //get all neighbouring corners of all waypoints
//            triangles = mesh.triangles;
//            foreach (Waypoint waypoint in currentWaypointsList)
//            {
//                List<Waypoint> directNeighbours = GetNeighbours(waypoint);
//                waypoint.directNeighbours = directNeighbours;
//            }

//            //CreateInbetweenWaypoints(waypointObstacle[i]);
            
//            waypointObstacle[i].waypointList = currentWaypointsList;    //set all assigned waypoints to waypointObstacle
//            allWaypoints[i] = currentWaypointsList.ToArray();           //fill array of all waypoints in the scene in regard to their objects
//        }

//        //create connections between different game objects
//        CheckOccupation();
//        for (int l = 0; l < waypointObstacle.Length; l++)
//        {
//            for (int m = 0; m < allWaypoints[l].Length; m++)
//            {
//                if (allWaypoints[l][m].isFree)
//                {
//                    Waypoint distantNeighbour = GetDistantNeighbour(waypointObstacle[l], allWaypoints[l][m]);
//                    if(distantNeighbour!=null)
//                        allWaypoints[l][m].AddNeighbour(distantNeighbour,true);
//                }  
//            }
//        }

//        float endTime = Time.realtimeSinceStartup;
//        float deltaTime = endTime - startTime;
//        Debug.Log("time: " + deltaTime);
//    }
    
//    private void CreateCornerWaypoints(WaypointObstacle obstacle)
//    {
//        //create waypoints at corners of obstacles 
//        for (int j = 0; j < vertices.Length; j++)
//        {
//            Vector3 globalPoint = obstacle.transform.localToWorldMatrix.MultiplyPoint3x4(vertices[j]);               //get point of vertex in global position              
//            float dist = Mathf.Sqrt(3 * Mathf.Pow(marginedRadius, 2));
//            Vector3 globalPosition = globalPoint + directions[j].normalized * dist;      //and calculate location of waypoint in global position
//            Vector3 localPosition = obstacle.transform.worldToLocalMatrix.MultiplyPoint3x4(globalPosition);          //then transform back to local position

//            currentWaypointsList.Add(Waypoint.Create(waypointPrefab, obstacle.transform, localPosition, vertices[j]));//add waypoint to list
//        }
//    }

//    private void CreateInbetweenWaypoints(WaypointObstacle obstacle)
//    {
//        List<Waypoint> corners = currentWaypointsList;

//        foreach(Waypoint startWP in corners)                   //
//        {
//            foreach(Waypoint neighbour in startWP.neighbouringCorner)         // for every neighbour of the start waypoint
//            {
//                float distance = Vector3.Distance(startWP.globalPosition, neighbour.globalPosition);
//                if (!neighbour.checkedBy.Contains(startWP)                                                                   //check if this connection has already been checked the other way around
//                    && distance >obstacle.maxWaypointDistance)     // and distance is big enough
//                {
//                    int extraCount = Mathf.CeilToInt(distance/obstacle.maxWaypointDistance);
//                    float stepDistance = distance / extraCount;
//                    Vector3 direction = (neighbour.globalPosition - startWP.globalPosition).normalized;
//                    for(int i=0;i<extraCount;i++)
//                    {
//                        Vector3 globalPosition = startWP.globalPosition + stepDistance * direction;
//                        Vector3 localPosition = obstacle.transform.worldToLocalMatrix.MultiplyPoint3x4(globalPosition);          // transform back to local position
//                        Vector3 vertexPosition = startWP.localPosition+direction*(Vector3.Distance(startWP.localPosition,neighbour.localPosition)/extraCount);

//                        currentWaypointsList.Add(Waypoint.Create(waypointPrefab, obstacle.transform, localPosition, vertexPosition));//add waypoint to list
//                    }

//                    neighbour.checkedBy.Add(startWP);
//                }
//            }
//        }
        
//    }
    
//    private Vector3[] GetAllDirections(int waypointObjectIndex)
//    {
//        Vector3[] directions = new Vector3[vertices.Length];
//        allNormals = mesh.normals;

//        for (int i=0;i<allVertices.Length;i++)
//        {
//            for(int j=0;j<vertices.Length;j++)
//            {
//                if (allVertices[i] == vertices[j])
//                {
//                    directions[j] += waypointObstacle[waypointObjectIndex].transform.TransformDirection(allNormals[i]);

//                    //    //for debugging normals
//                    //    Vector3 globalPoint = waypointObstacle[waypointObjectIndex].transform.localToWorldMatrix.MultiplyPoint3x4(vertices[j]);
//                    //    Vector3 globalDirection = waypointObstacle[waypointObjectIndex].transform.TransformDirection(allNormals[i]);
//                    //    Vector3 globalLocation = globalPoint + (globalDirection).normalized * 10;
//                    //    Debug.DrawLine(globalPoint, globalLocation, Color.blue, 20f);
//                    //
//                }
//            }
//        }
//        return directions;
//    }

//    private List<Waypoint> GetNeighbours(Waypoint waypoint)
//    {
//        List<Waypoint> neighbours = new List<Waypoint>();
//        Vector3 vertice = waypoint.vertexPosition;

//        for (int j = 0; j < triangles.Length; j++)                    //iterate through all triangles
//        {
//            if (allVertices[triangles[j]] == vertice)         //figure out which index the vertice has 
//            {                                  //see at which posiiton of triplet of integers the index is
//                int index = j % 3;
//                switch (index)
//                {
//                    case 2:
//                        if (j > 0) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j - 1]]));             //TODO: not all triangles are found
//                        if (j > 1) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j - 2]]));             // check that they are set up completely
//                        break;
//                    case 1:
//                        if (j > 0) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j - 1]]));
//                        if (j < triangles.Length - 2) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j + 1]]));
//                        break;
//                    case 0:
//                        if (j < triangles.Length - 2) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j + 1]]));
//                        if (j < triangles.Length - 3) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j + 2]]));
//                        break;
//                }
//            }
//        }

//        neighbours = neighbours.Distinct().ToList();
//        return neighbours;
//    }

//    private Waypoint GetWaypointAtVertice(Vector3 vertice)
//    {
//        Waypoint waypoint=null;
//        foreach(Waypoint point in currentWaypointsList)
//        {
//            if(point.vertexPosition==vertice)
//            {
//                return point;
//            }
//        }

//        return waypoint;
//    }

//    private bool LineOfSight(Waypoint from, Waypoint to)
//    {
//        Vector3 direction = (to.globalPosition - from.globalPosition);
//        RaycastHit hit;
//        // Does the ray intersect any objects excluding the player layer
//        bool inSight=!Physics.SphereCast(from.globalPosition,playerRadius, direction ,out hit, direction.magnitude,1<<layer,QueryTriggerInteraction.Collide);
//        ////for debuggin
//        //if(inSight)
//        //    Debug.DrawRay(from.globalPosition, direction.normalized*direction.magnitude,Color.green,10f);
//        //else
//        //    Debug.DrawRay(from.globalPosition, direction.normalized*direction.magnitude, Color.red, 10f);
//        return inSight;
//    }
    
//    public Waypoint GetDistantNeighbour(WaypointObstacle obstacle,Waypoint waypoint)
//    {
//        Vector3 vertice = waypoint.vertexPosition;

//        float minDistance = float.MaxValue;
//        Waypoint closestWaypoint = null;
//        for (int i = 0; i < allWaypoints.Length; i++)       //go through all obstacles
//        {   
//            if (waypointObstacle[i] != obstacle)            //except own
//            {
//                for (int j = 0; j < allWaypoints[i].Length; j++)
//                {
//                    if (LineOfSight(waypoint, allWaypoints[i][j]) && allWaypoints[i][j].isFree)                         //if there is a line of sight between waypoints
//                    {
//                        float distance = Vector3.Distance(waypoint.globalPosition, allWaypoints[i][j].globalPosition);  //measure distance of these waypoints
//                        if(minDistance>distance)                                                                        //if distance is smallest one, yet
//                        {
//                            minDistance = distance;                                                                    
//                            closestWaypoint = allWaypoints[i][j];                                                      //define this waypoint as indirect neighbour
//                        }
//                    }
//                }
//            }
//        }

//        return closestWaypoint;
//    }

//    public void CheckOccupation()
//    {
//        for (int i=0;i<allWaypoints.Length;i++)
//        {
//            for(int j=0;j<allWaypoints[i].Length;j++)
//            {
//                hitCollider = Physics.OverlapSphere(allWaypoints[i][j].globalPosition, playerRadius, 1 << layer, QueryTriggerInteraction.Collide);
 
//                allWaypoints[i][j].isFree = hitCollider.Length > 0 ? false : true;
//            }
//        }
//    }
//}
