using UnityEngine;

namespace DN
{
    public class EntityEvents : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] Entity entity;

        void OnFootstep(Object foot)
        {
            AudioClip[] clips = entity.Sfx.FootstepSfx;
            if (clips.Length == 0)
                return;
            AudioClip clip = clips.Random();

            Transform transform;
            if (foot != null)
                transform = (Transform)foot;
            else
                transform = entity.transform;

            clip.SpawnSound(transform);
        }
    }
}
