using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bench : MonoBehaviour
{
    [SerializeField]
    private bool interacted;

    private PlayerController player;
    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }

    private void Update()
    {
        if (playerInRange && player != null && PlayerController.Instance.InteractValue)
        {
            interacted = true;
        }
    }

    public bool Interacted
    {
        get { return interacted; }
        set { interacted = value; }
    }
}
