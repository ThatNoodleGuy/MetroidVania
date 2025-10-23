using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField]
    private string transitionTo;

    [SerializeField]
    private Transform startPoint;

    [SerializeField]
    private Vector2 exitDirection;

    [SerializeField]
    private float exitTime;

    private void Start()
    {
        if (GameManager.Instance.TransitionedFromScene == transitionTo)
        {
            PlayerController.Instance.transform.position = startPoint.position;

            StartCoroutine(
                PlayerController.Instance.WalkIntoNewSceneRoutine(exitDirection, exitTime)
            );
        }

        // Add null checks
        if (UIManager.Instance != null && UIManager.Instance.sceneFader != null)
        {
            StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
        }
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Player"))
        {
            CheckShadeData();

            GameManager.Instance.TransitionedFromScene = SceneManager.GetActiveScene().name;

            PlayerController.Instance.GetComponent<PlayerStateList>().IsInCutscene = true;

            StartCoroutine(
                UIManager.Instance.sceneFader.FadeAndLoadScene(
                    SceneFader.FadeDirection.In,
                    transitionTo
                )
            );
        }
    }

    void CheckShadeData()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");

        for (int i = 0; i < enemyObjects.Length; i++)
        {
            if (enemyObjects[i].GetComponent<Shade>() != null)
            {
                SaveData.Instance.SaveShadeData();
            }
        }
    }
}
