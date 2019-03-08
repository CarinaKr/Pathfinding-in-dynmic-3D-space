using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : UFOHealth {
    

    private UFOController controller;

    // Use this for initialization
    protected override void Start () {
        base.Start();
        controller = GetComponent<UFOController>();
	}
    
    

    public override void GameOver()
    {
        controller.Reset();
        Refill();
    }
}
