using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleHealth : MonoBehaviour {

    public ParticleSystem hitParticles;
    public ParticleSystem explosionParticles;

    private int lifepoints = 100;
    private int score = 20;

    private MeshRenderer mesh;
    private Color standardColor;

    void Awake()
    {
        mesh = gameObject.GetComponent<MeshRenderer>();
        if(mesh!=null)
        {
            standardColor = mesh.material.color;
            hitParticles = GetComponentInChildren<ParticleSystem>();
        }
        
        //explosionParticles = GetComponentInChildren<ParticleSystem>();
    }

    void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.CompareTag("bullet"))
        {
            CollideWithBullet(col.gameObject,col.contacts);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("bullet"))
        {
            CollideWithBullet(other.gameObject,null);
        }
    }

    public void CollideWithBullet(GameObject col,ContactPoint[] conPoint)
    {
        bool isRocket = col.gameObject.GetComponent<BulletMovement>().isRocket;
        //Debug.Log("col");
        if(conPoint!=null /*&& hitParticles!=null*/)
            ReceiveDamage(col.gameObject.GetComponent<BulletMovement>().dmg, conPoint[0].point, isRocket);
        else
            ReceiveDamage(col.gameObject.GetComponent<BulletMovement>().dmg, col.transform.position, isRocket);
        col.gameObject.GetComponent<BulletMovement>().isShot = false;
        if (isRocket)
            PoolBehaviour.rocketPool.ReleaseObject(col.gameObject);
        else
            PoolBehaviour.bulletPool.ReleaseObject(col.gameObject);
    }

    public void ReceiveDamage(int damageTake, Vector3 hitPoint, bool isRocket)
    {
        lifepoints -= damageTake;
        StartCoroutine(ShowDamageEffect(hitPoint, isRocket));
        if(lifepoints <= 0)
        {
            lifepoints = 100;
            //ScoreManager.increaseScore(score);
            gameObject.GetComponent<AsteroidMovement>().Reposition();
        }
    }

    IEnumerator ShowDamageEffect(Vector3 hitPoint, bool isRocket)
    {
        if(isRocket)
        {
            explosionParticles.transform.position = hitPoint;
            explosionParticles.transform.localScale = new Vector3();
            explosionParticles.Play();
        }
        //Debug.Log(lifepoints);
        hitParticles.transform.position = hitPoint;
        hitParticles.Play();
        mesh.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        mesh.material.color = standardColor;
        yield return new WaitForSeconds(0.5f);
        mesh.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        mesh.material.color = standardColor;
        yield return new WaitForSeconds(0.5f);
    }
}
