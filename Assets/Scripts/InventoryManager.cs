using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField]
    Image heartShard;

    [SerializeField]
    Image manaShard;

    [SerializeField]
    GameObject upCast,
        sideCast,
        downCast;

    [SerializeField]
    GameObject dash,
        varJump,
        wallJump;

    private void OnEnable()
    {
        //heart shard
        heartShard.fillAmount = PlayerController.Instance.HeartShards * 0.25f;

        //mana shards
        manaShard.fillAmount = PlayerController.Instance.OrbShard * 0.34f;

        //spells
        if (PlayerController.Instance.UnlockedUpCast)
        {
            upCast.SetActive(true);
        }
        else
        {
            upCast.SetActive(false);
        }
        if (PlayerController.Instance.UnlockedSideCast)
        {
            sideCast.SetActive(true);
        }
        else
        {
            sideCast.SetActive(false);
        }
        if (PlayerController.Instance.UnlockedDownCast)
        {
            downCast.SetActive(true);
        }
        else
        {
            downCast.SetActive(false);
        }

        //abilities
        if (PlayerController.Instance.UnlockedDash)
        {
            dash.SetActive(true);
        }
        else
        {
            dash.SetActive(false);
        }
        if (PlayerController.Instance.UnlockedVarJump)
        {
            varJump.SetActive(true);
        }
        else
        {
            varJump.SetActive(false);
        }
        if (PlayerController.Instance.UnlockedWallJump)
        {
            wallJump.SetActive(true);
        }
        else
        {
            wallJump.SetActive(false);
        }
    }
}
