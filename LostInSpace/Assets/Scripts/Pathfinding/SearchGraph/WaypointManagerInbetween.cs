using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Priority_Queue;
using UnityEngine.Profiling;
using System.Diagnostics;

public class WaypointManagerInbetween : SearchGraphManager
{
    public GameObject searchSpace;
    public GameObject waypointPrefab;
    public int marginPercent;
    public float maxDistanceIndirectNeighbour;

    public bool initOnStart;
    public PathfindingHelper pathfindingHelper;
    public LayerMask obstacleLayer;

    private List<Waypoint> currentWaypointsList;
    private List<List<Waypoint>> edges;             //sort waypoints by edges, corners are listed in all adjacest edges <edge<waypoint>>
    private int[] triangles;           //int values point towards index in vertices
    private Vector3[] vertices, directions, allVertices, allNormals;
    private MeshCollider meshCollider;
    private Mesh mesh;
    private Vector3 directionFromCentre;
    private WaypointObstacle[] waypointObstacle;
    private float localPlayerRadius;
    private Waypoint[][] allWaypoints;      //array of all waypoints in the scene [obstacle_index][waypoint_index]

    private Collider[] obstacleHitCollider;
    private float marginedRadius;

    private bool[][] oldFree, newFree;
    private Bounds searchSpaceBounds;
    Dictionary<WaypointObstacle, float> wpObstaclesDistances;

    private bool changed;
    

    // Use this for initialization
    void Start()
    {
        updateTimer = new Stopwatch();
        wpObstaclesDistances = new Dictionary<WaypointObstacle, float>();
        searchSpaceBounds = searchSpace.GetComponent<Collider>().bounds;
        marginedRadius = pathfindingHelper.playerRadius + (pathfindingHelper.playerRadius * ((float)marginPercent) / 100);
        searchSpace.transform.localScale = new Vector3(searchSpace.transform.lossyScale.x - (marginedRadius*2), searchSpace.transform.lossyScale.y - (marginedRadius * 2), searchSpace.transform.lossyScale.z - (marginedRadius * 2));
        if (initOnStart)
            StartCoroutine("InstantiateWaypoints");
    }

