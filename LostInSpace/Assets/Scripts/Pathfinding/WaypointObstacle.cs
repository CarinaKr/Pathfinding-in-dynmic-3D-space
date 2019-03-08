using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointObstacle : ShadowObstacle {

    public bool useConvexMesh;
    public Mesh convexMesh;
    public float maxWaypointDistance;

    public Transform shadowsContainer;
    public GameObject shadowPrefab;

    public Mesh mesh { private set; get; }
    public Vector3 globalPosition { get; set; }
    
    public bool isActive { get; set; }
    public bool startActive { get; set; }

    public bool shadowIsActive { get; set; }
    public GameObject shadowSelfObject { get; set; }
    public ShadowObstacle shadowSelf { get; set; }
    public bool isDynamic { get; set; }

    // Use this for initialization
    void Start () {

        if(shadowSelfObject==null)
            shadowSelfObject = gameObject;
        if (wpObstacle == null)
            wpObstacle = this;

        isActive = true;
        shadowIsActive = true;
        if (useConvexMesh)
            mesh = convexMesh;
        else
            mesh = GetComponent<MeshFilter>().mesh;

        if(maxWaypointDistance<=0)
        {
            maxWaypointDistance = float.MaxValue;
        }

        globalPosition = transform.position;
    }
    
    public void SetShadow()
    {
        shadowIsActive = isActive;
        shadowSelfObject.SetActive(shadowIsActive);
        if(shadowIsActive)
        {
            shadowSelfObject.transform.position = transform.position;
            shadowSelfObject.transform.rotation = transform.rotation;
            shadowSelfObject.transform.localScale = shadowSelfObject.transform.localScale;
        }
    }

    public void MakeShadowCopy()
    {
        shadowSelfObject = Instantiate(shadowPrefab, transform.position, transform.rotation, shadowsContainer);
        shadowSelfObject.transform.localScale = transform.localScale;
        CopyComponent(GetComponent<Collider>(), shadowSelfObject);
        shadowSelf = shadowSelfObject.GetComponent<ShadowObstacle>();
        shadowSelf.wpObstacle = this;
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    //https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html
    private Component CopyComponent(Component component, GameObject copyTo)
    {
        System.Type type = component.GetType();
        Component copy = copyTo.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(component));
        }
        return copy;
    }

    public override List<Waypoint> waypointList
    {
        get
        {
            return base.waypointList;
        }

        set
        {
            base.waypointList = value;
            if(isDynamic)
                shadowSelf.waypointList = value;
        }
    }
}
