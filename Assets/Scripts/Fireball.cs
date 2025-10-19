using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float hitForce;
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime = 1f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate() 
    {
        transform.position += speed * transform.right; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<EnemyCore>() != null)
        {
            other.GetComponent<EnemyCore>().EnemyHit(damage, (other.transform.position - transform.position).normalized, -hitForce);
        }
    }
}