    public IEnumerator InstantiateWaypoints()
    {
        yield return new WaitForSeconds(1f);
        waypointObstacle = GameObject.FindObjectsOfType<WaypointObstacle>();
        allWaypoints = new Waypoint[waypointObstacle.Length][];
        oldFree=new bool[waypointObstacle.Length][];
        newFree= new bool[waypointObstacle.Length][];

        for (int i = 0; i < waypointObstacle.Length; i++)
        {
            currentWaypointsList = new List<Waypoint>();
            edges = new List<List<Waypoint>>();

            //create corner waypoints
            mesh = waypointObstacle[i].mesh;                       //get mesh of current obstacle
            allVertices = mesh.vertices;                           //create List of all vertices of the current mesh
            vertices = allVertices.ToList().Distinct().ToArray();  //remove all duplicate elements
            directions = GetAllDirections(i);                      //get offset direction for all vertices

            CreateCornerWaypoints(waypointObstacle[i]);

            //get all neighbouring corners of corner waypoints
            triangles = mesh.triangles;
            foreach (Waypoint waypoint in currentWaypointsList)
            {
                List<Waypoint> neighbouringCorners = GetNeighbouringCorners(waypoint);
                waypoint.neighbouringCorner = neighbouringCorners;
            }

            //create inbetween waypoints
            CreateInbetweenWaypoints(waypointObstacle[i]);
            //int layerNum = Mathf.RoundToInt(Mathf.Log(pathfindingHelper.customObstacleLayer.value, 2));
            waypointObstacle[i].shadowSelfObject.gameObject.layer = Mathf.RoundToInt(Mathf.Log(pathfindingHelper.customObstacleLayer.value, 2));
            foreach (List<Waypoint> edge in edges)
            {
                foreach(Waypoint waypoint in edge)
                {
                    List<Waypoint> directNeighbours = GetDirectNeighbours(waypoint,edge);
                    waypoint.AddNeighbour(directNeighbours, false);
                }
            }
            waypointObstacle[i].shadowSelfObject.gameObject.layer = Mathf.RoundToInt(Mathf.Log(pathfindingHelper.defaultObstacleLayer.value, 2));

            waypointObstacle[i].waypointList = currentWaypointsList;    //set all assigned waypoints to waypointObstacle
            foreach (Waypoint wp in currentWaypointsList)
                wp.wpObstacle = waypointObstacle[i];
            allWaypoints[i] = currentWaypointsList.ToArray();           //fill array of all waypoints in the scene in regard to their objects
            oldFree[i] = new bool[allWaypoints[i].Length];
            newFree[i] = new bool[allWaypoints[i].Length];
        }

        //check which waypoints are blocked
        CheckOccupation();

        for (int l = 0; l < waypointObstacle.Length; l++)
        {
            obstacleHitCollider = Physics.OverlapBox(waypointObstacle[l].globalPosition, waypointObstacle[l].transform.lossyScale*2, waypointObstacle[l].transform.rotation, /*1 <<*/ obstacleLayer, QueryTriggerInteraction.Collide);
            for (int m = 0; m < allWaypoints[l].Length; m++)
            {
                if (allWaypoints[l][m].isFree)
                {
                    Waypoint indirectNeighbour = GetIndirectNeighbour(waypointObstacle[l], allWaypoints[l][m]);
                    //Waypoint indirectNeighbour = (Waypoint)GetClosesNodeViaPhysics(waypointObstacle[l], /*obstacleHitCollider,*/ allWaypoints[l][m]);
                    if (indirectNeighbour != null)
                    {
                        allWaypoints[l][m].AddNeighbour(indirectNeighbour, true);
                    }
                        
                }
            }
            yield return new WaitForEndOfFrame();
        }

        SetNodes();
        UpdateDirectPathCosts();
        UpdateIndirectPathCosts();
        yield return StartCoroutine("TakeScreenshot");
        base.GraphBuild();
    }

    private void CreateCornerWaypoints(WaypointObstacle obstacle)
    {
        //create waypoints at corners of obstacles 
        for (int j = 0; j < vertices.Length; j++)
        {
            Vector3 globalPoint = obstacle.transform.localToWorldMatrix.MultiplyPoint3x4(vertices[j]);               //get point of vertex in global position    
            float dist = Mathf.Sqrt(3 * Mathf.Pow(marginedRadius, 2));
            Vector3 globalPosition = globalPoint + directions[j].normalized * dist;      //and calculate location of waypoint in global position
            Vector3 localPosition = obstacle.transform.worldToLocalMatrix.MultiplyPoint3x4(globalPosition);          //then transform back to local position

            currentWaypointsList.Add(WaypointObject.Create(waypointPrefab, obstacle.transform, localPosition, vertices[j]));//add waypoint to list
        }
    }

    private void CreateInbetweenWaypoints(WaypointObstacle obstacle)
    {
        List<Waypoint> corners = new List<Waypoint>(currentWaypointsList);

        foreach (Waypoint startWP in corners)                   //
        {
            foreach (Waypoint neighbour in startWP.neighbouringCorner)         // for every neighbour of the start waypoint
            {
                if(!startWP.checkedBy.Contains(neighbour))                      //check if this connection has already been checked the other way around
                {
                    edges.Add(new List<Waypoint>());                //create a new edge
                    List<Waypoint> currentEdge = edges.Last();
                    currentEdge.Add(startWP);                       //add start point to edge

                    float distance = Vector3.Distance(startWP.globalPosition, neighbour.globalPosition);
                    if (distance > obstacle.maxWaypointDistance)     // and distance is big enough
                    {
                        int extraCount = Mathf.CeilToInt(distance / obstacle.maxWaypointDistance);
                        float stepDistance = distance / extraCount;
                        Vector3 direction = (neighbour.globalPosition - startWP.globalPosition).normalized;
                        for (int i = 0; i < extraCount-1; i++)
                        {
                            Vector3 globalPosition = startWP.globalPosition + (stepDistance*(i+1)) * direction;
                            Vector3 localPosition = obstacle.transform.worldToLocalMatrix.MultiplyPoint3x4(globalPosition);          // transform back to local position
                            Vector3 vertexPosition = startWP.localPosition + direction * (Vector3.Distance(startWP.localPosition, neighbour.localPosition) / extraCount);

                            currentWaypointsList.Add(WaypointObject.Create(waypointPrefab, obstacle.transform, localPosition, vertexPosition));   //add waypoint to list
                            currentEdge.Add(currentWaypointsList.Last());                                                                   //and add to list of edges
                        }
                        
                    }
                    neighbour.checkedBy.Add(startWP);
                    currentEdge.Add(neighbour);         //add end point to edge
                }
            }
        }

    }

