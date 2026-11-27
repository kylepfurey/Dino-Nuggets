using UnityEngine;

namespace DN
{
    public class EntityEvents : MonoBehaviour
    {
        Entity entity;
        [SerializeField] Animator animator;

        void Awake() => entity = transform.parent.GetComponent<Entity>();

        void OnFootstep(Object foot) => PlayRandomSfx(entity.Sfx.FootstepSfx, (Transform)foot);

        void PlayRandomSfx(AudioClip[] clips, Transform pivot)
        {
            if (clips == null || clips.Length == 0)
                return;
            clips.Random().SpawnSound(pivot != null ? pivot : entity.transform);
        }
    }
}
