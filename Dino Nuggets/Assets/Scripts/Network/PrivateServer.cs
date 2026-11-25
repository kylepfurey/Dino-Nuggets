#nullable enable
using System.Collections.Generic;
using Steamworks;

namespace DN
{
    /// <summary>
    /// Server-only
    /// </summary>
    public class PrivateServer : SocketManager
    {
        Dictionary<uint, PrivateClient> privateClients = new();

        public PublicServer PublicServer => Network.Instance.PublicServer;
        public IReadOnlyDictionary<uint, PrivateClient> PrivateClients => privateClients;
        public IReadOnlyDictionary<uint, PublicClient> PublicClients => PublicServer.PublicClients;
    }
}
