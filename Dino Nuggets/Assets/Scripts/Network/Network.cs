#nullable enable
using System;
using UnityEngine;

namespace DN
{
    public class Network : MonoBehaviour
    {
        public static Network Instance => GameManager.Instance.Network;

        public bool IsServer => PrivateServer != null;
        public PrivateServer? PrivateServer => PublicServer.PrivateServer;
        public PublicServer PublicServer { get; private set; }
        public PrivateClient? MyPrivateClient { get; private set; }
        public PublicClient? MyPublicClient => MyPrivateClient?.PublicClient;

        public static string Serialize(object obj)
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
        public static T? Deserialize<T>(string json)
        {
            if (json == string.Empty)
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
