using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DN
{
    public class VoiceChatUI : MonoBehaviour
    {
        [SerializeField] Image icon;
        public VoiceChat VoiceChat;
        [SerializeField] InputActionReference pushToTalk;

        void Update()
        {
            if (VoiceChat == null)
            {
                icon.enabled = false;
                return;
            }

            VoiceChat.SendAudio = GameManager.Instance.Settings.PushToTalk == pushToTalk.ToInputAction().IsPressed();
            icon.enabled = VoiceChat.SendAudio && SteamUser.HasVoiceData;
        }
    }
}
