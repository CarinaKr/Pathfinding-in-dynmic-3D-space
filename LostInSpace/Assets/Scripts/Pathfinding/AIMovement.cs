using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour {

    public float moveSpeed;
    public float rotationSpeed;
    public Transform startPoint;

    public float distanceTraveled { get; set; }
    private Vector3 lastPosition;

    public Vector3 targetPoint { private get; set; }

    // Use this for initialization
    void Start()
    {
        lastPosition = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {

        lastPosition = transform.position;
        
        if (targetPoint != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);
            transform.LookAt(targetPoint);
        }
        
        distanceTraveled += Vector3.Distance(lastPosition, transform.position);
    }

    public void Reposition()
    {
        //Debug.Log("Reposition");
        distanceTraveled = 0;
        if (startPoint != null)
        {
            transform.position = startPoint.position;
        }
    }
}
