using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun 
{
    private Transform cameraTransform;

    //private RectTransform reticle;
    
    private int cooldown;

    private IGunStrategy gunStrat;
    private Transform barrelEnd;
    private Transform[] barrelEnds;

    private Vector3 eulerAngleOffset = new Vector3(0, 0, 85);
    private Vector3 target;

    private bool _isBullet;

    public Gun() { }

    public Gun(IGunStrategy gunStrat, Transform leftBarrelEnd, Transform rightBarrelEnd)
    {
        barrelEnds = new Transform[2];
        _isBullet = true;
        this.gunStrat = gunStrat;
        this.barrelEnds[0] = leftBarrelEnd;
        this.barrelEnds[1] = rightBarrelEnd;
        //reticle = GameObject.FindGameObjectWithTag("CrossHair").GetComponent<RectTransform>();
        cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
    }

    public void Shoot()
    {
            //take bullets from the pool
            GameObject bulletLeft = isBullet ? PoolBehaviour.bulletPool.GetObject() : PoolBehaviour.rocketPool.GetObject();
            GameObject bulletRight = isBullet ? PoolBehaviour.bulletPool.GetObject() : PoolBehaviour.rocketPool.GetObject();

            //and put them in the right place
            bulletLeft.transform.position = barrelEnds[0].transform.position + Vector3.forward;
            bulletRight.transform.position = barrelEnds[1].transform.position + Vector3.forward;

            //give them their target
            bulletLeft.GetComponent<BulletMovement>().setDir(target);
            bulletRight.GetComponent<BulletMovement>().setDir(target);

            //set the properties of the bullet according to the gun (might want to implement an empty "projectile" interface so that i can share it along everywhere
            bulletLeft.GetComponent<BulletMovement>().dmg = gunStrat.dmg;
            bulletRight.GetComponent<BulletMovement>().dmg = gunStrat.dmg;
            //reduce Ammo
            gunStrat.currentAmmo -= 2;
    }
    
    public void Aim()
    {
       

        //Vector3 direction = -1 * (cameraTransform.position - reticle.position);
        RaycastHit hit;

        if(Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, gunStrat.range, gunStrat.mask)) {
            target = hit.point;
        }
        else
        {
            target = cameraTransform.forward * gunStrat.range;
        }

        //aim
        for (int i = 0; i < barrelEnds.Length; i++)
        {
            barrelEnds[i].parent.parent.LookAt(target);
        }
        if (!isBullet)
            barrelEnds[1].parent.parent.Rotate(eulerAngleOffset, Space.Self);

    }

    public void setGunStrat(IGunStrategy strat, Transform leftBarrelEnd, Transform rightBarrelEnd)
    {
        gunStrat = strat;
        barrelEnds[0] = leftBarrelEnd;
        barrelEnds[1] = rightBarrelEnd;
    }

    public bool isBullet
    {
        get
        {
            return _isBullet;
        }
        set
        {
            _isBullet = value;
        }
    }
}
