#nullable enable
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DN
{
    [Serializable]
    public struct InstantiateMessage
    {
        public uint NetId;
        public ulong ClientId;
        public string PrefabName;
        public uint ParentNetId; // Net id 0 is no parent
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
    }

    [Serializable]
    public struct KickMessage
    {
        public ulong ClientId;
        public string Reason;
    }

    public class RpcHandler : ReplicatedMonoBehaviour
    {
        [Rpc]
        public void LoadSceneRpc(string json)
        {
            string? sceneName = Network.Deserialize<string?>(json);
            if (!string.IsNullOrEmpty(sceneName))
                SceneManager.LoadScene(sceneName);
        }

        [Rpc]
        public void InstantiateRpc(string json)
        {
            InstantiateMessage? message = Network.Deserialize<InstantiateMessage?>(json);
            if (message == null)
                return;

            if (!Network.Instance.Prefabs.TryGetValue(message.Value.PrefabName, out var prefab))
            {
                Debug.LogError($"Unknown prefab \"{message.Value.PrefabName}\"", this.gameObject);
                return;
            }

            GameObject gameObject = Instantiate(prefab);
            gameObject.name = message.Value.PrefabName;

            ReplicatedMonoBehaviour component = gameObject.GetComponent<ReplicatedMonoBehaviour>();
            if (component == null)
            {
                Debug.LogError($"Prefab \"{message.Value.PrefabName}\" has no root ReplicatedMonoBehaviour", gameObject);
                return;
            }

            Transform transform = gameObject.transform;
            if (message.Value.ParentNetId == 0)
                transform.parent = null;
            else if (Network.Instance.ReplicatedComponents.TryGetValue(message.Value.ParentNetId, out var replicatedComponent))
                transform.parent = replicatedComponent.transform;
            else
            {
                Debug.LogError($"Insantiated prefab \"{message.Value.PrefabName}\" attached to missing component", gameObject);
                transform.parent = null;
            }

            transform.localPosition = message.Value.LocalPosition;
            transform.localRotation = message.Value.LocalRotation;

            component.Initialize(message.Value.NetId, message.Value.ClientId);
        }

        [Rpc]
        public void DestroyRpc(string json)
        {
            uint? netId = Network.Deserialize<uint?>(json);
            if (netId == null)
                return;

            if (Network.Instance.ReplicatedComponents.TryGetValue(netId.Value, out var replicatedComponent))
                Destroy(replicatedComponent.gameObject);
        }

        [Rpc]
        public void KickRpc(string json)
        {
            if (Network.Instance.IsServer)
                return;

            KickMessage? message = Network.Deserialize<KickMessage?>(json);
            if (message != null && message.Value.ClientId == Network.Instance.MyClient!.ClientId)
            {
                Debug.LogError($"Kicked: {message.Value.Reason}", gameObject);
                Network.Instance.Disconnect(Network.Instance.MainMenuScene);
            }
        }
    }
}
