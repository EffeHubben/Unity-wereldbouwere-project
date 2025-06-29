using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class ApiWorldClient : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_Text nameText;
    public GameObject worldPrefab; // Reference to the prefab
    public RectTransform worldContainer; // Reference to the container\
    public Button createButton; // Reference to the create button\
    public int minWorldNameLength = 1;
    public int maxWorldNameLength = 25;
    public TMP_Text waarschuwingText;
    public GameObject waarschuwingBord;
    public static ApiWorldClient instance { get; private set; }
    void Awake()
    {
        LoadWorld();
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
    public async void RegisterWorld(WorldPrefabController controller)
    {
        string sanitizedWorldName = SanitizeInput(controller.inputName.text);
        Debug.Log($"InputName: {sanitizedWorldName}");

        // Controleer de lengte van de wereldnaam
        if (sanitizedWorldName.Length < minWorldNameLength || sanitizedWorldName.Length > maxWorldNameLength)
        {
            Debug.LogError($"Wereldnaam moet tussen {minWorldNameLength} en {maxWorldNameLength} karakters lang zijn.");
            return; // Stop de registratie als de lengte niet klopt
        }

        if (SessionData.token != null)
        {

            if (await WorldNameExists(sanitizedWorldName, SessionData.ownerUserId))
            {
                Debug.LogError($"Wereldnaam '{sanitizedWorldName}' bestaat al voor deze gebruiker.");
                waarschuwingText.text = $"Wereld naam '{sanitizedWorldName}' bestaat al voor deze account, gebruik een andere naam";
                waarschuwingBord.SetActive(true);
                return;
            }

            waarschuwingBord.SetActive(false);
            waarschuwingText.text = "";

            var registerDto = new PostWorldRegisterRequestDto()
            {
                name = sanitizedWorldName,
                ownerUserId = SessionData.ownerUserId,
                maxLength = 200,
                maxHeight = 200
            };
            string jsonData = JsonUtility.ToJson(registerDto);
            var response = await PerformApiCall("https://localhost:7015/wereldbouwer", "POST", jsonData, SessionData.token);
            Debug.Log(response);
        } else
        {
            Debug.LogError("SessionData token is null");
        }
        Debug.Log(SessionData.token);
        Debug.Log(nameInput.text);
        LoadWorld();
    }

    private async Task<bool> WorldNameExists(string worldName, string ownerUserId)
    {
        string url = $"https://localhost:7015/wereldbouwer/getwereld/{ownerUserId}";
        var response = await PerformApiCall(url, "GET", null, SessionData.token);

        if (response == null)
        {
            Debug.LogError("API response is null bij het controleren van de wereldnaam.");
            return false; // Ga ervan uit dat de naam niet bestaat bij een fout
        }

        try
        {
            List<PostWorldLoadResponseDto> worlds = JsonConvert.DeserializeObject<List<PostWorldLoadResponseDto>>(response);
            if (worlds != null)
            {
                foreach (var world in worlds)
                {
                    if (world.name == worldName)
                    {
                        return true; // De naam bestaat al
                    }
                }
            }
            return false; // De naam bestaat niet
        }
        catch (Exception ex)
        {
            Debug.LogError("Error deserializing JSON response bij het controleren van de wereldnaam: " + ex.Message);
            return false; // Ga ervan uit dat de naam niet bestaat bij een fout
        }
    }

    public async Task LoadWorld()
    {
        if (SessionData.token != null)
        {
            foreach (Transform child in worldContainer)
            {
                Destroy(child.gameObject);
            }
            string url = $"https://localhost:7015/wereldbouwer/getwereld/{SessionData.ownerUserId}";
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
        WorldPrefabController controller = worldObject.GetComponent<WorldPrefabController>();
        controller.Initialize(this, world);
    }

    private void CreateEmptyWorldPrefab()
    {
        GameObject worldObject = Instantiate(worldPrefab, worldContainer);
        WorldPrefabController controller = worldObject.GetComponent<WorldPrefabController>();
        controller.Initialize(this);
    }

    public void LoadSpecificWorld(string worldId)
    {
        SessionData.worldId = worldId;
        Debug.Log($"Loading world with ID: {SessionData.worldId}");
        // Implement the logic to load the specific world
        SceneManager.LoadScene("World 1");
    }

    public async void DeleteSpecificWorld(string worldId)
    {
        if (SessionData.token != null)
        {
            await DeleteWorldObjectsFromLoad(worldId);
            string url = $"https://localhost:7015/wereldbouwer/{worldId}";
            var response = await PerformApiCall(url, "DELETE", null, SessionData.token);

            if (response != null)
            {
                Debug.Log($"Wereld met ID {worldId} verwijderd: {response}");
                // Hier kun je eventueel de UI updaten of andere acties uitvoeren na het verwijderen
                await LoadWorld(); // Herlaad de wereldlijst na het verwijderen
            }
            else
            {
                Debug.LogError($"Fout bij het verwijderen van wereld met ID {worldId}");
            }
        }
        else
        {
            Debug.LogError("SessionData token is null");
        }
    }

    public async Task DeleteWorldObjectsFromLoad(string worldId)
    {
        string url = $"https://avansict2228256.azurewebsites.net/Object2D/environment/{worldId}";
        string response = await PerformApiCall(url, "DELETE", null, SessionData.token);

        Debug.Log(response);
    }

    private string SanitizeInput(string input)
    {
        // Remove or escape special characters
        return Regex.Replace(input, @"[;'\-\\""]", ""); // Remove ; ' - " \
    }
}