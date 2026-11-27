using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DN
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Serializable]
        public class SaveData
        {
            public float Sensitivity;
            public float Volume;
            public bool PushToTalk;
        }

        [SerializeField] Spreadsheet entitiesSpreadsheet;
        [SerializeField] Spreadsheet itemsSpreadsheet;
        [SerializeField] Spreadsheet craftingSpreadsheet;
        [SerializeField] Spreadsheet biomesSpreadsheet;
        [SerializeField] Table modelTable;
        [SerializeField] SfxTable sfxTable;
        [SerializeField] Table materialTable;
        [SerializeField] Network network;
        public SaveData Settings;

        public Spreadsheet EntitiesSpreadsheet => entitiesSpreadsheet;
        public Spreadsheet ItemsSpreadsheet => itemsSpreadsheet;
        public Spreadsheet CraftingSpreadsheet => craftingSpreadsheet;
        public Spreadsheet BiomesSpreadsheet => biomesSpreadsheet;
        public Table ModelTable => modelTable;
        public SfxTable SfxTable => sfxTable;
        public Table MaterialTable => materialTable;
        public Network Network => network;

        void Start()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.LoadScene(Network.Instance.MainMenuScene);
        }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public void SetSeed(int seed) => UnityEngine.Random.InitState(seed);
        public void SetSeed(string seed)
        {
            // Random seed
            if (string.IsNullOrEmpty(seed))
            {
                UnityEngine.Random.InitState(new System.Random().Next());
                return;
            }

            // Integer seed
            seed = seed.Trim();
            if (int.TryParse(seed, out var result))
            {
                UnityEngine.Random.InitState(result);
                return;
            }

            // FNV-1a hash seed
            uint hash = 2166136261;
            const uint prime = 16777619;
            byte[] bytes = Encoding.UTF8.GetBytes(seed);
            unchecked
            {
                foreach (byte b in bytes)
                {
                    hash ^= b;
                    hash *= prime;
                }
            }
            UnityEngine.Random.InitState((int)hash);
        }
    }
}
