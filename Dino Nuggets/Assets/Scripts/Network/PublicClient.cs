#nullable enable
using Steamworks;

namespace DN
{
    /// <summary>
    /// Server and all clients
    /// </summary>
    public class PublicClient
    {
        uint clientId;
        PrivateClient? privateClient;

        public uint ClientId => clientId;
        public PrivateClient? PrivateClient => privateClient;
    }
}
