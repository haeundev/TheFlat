using System.Collections.Generic;
using System.Linq;
using Proto.SoundSystem;
using UI;
using UnityEngine;

public class DoorProxy : MonoBehaviour
{
    private Transform _playerTransform;
    private List<Renderer> _renderers;
    private Collider _collider;
    private AudioSource _audioSource;
    private UI_Popup_NumberLock_Controller _quizUI;
    private bool _isDoorOpened;
    [SerializeField] private float proxDistance = 2f;
    [SerializeField] private bool isDoorUnlocked;
    [SerializeField] private GameObject light;
    
    private void Start()
    {
        light.SetActive(isDoorUnlocked);
        _playerTransform = GameObject.FindWithTag("Player").transform;
        _collider = GetComponentInChildren<Collider>();
        _collider.enabled = true;
        _renderers = GetComponentsInChildren<Renderer>().ToList();
        _audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            isDoorUnlocked = !isDoorUnlocked;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var currentDistance = Vector3.Distance(transform.position, _playerTransform.position);
            if (currentDistance > proxDistance)
                return;
            Debug.Log($"{currentDistance}");
            if (isDoorUnlocked == false) // quiz not solved
            {
                StartCoroutine(SoundService.PlaySimple("Hand_ROCK_1", _audioSource));
                _isDoorOpened = false;
                UIWindowService.OpenWindow<UI_Popup_NumberLock_Controller>(ui =>
                {
                    _quizUI = ui;
                    ui.Window.OnUnlockQuizSuccess += OnUnlockQuizSuccess;
                });
            }
            else // unlocked
            {

            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _quizUI?.Close();
        }

    }

    private void OnUnlockQuizSuccess()
    {
        UnlockDoor();
    }

    private void UnlockDoor()
    {
        isDoorUnlocked = true;
        StartCoroutine(SoundService.PlaySimple("Door_Open_2", _audioSource));
        _isDoorOpened = true;
        _renderers.ForEach(p => p.enabled = false);
        _collider.enabled = false;
        _quizUI.Close();
        light.SetActive(isDoorUnlocked);
    }
}