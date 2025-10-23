using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bat : EnemyCore
{
    [SerializeField]
    private float chaseDistance;

    [SerializeField]
    private float stunDuration;

    private float timer;

    protected override void Start()
    {
        base.Start();

        ChangeState(EnemyState.Bat_Idle);
    }

    protected override void Update()
    {
        base.Update();

        if (!PlayerController.Instance.GetComponent<PlayerStateList>().IsAlive)
        {
            ChangeState(EnemyState.Bat_Idle);
            return;
        }
    }

    protected override void UpdateEnemyStates()
    {
        float distance = Vector2.Distance(
            transform.position,
            PlayerController.Instance.transform.position
        );

        switch (GetCurrentEnemyState)
        {
            case EnemyState.Bat_Idle:
                // Idle behavior here
                rb.linearVelocity = new Vector2(0, 0);
                if (distance < chaseDistance)
                {
                    ChangeState(EnemyState.Bat_Chase);
                }
                break;

            case EnemyState.Bat_Chase:
                // Chase behavior here
                rb.MovePosition(
                    Vector2.MoveTowards(
                        transform.position,
                        PlayerController.Instance.transform.position,
                        speed * Time.deltaTime
                    )
                );

                FlipDirection();

                if (distance > chaseDistance)
                {
                    ChangeState(EnemyState.Bat_Idle);
                }

                break;

            case EnemyState.Bat_Stunned:
                // Stunned behavior here
                timer += Time.deltaTime;

                if (timer > stunDuration)
                {
                    ChangeState(EnemyState.Bat_Chase);
                    timer = 0f;
                }
                break;

            case EnemyState.Bat_Death:
                // Death behavior here
                Death(UnityEngine.Random.Range(5f, 10f));
                break;
        }
    }

    private void FlipDirection()
    {
        if (PlayerController.Instance.transform.position.x < transform.position.x)
        {
            sr.flipX = true;
        }
        else
        {
            sr.flipX = false;
        }
    }

    public override void EnemyGetsHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        base.EnemyGetsHit(damageDone, hitDirection, hitForce);

        if (health > 0)
        {
            ChangeState(EnemyState.Bat_Stunned);
        }
        else
        {
            ChangeState(EnemyState.Bat_Death);
        }
    }

    protected override void ChangeCurrentAnimation()
    {
        animator.SetBool("Idle", GetCurrentEnemyState == EnemyState.Bat_Idle);
        animator.SetBool("Chase", GetCurrentEnemyState == EnemyState.Bat_Chase);
        animator.SetBool("Stunned", GetCurrentEnemyState == EnemyState.Bat_Stunned);

        if (GetCurrentEnemyState == EnemyState.Bat_Death)
        {
            animator.SetTrigger("Death");
        }
    }

    protected override void Death(float delay)
    {
        rb.gravityScale = 12f;

        base.Death(delay);
    }
}
