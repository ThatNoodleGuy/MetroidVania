using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateList : MonoBehaviour
{
    private bool isGrounded;
    private bool isJumping;
    private bool isDashing;


    public bool IsGrounded { get => isGrounded; set => isGrounded = value; }
    public bool IsJumping { get => isJumping; set => isJumping = value; }
    public bool IsDashing { get => isDashing; set => isDashing = value; }


}