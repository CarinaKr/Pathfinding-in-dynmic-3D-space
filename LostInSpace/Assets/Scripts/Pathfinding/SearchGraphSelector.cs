using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchGraphSelector : MonoBehaviour {

    public enum CreatingMethod
    {
        CELL_GRID,
        WAYPOINTS
    }

    public static SearchGraphSelector self;
    public CreatingMethod creationMethod;
    public Transform start, goal;

    public bool checkInIntervals;
    public float intervalTime;

    public Vector3 startPosition;
    public Vector3 goalPosition;

    public SearchGraphManager searchGraphManager { private set; get; }
    public bool isUpdatingGraph { get;  private set; }

    private float _intervalTime;

    private void Awake()
    {
        if(self==null)
        {
            self = this;
        }
        if(self!=this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        switch (creationMethod)
        {
            case CreatingMethod.CELL_GRID:
                searchGraphManager = GetComponentInChildren<CellGridManager>(true);
                searchGraphManager.gameObject.SetActive(true);
                break;
            case CreatingMethod.WAYPOINTS:
                searchGraphManager = GetComponentInChildren<WaypointManagerInbetween>(true);
                searchGraphManager.gameObject.SetActive(true);
                break;
        }

        _intervalTime = intervalTime;
    }

    // Use this for initialization
    void Start () {
		
    }

    public void ManageChanges()
    {
        if (checkInIntervals)
            StartCoroutine("IntervalLoop");
    }

    private IEnumerator IntervalLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_intervalTime);
            isUpdatingGraph = true;
            yield return searchGraphManager.StartCoroutine("UpdateSearch");
            isUpdatingGraph = false;
        }
    }

    private void Update()
    {
        startPosition = start.position;
        goalPosition = goal.position;
    }

}
