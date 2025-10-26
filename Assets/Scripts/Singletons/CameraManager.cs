using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
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

    protected override void Awake()
    {
        base.Awake();

        // If this is a duplicate instance being destroyed, don't initialize
        if (Instance != this)
            return;

        // Safety check for camera array
        if (allVirtualCameras == null || allVirtualCameras.Length == 0)
        {
            Debug.LogWarning("CameraManager: No virtual cameras assigned!");
            return;
        }

        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            if (allVirtualCameras[i] != null && allVirtualCameras[i].enabled)
            {
                currentCamera = allVirtualCameras[i];

                framingTransposer =
                    currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                break; // Found the current camera, no need to continue
            }
        }

        // Safety check for framingTransposer
        if (framingTransposer != null)
        {
            normalYDamp = framingTransposer.m_YDamping;
        }
    }

    protected override void OnDisable()
    {
        // Prevent duplicate instances from running cleanup code
        // Duplicates are destroyed before initialization completes
        if (Instance != this)
            return;
    }

    private void Start()
    {
        if (PlayerController.Instance == null)
        {
            Debug.LogWarning(
                "CameraManager: PlayerController.Instance is null in Start(). Cameras will not follow player."
            );
            return;
        }

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
