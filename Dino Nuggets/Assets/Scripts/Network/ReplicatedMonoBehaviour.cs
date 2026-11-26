#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace DN
{
    public abstract class ReplicatedMonoBehaviour : MonoBehaviour
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        protected class RpcAttribute : Attribute { }
        public delegate void Rpc(string json);

        Dictionary<string, Rpc> rpcMethods = new(); // name -> method
        Dictionary<Rpc, string> rpcNames = new(); // method -> name

        public uint NetId { get; private set; } = 0; // Net id 0 is not set
        public bool HasAuthority => Network.Instance.IsServer || IsOwned;
        public bool IsOwned => OwnerId != 0 && Network.Instance.MyClient != null && OwnerId == Network.Instance.MyClient.ClientId;
        public ulong OwnerId { get; private set; } = 0; // Client id 0 is always the server

        protected virtual void Awake() =>
            BindRpcs();
        public void Initialize(uint netId, ulong ownerId)
        {
            if (Network.Instance.IsServer)
                Network.Instance.Server!.ReplicatedComponents[netId] = this;

            if (Network.Instance.IsClient)
                Network.Instance.MyClient!.ReplicatedComponents[netId] = this;

            OwnerId = ownerId;
            NetId = netId;
        }
        protected virtual void OnDestroy()
        {
            if (GameManager.Instance == null || Network.Instance == null || Network.Instance.RpcHandler == null)
                return;

            if (Network.Instance.IsServer)
            {
                Network.Instance.Server!.Destroy(NetId);
                Network.Instance.Server!.ReplicatedComponents.Remove(NetId);
            }

            if (Network.Instance.IsClient)
                Network.Instance.MyClient!.ReplicatedComponents.Remove(NetId);
        }

        void BindRpcs()
        {
            Type type = GetType();
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!method.IsDefined(typeof(RpcAttribute), true))
                    continue;

                Delegate rpc = method.IsStatic ?
                    Delegate.CreateDelegate(typeof(Rpc), method, throwOnBindFailure: false) :
                    Delegate.CreateDelegate(typeof(Rpc), this, method, throwOnBindFailure: false);

                if (rpc == null || !method.Name.EndsWith("Rpc"))
                {
                    Debug.LogError($"{method.Name} is not an rpc calling convention: void MethodNameRpc(string json)", gameObject);
                    continue;
                }

                rpcMethods[method.Name] = (Rpc)rpc;
                rpcNames[(Rpc)rpc] = method.Name;
            }
        }
        public bool HasRpc(string name) => rpcMethods.ContainsKey(name);
        public async void CallRpc(Rpc? rpc, object? arg, bool reliable = true)
        {
            // Wait for net id to be set
            while (NetId == 0)
            {
                await Task.Delay(100);
                if (this == null)
                    return;
            }

            if (!HasAuthority)
            {
                Debug.LogError("Calling rpc on unowned component", gameObject);
                return;
            }

            if (rpc == null)
                return; // No-op

            if (!rpcNames.TryGetValue(rpc, out var name))
            {
                Debug.LogError($"Calling rpc without the [Rpc] attribute", gameObject);
                return;
            }

            if (Network.Instance.IsServer)
                Network.Instance.Server!.SendRpcToClients(NetId, name, Network.Serialize(arg), reliable); // Broadcast rpc as server
            else
                Network.Instance.MyClient!.SendRpcToServer(NetId, name, Network.Serialize(arg), reliable); // Send rpc to server for broadcast
        }
        public void ReceiveRpc(string name, string json)
        {
            if (!rpcMethods.TryGetValue(name, out var rpc))
            {
                Debug.LogError($"Received unknown rpc \"{name}\"", gameObject);
                return;
            }

            rpc(json);
        }
    }
}
