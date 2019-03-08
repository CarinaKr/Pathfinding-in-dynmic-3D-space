using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAsteroids : MonoBehaviour {

    public static CreateAsteroids self;
    public GameObject[] asteroids;
    public Transform ufo;
    public int layerNum;

    //public int circlesNumber;
    public int[] asteroidsNumber;
    public int[] minSize, maxSize;
    public int[] moveSpeed;
    public int[] radius;
    public int minRadius;
    public float ySpread;
    public float maxNegYOffset;
    
    private float sphereSize;
    private float x, y, z,xRot,yRot,zRot;
    private float size;
    private GameObject asteroid;

    private void Awake()
    {
        if(!self)
        {
            self = this;
        }

        if(self!=this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start () {

        maxNegYOffset = (layerNum-1)*(ySpread/2);

        for(int k=0;k<layerNum;k++)
        {
            float yOffset = (-1 * maxNegYOffset) + (k * ySpread);
            for (int j = 0; j < asteroidsNumber.Length; j++)
            {
                for (int i = 0; i < asteroidsNumber[j]; i++)
                {
                    sphereSize = radius[j];
                    float initSize = sphereSize / (Mathf.Sqrt(2));
                    x = Random.Range(-initSize,initSize);
                    y = Random.Range(-ySpread,ySpread);
                    // y = Random.Range(-sphereSize, sphereSize);
                    z = Random.Range(-initSize, initSize);

                    xRot = Random.Range(0, 360);
                    yRot = Random.Range(0, 360);
                    zRot = Random.Range(0, 360);

                    size = Random.Range(minSize[j], maxSize[j]);

                    asteroid = asteroids[Random.Range(0, asteroids.Length)];

                    if (asteroid != null)
                    {
                        Vector3 newPosition = new Vector3(x, 0, z);    //direction in (set y=0, to keep asteroids in xz-layer)
                        asteroid.transform.localScale = new Vector3(size, size, size);
                        newPosition=newPosition.normalized* Random.Range(minRadius,radius[j]);
                        newPosition.y = ufo.position.y + yOffset + y;       //set semi-random height relative to UFO position
                        GameObject newAsteroid = GameObject.Instantiate(asteroid, newPosition, Quaternion.Euler(xRot, yRot, zRot), gameObject.transform);
                        AsteroidMovement move = newAsteroid.GetComponentInChildren<AsteroidMovement>();
                        move.moveSpeed = moveSpeed[j];
                        move.radius = radius[j];
                        move.yOffset = yOffset;
                        // move.SetMovement();

                    }
                }
            }
        
        }
       

	}
	
	
}
