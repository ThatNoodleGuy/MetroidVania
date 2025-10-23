using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Crawler : EnemyCore
{
    [SerializeField]
    private float flipWaitTime;

    [SerializeField]
    private float ledgeCheckX;

    [SerializeField]
    private float ledgeCheckY;

    [SerializeField]
    private LayerMask whatIsGround;

    private float timer;

    protected override void Start()
    {
        base.Start();

        rb.gravityScale = 12f;
    }

    protected override void Update()
    {
        base.Update();

        if (!PlayerController.Instance.GetComponent<PlayerStateList>().IsAlive)
        {
            ChangeState(EnemyState.Crawler_Idle);
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<EnemyCore>())
        {
            ChangeState(EnemyState.Crawler_Flip);
        }
    }

    protected override void UpdateEnemyStates()
    {
        if (health <= 0)
        {
            Death(0.1f);
        }

        switch (GetCurrentEnemyState)
        {
            case EnemyState.Crawler_Idle:
                // Idle behavior here
                Vector3 ledgeCheckStart =
                    transform.localScale.x > 0
                        ? new Vector3(ledgeCheckX, 0)
                        : new Vector3(-ledgeCheckX, 0);
                Vector2 wallCheckDir =
                    transform.localScale.x > 0 ? transform.right : -transform.right;

                if (
                    !Physics2D.Raycast(
                        transform.position + ledgeCheckStart,
                        Vector2.down,
                        ledgeCheckY,
                        whatIsGround
                    )
                    || Physics2D.Raycast(
                        transform.position,
                        wallCheckDir,
                        ledgeCheckX,
                        whatIsGround
                    )
                )
                {
                    ChangeState(EnemyState.Crawler_Flip);
                }

                if (transform.localScale.x > 0)
                {
                    rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
                }
                break;

            case EnemyState.Crawler_Flip:
                // Flip behavior here
                timer += Time.deltaTime;

                if (timer > flipWaitTime)
                {
                    timer = 0f;
                    transform.localScale = new Vector2(
                        transform.localScale.x * -1,
                        transform.localScale.y
                    );
                    ChangeState(EnemyState.Crawler_Idle);
                }
                break;

            default:
                break;
        }
    }
}
