using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Profiling;

public class UFOController : AIController {

    public delegate void ResetEnemy();
    public static event ResetEnemy OnResetEnemy;

    public bool useAI;
    public bool followPlayer;
    public Transform player;

    protected new UFOMovement movement;


    private void OnEnable()
    {
        Pathfinder.OnNewCorridor += NewCorridor;
    }
    private void OnDisable()
    {
        Pathfinder.OnNewCorridor -= NewCorridor;
    }

    // Use this for initialization
    void Start () {
        startNodes = new List<int>();
        lookaheadDummy = new Vector3();
        movement = GetComponent<UFOMovement>();
        corridor = new List<Node>();
        fullCorridor = new List<Node>();
        Reset();
    }
	
	// Update is called once per frame
	void Update () {
        if(useAI)
        {
            if(Vector3.Distance(nextPosition,transform.position)<0.1)
            {
                if (corridor.Count > 0/*&&stepsSkipted*/ && !corridorDone)
                {

                    nextNode = corridor[0];
                    if (corridor.Count > 1)
                        lookaheadNode = corridor[1];
                    else
                        lookaheadNode = corridor[0];
                    nextPosition = corridor[0].publicGlobalPosition;
                    //corridorStep++;
                    
                    corridor.RemoveAt(0);

                    if (corridor.Count == 0)
                    {
                        corridorDone = true;
                    }
                }
                else if (pathfindingHelper.LineOfSight(transform.position, player.position, true))
                {
                    nextPosition = player.position;
                    corridorDone = true;
                }
                else
                {
                    nextPosition = transform.position;
                    corridorDone = false;
                }
            }
            movement.targetPoint = nextPosition;
        }
        else if(followPlayer)
        {
             transform.LookAt(player.transform);
        }
        else if(movement!=null)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            movement.input = new Vector3(y, x, 0);
        }
        if (drawCorridor && !corridorDrawn)
        {
            DrawCorridor();
            corridorDrawn = true;
        }
    }

    override public void Reset()
    {
        if(movement!=null)
            movement.Reposition();
        corridorDone = false;

        if (useAI)
        {
            FileHandler.self.WriteString("\nreset\n");
            FileHandler.self.WriteString("distance travelled: " + movement.distanceTraveled + "\n");
            
            nextPosition = transform.position;
            nextNode = null;
            lookaheadNode = null;
            corridor.Clear();
            OnResetEnemy();
        }
    }

    public override void ResetCorridorMovement()
    {
        nextNode = null;
        lookaheadNode = null;
        corridor.Clear();
    }

    private void NewCorridor()
    {
        corridor = new List<Node>(PathfindingSelector.self.pathfinder.corridor);
        fullCorridor = new List<Node>(corridor);
        corridorDrawn = false;
        corridorStep = 0;
        if(!corridorDone)
            nextPosition = corridor[0].publicGlobalPosition;
    }

    public override void SkipSteps(float seconds)    //TODO finish!!
    {
        List<Node> inbetweenCorridor = new List<Node>();
        List<Node> overlapCorridor = new List<Node>();

        for(int k=0;k<corridorStep;k++)
        {
            if(corridor.Contains(oldCorridor[k]))               //check overlapping nodes of old and new corridor
            {
                overlapCorridor.Add(oldCorridor[k]);
            }   
            if(!corridor.Contains(oldCorridor[k]))              //all nodes that have been travelled on the old corridor, but are not 
            {                                                   //in the new one, might cause the AI to backtrack
                inbetweenCorridor.Add(oldCorridor[k]);          //this needs work TODO
                if (k>0)
                {
                    if (!inbetweenCorridor.Contains(oldCorridor[k - 1]))
                        inbetweenCorridor.Add(oldCorridor[k - 1]);
                }
            }
        }


        if(overlapCorridor.Count>0)
        {
            corridor = corridor.Except(overlapCorridor).ToList<Node>();
        }

        if(inbetweenCorridor.Count>0)
        {
            corridor.InsertRange(0, inbetweenCorridor);
        }

        corridorStep = 0;
    }


    override public Node GetLookAheadNode(int steps)
    {
        if (steps == 0)
        {
            return nextNode;
        }
        else
        {
            return lookaheadNode;
        }
    }

    override protected void DrawCorridor()
    {
        for (int i = 0; i < fullCorridor.Count - 1; i++)
        {
            Debug.DrawLine(fullCorridor[i].publicGlobalPosition, fullCorridor[i + 1].publicGlobalPosition, corridorColors[corridorCounter % corridorColors.Length], 1f);
        }
        corridorCounter++;
    }


}
