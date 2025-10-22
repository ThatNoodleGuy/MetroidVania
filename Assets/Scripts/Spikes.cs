using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            StartCoroutine(RespawnPointRoutine());
        }
    }

    private IEnumerator RespawnPointRoutine()
    {
        PlayerController.Instance.PlayerStateList.IsInCutscene = true;
        PlayerController.Instance.PlayerStateList.IsInvincible = true;
        PlayerController.Instance._Rigidbody2D.linearVelocity = Vector2.zero;
        Time.timeScale = 0f;
        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.In));
        PlayerController.Instance.TakeDamage(1);
        yield return new WaitForSecondsRealtime(1f);
        PlayerController.Instance.transform.position = GameManager.Instance.PlatfromingRespawnPoint;
        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
        yield return new WaitForSecondsRealtime(UIManager.Instance.sceneFader.FadeTime);
        PlayerController.Instance.PlayerStateList.IsInCutscene = false;
        PlayerController.Instance.PlayerStateList.IsInvincible = false;
        Time.timeScale = 1.0f;
    }
}
