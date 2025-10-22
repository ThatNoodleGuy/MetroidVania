using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField]
    CinemachineVirtualCamera newCamera;

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.GetComponent<PlayerController>())
        {
            CameraManager.Instance.SwapCamera(newCamera);
        }
    }
}
