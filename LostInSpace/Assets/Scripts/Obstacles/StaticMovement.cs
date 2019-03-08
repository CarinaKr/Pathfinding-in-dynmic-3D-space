using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMovement : MonoBehaviour {

    public bool usePatrolPoints;
    public Transform[] patrolPoints;
    public float moveSpeed, turnSpeed;
    public Vector3 turnAround;
    public bool forward, right, up;
    public bool temporaryBlock;
    public float switchTime;
    public bool startActive;
    public Transform AI;
    public Vector3 boxMultiply;

    private Vector3[] patrolPositions;
    private int patrolCounter;
    private float switchTimeCounter;
    private bool isActive;
    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;
    private WaypointObstacle wpObstacle;
    private Transform largerCollider;
    private Bounds box;
    
    private void Start()
    {
        
        wpObstacle = GetComponent<WaypointObstacle>();
        wpObstacle.isDynamic = true;
        if (SearchGraphSelector.self.searchGraphManager is WaypointManagerInbetween)
        {
            largerCollider = transform.Find("largerCollider");
            if(largerCollider)largerCollider.gameObject.SetActive(false);
            wpObstacle.MakeShadowCopy();
            
        }
            

        if (forward)
            turnAround = transform.forward;
        else if (right)
            turnAround = transform.right;
        else if (up)
            turnAround = transform.up;

        if(usePatrolPoints )
        {
            if (patrolPoints.Length==0)
            {
                //get patrol points from children
                patrolPoints = GetComponentsInChildren<Transform>();
            }
            //save position of children in extra array since they will move along with the parent
            patrolPositions = new Vector3[patrolPoints.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                patrolPositions[i] = patrolPoints[i].position;
            }
        }

        if(temporaryBlock)
        {
            boxCollider = GetComponent<BoxCollider>();
            box = boxCollider.bounds;
            box.extents = new Vector3(box.extents.x* boxMultiply.x, box.extents.y*boxMultiply.y,box.extents.z*boxMultiply.z);
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            isActive = !startActive;
            ToggleActive();
        }
    }


    // Update is called once per frame
    void Update () {
		if(usePatrolPoints)
        {
            transform.position = Vector3.MoveTowards(transform.position, patrolPositions[patrolCounter], moveSpeed * Time.deltaTime);
            if(Vector3.Distance(transform.position, patrolPositions[patrolCounter])<=0.5)
            {
                patrolCounter = (patrolCounter + 1) % patrolPositions.Length;
            }
        }

        if(turnSpeed!=0)
        {
            transform.Rotate(turnAround, turnSpeed * Time.deltaTime);
        }

        if(temporaryBlock)
        {
            switchTimeCounter += Time.deltaTime;
            if(switchTime>0 && switchTimeCounter>=switchTime)
            {
                if(!box.Contains(AI.position))
                {
                    ToggleActive();
                    switchTimeCounter = 0;
                }
                
            }
        }
	}

    public void ToggleActive()
    {
        if(isActive)
        {
            wpObstacle.isActive = false;
            boxCollider.enabled = false;
            meshRenderer.enabled = false;
            isActive = false;
        }
        else
        {
            wpObstacle.isActive = true;
            boxCollider.enabled = true;
            meshRenderer.enabled = true;
            isActive = true;
        }
    }

    
}
