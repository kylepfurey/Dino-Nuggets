using UnityEngine;

namespace DN
{
    public class Entity : ReplicatedMonoBehaviour
    {
        public enum Behavior { Static, Passive, Neutral, Hostile, Projectile, }
        public enum Habitat { Land, Air, Sea, }

        [SerializeField] string entityId;
        EntitySfx entitySfx;

        public string EntityId => entityId;
        public EntitySfx EntitySfx => entitySfx;

        [Rpc]
        void SetEntityId(string json)
        {
            entityId = Network.Deserialize<string>(json);

            Spreadsheet.Row properties = GameManager.Instance.EntitiesSpreadsheet[entityId];
            entitySfx = GameManager.Instance.SfxTable.Get(properties.String("sfx_id"));
        }
    }
}
