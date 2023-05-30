using System;
using System.Collections.Generic;
using Proto.Data;
using Proto.SoundSystem;
using UnityEngine;

public enum SoundType
{
    BGM,
    Wall,
    OP,
    Radio,
    Phone,
    Ambience
}

public class SoundSource : MonoBehaviour
{
    private static readonly List<SoundSource> Instances = new();
    [SerializeField] private int id;
    [SerializeField] private SoundType type;

    private void Start()
    {
        var path = DataTableManager.SoundClips.Find(id);
        if (path != default)
        {
            StartCoroutine(SoundService.Load(path, gameObject.GetComponent<AudioSource>(), true));
        }
    }

    private void OnEnable()
    {
        Instances.Add(this);
    }
    
    private void OnDisable()
    {
        Instances.Remove(this);
    }
}