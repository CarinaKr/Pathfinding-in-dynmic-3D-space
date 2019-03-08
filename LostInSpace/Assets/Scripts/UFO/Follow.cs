﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour {

    public Transform target;
    public Transform centerPoint;

    private Vector3 offset;

	// Use this for initialization
	void Start () {
        offset = transform.position - target.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = target.position + offset;
        
        transform.rotation=Quaternion.LookRotation( transform.position-centerPoint.position);
		
	}
}