    private Vector3[] GetAllDirections(int waypointObjectIndex)
    {
        Vector3[] directions = new Vector3[vertices.Length];
        allNormals = mesh.normals;

        for (int i = 0; i < allVertices.Length; i++)
        {
            for (int j = 0; j < vertices.Length; j++)
            {
                if (allVertices[i] == vertices[j])
                {
                    directions[j] += waypointObstacle[waypointObjectIndex].transform.TransformDirection(allNormals[i]);

                    //    //for debugging normals
                    //    Vector3 globalPoint = waypointObstacle[waypointObjectIndex].transform.localToWorldMatrix.MultiplyPoint3x4(vertices[j]);
                    //    Vector3 globalDirection = waypointObstacle[waypointObjectIndex].transform.TransformDirection(allNormals[i]);
                    //    Vector3 globalLocation = globalPoint + (globalDirection).normalized * 10;
                    //    Debug.DrawLine(globalPoint, globalLocation, Color.blue, 20f);
                    //
                }
            }
        }
        return directions;
    }

    private List<Waypoint> GetNeighbouringCorners(Waypoint waypoint)
    {
        List<Waypoint> neighbours = new List<Waypoint>();
        Vector3 vertice = waypoint.vertexPosition;

        for (int j = 0; j < triangles.Length; j++)                    //iterate through all triangles
        {
            if (allVertices[triangles[j]] == vertice)         //figure out which index the vertice has 
            {                                  //see at which posiiton of triplet of integers the index is
                int index = j % 3;
                switch (index)
                {
                    case 2:
                        if (j > 0) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j - 1]]));             //TODO: not all triangles are found
                        if (j > 1) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j - 2]]));             // check that they are set up completely
                        break;
                    case 1:
                        if (j > 0) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j - 1]]));
                        if (j < triangles.Length - 2) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j + 1]]));
                        break;
                    case 0:
                        if (j < triangles.Length - 2) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j + 1]]));
                        if (j < triangles.Length - 3) neighbours.Add(GetWaypointAtVertice(allVertices[triangles[j + 2]]));
                        break;
                }
            }
        }

        neighbours = neighbours.Distinct().ToList();
        return neighbours;
    }

    private List<Waypoint> GetDirectNeighbours(Waypoint waypoint,List<Waypoint>edge)
    {
        List<Waypoint> neighbours = new List<Waypoint>();
        

        int index = edge.IndexOf(waypoint);
        if(edge.Count>index+1)
        {
            Waypoint neighbour = edge.ElementAt(index + 1);         //get neighbours on same edge
            //if (pathfindingHelper.LineOfSight(neighbour,waypoint))
                neighbours.Add(neighbour);          
        }
        if (index > 0)
        {
            Waypoint neighbour = edge.ElementAt(index - 1);         //get neighbours on same edge
            //if (pathfindingHelper.LineOfSight(neighbour, waypoint))
                neighbours.Add(neighbour);
        }       
        if (index == 0 || index == edge.Count)                      //if corner don't get any other neighbours
            return neighbours;
        
        foreach(List<Waypoint> otherEdge in edges)                                   //get neighbours on other edge
        {
            Waypoint closestWaypoint = null;
            float minDistance = float.MaxValue;
            Waypoint start = edge.ElementAt(0);
            Waypoint end = edge.Last();
            Waypoint sharedCorner = Vector3.Distance(waypoint.globalPosition, start.globalPosition) < Vector3.Distance(waypoint.globalPosition, end.globalPosition) ? start : end;
            if (otherEdge!=edge && otherEdge.Contains(sharedCorner))         //don't look on own edge and only look ad adjacent edges
            {
                foreach(Waypoint wp in otherEdge)
                {
                    if (wp != waypoint /*&& wp!=start && wp!=end*/)                                 //don't count yourself
                    {
                        float distance = Vector3.Distance(wp.globalPosition, waypoint.globalPosition);  //measure distance of these waypoints
                        
                        if (minDistance > distance && pathfindingHelper.LineOfSight(wp,waypoint,false))           //if distance is smallest one, yet
                        {
                            minDistance = distance;
                            closestWaypoint = wp;                                                      //define this waypoint as direct neighbour
                        }
                    }
                }
            }
            if (closestWaypoint != null && closestWaypoint!=start&&closestWaypoint!=end)
                neighbours.Add(closestWaypoint);
        }
        return neighbours;
    }

    private Waypoint GetWaypointAtVertice(Vector3 vertice)
    {
        Waypoint waypoint = null;
        foreach (Waypoint point in currentWaypointsList)
        {
            if (point.vertexPosition == vertice)
            {
                return point;
            }
        }

        return waypoint;
    }
    
    public Waypoint GetIndirectNeighbour(WaypointObstacle obstacle, Waypoint waypoint)
    {
        Vector3 vertice = waypoint.vertexPosition;
        float minDistance = float.MaxValue;
        float distance = 0;

        int lineOfSightChecks = 0;
        
        Waypoint closestWaypoint = null;


        SetWPObstacleDistances(obstacle);
        while (wpObstaclesDistances.Count != 0)
        {
            int index = GetClosestObstacleIndex();
            for (int j = 0; j < allWaypoints[index].Length; j++)
            {
                if (allWaypoints[index][j].isFree && waypointObstacle[index].shadowIsActive )                         
                {
                    distance = Vector3.Distance(waypoint.globalPosition, allWaypoints[index][j].globalPosition);  //measure distance of these waypoints
                    if (distance < minDistance)
                    {
                        lineOfSightChecks++;
                        if (pathfindingHelper.LineOfSight(waypoint, allWaypoints[index][j],true))           //if there is a line of sight between waypoints
                        {
                            minDistance = distance;
                            closestWaypoint = allWaypoints[index][j];                                                      //define this waypoint as indirect neighbour
                        }
                    }
                }
            }
        }
        return closestWaypoint;
    }

    public void CheckOccupation()
    {
        for (int i = 0; i < waypointObstacle.Length; i++)
        {
            for (int j = 0; j < allWaypoints[i].Length; j++)
            {
                obstacleHitCollider = Physics.OverlapSphere(allWaypoints[i][j].globalPosition, pathfindingHelper.playerRadius, obstacleLayer, QueryTriggerInteraction.Collide);

                oldFree[i][j] = allWaypoints[i][j].isFree;
                if (obstacleHitCollider.Length > 0 || !searchSpaceBounds.Contains(allWaypoints[i][j].globalPosition) || !waypointObstacle[i].shadowIsActive)
                {
                    allWaypoints[i][j].isFree = false;
                }
                else
                {
                    allWaypoints[i][j].isFree = true;
                }
                newFree[i][j] = allWaypoints[i][j].isFree;
            }
        }
    }

    public void UpdateDirectPathCosts()
    {
        for (int l = 0; l < waypointObstacle.Length; l++)
        {
            for (int m = 0; m < allWaypoints[l].Length; m++)
            {
                UpdateDirectPathCosts(waypointObstacle[l],allWaypoints[l][m]);
            }
        }

    }
    public bool UpdateDirectPathCosts(WaypointObstacle obstacle,Waypoint wp)
    {
        changed = false;
        float oldDistance = 0;
        foreach (Waypoint neighbour in wp.directNeighbours)
        {
            oldDistance = wp.pathCosts[neighbour];
           
            if (wp.isFree && neighbour.isFree && pathfindingHelper.LineOfSight(wp,neighbour,true) && obstacle.shadowIsActive)
            {
                wp.pathCosts[neighbour] = Vector3.Distance(wp.globalPosition, neighbour.globalPosition);
            }
            else
            {
                wp.pathCosts[neighbour] = Mathf.Infinity;
            }

            if (oldDistance != wp.pathCosts[neighbour])
            {
                changed = true;
            }
        }
        return changed;
    }

    public void UpdateIndirectPathCosts()
    {
        for (int l = 0; l < waypointObstacle.Length; l++)
        {
            for (int m = 0; m < allWaypoints[l].Length; m++)
            {
                UpdateIndirectPathCosts(allWaypoints[l][m]);
            }
        }
    }
    public void UpdateIndirectPathCosts(Waypoint wp)
    {
        foreach(Waypoint neighbour in wp.indirectNeighbours)
        {
            if (neighbour.isFree && wp.isFree)
            {
                wp.pathCosts[neighbour] = Vector3.Distance(wp.globalPosition, neighbour.globalPosition); ;
            }
            else
            {
                wp.pathCosts[neighbour] = Mathf.Infinity;
            }
        }
    }

    public bool UpdateIndirectNeighbours(WaypointObstacle obstacle, Waypoint wp, out Node oldNeighbour)
    {
        float oldIndirectPathCost = 0;
        changed = false;
        oldNeighbour = null;
        if(wp.indirectNeighbours.Count>0)
            oldNeighbour = wp.indirectNeighbours[0];
        Waypoint newIndirectNeighbour = (Waypoint)GetClosesNodeViaPhysics(obstacle, wp);                   //for simplicity let's assume ever node only has one indirect neighour

        if (newIndirectNeighbour == null)                           //waypoint had indirect neighbour before, but now doesn't
        {
            if (wp.indirectNeighbours.Count == 0) changed = false;
            else
            {
                changed = true;
                foreach (Node node in wp.indirectNeighbours)    //remove path costs of all indirect neighbours
                {
                    wp.pathCosts.Remove(node);
                }
                wp.indirectNeighbours.Clear();
            }
            return changed;
        }
        else if(wp.indirectNeighbours.Count==0 && newIndirectNeighbour!=null)                      //waypoint didn't have indirect neighbour before, but now does
        {
            wp.AddNeighbour(newIndirectNeighbour, true);
            changed = true;
        }
        else if (newIndirectNeighbour != null && newIndirectNeighbour!=wp.indirectNeighbours[0])     //indirect neighbour has changed
        {
            changed = true;
            foreach (Node node in wp.indirectNeighbours)    //remove path costs of all indirect neighbours
            {
                wp.pathCosts.Remove(node);
            }
            wp.indirectNeighbours.Clear();
            wp.AddNeighbour(newIndirectNeighbour, true);
            
        }
        else if(newIndirectNeighbour == wp.indirectNeighbours[0])                           //indirect neigbour stayed the same
        {
            wp.indirectNeighbours[0].ToWaypoint().indirectPredecessors.Add(wp);
            oldIndirectPathCost = wp.pathCosts[wp.indirectNeighbours[0]];
            float newindirectPathCost= Vector3.Distance(wp.globalPosition, newIndirectNeighbour.globalPosition);
            if (oldIndirectPathCost !=newindirectPathCost)
                changed = true;
        }
        return changed;
    }
   

    private void SetNodes()
    {
        nodes = new List<Node>();
        for (int l = 0; l < waypointObstacle.Length; l++)
        {
            for (int m = 0; m < allWaypoints[l].Length; m++)
            {
                nodes.Add(allWaypoints[l][m]);
            }
        }
    }

    private void UpdateGlobalPosition()
    {
        for (int i = 0; i < waypointObstacle.Length; i++)
        {
            if (waypointObstacle[i].isDynamic)
                waypointObstacle[i].SetShadow();

            for (int j = 0; j < allWaypoints[i].Length; j++)
            {
                allWaypoints[i][j].globalPosition = allWaypoints[i][j].waypointObject.transform.position;    
                allWaypoints[i][j].localPosition= allWaypoints[i][j].waypointObject.transform.localPosition;
            }
        }
    }

    public override IEnumerator UpdateSearch()
    {
        updateTimer.Restart();
        List<Node> changedNodes = new List<Node>();
        List<Node> directPathCostChanged = new List<Node>();
        List<Node> indirectNeighboursChanged = new List<Node>();
        HashSet<Node> changed = new HashSet<Node>();
        
        UpdateGlobalPosition();
        
        CheckOccupation();

        
        int pathCostCounter = 0;
        for (int i = 0; i < waypointObstacle.Length; i++)
        {
            for (int j = 0; j < allWaypoints[i].Length; j++)
            {
                pathCostCounter++;
                allWaypoints[i][j].indirectPredecessors.Clear();    //clear all indirect predecessors, since indirect neighbours will be reassigned   
                if (UpdateDirectPathCosts(waypointObstacle[i],allWaypoints[i][j]))
                {
                    directPathCostChanged.Add(allWaypoints[i][j]);
                }
            }
        }
        changedNodes.AddRange(directPathCostChanged);

        yield return new WaitForUpdate();
        
        //update indirect neighbours
        int indirectCounter = 0;
        int counter = 0;
        Node oldIndirectNeighbour;
        for (int i = 0; i < waypointObstacle.Length; i++)
        {
            for (int j = 0; j < allWaypoints[i].Length; j++)
            {
                indirectCounter++;
                Vector3 checkInBox = waypointObstacle[i].transform.lossyScale + new Vector3(maxDistanceIndirectNeighbour, maxDistanceIndirectNeighbour, maxDistanceIndirectNeighbour);
                obstacleHitCollider = Physics.OverlapBox(waypointObstacle[i].globalPosition, checkInBox, waypointObstacle[i].transform.rotation, /*1 << */obstacleLayer, QueryTriggerInteraction.Collide);
                if (UpdateIndirectNeighbours(waypointObstacle[i],allWaypoints[i][j],out oldIndirectNeighbour))       //set new indirect neighbours for all nodes
                {
                    indirectNeighboursChanged.Add(allWaypoints[i][j]);
                    if(oldIndirectNeighbour!=null)
                        changedNodes.Add(oldIndirectNeighbour);         //inform old neighbour that its predecessor might have changed
                }
            }
            counter++;
            if (counter >= 5)
            {
                //yield return new WaitForEndOfFrame();
                yield return null;
                counter = 0;
            }
        }
        changedNodes.AddRange(indirectNeighboursChanged);

        //update path costs for changed indirect neighbours
        changed.Clear();
        foreach (Node node in indirectNeighboursChanged)      //update indirect path costs of all nodes that have a new indirect neighbour or a different path distance to their old neighbour
        {
            Waypoint wp = (Waypoint)node;
            if (!changed.Contains(node))          //make sure to check each node only once
            {
                UpdateIndirectPathCosts((Waypoint)node);
                changed.Add(node);             
            }
            foreach (Node succ in wp.indirectNeighbours)  //inform neighbours that they have a new predecessor
            {
                if (!changed.Contains(succ))
                {
                    //UpdateIndirectPathCosts((Waypoint)succ);
                    changed.Add(succ);
                    changedNodes.Add(succ);
                }
            }
        }

        changedNodes =changedNodes.Distinct().ToList();
        yield return StartCoroutine("TakeScreenshotPart", changedNodes);
        DetectedChanges(changedNodes);
        updateTimer.Stop();
    }

    public override Node GetStartNode()
    {
        return GetClosestNode(SearchGraphSelector.self.startPosition);
    }
    public override Node GetGoalNode()
    {
        return GetClosestNode(SearchGraphSelector.self.goalPosition);
    }
    public override Node GetClosestNode(Vector3 position)
    {
        float minDistance = float.MaxValue;
        float distance = 0;
        
        Waypoint closestWaypoint = null;

        for(int i=0;i<waypointObstacle.Length;i++)
        {
            for (int j = 0; j < allWaypoints[i].Length; j++)
            {
                if (allWaypoints[i][j].isFree && waypointObstacle[i].shadowIsActive)                         
                {
                    distance = Vector3.Distance(position, allWaypoints[/*index*/i][j].globalPosition);  //measure distance of these waypoints
                    if (distance < minDistance)
                    {
                        //lineOfSightChecks++;
                        //if (pathfindingHelper.LineOfSight(position, allWaypoints[index][j],true))     //for accuracy there should be a line of sight check
                        //{                                                                             //to avoid physics the line-of-sight check is neglegted
                            minDistance = distance;                                                     //this might cause the algorithm to find the wrong goal node
                            closestWaypoint = allWaypoints[/*index*/i][j];                                                      //define this waypoint as indirect neighbour
                        //}
                    }
                }
            }
        }

        return (Node)closestWaypoint;
    }

    private Node GetClosesNodeViaPhysics(WaypointObstacle obstacle, Waypoint waypoint)
    {
        if (!waypoint.isFree || !obstacle.shadowIsActive) return null;
        int lineOfSightChecks = 0;
        Node closestNode = null;
        float minDistance = float.MaxValue;

        foreach (Collider collider in obstacleHitCollider)
        {
            ShadowObstacle waypointObstacle = collider.GetComponent<WaypointObstacle>();
            if (waypointObstacle == null) waypointObstacle = collider.GetComponent<ShadowObstacle>();
            if (waypointObstacle != obstacle && waypointObstacle.wpObstacle!=obstacle)
            {
                foreach (Waypoint wp in waypointObstacle.waypointList)
                {
                    float distance = Vector3.Distance(wp.globalPosition, waypoint.globalPosition);
                    if (distance < minDistance && wp.isFree)
                    {
                        lineOfSightChecks++;
                        if (pathfindingHelper.LineOfSight(wp, waypoint,true))
                        {
                            minDistance = distance;
                            closestNode = wp;
                        }
                    }
                }
            }
        }
        return closestNode;
    }

    private void SetWPObstacleDistances(WaypointObstacle obstacle)
    {
        wpObstaclesDistances.Clear();
        foreach(WaypointObstacle wpo in waypointObstacle)
        {
            if(wpo!=obstacle)
            {
                wpObstaclesDistances.Add(wpo, Vector3.Distance(obstacle.transform.position, wpo.globalPosition));
            }
        }
    }
    private void SetWPObstacleDistances(Vector3 position)
    {
        wpObstaclesDistances.Clear();
        foreach (WaypointObstacle wpo in waypointObstacle)
        {
            wpObstaclesDistances.Add(wpo, Vector3.Distance(position, wpo.globalPosition));
        }
    }

    private int GetClosestObstacleIndex()
    {
        WaypointObstacle closestObstacle=null;
        int closestObstacleIndex = 0 ;
        float minDistance = float.MaxValue;
        foreach(KeyValuePair<WaypointObstacle,float> wpo in wpObstaclesDistances)
        {
            if(wpo.Value<minDistance)
            {
                minDistance = wpo.Value;
                closestObstacle = wpo.Key;
            }
        }
        wpObstaclesDistances.Remove(closestObstacle);
        for(int i=0;i<waypointObstacle.Length;i++)
        {
            if(waypointObstacle[i]==closestObstacle)
            {
                closestObstacleIndex = i;
                return closestObstacleIndex;
            }
        }

        return -1;
    }

}
