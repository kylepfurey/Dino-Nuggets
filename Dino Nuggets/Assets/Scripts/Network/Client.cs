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
    public class Client : ConnectionManager
    {
        public Dictionary<uint, ReplicatedMonoBehaviour> ReplicatedComponents = new();

        public ulong ClientId => SteamClient.SteamId.Value; // Client id 0 is always the server

        public void SendRpcToServer(uint netId, string name, string json, bool reliable)
        {
            if (netId == 0)
            {
                Debug.LogError($"Outgoing rpc to net id of 0", Network.Instance.gameObject);
                return;
            }

            if (!ReplicatedComponents.TryGetValue(netId, out var replicatedComponent) || replicatedComponent == null)
            {
                Debug.LogError($"Outgoing rpc to missing component", Network.Instance.gameObject);
                return;
            }

            if (!replicatedComponent.HasRpc(name))
            {
                Debug.LogError($"Unknown outgoing rpc \"{name}\"", Network.Instance.gameObject);
                return;
            }

            if (replicatedComponent.OwnerId == 0 || replicatedComponent.OwnerId != ClientId)
            {
                Debug.LogError($"Outgoing rpc to unowned component", Network.Instance.gameObject);
                return;
            }

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

                Connection.SendMessage(message, reliable ? SendType.Reliable : SendType.Unreliable); // Send rpc to server for broadcast
            }
            catch (Exception error)
            {
                Debug.LogError($"Outgoing rpc error: {error.Message}", Network.Instance.gameObject);
            }
        }
        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            base.OnMessage(data, size, messageNum, recvTime, channel);

            try
            {
                // TODO: May be slow, check performance
                byte[] buffer = new byte[size];
                Marshal.Copy(data, buffer, 0, size);
                Network.Message message = Network.Deserialize<Network.Message>(Encoding.UTF8.GetString(buffer));

                if (!ReplicatedComponents.TryGetValue(message.NetId, out var replicatedComponent) || replicatedComponent == null)
                {
                    Debug.LogError($"Incoming rpc to missing component", Network.Instance.gameObject);
                    return;
                }

                replicatedComponent.ReceiveRpc(message.RpcName, message.Json);
            }
            catch (Exception error)
            {
                Debug.LogError($"Incoming rpc error: {error.Message}", Network.Instance.gameObject);
            }
        }
        public override void OnDisconnected(ConnectionInfo info)
        {
            if (Network.Instance.MyClient != this)
                return;

            Network.Instance.Disconnect(Network.Instance.MainMenuScene); // Server cleans up net id

            base.OnDisconnected(info);
        }
    }
}
