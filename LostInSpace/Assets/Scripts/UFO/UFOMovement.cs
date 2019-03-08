using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOMovement : MonoBehaviour {

    public float moveSpeed;
    public float rotationSpeed;
    public Transform startPoint;

    public bool useTargetPoint;
    public bool startCentre;

    public float distanceTraveled { get; set; }
    private Vector3 lastPosition;

    public Vector3 targetPoint { private get; set; }
    public Vector3 input { private get; set; }
    private int radius;
    private float minY, maxY;

	// Use this for initialization
	void Start () {
        lastPosition = new Vector3();
        if(startPoint==null)
        {
            
                radius = CreateAsteroids.self.radius[0];
                minY = -1*CreateAsteroids.self.maxNegYOffset;
                maxY = CreateAsteroids.self.maxNegYOffset;
            
            
        }
	}
	
	// Update is called once per frame
	void Update () {

        lastPosition = transform.position;
		if(useTargetPoint)
        {
            if (targetPoint != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);
                transform.LookAt(targetPoint);
            }
        }
        else
        {
            transform.Rotate(Vector3.right, input.x, Space.Self);
            transform.Rotate(Vector3.up, input.y, Space.World);
            transform.Translate(transform.forward*moveSpeed*Time.deltaTime, Space.World);
            //Debug.DrawRay(transform.position, (transform.forward * moveSpeed).normalized*100 , Color.red, 10f);
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
            return;
        }

        if (startCentre)
        {
            transform.position = Vector3.zero;
        }
        else
        {
            float x = Random.Range(-radius, radius);
            float y = Random.Range(minY, maxY);
            float z = Random.Range(-radius, radius);

            Vector3 direction = new Vector3(x, y, z);

            direction.Normalize();

            transform.position = direction * (radius + (radius / 4));
        }
    }

}
