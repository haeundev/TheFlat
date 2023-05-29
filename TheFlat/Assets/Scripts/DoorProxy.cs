using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DoorProxy : MonoBehaviour
{
    private Transform _playerTransform;
    private List<Renderer> _renderers;
    private Collider _collider;
    private AudioSource _audioSource;
    private bool _isDoorOpened;
    [SerializeField] private float proxDistance = 2f;
    [SerializeField] private bool isDoorUnlocked;
    
    private void Start()
    {
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
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            var currentDistance = Vector3.Distance(transform.position, _playerTransform.position);
            if (currentDistance > proxDistance)
                return;
            Debug.Log($"{currentDistance}");
            if (isDoorUnlocked == false)
            {
                StartCoroutine(PlayAudioClip("Hand_ROCK_1"));
                _isDoorOpened = false;
            }
            else // unlocked
            {
                StartCoroutine(PlayAudioClip("Door_Open_2"));
                _isDoorOpened = true;
                _renderers.ForEach(p => p.enabled = false);
                _collider.enabled = false;
            }
        }
    }
    
    public IEnumerator PlayAudioClip(string path)
    {
        // Get and assign the AudioClips
        var handle = Addressables.LoadAssetAsync<AudioClip>(path);
        yield return handle;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            _audioSource.clip = handle.Result;
        _audioSource.Play();
        yield return new WaitWhile(() => _audioSource.isPlaying);
        Addressables.Release(handle);
    }
}