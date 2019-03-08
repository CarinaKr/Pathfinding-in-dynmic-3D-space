using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public interface IGunStrategy {

    float cooldown
    {
        get;
    }

    string gunName
    {
        get;
    }

    Material img
    {
        get;
    }

    bool isReloading
    {
        get;
    }

    int reloadTime
    {
        get;
    }

    int range
    {
        get;
    }

    int dmg
    {
        get;
    }

    int currentAmmo
    {
        get;
        set;
    }

    int capacity
    {
        get;
    }

    LayerMask mask
    {
        get;
    }

    float firerate
    {
        get;
    }

    IEnumerator Reload();

    IEnumerator ShotEffect();

    void setProjectile();

    GameObject getGameObject();
}
