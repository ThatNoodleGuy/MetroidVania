using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(RespawnPointRoutine());
        }
    }

    private IEnumerator RespawnPointRoutine()
    {
        // Safety checks
        if (PlayerController.Instance == null)
        {
            Debug.LogError("PlayerController.Instance is null!");
            yield break;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            yield break;
        }

        if (UIManager.Instance == null || UIManager.Instance.sceneFader == null)
        {
            Debug.LogError("UIManager or SceneFader is null!");
            yield break;
        }

        // Log the respawn position for debugging
        Debug.Log(
            "Player hit spikes! Respawning to: " + GameManager.Instance.PlatfromingRespawnPoint
        );

        PlayerController.Instance.PlayerStateList.IsInCutscene = true;
        PlayerController.Instance.PlayerStateList.IsInvincible = true;
        PlayerController.Instance._Rigidbody2D.linearVelocity = Vector2.zero;
        Time.timeScale = 0.01f; // Slow motion instead of freeze
        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.In));
        PlayerController.Instance.TakeDamage(1);
        yield return new WaitForSeconds(0.01f); // Very short wait with scaled time
        Time.timeScale = 1; // Restore time BEFORE repositioning

        // Reposition player
        PlayerController.Instance.transform.position = GameManager.Instance.PlatfromingRespawnPoint;

        // Reset velocity after repositioning to prevent continued falling
        PlayerController.Instance._Rigidbody2D.linearVelocity = Vector2.zero;

        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
        yield return new WaitForSeconds(UIManager.Instance.sceneFader.FadeTime);
        PlayerController.Instance.PlayerStateList.IsInCutscene = false;
        PlayerController.Instance.PlayerStateList.IsInvincible = false;

        Debug.Log("Respawn complete!");
    }
}
