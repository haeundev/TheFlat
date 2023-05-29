using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    BGM,
    Wall,
    OP,
    Radio,
    Telephone,
    
}

public class SoundSource : MonoBehaviour
{
    private static readonly List<SoundSource> Instances = new();
    [SerializeField] private int id;
    [SerializeField] private SoundType type;
    
    
    private void OnEnable()
    {
        Instances.Add(this);
    }
    
    private void OnDisable()
    {
        Instances.Remove(this);
    }
}