#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DN
{
    public abstract class ReplicatedMonoBehaviour : MonoBehaviour
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        protected class RpcAttribute : Attribute { }
        public delegate void Rpc(string json);

        uint netId = 0;
        Dictionary<string, Rpc> rpcMethods = new(); // name -> method
        Dictionary<Rpc, string> rpcNames = new(); // method -> name

        public uint NetId => netId;
        public bool IsOwned => Network.Instance.IsServer || Owner == Network.Instance.MyClient;
        public Client? Owner { get; private set; }

        protected virtual void Awake() => BindRpcs();

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

                if (rpc == null)
                {
                    Debug.LogError($"{method.Name} is not an rpc calling convention: void (string json)", gameObject);
                    continue;
                }

                rpcMethods[method.Name] = (Rpc)rpc;
                rpcNames[(Rpc)rpc] = method.Name;
            }
        }
        public bool HasRpc(string name) => rpcMethods.ContainsKey(name);
        public void CallRpc(Rpc? rpc, object? arg, bool reliable = true)
        {
            if (!IsOwned)
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
                Network.Instance.Server!.SendRpcToClients(netId, name, Network.Serialize(arg), reliable); // Broadcast rpc as server
            else
                Network.Instance.MyClient!.SendRpcToServer(netId, name, Network.Serialize(arg), reliable); // Send rpc to server for broadcast
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
