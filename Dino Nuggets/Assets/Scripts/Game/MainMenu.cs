using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DN
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject hostMenu;
        [SerializeField] GameObject joinMenu;
        [SerializeField] GameObject settingsMenu;
        [SerializeField] TextMeshProUGUI playerList;
        [SerializeField] Button startButton;
        [SerializeField] Button[] lobbyButtons;
        Selectable previousSelectable;
        Lobby[] lobbies;

        void Update()
        {
            if (hostMenu.gameObject.activeSelf)
            {
                playerList.text = "Players:\n";
                foreach (var client in Network.Instance.Lobby!.Value.Members)
                    playerList.text += client.Name;
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
                lobbyButtons[i].gameObject.SetActive(true);
                lobbyButtons[i].GetComponent<TextMeshProUGUI>().text = lobbies[i].GetData("name") +
                    $"{lobbies[i].MemberCount}/{lobbies[i].MaxMembers}";
                lobbyButtons[i].onClick.AddListener(async () =>
                {
                    if (!await Network.Instance.Connect(lobbies[i]))
                        return;

                    GoToLobbyMenu();
                });
            }
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
