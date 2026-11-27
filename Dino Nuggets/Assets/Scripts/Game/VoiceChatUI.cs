using Steamworks;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DN
{
    public class VoiceChatUI : MonoBehaviour
    {
        VoiceChat voiceChat;
        [SerializeField] Image icon;
        [SerializeField] InputActionReference pushToTalk;

        void FixedUpdate()
        {
            if (voiceChat == null)
            {
                icon.enabled = false;
                voiceChat = Network.Instance.ReplicatedComponents.Values
                    .Where((component) => component is VoiceChat && component.IsOwned)
                    .FirstOrDefault()
                    as VoiceChat;
                return;
            }

            if (voiceChat.Muted)
            {
                icon.enabled = false;
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
