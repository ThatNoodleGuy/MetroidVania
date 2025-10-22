using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private string transitionedFromScene;

    [SerializeField]
    private Vector2 platfromingRespawnPoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string TransitionedFromScene
    {
        get { return transitionedFromScene; }
        set { transitionedFromScene = value; }
    }

    public Vector2 PlatfromingRespawnPoint
    {
        get { return platfromingRespawnPoint; }
        set { platfromingRespawnPoint = value; }
    }
}
