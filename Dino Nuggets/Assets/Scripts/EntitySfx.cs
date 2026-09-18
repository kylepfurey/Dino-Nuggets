using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DN
{
    [Serializable]
    public class EntitySfx
    {
        [SerializeField] AudioClip[] idleSfx;
        [SerializeField] AudioClip[] footstepSfx;
        [SerializeField] AudioClip[] perceptionSfx;
        [SerializeField] AudioClip[] hurtSfx;
        [SerializeField] AudioClip[] deathSfx;

        public AudioClip[] IdleSfx => idleSfx;
        public AudioClip[] FootstepSfx => footstepSfx;
        public AudioClip[] PerceptionSfx => perceptionSfx;
        public AudioClip[] HurtSfx => hurtSfx;
        public AudioClip[] DeathSfx => deathSfx;
    }

    [CreateAssetMenu(fileName = "SfxTable", menuName = "Scriptable Objects/Sfx Table")]
    public class SfxTable : ScriptableObject
    {
        [SerializeField] Dictionary<string, EntitySfx> table;
        public EntitySfx Get(string id)
        {
            if (!table.TryGetValue(id.Trim().ToLower(), out var result))
                return table["default"];
            return result;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Dictionary<string, EntitySfx> table = new();
            foreach (var pair in this.table)
                table[pair.Key.Trim().ToLower()] = pair.Value;
            this.table = table;
            if (!this.table.ContainsKey("default"))
                this.table["default"] = null;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
