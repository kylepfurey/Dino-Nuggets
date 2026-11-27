#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DN
{
    public class Network : MonoBehaviour
    {
        public static Network Instance => GameManager.Instance.Network;

        [Serializable]
        public struct Message
        {
            public uint NetId;
            public string RpcName;
            public string Json;
            public bool Reliable;
        }

        [SerializeField] int appId;
        [SerializeField] string mainMenuScene;
        [SerializeField] Dictionary<string, GameObject> prefabs;
        public readonly Dictionary<uint, ReplicatedMonoBehaviour> ReplicatedComponents = new();

        public string MainMenuScene => mainMenuScene;
        public Lobby? Lobby { get; private set; }
        public bool IsServer => Server != null;
        public Server? Server { get; private set; }
        public bool IsClient => MyClient != null;
        public Client? MyClient { get; private set; }
        public bool IsHost => IsServer && IsClient;
        public RpcHandler? RpcHandler { get; private set; }
        public IReadOnlyDictionary<string, GameObject> Prefabs => prefabs;
        public bool MuteVoiceChat => GameManager.Instance.Settings.PushToTalk == 2; // 0 = push to talk, 1 = open mic, 2 = muted

        void Awake()
        {
            try
            {
                SteamClient.Init((uint)appId, asyncCallbacks: false);
            }
            catch (Exception error)
            {
                Debug.LogError($"Steam error: {error.Message}", gameObject);
            }
        }
        void LateUpdate()
        {
            SteamClient.RunCallbacks();
            while (MyClient != null && MyClient.Receive() > 0) ;
            while (Server != null && Server.Receive() > 0) ;
        }
        void OnDestroy()
        {
            Disconnect();
            SteamClient.Shutdown();
        }

        public async Task<bool> Listen(int maxPlayers, bool isPublic = true)
        {
            if (Server == null)
                Server = SteamNetworkingSockets.CreateRelaySocket<Server>();

            if (Server != null)
            {
                if (Lobby == null)
                {
                    Lobby = await SteamMatchmaking.CreateLobbyAsync(maxPlayers + 1);
                    if (Lobby == null)
                    {
                        Debug.LogError("Failed to create server lobby", gameObject);
                        Server.Close();
                        Server = null;
                        return false;
                    }

                    Lobby.Value.SetData("name", SteamClient.Name);
                    Lobby.Value.SetData("hostid", SteamClient.SteamId.Value.ToString());

                    if (isPublic)
                        Lobby.Value.SetPublic();
                    else
                        Lobby.Value.SetFriendsOnly();

                    Lobby.Value.SetJoinable(true);
                }

                if (RpcHandler == null)
                    RpcHandler = gameObject.AddComponent<RpcHandler>();
                RpcHandler.Initialize(1, 0); // Bootstrap RpcHandler
            }
            else
                Debug.LogError("Failed to create server", gameObject);

            return Server != null && Lobby != null && RpcHandler != null;
        }
        public async Task<Lobby[]> GetLobbies(int maxLobbies)
        {
            return await SteamMatchmaking.LobbyList
                .WithMaxResults(maxLobbies)
                .WithSlotsAvailable(1)
                .RequestAsync();
        }
        public async Task<bool> Connect(Lobby lobby)
        {
            if (MyClient == null)
            {
                try
                {
                    Lobby = lobby;
                    if (await lobby.Join() != RoomEnter.Success)
                    {
                        Debug.LogError($"Failed to join lobby", gameObject);
                        if (!IsServer)
                        {
                            Lobby.Value.Leave();
                            Lobby = null;
                        }
                        return false;
                    }

                    MyClient = SteamNetworkingSockets.ConnectRelay<Client>(ulong.Parse(lobby.GetData("hostid")));
                    if (MyClient != null)
                    {
                        if (RpcHandler == null)
                            RpcHandler = gameObject.AddComponent<RpcHandler>();
                        RpcHandler.Initialize(1, 0); // Bootstrap RpcHandler
                    }
                    else
                    {
                        Debug.LogError("Failed to create client", gameObject);
                        if (!IsServer)
                        {
                            Lobby.Value.Leave();
                            Lobby = null;
                        }
                    }
                }
                catch (Exception error)
                {
                    Debug.LogError($"Failed to create client: {error.Message}", gameObject);
                    if (Lobby != null && !IsServer)
                    {
                        Lobby.Value.Leave();
                        Lobby = null;
                    }
                }
            }

            return MyClient != null && RpcHandler != null;
        }
        public async Task<bool> Host(int maxPlayers, bool isPublic = true)
        {
            if (!await Listen(maxPlayers - 1, isPublic))
                return false;

            if (!await Connect(Lobby!.Value))
            {
                Disconnect();
                return false;
            }

            return true;
        }
        public bool StartGame(string? loadedScene = null)
        {
            if (Server == null || Lobby == null || RpcHandler == null || Server.Connecting.Count > 0) // Wait for all players to connect
                return false;

            Lobby.Value.SetJoinable(false);
            if (!string.IsNullOrEmpty(loadedScene))
                Server.LoadScene(loadedScene);

            return true;
        }
        bool disconnecting = false;
        public void Disconnect(string? loadedScene = null)
        {
            if (disconnecting)
                return;
            disconnecting = true;

            try
            {
                if (Lobby != null)
                {
                    if (Server != null)
                        Lobby.Value.SetJoinable(false);
                    Lobby.Value.Leave();
                    Lobby = null;
                }

                var replicatedComponents = ReplicatedComponents.Values;

                if (MyClient != null)
                {
                    MyClient.Close(); // Cleans up remote objects
                    MyClient = null;
                }

                if (Server != null)
                {
                    Server.Close();
                    Server = null;
                }

                foreach (var replicatedComponent in replicatedComponents)
                    if (replicatedComponent != null && replicatedComponent.NetId != 1) // RpcHandler handled
                        Destroy(replicatedComponent.gameObject);

                if (RpcHandler != null)
                {
                    Destroy(RpcHandler);
                    RpcHandler = null;
                }

                if (!string.IsNullOrEmpty(loadedScene))
                    SceneManager.LoadScene(loadedScene);
            }
            finally
            {
                disconnecting = false;
            }
        }

        public static string Serialize(object? obj)
        {
            if (obj == null)
                return string.Empty;
            try
            {
                return JsonConvert.SerializeObject(obj, serializerSettings);
            }
            catch (Exception error)
            {
                Debug.LogError($"Serialization error: {error.Message}", Instance.gameObject);
                return string.Empty;
            }
        }
        public static T? Deserialize<T>(string? json)
        {
            if (json == null || json == string.Empty)
                return default(T);
            try
            {
                return JsonConvert.DeserializeObject<T>(json, serializerSettings);
            }
            catch (Exception error)
            {
                Debug.LogError($"Deserialization error: {error.Message}", Instance.gameObject);
                return default(T);
            }
        }
        static JsonSerializerSettings serializerSettings = new() { ContractResolver = new PublicFieldSerializer() };
        class PublicFieldSerializer : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization ms)
            {
                List<JsonProperty> properties = new();
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    JsonProperty property = base.CreateProperty(field, ms);
                    property.Readable = true;
                    property.Writable = true;
                    properties.Add(property);
                }
                return properties;
            }
        }
    }
}
