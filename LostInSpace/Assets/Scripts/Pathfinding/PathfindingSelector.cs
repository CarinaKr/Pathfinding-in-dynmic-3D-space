using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingSelector : MonoBehaviour {

    public enum PathfindingMethod
    {
        IDLE,
        A_STAR,
        ARA_STAR,
        THETA_STAR,
        D_STAR_LITE,
        AD_STAR,
        MT_D_STAR_LITE
    }

    public static PathfindingSelector self;
    public PathfindingMethod pathfindingMethod;
    

    public Pathfinder pathfinder { get; set; }

    private void Awake()
    {
        if (self == null)
        {
            self = this;
        }
        if (self != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
        switch (pathfindingMethod)
        {
            case PathfindingMethod.A_STAR:
                pathfinder = GetComponentInChildren<A_star>(true);
                break;
            case PathfindingMethod.THETA_STAR:
                pathfinder = GetComponentInChildren<Theta_star>(true);
                break;
            case PathfindingMethod.ARA_STAR:
                pathfinder = GetComponentInChildren<ARA_star>(true);
                break;
            case PathfindingMethod.D_STAR_LITE:
                pathfinder = GetComponentInChildren<D_StarLite>(true);
                break;
            case PathfindingMethod.AD_STAR:
                pathfinder = GetComponentInChildren<AD_star>(true);
                break;
            case PathfindingMethod.MT_D_STAR_LITE:
                pathfinder = GetComponentInChildren<MTD_StarLite>(true);
                break;
        }
        if(pathfinder!=null)
            pathfinder.gameObject.SetActive(true);
    }
    

}
