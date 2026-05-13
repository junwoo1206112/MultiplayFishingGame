using UnityEngine;

namespace MultiplayFishing.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class DelayedAudioStart : MonoBehaviour
    {
        [Min(0f)]
        [SerializeField] private float startDelay = 0.3f;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.Stop();
        }

        private void Start()
        {
            PlayDelayed();
        }

        private void OnDisable()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.Stop();
        }

        private void PlayDelayed()
        {
            if (audioSource == null || audioSource.clip == null)
            {
                return;
            }

            audioSource.playOnAwake = false;
            audioSource.Stop();
            audioSource.PlayDelayed(Mathf.Max(0f, startDelay));
        }
    }
}
