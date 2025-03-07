using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneChangerWorlds : MonoBehaviour
{
    public string sceneName;
    public TextMeshProUGUI levelText;

    void Start()
    {
        levelText.text = sceneName;
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }
    public void ExitApplication()
    {
        Application.Quit();
    }
}