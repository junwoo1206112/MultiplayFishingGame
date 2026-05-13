using UnityEngine;
using Mirror;
using MultiplayFishing.Network;

namespace MultiplayFishing.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class LobbyBgmOnHostReady : MonoBehaviour
    {
        [SerializeField] private bool loop = true;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = loop;
            audioSource.Stop();
        }

        private void OnEnable()
        {
            FishingRoomManager.NetworkStateChanged += RefreshPlayback;
            RefreshPlayback();
        }

        private void Start()
        {
            RefreshPlayback();
        }

        private void OnDisable()
        {
            FishingRoomManager.NetworkStateChanged -= RefreshPlayback;

            if (audioSource == null)
            {
                return;
            }

            audioSource.Stop();
        }

        private void RefreshPlayback()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.playOnAwake = false;
            audioSource.loop = loop;

            FishingRoomManager manager = NetworkManager.singleton as FishingRoomManager;
            bool shouldPlay = manager != null
                && NetworkServer.active
                && NetworkClient.isConnected
                && manager.IsRelayReady;

            if (shouldPlay)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }

                return;
            }

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
