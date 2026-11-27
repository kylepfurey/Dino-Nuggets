using UnityEngine;

namespace DN
{
    public class EntityEvents : MonoBehaviour
    {
        [SerializeField] Entity entity;
        [SerializeField] Animator animator;

        void OnFootstep(Object foot) => PlayRandomSfx(entity.EntitySfx.FootstepSfx, (Transform)foot);

        void PlayRandomSfx(AudioClip[] clips, Transform pivot)
        {
            if (clips == null || clips.Length == 0)
                return;
            clips.Random().SpawnSound(pivot != null ? pivot : entity.transform);
        }
    }
}
