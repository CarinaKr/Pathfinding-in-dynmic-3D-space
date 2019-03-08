using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidMovement : IObstacleMovement {

    //public Transform center;
    
    public bool randMovement;
    
    private CreateAsteroids asteroidManager;
    

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        
        asteroidManager = CreateAsteroids.self;
        ufo = asteroidManager.ufo;
        minY = -1*asteroidManager.ySpread;
        maxY = asteroidManager.ySpread;
        SetMovement();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        //transform.position = Vector3.MoveTowards(transform.position, goal, moveSpeed * Time.deltaTime);
        float xzDistance = Vector3.Distance(new Vector3(transform.position.x,0,transform.position.z),new Vector3( ufo.transform.position.x,0,ufo.transform.position.z));
        float yDistance = Mathf.Abs(transform.position.y - (ufo.transform.position.y + yOffset)) ;
        if (xzDistance > radius || (yDistance>maxY+5))
        {
            Reposition();
        }
    }

    public void Reposition()
    {
        rb.velocity = new Vector3(0, 0, 0);
        Vector3 direction = new Vector3(Random.Range(-radius, radius),0,
                                                         Random.Range(-radius, radius));
        direction.Normalize();
        Vector3 newPosition = new Vector3(direction.x * radius, Random.Range(minY, maxY) + yOffset, direction.z * radius);
        //transform.position = ufo.position + (direction * radius);
        transform.position = ufo.position + newPosition;
        //transform.position = newPosition;
        SetMovement();
        isTagged = false;

    }

    public void SetMovement()
    {
        transform.rotation = Quaternion.LookRotation(ufo.position - transform.position);

        if (randMovement)
        {

            transform.Rotate(/*Random.Range(-45,45)*/0,Random.Range(-75,75),/*Random.Range(-45,45),Space.Self*/0);
            
            //Debug.DrawRay(transform.position, transform.forward * radius, Color.red, 2);
            //Debug.DrawRay(transform.position,ufo.position-transform.position, Color.blue, 2);
        }
        rb.AddForce(transform.forward * moveSpeed);
    }

    

    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.tag == "Player")
    //    {
    //        Reposition();
    //    }
    //}
}
