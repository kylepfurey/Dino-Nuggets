#nullable enable
using Steamworks;
using System;
using UnityEngine.Events;

namespace DN
{
    public class TextChat : ReplicatedMonoBehaviour
    {
        public UnityEvent<string, string> OnReceiveMessage;

        public void SendChat(string text)
        {
            ChatMessage message = new()
            {
                Name = Network.Instance.IsClient ? SteamClient.Name : "Server",
                Text = text
            };
            CallRpc(SendChatRpc, message);
        }

        [Rpc]
        void SendChatRpc(string json)
        {
            ChatMessage? message = Network.Deserialize<ChatMessage?>(json);
            if (message == null || !enabled)
                return;

            OnReceiveMessage.Invoke(message.Value.Name, message.Value.Text);
        }
        [Serializable] struct ChatMessage { public string Name; public string Text; }
    }
}
