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
        uint nextNetId = 2; // Net id 0 is not set, net id 1 is always the RpcHandler
        public Dictionary<uint, ReplicatedMonoBehaviour> ReplicatedComponents = new();

        public override void OnConnecting(Connection connection, ConnectionInfo info)
        {
            foreach (var member in Network.Instance.Lobby!.Value.Members)
            {
                if (info.Identity.SteamId.Value == member.Id.Value)
                {
                    connection.Accept();
                    return;
                }
            }
            connection.Close();
        }
        public void SendRpcToClients(uint netId, string name, string json, bool reliable)
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

            try
            {
                // We don't have our own client and need to ensure the rpc is called on the server
                if (!Network.Instance.IsHost)
                    replicatedComponent.ReceiveRpc(name, json);

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

                if (!ReplicatedComponents.TryGetValue(message.NetId, out var replicatedComponent) || replicatedComponent == null)
                {
                    Debug.LogError($"Incoming rpc to missing component", Network.Instance.gameObject);
                    return;

                }

                if (!replicatedComponent.HasRpc(message.RpcName))
                {
                    Debug.LogError($"Unknown incoming rpc \"{message.RpcName}\"", Network.Instance.gameObject);
                    return;
                }

                if (message.NetId == 1 || replicatedComponent.OwnerId == 0 || identity.SteamId != replicatedComponent.OwnerId)
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
        public override void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            List<ReplicatedMonoBehaviour> deleted = new();
            foreach (var replicatedComponent in ReplicatedComponents.Values)
                if (replicatedComponent.OwnerId != 0 && info.Identity.SteamId == replicatedComponent.OwnerId)
                    deleted.Add(replicatedComponent);
            foreach (var replicatedComponent in deleted)
                UnityEngine.Object.Destroy(replicatedComponent.gameObject); // Cleans up net id

            connection.Close();
        }

        public void LoadScene(string sceneName) =>
            Network.Instance.RpcHandler!.CallRpc(Network.Instance.RpcHandler.LoadSceneRpc, sceneName);
        public uint Instantiate(ulong clientId, string prefabName, uint parentNetId, Vector3 localPosition, Quaternion localRotation)
        {
            uint netId = nextNetId++;
            Network.Instance.RpcHandler!.CallRpc(
                Network.Instance.RpcHandler.InstantiateRpc,
                new InstantiateMessage()
                {
                    NetId = netId,
                    ClientId = clientId,
                    PrefabName = prefabName,
                    ParentNetId = parentNetId,
                    LocalPosition = localPosition,
                    LocalRotation = localRotation
                }
            );
            return netId;
        }
        public void Destroy(uint netId) =>
            Network.Instance.RpcHandler!.CallRpc(Network.Instance.RpcHandler.DestroyRpc, netId);
        public void Kick(ulong clientId, string reason) =>
            Network.Instance.RpcHandler!.CallRpc(Network.Instance.RpcHandler.KickRpc, new KickMessage() { ClientId = clientId, Reason = reason });
    }
}
