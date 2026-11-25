#nullable enable
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace DN
{
    /// <summary>
    /// Server and all clients
    /// </summary>
    public class PublicServer
    {
        Dictionary<uint, PublicClient> publicClients = new();
        Dictionary<uint, ReplicatedMonoBehaviour> replicatedComponents = new();

        public PrivateServer? PrivateServer => Network.Instance.PrivateServer;
        public IReadOnlyDictionary<uint, PublicClient> PublicClients => publicClients;

        public void BroadcastRPC(uint netId, string name, string json, bool reliable)
        {
        }
    }
}
