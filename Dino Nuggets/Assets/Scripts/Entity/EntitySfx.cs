using System;
using UnityEngine;

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
}
