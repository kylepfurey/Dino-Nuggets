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

        void Update()
        {
            if (voiceChat == null)
            {
                icon.enabled = false;
                voiceChat = FindAnyObjectByType<VoiceChat>();
                return;
            }

            if (GameManager.Instance.Settings.PushToTalk)
            {
                voiceChat.SendAudio = pushToTalk.ToInputAction().IsPressed();
                icon.enabled = voiceChat.SendAudio;
            }
            else
            {
                voiceChat.SendAudio = !pushToTalk.ToInputAction().IsPressed();
                icon.enabled = voiceChat.SendAudio && SteamUser.HasVoiceData;
            }
        }
    }
}
