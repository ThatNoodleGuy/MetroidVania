using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    Animator animator;
    float animationDuration;
    void Start()
    {
        animator = GetComponent<Animator>();
        
        animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        Destroy(gameObject, animationDuration);
    }
}