using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SoundEffectConfig", fileName = "SoundEffect")]
public class SoundEffectConfig : ScriptableObject
{
    public AudioClip[] clips;

    public float minVolume = 0.9f;
    public float maxVolume = 1.1f;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
}
