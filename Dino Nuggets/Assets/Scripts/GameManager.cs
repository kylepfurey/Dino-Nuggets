using UnityEngine;

namespace DN
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] Spreadsheet entitiesSpreadsheet;
        [SerializeField] Spreadsheet itemsSpreadsheet;
        [SerializeField] Spreadsheet craftingSpreadsheet;
        [SerializeField] Spreadsheet biomesSpreadsheet;
        [SerializeField] Table modelTable;
        [SerializeField] SfxTable sfxTable;
        [SerializeField] Table materialTable;

        public static GameManager Instance { get; private set; }
        public Spreadsheet EntitiesSpreadsheet => entitiesSpreadsheet;
        public Spreadsheet ItemsSpreadsheet => itemsSpreadsheet;
        public Spreadsheet CraftingSpreadsheet => craftingSpreadsheet;
        public Spreadsheet BiomesSpreadsheet => biomesSpreadsheet;
        public Table ModelTable => modelTable;
        public SfxTable SfxTable => sfxTable;
        public Table MaterialTable => materialTable;

        void Awake() => Instance = this;
        void OnDestroy() { if (Instance == this) Instance = null; }

        public void SetSeed(int seed) => Random.InitState(seed);
        public void SetSeed(string seed)
        {
            // Random seed
            if (string.IsNullOrEmpty(seed))
            {
                Random.InitState(new System.Random().Next());
                return;
            }

            // Integer seed
            seed = seed.Trim();
            if (int.TryParse(seed, out var result))
            {
                Random.InitState(result);
                return;
            }

            // FNV-1a hash seed
            uint hash = 2166136261;
            const uint prime = 16777619;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(seed);
            unchecked
            {
                foreach (byte b in bytes)
                {
                    hash ^= b;
                    hash *= prime;
                }
            }
            Random.InitState((int)hash);
        }
    }
}
