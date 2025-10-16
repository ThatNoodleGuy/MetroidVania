using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    [SerializeField] protected float health;
    protected float maxHealth;
    [SerializeField] protected float recuilLength;
    [SerializeField] protected float reciolFactor;
    protected float recoilTimer;
    [SerializeField] protected bool isRecoiling = false;

    protected Rigidbody2D rb;

    [SerializeField] protected PlayerController player;
    [SerializeField] protected float speed;

    [SerializeField] protected float damage;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance;

        maxHealth = health;
    }
    
    protected virtual void Start()
    {
        
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
            if (recoilTimer < recuilLength)
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
            rb.AddForce(-hitForce * reciolFactor * hitDirection);
            isRecoiling = true;
        }
    }

    protected void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() && !PlayerController.Instance.GetComponent<PlayerStateList>().IsInvincible)
        {
            Attack();
        }
    }

    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
    }

    public float Health { get => health; set => health = value; }
    public float RecuilLength { get => recuilLength; set => recuilLength = value; }
    public PlayerController Player { get => player; set => player = value; }


}