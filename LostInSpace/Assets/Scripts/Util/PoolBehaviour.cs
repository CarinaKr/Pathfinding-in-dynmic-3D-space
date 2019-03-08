using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolBehaviour : MonoBehaviour {

    public static PoolManager bulletPool;
    public static PoolManager rocketPool;
    public GameObject bulletObj;
    public int bulletPoolSize;
    public GameObject rocketObj;
    public int rocketPoolSize;

    // Use this for initialization
    void Awake ()
    {
        bulletPool = new PoolManager(bulletPoolSize, bulletObj);
        rocketPool = new PoolManager(rocketPoolSize, rocketObj);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
