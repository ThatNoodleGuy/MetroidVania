using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    [SerializeField] protected float health;
    protected float maxHealth;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    protected float recoilTimer;
    [SerializeField] protected bool isRecoiling = false;

    protected Rigidbody2D rb;

    [SerializeField] protected PlayerController player;
    [SerializeField] protected float speed;

    [SerializeField] protected float damage;
    
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance;

        maxHealth = health;
    }

    protected virtual void Update()
    {
        if (health <= 0)
        {
            health = 0;
            Destroy(gameObject);
        }

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    public virtual void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        health -= damageDone;
        health = Mathf.Clamp(health, 0, maxHealth);

        if (!isRecoiling)
        {
            rb.AddForce(-hitForce * recoilFactor * hitDirection);
            isRecoiling = true;
        }
    }

    protected void OnCollisionStay2D(Collision2D other) 
    {
        if (other.gameObject.GetComponent<PlayerController>() && !PlayerController.Instance.GetComponent<PlayerStateList>().IsInvincible)
        {
            Attack();
            PlayerController.Instance.HitStopTime(0, 5, 0.5f);  // time scale = 0, restore speed = 5, delay = 0.5
        }
    }

    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
    }

    public float Health { get => health; set => health = value; }
    public float RecoilLength { get => recoilLength; set => recoilLength = value; }
    public PlayerController Player { get => player; set => player = value; }


}