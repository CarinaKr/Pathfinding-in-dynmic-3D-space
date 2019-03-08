using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolMovement : MonoBehaviour {

    public Transform[] patrolPoints;
    public float moveSpeed;

    private Vector3[] patrolPositions;
    private int patrolCounter;

    // Use this for initialization
    void Start () {
        if (patrolPoints.Length == 0)
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
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.MoveTowards(transform.position, patrolPositions[patrolCounter], moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, patrolPositions[patrolCounter]) <= 0.5)
        {
            patrolCounter = (patrolCounter + 1) % patrolPositions.Length;
        }
    }
}
