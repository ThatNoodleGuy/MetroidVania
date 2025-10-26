using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public SceneFader sceneFader;

    [SerializeField]
    private GameObject deathScreen;

    [SerializeField]
    private GameObject halfMana;

    [SerializeField]
    private GameObject fullMana;

    [SerializeField]
    private GameObject mapHandler;

    public enum ManaState
    {
        FullMana,
        HalfMana,
    }

    [SerializeField]
    private ManaState manaState;

    protected override void Awake()
    {
        base.Awake();

        sceneFader = GetComponentInChildren<SceneFader>();
    }

    public IEnumerator ActivateDeathScreenRoutine()
    {
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(sceneFader.Fade(SceneFader.FadeDirection.In));
        yield return new WaitForSeconds(0.8f);
        deathScreen.SetActive(true);
    }

    public IEnumerator DeactivateDeathScreenRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        deathScreen.SetActive(false);
        StartCoroutine(sceneFader.Fade(SceneFader.FadeDirection.Out));
    }

    public void SwitchManaState(ManaState _manaState)
    {
        switch (_manaState)
        {
            case ManaState.FullMana:
                halfMana.SetActive(false);
                fullMana.SetActive(true);
                break;
            case ManaState.HalfMana:
                halfMana.SetActive(true);
                fullMana.SetActive(false);
                break;
            default:
                break;
        }

        manaState = _manaState;
    }

    public GameObject MapHandler
    {
        get { return mapHandler; }
        set { mapHandler = value; }
    }
}
