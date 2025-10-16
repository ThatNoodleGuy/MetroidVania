using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float followSpeed = 0.1f;
    [SerializeField] private Vector3 offset;

    private void Update()
    {
        var player = PlayerController.Instance.gameObject;
        transform.position = Vector3.Lerp(transform.position, player.transform.position + offset, followSpeed);
    }
}