using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UFOHealth : MonoBehaviour {

    

    public float maxHealth;
    protected bool isCollided = false;
    protected float _health;

    // Use this for initialization
    protected virtual void Start () {
        Refill();
    }

    virtual public void Refill()
    {
        health = maxHealth;
    }

    virtual public float health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
            if(_health<=0)
            {
                GameOver();
            }
        }
    }

    public abstract void GameOver();
}
