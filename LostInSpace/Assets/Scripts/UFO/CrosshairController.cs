using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public float aimingSpeed;

    private RectTransform rectTrans;
    private Vector2 crosshairMovement;

    private Vector3 centerPoint;
    private float centerPointXRounded;
    private float centerPointYRounded;
    private float step = 0.7f;

    // Use this for initialization
    void Start()
    {
        rectTrans = GetComponent<RectTransform>();
        centerPoint = rectTrans.localPosition;
        centerPointXRounded = (float)Math.Round(centerPoint.x, 2);
        centerPointYRounded = (float)Math.Round(centerPoint.y, 2);
    }

    // Update is called once per frame
    void Update()
    {
        //read input
        float moveX = Input.GetAxis("RotationX");
        float moveY = Input.GetAxis("RotationY");

        //if input on an axis != 0, move towards target position, else move back to center point
        if (moveX != 0 || moveY != 0)
        {
            crosshairMovement = new Vector2(moveX, moveY);
            gameObject.transform.Translate(crosshairMovement * aimingSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            float currentXRounded = (float)Math.Round(rectTrans.localPosition.x, 2);
            float currentYRounded = (float)Math.Round(rectTrans.localPosition.y, 2);
            //only if were not in the center already
            if (!(currentXRounded == centerPointXRounded && currentYRounded == centerPointYRounded))
            {
                rectTrans.localPosition = Vector3.MoveTowards(rectTrans.localPosition, centerPoint, step * aimingSpeed * Time.deltaTime);
            }
        }
    }
}
