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

        public class SaveData
        {
            public float Sensitivity
            {
                get => PlayerPrefs.GetFloat("sensitivity", 1.0f);
                set
                {
                    PlayerPrefs.SetFloat("sensitivity", value);
                    PlayerPrefs.Save();
                }
            }
            public float Volume
            {
                get => PlayerPrefs.GetFloat("volume", 0.5f);
                set
                {
                    PlayerPrefs.SetFloat("volume", value);
                    PlayerPrefs.Save();
                }
            }
            public int PushToTalk // 0 = push to talk, 1 = open mic, 2 = muted
            {
                get => PlayerPrefs.GetInt("pushToTalk", 0);
                set
                {
                    PlayerPrefs.SetInt("pushToTalk", value % 3);
                    PlayerPrefs.Save();
                }
            }
        }

        [SerializeField] Spreadsheet entitiesSpreadsheet;
        [SerializeField] Spreadsheet itemsSpreadsheet;
        [SerializeField] Spreadsheet craftingSpreadsheet;
        [SerializeField] Spreadsheet biomesSpreadsheet;
        [SerializeField] Table modelTable;
        [SerializeField] SfxTable sfxTable;
        [SerializeField] Table materialTable;
        [SerializeField] Table imageTable;
        [SerializeField] Network network;

        public Spreadsheet EntitiesSpreadsheet => entitiesSpreadsheet;
        public Spreadsheet ItemsSpreadsheet => itemsSpreadsheet;
        public Spreadsheet CraftingSpreadsheet => craftingSpreadsheet;
        public Spreadsheet BiomesSpreadsheet => biomesSpreadsheet;
        public Table ModelTable => modelTable;
        public SfxTable SfxTable => sfxTable;
        public Table MaterialTable => materialTable;
        public Table ImageTable => imageTable;
        public Network Network => network;
        public SaveData Settings { get; } = new SaveData();

        void Start()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.LoadScene(Network.Instance.MainMenuScene);
        }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public static void SetSeed(int seed) => UnityEngine.Random.InitState(seed);
        public static void SetSeed(string seed)
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
