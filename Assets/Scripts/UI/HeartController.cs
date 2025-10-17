using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    [SerializeField] private Transform heartsParent;
    [SerializeField] private GameObject heartsContainerPrefab;

    private PlayerController playerController;
    private GameObject[] heartContainers;
    private Image[] heartFills;

    private void Start()
    {
        playerController = PlayerController.Instance;
        heartContainers = new GameObject[playerController.GetMaxHealth()];
        heartFills = new Image[playerController.GetMaxHealth()];

        PlayerController.Instance.OnHealthChangedCallback += UpdateHeartsHUD;

        InstantiateHeartContainers();
        UpdateHeartsHUD();
    }

    private void SetHeartContainers()
    {
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < playerController.GetMaxHealth())
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }
    }

    private void SetFilledHeart()
    {
        for (int i = 0; i < heartFills.Length; i++)
        {
            if (i < playerController.GetHealth())
            {
                heartFills[i].fillAmount = 1f;
            }
            else
            {
                heartFills[i].fillAmount = 0f;
            }
        }
    }
    
    private void InstantiateHeartContainers()
    {
        for (int i = 0; i < playerController.GetMaxHealth(); i++)
        {
            GameObject temp = Instantiate(heartsContainerPrefab);
            temp.transform.SetParent(heartsParent, false);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();
        }
    }

    private void UpdateHeartsHUD()
    {
        SetHeartContainers();
        SetFilledHeart();
    }
}
