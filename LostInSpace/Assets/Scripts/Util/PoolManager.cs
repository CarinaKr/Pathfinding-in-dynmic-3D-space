using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager {

    private GameObject[] _objects;
    private int nextReturnIndex = 0;
    private int nextInsertIndex = 0;

    public PoolManager(int poolSize, GameObject toPool)
    {
        _objects = new GameObject[poolSize];
        for(int i = 0; i < _objects.Length; i++)
        {
            _objects[i] = GameObject.Instantiate(toPool);
            _objects[i].SetActive(false);
        }
    }
    public PoolManager(int poolSize,GameObject toPool, Transform parent)
    {
        _objects = new GameObject[poolSize];
        for (int i = 0; i < _objects.Length; i++)
        {
            _objects[i] = GameObject.Instantiate(toPool,parent);
            _objects[i].SetActive(false);
        }
    }
    
    public GameObject GetObject()
    {
        GameObject returnValue = _objects[nextReturnIndex];
        _objects[nextReturnIndex] = null;
        nextReturnIndex++;
        nextReturnIndex %= _objects.Length;
        returnValue.SetActive(true);
        return returnValue;
    }

    public void ReleaseObject(GameObject obj)
    {
        _objects[nextInsertIndex] = obj;
        nextInsertIndex++;
        nextInsertIndex %= _objects.Length;
        obj.SetActive(false);
    }

    public GameObject[] objects
    {
        get
        {
            return _objects;
        }
    }
}
