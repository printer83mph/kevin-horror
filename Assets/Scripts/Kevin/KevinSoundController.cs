using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Kevin
{
    public class KevinSoundController : MonoBehaviour
    {
        [SerializeField] private SoundEffectConfig walk;
        [SerializeField] private SoundEffectConfig sprint;
        [SerializeField] private SoundEffectConfig whisper;
        
        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            StartCoroutine(PlaySoundLoop());
        }

        private void OnDisable()
        {
            StopCoroutine(PlaySoundLoop());
        }

        private IEnumerator PlaySoundLoop()
        {
            PlaySound(whisper);
            yield return new WaitForSeconds(Random.Range(24f, 40f));
            StartCoroutine(PlaySoundLoop());
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
