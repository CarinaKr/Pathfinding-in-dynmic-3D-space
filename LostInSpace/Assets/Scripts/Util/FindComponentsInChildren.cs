using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{

    public static List<Component> masterList = new List<Component>();

    public static void FindComponentsInChildrenWithTag<T>(this GameObject parent, string tag) where T : Component
    {
        Transform t = parent.transform;
        foreach (Transform tr in t)
        {
            //Debug.Log("Currently at " + t.gameObject.ToString());
            if (tr.CompareTag(tag))
            {
                //Debug.Log("Comp.: " + tr.parent.parent.name);
                masterList.Add(tr.GetComponent<T>());
            }
            else
            {
                tr.gameObject.FindComponentsInChildrenWithTag<T>(tag);
            }
        }
    }
}
