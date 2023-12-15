using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Kevin
{
    public class KevinSoundController : MonoBehaviour
    {
        [SerializeField] private SoundEffectConfig walk;
        [SerializeField] private SoundEffectConfig sprint;
        
        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private void PlaySound(SoundEffectConfig sound)
        {
            _source.pitch = Random.Range(sound.minPitch, sound.maxPitch);
            _source.PlayOneShot(sound.clips[Mathf.FloorToInt(Random.value * sound.clips.Length)],
                Random.Range(sound.minVolume, sound.maxVolume));
        }

        public void Footstep()
        {
            PlaySound(walk);
        }
    
        public void RunFootstep()
        {
            PlaySound(sprint);
        }
    }
}
