using UnityEngine;

namespace DN
{
    public class Entity : ReplicatedMonoBehaviour
    {
        public enum Behavior { Static, Passive, Neutral, Hostile, Projectile, }
        public enum Habitat { Land, Air, Sea, }

        [SerializeField] string entityId;

        EntitySfx entitySfx;

        public string EntityId
        {
            get => entityId;
            set
            {
                if (string.IsNullOrEmpty(entityId))
                    Debug.LogError($"Overwriting entity \"{gameObject.name}\"'s id of {entityId} with {value}", gameObject);
                entityId = value;
            }
        }
        public EntitySfx EntitySfx => entitySfx;

        void Start()
        {
            Spreadsheet.Row properties = GameManager.Instance.EntitiesSpreadsheet[entityId];
            entitySfx = GameManager.Instance.SfxTable.Get(properties.String("sfx_id"));
        }
    }
}
