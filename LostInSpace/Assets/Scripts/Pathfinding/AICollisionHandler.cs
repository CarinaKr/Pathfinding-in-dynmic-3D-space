using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICollisionHandler : MonoBehaviour {

    public string[] obstacleTags;

    private AIController ownController;
    private IObstacleMovement currentMovement;
    private bool isCollided;

    // Use this for initialization
    void Start()
    {
        ownController = GetComponent<AIController>();
    }

    void Update()
    {
        isCollided = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < obstacleTags.Length; i++)
        {
            if (other.transform.tag == obstacleTags[i])
            {
                currentMovement = other.transform.GetComponentInParent<IObstacleMovement>();

                if (isCollided || currentMovement.tagged)
                {
                    return;
                }
                isCollided = true;
                currentMovement.tagged = true;

                ownController.Reset();
            }
        }


        if (other.transform.tag == "Player")
            {
                ownController.Reset();
                //count success
                Debug.Log("success");
                FileHandler.self.WriteString("success\n\n");
            }


    }
}
