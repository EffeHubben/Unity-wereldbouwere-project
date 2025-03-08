using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ApiWorldClient : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_Text nameText;
    public GameObject worldPrefab; // Reference to the prefab
    public Transform worldContainer; // Reference to the container\
    public Button createButton; // Reference to the create button
    public static ApiWorldClient instance { get; private set; }
    void Awake()
    {
        // hier controleren we of er al een instantie is van deze singleton
        // als dit zo is dan hoeven we geen nieuwe aan te maken en verwijderen we deze
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
    }
    private async Task<string> PerformApiCall(string url, string method, string jsonData = null, string token = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, method))
        {
            if (!string.IsNullOrEmpty(jsonData))
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError("Fout bij API-aanroep: " + request.error);
                return null;
            }
        }
    }
    public async void RegisterWorld()
    {
        if (SessionData.token != null)
        {
            var registerDto = new PostWorldRegisterRequestDto()
            {
                name = nameInput.text,
                ownerUserId = SessionData.ownerUserId,
                maxLength = 200,
                maxHeight = 200
            };
            string jsonData = JsonUtility.ToJson(registerDto);
            var response = await PerformApiCall("https://avansict2228256.azurewebsites.net/wereldbouwer", "POST", jsonData, SessionData.token);
            Debug.Log(response);
        } else
        {
            Debug.LogError("SessionData token is null");
        }
        Debug.Log(SessionData.token);
        Debug.Log(nameInput.text);

    }

    public async void LoadWorld()
    {
        if (SessionData.token != null)
        {
            string url = $"https://avansict2228256.azurewebsites.net/wereldbouwer/getwereld/{SessionData.ownerUserId}";
            var response = await PerformApiCall(url, "GET", null, SessionData.token);

            if (response == null)
            {
                Debug.LogError("API response is null");
                return;
            }

            Debug.Log("API Response: " + response);

            try
            {
                List<PostWorldLoadResponseDto> worlds = JsonConvert.DeserializeObject<List<PostWorldLoadResponseDto>>(response);
                if (worlds != null)
                {
                    int worldCount = worlds.Count;
                    for (int i = 0; i < worldCount && i < 5; i++)
                    {
                        var world = worlds[i];
                        CreateWorldPrefab(world);
                    }

                    for (int i = worldCount; i < 5; i++)
                    {
                        CreateEmptyWorldPrefab();
                    }
                }
                else
                {
                    Debug.LogError("Failed to load worlds or no worlds found.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error deserializing JSON response: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("SessionData token is null");
        }
    }

    private void CreateWorldPrefab(PostWorldLoadResponseDto world)
    {
        GameObject worldObject = Instantiate(worldPrefab, worldContainer);
        TMP_InputField inputField = worldObject.transform.Find("NameInput").GetComponent<TMP_InputField>();
        TMP_Text nameText = worldObject.transform.Find("NameText").GetComponent<TMP_Text>();
        Button loadButton = worldObject.transform.Find("LoadButton").GetComponent<Button>();
        Button createButton = worldObject.transform.Find("CreateButton").GetComponent<Button>();

        inputField.text = world.name;
        nameText.text = world.name;
        loadButton.onClick.AddListener(() => LoadSpecificWorld(world.id));

        createButton.gameObject.SetActive(false);
        inputField.gameObject.SetActive(false);
    }

    private void CreateEmptyWorldPrefab()
    {
        GameObject worldObject = Instantiate(worldPrefab, worldContainer);
        TMP_InputField inputField = worldObject.transform.Find("NameInput").GetComponent<TMP_InputField>();
        TMP_Text nameText = worldObject.transform.Find("NameText").GetComponent<TMP_Text>();
        Button createButton = worldObject.transform.Find("CreateButton").GetComponent<Button>();

        inputField.text = "";
        nameText.text = "Create New World";
        createButton.gameObject.SetActive(true);
        createButton.onClick.AddListener(() => RegisterWorld());
    }

    public void LoadSpecificWorld(string worldId)
    {
        Debug.Log($"Loading world with ID: {worldId}");
        // Implement the logic to load the specific world
    }
}