using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnlockWallJump : MonoBehaviour
{
    [SerializeField]
    private GameObject unlockEffectVFX;

    [SerializeField]
    private GameObject canvasUI;

    private bool used;

    private void Start()
    {
        if (PlayerController.Instance.UnlockedWallJump)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !used)
        {
            used = true;
            StartCoroutine(ShowUIRoutine());
        }
    }

    private IEnumerator ShowUIRoutine()
    {
        GameObject effect = Instantiate(unlockEffectVFX, transform.position, Quaternion.identity);
        effect.transform.parent = gameObject.transform.parent;
        yield return new WaitForSeconds(0.5f);
        canvasUI.SetActive(true);
        yield return new WaitForSeconds(4f);
        PlayerController.Instance.UnlockedWallJump = true;
        SaveData.Instance.SavePlayerData();
        canvasUI.SetActive(false);
        Destroy(gameObject);
    }
}
