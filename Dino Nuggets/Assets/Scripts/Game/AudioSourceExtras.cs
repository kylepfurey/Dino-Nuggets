using UnityEngine;

namespace DN
{
    public class AudioSourceExtras : MonoBehaviour
    {
        AudioSource audioSource;

        void Awake() => audioSource = GetComponent<AudioSource>();
        void FixedUpdate() => audioSource.volume = GameManager.Instance.Settings.Volume;
    }
}
