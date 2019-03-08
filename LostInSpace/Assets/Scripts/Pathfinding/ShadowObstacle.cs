using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowObstacle : MonoBehaviour {

    virtual public List<Waypoint> waypointList { get; set; }

    public WaypointObstacle wpObstacle { get; set; }
}
