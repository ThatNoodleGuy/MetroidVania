using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bench : MonoBehaviour
{
    [SerializeField]
    private bool interacted;

    [SerializeField]
    private bool isInRange;

    private PlayerController player;

    private void Update()
    {
        if (isInRange && PlayerController.Instance.InteractValue)
        {
            // Debug.Log("interacted with bench");
            // Debug.Log(PlayerController.Instance.InteractValue);

            if (!interacted) // Only trigger once per press
            {
                interacted = true;

                SaveData.Instance.benchSceneName = SceneManager.GetActiveScene().name;
                SaveData.Instance.benchPos = new Vector2(
                    gameObject.transform.position.x,
                    gameObject.transform.position.y
                );
                SaveData.Instance.SaveBench();
                SaveData.Instance.SavePlayerData();
                Debug.Log("benched");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = false;
            interacted = false;
        }
    }

    public bool Interacted
    {
        get { return interacted; }
        set { interacted = value; }
    }

    public bool IsInRange
    {
        get { return isInRange; }
        set { isInRange = value; }
    }
}
