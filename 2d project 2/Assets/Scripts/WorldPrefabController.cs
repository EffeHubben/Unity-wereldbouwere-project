using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WorldPrefabController : MonoBehaviour
{
    public ApiWorldClient apiManager;
    public TMP_InputField inputName;
    public TMP_Text worldName;
    public Button loadWorldButton;
    public Button createWorldButton;
    public Button deleteWorldButton;

    public void Initialize(ApiWorldClient manager, PostWorldLoadResponseDto world = null)
    {
        apiManager = manager;

        if (world != null)
        {
            inputName.text = world.name;
            worldName.text = world.name;

            // Stel de onClick listener in voor de Load World button
            loadWorldButton.onClick.AddListener(() => apiManager.LoadSpecificWorld(world.id));
            deleteWorldButton.onClick.AddListener(() => apiManager.DeleteSpecificWorld(world.id));

            createWorldButton.gameObject.SetActive(false);
            inputName.gameObject.SetActive(false);
        }
        else
        {
            inputName.text = "";
            worldName.text = "Create New World";

            // Stel de onClick listener in voor de Create World button
            createWorldButton.onClick.AddListener(() => apiManager.RegisterWorld(this));

            loadWorldButton.gameObject.SetActive(false);
            createWorldButton.gameObject.SetActive(true);
            deleteWorldButton.gameObject.SetActive(false);
        }
    }
}