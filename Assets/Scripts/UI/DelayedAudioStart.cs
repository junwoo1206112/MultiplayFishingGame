using System.Collections;
using UnityEngine;

namespace MultiplayFishing.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class DelayedAudioStart : MonoBehaviour
    {
        [SerializeField] private float startDelay = 0.2f;

        private AudioSource audioSource;
        private Coroutine playRoutine;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        private void OnEnable()
        {
            playRoutine = StartCoroutine(PlayAfterDelay());
        }

        private void OnDisable()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }
        }

        private IEnumerator PlayAfterDelay()
        {
            float delay = Mathf.Max(0f, startDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }

            playRoutine = null;
        }
    }
}
