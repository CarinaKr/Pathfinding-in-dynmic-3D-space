using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IObstacleMovement : MonoBehaviour {

    public int damage;

    public float moveSpeed { protected get; set; }
    public float radius { protected get; set; }
    public float yOffset { protected get; set; }

    protected bool isTagged;
    protected Rigidbody rb;
    protected float minY, maxY;
    protected Transform ufo;

    // Use this for initialization
    protected virtual void Start () {
        isTagged = false;
        rb = GetComponent<Rigidbody>();

    }

    protected virtual void Update()
    {
        isTagged = false;
    }

    public int getDamage()
    {
        return damage;
    }

    public bool tagged
    {
        get
        {
            return isTagged;
        }
        set
        {
            isTagged = value;
        }
    }

}
