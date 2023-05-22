using UnityEngine;

public class SoundServiceSettings : ScriptableObject
{
    [SerializeField] private AnimationCurve rolloffCurveFor3DEnvironmentSound;
    public AnimationCurve RolloffCurveFor3DEnvironmentSound => rolloffCurveFor3DEnvironmentSound;
}