using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : UFOHealth {

    
    //public GameObject screen;
    public Image healthTop;
    public Material[] healthColor;
    public int gameOverLevelNumber;

    private int asteroidDamage;
    private float healthFull;
    
    private Light lifeLight;
    

	// Use this for initialization
	protected override void Start () {
        base.Start();
        
        lifeLight = GameObject.Find("LowLifeLight").GetComponent<Light>();
        lifeLight.enabled = false;
    }

    public override float health
    {
        get
        {
            return base.health;
        }

        set
        {
            base.health = value;
            float healthPercent = health / maxHealth;
            healthTop.fillAmount = healthPercent;
            if (healthPercent < 0.3)
            {
                StartCoroutine(LowLifeEffect());
                healthTop.material = healthColor[2];
            }
            else if (healthPercent < 0.6)
            {
                healthTop.material = healthColor[1];
            }
        }
    }

    IEnumerator LowLifeEffect()
    {
        bool decrease = false;
        lifeLight.enabled = true;
        while(true)
        {
            if(lifeLight.intensity > 0.3 && decrease)
            {
                lifeLight.intensity -= 0.3f;
                if(lifeLight.intensity < 0.3f)
                {
                    decrease = false;
                }
            }
            else if (lifeLight.intensity < 2.7 && !decrease)
            {
                lifeLight.intensity += 0.3f;
                if (lifeLight.intensity > 2.7f)
                {
                    decrease = true;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

    }

    public override void GameOver()
    {
        Debug.Log("Game Over");
        Refill();
        healthTop.material = healthColor[0];
    }
}
