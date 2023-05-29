using System;
using UnityEngine;

public class Footstep : MonoBehaviour
{
    private bool _isWalking;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)
            || Input.GetKeyDown(KeyCode.A)
            || Input.GetKeyDown(KeyCode.S)
            || Input.GetKeyDown(KeyCode.D))
        {
            _isWalking = true;
            _audioSource.enabled = true;
        }

        if (Input.GetKeyUp(KeyCode.W)
            || Input.GetKeyUp(KeyCode.A)
            || Input.GetKeyUp(KeyCode.S)
            || Input.GetKeyUp(KeyCode.D))
        {
            _isWalking = false;
            _audioSource.enabled = false;
        }
    }
}