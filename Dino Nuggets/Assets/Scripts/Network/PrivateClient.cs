#nullable enable
using Steamworks;

namespace DN
{
    /// <summary>
    /// Server and owning client
    /// </summary>
    public class PrivateClient : ConnectionManager
    {
        PublicClient publicClient;

        public uint ClientId => publicClient.ClientId;
        public PublicClient PublicClient => publicClient;
    }
}
