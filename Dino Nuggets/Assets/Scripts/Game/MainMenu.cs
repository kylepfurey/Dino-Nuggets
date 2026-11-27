using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DN
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] Transform camera;
        [SerializeField] Vector3 deltaRotation;

        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject hostMenu;
        [SerializeField] GameObject joinMenu;
        [SerializeField] GameObject settingsMenu;

        [SerializeField] TextMeshProUGUI playerList;
        [SerializeField] TMP_InputField seedInput;
        [SerializeField] Button startButton;

        [SerializeField] Button[] lobbyButtons;
        [SerializeField] Slider sensitivitySlider;
        [SerializeField] Slider volumeSlider;
        [SerializeField] TextMeshProUGUI pushToTalkText;

        [SerializeField] InputActionReference backButton;
        [SerializeField] AudioClip backSfx;

        Selectable previousSelectable;
        Lobby[] lobbies;

        void Start()
        {
            seedInput.text = Random.Range(0, int.MaxValue).ToString();
            sensitivitySlider.value = GameManager.Instance.Settings.Sensitivity;
            volumeSlider.value = GameManager.Instance.Settings.Volume;
            pushToTalkText.text = GameManager.Instance.Settings.PushToTalk ? "Push To Talk" : "Open Mic";
        }
        void Update()
        {
            camera.Rotate(deltaRotation * Time.deltaTime);

            if (backButton.ToInputAction().WasPressedThisFrame() &&
                EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() == null)
            {
                backSfx.SpawnSound();

                if (mainMenu.activeSelf)
                    QuitGame();
                else
                    GoToMainMenu();
            }
        }
        void FixedUpdate()
        {
            if (hostMenu.activeSelf)
            {
                playerList.text = "Players:\n";
                foreach (var client in Network.Instance.Lobby!.Value.Members)
                    playerList.text += client.Name;

                seedInput.gameObject.SetActive(Network.Instance.IsHost);
                startButton.gameObject.SetActive(Network.Instance.IsHost);
            }
        }

        public void GoToMainMenu()
        {
            Network.Instance.Disconnect(null);

            mainMenu.SetActive(true);
            hostMenu.SetActive(false);
            joinMenu.SetActive(false);
            settingsMenu.SetActive(false);

            ResetSelectable(null);
        }
        public async void GoToHostMenu(int maxPlayers)
        {
            if (!await Network.Instance.Host(maxPlayers))
                return;

            mainMenu.SetActive(false);
            hostMenu.SetActive(true);
            joinMenu.SetActive(false);
            settingsMenu.SetActive(false);

            ResetSelectable(hostMenu);
        }
        public async void GoToJoinMenu(int maxLobbies)
        {
            RefreshLobbyList();

            mainMenu.SetActive(false);
            hostMenu.SetActive(false);
            joinMenu.SetActive(true);
            settingsMenu.SetActive(false);

            ResetSelectable(joinMenu);
        }
        public void GoToLobbyMenu()
        {
            mainMenu.SetActive(false);
            hostMenu.SetActive(true);
            joinMenu.SetActive(false);
            settingsMenu.SetActive(false);

            ResetSelectable(hostMenu);
        }
        public void GoToSettingsMenu()
        {
            mainMenu.SetActive(false);
            hostMenu.SetActive(false);
            joinMenu.SetActive(false);
            settingsMenu.SetActive(true);

            ResetSelectable(settingsMenu);
        }

        public void StartGame(string gameScene)
        {
            GameManager.Instance.SetSeed(seedInput.text);

            if (!Network.Instance.StartGame(gameScene))
                return;

            mainMenu.SetActive(false);
            hostMenu.SetActive(false);
            joinMenu.SetActive(false);
            settingsMenu.SetActive(false);
        }
        public async void RefreshLobbyList()
        {
            foreach (var lobbyButton in lobbyButtons)
            {
                lobbyButton.gameObject.SetActive(false);
                lobbyButton.onClick.RemoveAllListeners();
            }

            lobbies = await Network.Instance.GetLobbies(lobbyButtons.Length);
            if (lobbies == null)
                return;

            for (int i = 0; i < lobbies.Length; ++i)
            {
                Button lobbyButton = lobbyButtons[i];
                Lobby lobby = lobbies[i];
                lobbyButton.gameObject.SetActive(true);
                lobbyButton.GetComponentInChildren<TextMeshProUGUI>().text = lobby.GetData("name") +
                    $" {lobby.MemberCount}/{lobby.MaxMembers}";
                lobbyButton.onClick.AddListener(async () =>
                {
                    if (!await Network.Instance.Connect(lobby))
                        return;

                    GoToLobbyMenu();
                });
            }
        }
        public void SetSensitivity(Slider slider) => GameManager.Instance.Settings.Sensitivity = slider.value;
        public void SetVolume(Slider slider) => GameManager.Instance.Settings.Volume = slider.value;
        public void TogglePushToTalk()
        {
            GameManager.Instance.Settings.PushToTalk = !GameManager.Instance.Settings.PushToTalk;
            pushToTalkText.text = GameManager.Instance.Settings.PushToTalk ? "Push To Talk" : "Open Mic";
        }
        public void QuitGame() => Application.Quit();

        void ResetSelectable(GameObject menu)
        {
            if (menu != null)
            {
                previousSelectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

                Selectable selectable = menu.GetComponentInChildren<Selectable>(true);
                if (selectable != null)
                    selectable.Select();
            }
            else
                previousSelectable.Select();
        }
    }
}
