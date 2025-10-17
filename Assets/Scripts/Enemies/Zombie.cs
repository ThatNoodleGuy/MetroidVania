using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : EnemyCore
{
    protected override void Awake()
    {
        base.Awake();


    }

    protected override void Start()
    {
        rb.gravityScale = 12f;
    }

    protected override void Update()
    {
        base.Update();

        if (player == null)
        {
            player = PlayerController.Instance;
        }

        if (!isRecoiling)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.transform.position.x, transform.position.y), speed * Time.deltaTime);
        }
    }
    
    public override void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        base.EnemyHit(damageDone, hitDirection, hitForce);
    }
}