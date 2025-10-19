using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateList : MonoBehaviour
{
    public bool IsGrounded;
    public bool IsJumping;
    public bool IsDashing;
    public bool IsAttacking;
    public bool IsRecoilingXAxis;
    public bool IsRecoilingYAxis;
    public bool IsLookingRight;
    public bool IsInvincible;
    public bool IsHealing;
    public bool IsDead;
    public bool IsPaused;
    public bool IsRespawning;
    public bool IsRespawned;
    public bool IsCasting;
}