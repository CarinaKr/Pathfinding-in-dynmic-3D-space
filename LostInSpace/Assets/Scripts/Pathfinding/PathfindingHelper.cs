using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingHelper : ScriptableObject {

    public float playerRadius;

    public LayerMask defaultObstacleLayer,customObstacleLayer;

    private bool inSight;

    public bool LineOfSight(Node from, Node to,bool onDefaultObstacleLayer)
    {
        LayerMask currentObstacleLayer = onDefaultObstacleLayer ? defaultObstacleLayer : customObstacleLayer;

        inSight = false;
        try
        {
            Vector3 direction = (to.globalPosition - from.globalPosition);
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            bool inSight = !Physics.Raycast(from.globalPosition, direction, out hit, direction.magnitude, currentObstacleLayer, QueryTriggerInteraction.Collide);
            if (inSight)
            {
                inSight = !Physics.SphereCast(from.globalPosition, playerRadius, direction, out hit, direction.magnitude, /*1 <<*/ currentObstacleLayer, QueryTriggerInteraction.Collide);
            }
            return inSight;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    public bool LineOfSight(Vector3 from, Node to, bool onDefaultObstacleLayer)
    {
        LayerMask currentObstacleLayer = onDefaultObstacleLayer ? defaultObstacleLayer : customObstacleLayer;
        inSight = false;
        try
        {
            Vector3 direction = (to.globalPosition - from);
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer

            bool inSight = !Physics.Raycast(from, direction, out hit, direction.magnitude, currentObstacleLayer, QueryTriggerInteraction.Collide);
            if (inSight)
            {
                inSight = !Physics.SphereCast(from, playerRadius, direction, out hit, direction.magnitude, /*1 <<*/ currentObstacleLayer, QueryTriggerInteraction.Collide);
            }
            return inSight;
        }
        catch (System.Exception)
        {

            return false;
        }
    }

    public bool LineOfSight(Vector3 from, Vector3 to, bool onDefaultObstacleLayer)
    {
        LayerMask currentObstacleLayer = onDefaultObstacleLayer ? defaultObstacleLayer : customObstacleLayer;
        inSight = false;
        try
        {
            Vector3 direction = (to - from);
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            bool inSight = !Physics.Raycast(from, direction, out hit, direction.magnitude, currentObstacleLayer, QueryTriggerInteraction.Collide);
            if (inSight)
            {
                inSight = !Physics.SphereCast(from, playerRadius, direction, out hit, direction.magnitude, /*1 <<*/ currentObstacleLayer, QueryTriggerInteraction.Collide);
            }
            return inSight;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
}
