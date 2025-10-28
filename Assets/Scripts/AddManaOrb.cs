using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddManaOrb : MonoBehaviour
{
    [SerializeField]
    GameObject particles;

    [SerializeField]
    GameObject canvasUI;

    [SerializeField]
    OrbShard orbShard;

    bool used;

    void Start()
    {
        if (PlayerController.Instance.ManaOrbs >= 3)
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
        orbShard.initialFillAmount = PlayerController.Instance.OrbShard * 0.34f;
        PlayerController.Instance.OrbShard++;
        orbShard.targetFillAmount = PlayerController.Instance.OrbShard * 0.34f;

        StartCoroutine(orbShard.LerpFill());

        yield return new WaitForSeconds(2.5f);
        PlayerController.Instance.UnlockedWallJump = true;
        SaveData.Instance.SavePlayerData();
        canvasUI.SetActive(false);
        Destroy(gameObject);
    }
}
