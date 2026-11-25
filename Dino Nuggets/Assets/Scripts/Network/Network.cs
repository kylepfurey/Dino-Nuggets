#nullable enable
using System;
using UnityEngine;

namespace DN
{
    public class Network : MonoBehaviour
    {
        public static Network Instance => GameManager.Instance.Network;

        [Serializable]
        public struct Message
        {
            public uint NetId;
            public string RpcName;
            public string Json;
            public bool Reliable;
        }

        [SerializeField] int appId;

        public bool IsServer => Server != null;
        public Server? Server { get; private set; }
        public bool IsClient => MyClient != null;
        public Client? MyClient { get; private set; }
        public bool IsHost => IsServer && IsClient;

        public static string Serialize(object? obj)
        {
            if (obj == null)
                return string.Empty;

            try
            {
                return JsonUtility.ToJson(obj);
            }
            catch (Exception error)
            {
                Debug.LogError($"Serialization error: {error.Message}", Instance.gameObject);
                return string.Empty;
            }
        }
        public static T? Deserialize<T>(string? json)
        {
            if (json == null || json == string.Empty)
                return default(T);

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception error)
            {
                Debug.LogError($"Deserialization error: {error.Message}", Instance.gameObject);
                return default(T);
            }
        }
    }
}
