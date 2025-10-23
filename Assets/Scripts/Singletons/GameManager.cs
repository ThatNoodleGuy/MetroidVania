using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private string transitionedFromScene;

    [SerializeField]
    private Vector2 platfromingRespawnPoint;

    [SerializeField]
    private Vector2 respawnPoint;

    [SerializeField]
    Vector2 defaultRespawnPoint;

    [SerializeField]
    private Bench bench;

    [SerializeField]
    private GameObject playerShade;

    [SerializeField]
    private PlayerControls playerControls;

    private void Awake()
    {
        SaveData.Instance.Initialize();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (PlayerController.Instance != null)
        {
            // Initialize platforming respawn point to player's starting position if not set
            if (platfromingRespawnPoint == Vector2.zero)
            {
                platfromingRespawnPoint = PlayerController.Instance.transform.position;
                Debug.Log("Initialized platforming respawn point to: " + platfromingRespawnPoint);
            }

            if (PlayerController.Instance.HalfMana)
            {
                SaveData.Instance.LoadShadeData();
                if (
                    SaveData.Instance.sceneWithShade == SceneManager.GetActiveScene().name
                    || SaveData.Instance.sceneWithShade == ""
                )
                {
                    Instantiate(
                        playerShade,
                        SaveData.Instance.shadePos,
                        SaveData.Instance.shadeRot
                    );
                }
            }
        }

        bench = FindFirstObjectByType<Bench>();

        SaveScene();
    }

    private void Update()
    {
        if (
            PlayerController.Instance != null
            && PlayerController.Instance.GetPlayerControls().Player.Save.WasPressedThisFrame()
        )
        {
            SaveData.Instance.SavePlayerData();
        }
    }

    public void SaveScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SaveData.Instance.sceneNames.Add(currentSceneName);
    }

    public void RespawnPlayer()
    {
        SaveData.Instance.LoadBench();
        if (SaveData.Instance.benchSceneName != null) //load the bench's scene if it exists.
        {
            SceneManager.LoadScene(SaveData.Instance.benchSceneName);
        }

        if (SaveData.Instance.benchPos != null) //set the respawn point to the bench's position.
        {
            respawnPoint = SaveData.Instance.benchPos;
        }
        else
        {
            respawnPoint = defaultRespawnPoint;
        }

        PlayerController.Instance.transform.position = respawnPoint;

        StartCoroutine(UIManager.Instance.DeactivateDeathScreenRoutine());
        PlayerController.Instance.Respawned();
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

    public Vector2 RespawnPoint
    {
        get { return respawnPoint; }
        set { respawnPoint = value; }
    }

    public GameObject PlayerShade
    {
        get { return playerShade; }
        set { playerShade = value; }
    }
}
