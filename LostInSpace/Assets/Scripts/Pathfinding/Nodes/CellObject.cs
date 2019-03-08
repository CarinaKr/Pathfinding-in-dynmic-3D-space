using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellObject : MonoBehaviour {

    public bool showAllNodes;
    public Color debugColor;
    public bool useDebugColor;
    public bool drawNeighbours;
    public bool drawParent;
    public double rhs, g, heuristic, plainHeuristic;

    private Cell cell;

    public static Cell Create(GameObject prefab, Transform parent, Vector3 position, float size)
    {
        GameObject newObject = Instantiate(prefab, parent);
        newObject.transform.localPosition = position;
        Cell cell = new Cell(position,newObject.transform.position, size);
        CellObject cellObject = newObject.GetComponent<CellObject>();
        cellObject.cell = cell;

        return cell;
    }

    private void OnDrawGizmosSelected()
    {
        rhs = cell.rhsValue;
        g = cell.gValue;
        heuristic = cell.heuristic;
        plainHeuristic = cell.plainHeuristic;
        //if (!isNext) return;
        if (cell.isNextNode)
            Gizmos.color = Color.cyan;
        else if (cell.isLookaheadNode)
            Gizmos.color = Color.blue;
        else if (cell.isStartNode)
            Gizmos.color = Color.magenta;

        else if (cell.isInOpenAfterChange)
            Gizmos.color = Color.yellow;
        else if (cell.isInOpen)
            Gizmos.color = Color.magenta;
        else if (cell.isChanged)
            Gizmos.color = Color.blue;
        else if (cell.isInIncons)
            Gizmos.color = Color.cyan;
        else if (cell.isInClosed)
            Gizmos.color = Color.gray;

        else if (cell.isFree)
        {
            Gizmos.color = Color.green;
        }
        else if (!cell.isFree)
        {
            Gizmos.color = Color.red;
        }
        else if (useDebugColor)
        {
            Gizmos.color = debugColor;
        }

        //if(cell.isChanged)
        //Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));

        if (!showAllNodes)
        {
            if (cell.isInOpen || cell.isInClosed || cell.isInIncons || cell.isChanged)    //DEBUG dynamic algorithms
                Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
        }
        else
            Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));

        //if (cell.isNextNode||cell.isLookaheadNode||cell.isStartNode)
        //    Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));



        if (cell.parentNode != null && drawParent)
        {
            Debug.DrawLine(cell.globalPosition, cell.parentNode.globalPosition, Color.green, 0.1f);
        }

        if (drawNeighbours)
        {
            for (int i = 0; i < cell.neighbours.Count; i++)
            {
                //if (localPosition.x == 1 && localPosition.y == 1 && localPosition.z == 1)
                Debug.DrawLine(cell.globalPosition, cell.neighbours[i].globalPosition, Color.yellow, 1f);
            }
        }
    }
}
