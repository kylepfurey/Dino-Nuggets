using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DN
{
    public class VoiceChatUI : MonoBehaviour
    {
        [SerializeField] Image icon;
        [SerializeField] InputActionReference pushToTalk;
        VoiceChat voiceChat;

        void FixedUpdate()
        {
            if (voiceChat == null)
            {
                icon.enabled = false;
                voiceChat = FindAnyObjectByType<VoiceChat>();
                return;
            }

            switch (GameManager.Instance.Settings.PushToTalk)
            {
                case 0:
                    voiceChat.SendAudio = pushToTalk.ToInputAction().IsPressed();
                    icon.enabled = voiceChat.SendAudio;
                    break;
                case 1:
                    voiceChat.SendAudio = !pushToTalk.ToInputAction().IsPressed();
                    icon.enabled = voiceChat.SendAudio && SteamUser.HasVoiceData;
                    break;
                case 2:
                    icon.enabled = false;
                    break;
            }
        }
    }
}
