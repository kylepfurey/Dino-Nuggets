#nullable enable
using Steamworks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DN
{
    public class VoiceChat : ReplicatedMonoBehaviour
    {
        public AudioSource AudioSource;
        public bool Muted;
        Queue<float> samples = new();
        int maxSamples;
        MemoryStream pcm = new();

        public bool SendAudio
        {
            get => IsOwned && SteamUser.VoiceRecord;
            set
            {
                if (IsOwned)
                    SteamUser.VoiceRecord = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (AudioSource == null)
            {
                Debug.LogError($"Missing VoiceChat AudioSource", gameObject);
                enabled = false;
                return;
            }

            int sampleRate = (int)SteamUser.SampleRate;
            maxSamples = sampleRate / 2;
            AudioSource.clip = AudioClip.Create("VOIP", sampleRate, 1, sampleRate, true, OnReadAudio);
            AudioSource.loop = true;
            AudioSource.Play();
        }
        void Update()
        {
            if (NetId == 0 || !SendAudio || !SteamUser.HasVoiceData)
                return;

            byte[] audio = SteamUser.ReadVoiceDataBytes();
            if (audio == null || audio.Length == 0 || Muted || Network.Instance.MuteVoiceChat)
                return;

            CallRpc(SendVoiceRpc, audio, reliable: false);
        }
        protected override void OnDestroy()
        {
            if (GameManager.Instance != null && Network.Instance != null)
                SendAudio = false;
            base.OnDestroy();
        }

        void OnReadAudio(float[] data)
        {
            lock (samples)
            {
                for (int i = 0; i < data.Length; i++)
                    data[i] = samples.Count > 0 ? samples.Dequeue() : 0.0f;
            }
        }

        [Rpc]
        void SendVoiceRpc(string json)
        {
            if (!enabled || IsOwned || !Network.Instance.IsClient || Muted || Network.Instance.MuteVoiceChat)
                return;

            byte[]? audio = Network.Deserialize<byte[]?>(json);
            if (audio == null || audio.Length == 0)
                return;

            pcm.SetLength(0);
            SteamUser.DecompressVoice(audio, pcm);

            byte[] bytes = pcm.GetBuffer();
            int count = (int)pcm.Length / 2;

            lock (samples)
            {
                for (int i = 0; i < count; i++)
                    samples.Enqueue((short)(bytes[i * 2] | (bytes[i * 2 + 1] << 8)) / 32768.0f);

                while (samples.Count > maxSamples)
                    samples.Dequeue();
            }
        }
    }
}
