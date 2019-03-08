using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOCollisionHandler : MonoBehaviour {
    
    public string[] obstacleTags;
    public bool isPlayer;
    public float causeDamage;

    private UFOController ownController;
    private UFOHealth ownHealth;
    private IObstacleMovement currentMovement;
    private bool isCollided;

	// Use this for initialization
	void Start () {
        ownController = GetComponent<UFOController>();
        ownHealth = GetComponent<UFOHealth>();
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

                if(currentMovement!=null && ownHealth!=null)
                    ownHealth.health -= currentMovement.damage;
            }
        }
        

        if(!isPlayer)
        {
            if (other.transform.tag == "Player" )
            {
                other.transform.GetComponent<UFOHealth>().health -= causeDamage;
                ownController.Reset();
                ownHealth.Refill();
                //count success
                Debug.Log("success");
                FileHandler.self.WriteString("success\n\n");
            }

            if (other.gameObject.CompareTag("bullet"))
            {
                bool isRocket = other.gameObject.GetComponent<BulletMovement>().isRocket;
                Debug.Log("other");
                ownHealth.health -= other.gameObject.GetComponent<BulletMovement>().dmg;
                other.gameObject.GetComponent<BulletMovement>().isShot = false;
                if (isRocket)
                    PoolBehaviour.rocketPool.ReleaseObject(other.gameObject);
                else
                    PoolBehaviour.bulletPool.ReleaseObject(other.gameObject);
            }
        }

        
    }
    
}
