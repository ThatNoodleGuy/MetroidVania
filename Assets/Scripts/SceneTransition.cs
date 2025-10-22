using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string transitionTo;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Vector2 exitDirection;
    [SerializeField] private float exitTime;

    private void Start() 
    {
        if (transitionTo == GameManager.Instance.TransitionedFromScene)
        {
            PlayerController.Instance.transform.position = startPoint.position;

            StartCoroutine(PlayerController.Instance.WalkIntoNewSceneRoutine(exitDirection, exitTime));
        }

        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>())
        {
            GameManager.Instance.TransitionedFromScene = SceneManager.GetActiveScene().name;

            PlayerController.Instance.GetComponent<PlayerStateList>().IsInCutscene = true;
            PlayerController.Instance.GetComponent<PlayerStateList>().IsInvincible = true;

            StartCoroutine(UIManager.Instance.sceneFader.FadeAndLoadScene(SceneFader.FadeDirection.In, transitionTo));
        }
    }
}