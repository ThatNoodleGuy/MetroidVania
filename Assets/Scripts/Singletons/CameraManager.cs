using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    CinemachineVirtualCamera[] allVirtualCameras;

    private CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;

    [Header("Y Damping Settings for Player Jump/Fall:")]
    [SerializeField]
    private float panAmount = 0.1f;

    [SerializeField]
    private float panTime = 0.2f;

    [SerializeField]
    private float playerFallSpeedTheshold = -10;

    [SerializeField]
    private bool isLerpingYDamping;

    [SerializeField]
    private bool hasLerpedYDamping;

    private float normalYDamp;

    public static CameraManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            if (allVirtualCameras[i].enabled)
            {
                currentCamera = allVirtualCameras[i];

                framingTransposer =
                    currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        normalYDamp = framingTransposer.m_YDamping;
    }

    private void Start()
    {
        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            allVirtualCameras[i].Follow = PlayerController.Instance.transform;
        }
    }

    public void SwapCamera(CinemachineVirtualCamera _newCam)
    {
        currentCamera.enabled = false;
        currentCamera = _newCam;
        currentCamera.enabled = true;
    }

    public IEnumerator LerpYDamping(bool _isPlayerFalling)
    {
        isLerpingYDamping = true;
        //take start y damp amount
        float _startYDamp = framingTransposer.m_YDamping;
        float _endYDamp = 0;
        //determine end damp amount
        if (_isPlayerFalling)
        {
            _endYDamp = panAmount;
            hasLerpedYDamping = true;
        }
        else
        {
            _endYDamp = normalYDamp;
        }
        //lerp panAmount
        float _timer = 0;
        while (_timer < panTime)
        {
            _timer += Time.deltaTime;
            float _lerpedPanAmount = Mathf.Lerp(_startYDamp, _endYDamp, (_timer / panTime));
            framingTransposer.m_YDamping = _lerpedPanAmount;
            yield return null;
        }
        isLerpingYDamping = false;
    }

    public float PlayerFallSpeedTheshold
    {
        get { return playerFallSpeedTheshold; }
        set { playerFallSpeedTheshold = value; }
    }

    public bool HasLerpedYDamping
    {
        get { return hasLerpedYDamping; }
        set { hasLerpedYDamping = value; }
    }

    public bool IsLerpingYDamping
    {
        get { return isLerpingYDamping; }
        set { isLerpingYDamping = value; }
    }

    public float PanAmount
    {
        get { return panAmount; }
        set { panAmount = value; }
    }

    public float PanTime
    {
        get { return panTime; }
        set { panTime = value; }
    }
}
