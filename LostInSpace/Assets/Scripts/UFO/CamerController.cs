using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerController : MonoBehaviour {

    public float rotationSpeed;
    public Quaternion parentRotation;
    private Quaternion startRotation;

	// Use this for initialization
	void Start () {
        //originalRotation = transform.localRotation;
        startRotation = transform.localRotation;
	}
	
	// Update is called once per frame
	void Update () {
        //read input
        float rotationX = Input.GetAxis("RotationX");
        float rotationY = Input.GetAxis("RotationY") ;

        if(rotationX!=0||rotationY!=0)
        {
            transform.Rotate(Vector3.left, rotationY * rotationSpeed * Time.deltaTime);
            transform.Rotate(Vector3.up, rotationX * rotationSpeed * Time.deltaTime * -1);
        }
        else
        {
            float currentXRounded = (float)System.Math.Round(transform.rotation.x, 2);
            float currentYRounded = (float)System.Math.Round(transform.rotation.z, 2);

            //only if were not in the center already
            if (!(currentXRounded == 0 && currentYRounded == 0))
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, startRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
