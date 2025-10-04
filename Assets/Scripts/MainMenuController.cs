using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public InputField aliasInput;
    public InputField ipInput;
    public Button createButton;
    public Button joinButton;

    void Start()
    {
        createButton.onClick.AddListener(CreateGame);
        joinButton.onClick.AddListener(JoinGame);
    }

    void CreateGame()
    {
        PlayerPrefs.SetString("Alias", aliasInput.text);
        PlayerPrefs.SetString("Role", "Server");
        SceneManager.LoadScene("GameScene");
    }

    void JoinGame()
    {
        PlayerPrefs.SetString("Alias", aliasInput.text);
        PlayerPrefs.SetString("Role", "Client");
        PlayerPrefs.SetString("ServerIP", ipInput.text);
        SceneManager.LoadScene("GameScene");
    }
}
