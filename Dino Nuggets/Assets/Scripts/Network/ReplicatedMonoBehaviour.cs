#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DN
{
    public abstract class ReplicatedMonoBehaviour : MonoBehaviour
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        protected class RPCAttribute : Attribute { }
        public delegate void RPC(string json);

        uint netId;
        Dictionary<string, RPC> serverRPCs = new(); // name -> method
        Dictionary<RPC, string> clientRPCs = new(); // method -> name

        public uint NetId => netId;
        public PublicClient? OwningClient { get; private set; }
        public bool HasOwnership => Network.Instance.IsServer || OwningClient == Network.Instance.MyPublicClient;

        protected virtual void Awake() => BindRPCs();

        void BindRPCs()
        {
            Type type = GetType();
            foreach (var method in type.GetMethods())
            {
                if (!method.IsDefined(typeof(RPCAttribute), true))
                    continue;

                Delegate callback = method.IsStatic ?
                    Delegate.CreateDelegate(typeof(RPC), method, throwOnBindFailure: false) :
                    Delegate.CreateDelegate(typeof(RPC), this, method, throwOnBindFailure: false);

                if (callback == null)
                {
                    Debug.LogError($"{method.Name} is not an RPC calling convention: void RPC(string json)", gameObject);
                    continue;
                }

                serverRPCs[method.Name] = (RPC)callback;
                clientRPCs[(RPC)callback] = method.Name;
            }
        }

        public void CallRPC(RPC rpc, object arg, bool reliable = true)
        {
            if (!HasOwnership)
            {
                Debug.LogError("Calling RPC on unowned component", gameObject);
                return;
            }

            if (rpc == null)
                return; // No-op

            if (!clientRPCs.TryGetValue(rpc, out var name))
            {
                Debug.LogError($"Calling an RPC without the [RPC] attribute", gameObject);
                return;
            }

            Network.Instance.PublicServer.BroadcastRPC(netId, name, Network.Serialize(arg), reliable);
        }

        public void ReceiveRPC(string name, string json)
        {
            if (!serverRPCs.TryGetValue(name, out var rpc))
            {
                Debug.LogError($"Received unknown RPC \"{name}\"", gameObject);
                return;
            }

            rpc(json);
        }
    }
}
