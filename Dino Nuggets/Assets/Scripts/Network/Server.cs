#nullable enable
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace DN
{
    public class Server : SocketManager
    {
        Dictionary<uint, Client> clients = new();
        uint nextNetId = 1;
        Dictionary<uint, ReplicatedMonoBehaviour> replicatedComponents = new();

        public IReadOnlyDictionary<uint, Client> Clients => clients;

        public void SendRpcToClients(uint netId, string name, string json, bool reliable)
        {
            if (!replicatedComponents.TryGetValue(netId, out var replicatedComponent) || replicatedComponent == null)
            {
                Debug.LogError($"Outgoing rpc to missing component", Network.Instance.gameObject);
                return;
            }

            if (!replicatedComponent.HasRpc(name))
            {
                Debug.LogError($"Unknown outgoing rpc \"{name}\"", Network.Instance.gameObject);
                return;
            }

            // We don't have our own client and need to ensure the rpc is called on the server
            if (!Network.Instance.IsHost)
                replicatedComponent.ReceiveRpc(name, json);

            try
            {

                // TODO: May be slow, check performance
                byte[] message = Encoding.UTF8.GetBytes(
                    Network.Serialize(
                        new Network.Message()
                        {
                            NetId = netId,
                            RpcName = name,
                            Json = json,
                            Reliable = reliable
                        }
                    )
                );

                foreach (var client in Connected)
                    client.SendMessage(message, reliable ? SendType.Reliable : SendType.Unreliable); // Broadcast rpc to clients
            }
            catch (Exception error)
            {
                Debug.LogError($"Outgoing rpc error: {error.Message}", Network.Instance.gameObject);
            }
        }
        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);

            try
            {
                // TODO: May be slow, check performance
                byte[] buffer = new byte[size];
                Marshal.Copy(data, buffer, 0, size);
                Network.Message message = Network.Deserialize<Network.Message>(Encoding.UTF8.GetString(buffer));

                if (!replicatedComponents.TryGetValue(message.NetId, out var replicatedComponent) || replicatedComponent == null)
                {
                    Debug.LogError($"Incoming rpc to missing component", Network.Instance.gameObject);
                    return;

                }

                if (!replicatedComponent.HasRpc(message.RpcName))
                {
                    Debug.LogError($"Unknown incoming rpc \"{message.RpcName}\"", Network.Instance.gameObject);
                    return;
                }

                if (replicatedComponent.Owner == null || connection.Id != replicatedComponent.Owner!.ClientId)
                {
                    Debug.LogError($"Incoming rpc to component not owned by client", Network.Instance.gameObject);
                    return;
                }

                // We don't have our own client and need to ensure the rpc is called on the server
                if (!Network.Instance.IsHost)
                    replicatedComponent.ReceiveRpc(message.RpcName, message.Json);

                foreach (var client in Connected)
                    client.SendMessage(data, size, message.Reliable ? SendType.Reliable : SendType.Unreliable); // Broadcast rpc to clients
            }
            catch (Exception error)
            {
                Debug.LogError($"Incoming rpc error: {error.Message}", Network.Instance.gameObject);
            }
        }

        public uint GetNextNetId() => nextNetId++;
    }
}
