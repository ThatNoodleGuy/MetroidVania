using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseMaxHealth : MonoBehaviour
{
    [SerializeField]
    GameObject particles;

    [SerializeField]
    GameObject canvasUI;

    [SerializeField]
    HeartShards heartShards;

    bool used;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerController.Instance.MaxHealth >= PlayerController.Instance.MaxTotalHealth)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player") && !used)
        {
            used = true;
            StartCoroutine(ShowUI());
        }
    }

    IEnumerator ShowUI()
    {
        GameObject _particles = Instantiate(particles, transform.position, Quaternion.identity);
        Destroy(_particles, 0.5f);
        yield return new WaitForSeconds(0.5f);

        canvasUI.SetActive(true);
        heartShards.initialFillAmount = PlayerController.Instance.HeartShards * 0.25f;
        PlayerController.Instance.HeartShards++;
        heartShards.targetFillAmount = PlayerController.Instance.HeartShards * 0.25f;

        StartCoroutine(heartShards.LerpFill());

        yield return new WaitForSeconds(2.5f);
        PlayerController.Instance.UnlockedWallJump = true;
        SaveData.Instance.SavePlayerData();
        canvasUI.SetActive(false);
        Destroy(gameObject);
    }
}
