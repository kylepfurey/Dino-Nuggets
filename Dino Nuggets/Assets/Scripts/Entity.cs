using UnityEngine;

namespace DN
{
    public class Entity : MonoBehaviour
    {
        public enum Behavior { Static, Passive, Neutral, Hostile, Projectile, }
        public enum Habitat { Land, Air, Sea, }

        [SerializeField] string id;

        EntitySfx sfx;

        public string Id => id;
        public EntitySfx Sfx => sfx;

        public void InitId(string id) { this.id = id; }
        void Start()
        {
            Spreadsheet.Row properties = GameManager.Instance.EntitiesSpreadsheet[id];
            sfx = GameManager.Instance.SfxTable.Get(properties.String("sfx_id"));
        }
    }
}
