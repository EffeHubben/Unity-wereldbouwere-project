using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.InputSystem.XR;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class ApiWorldLoaderClient : MonoBehaviour
{
    public GameObject prefabName;
    public Button saveButton;
    public static ApiWorldLoaderClient instance { get; private set; }

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

    void Start()
    {
        // Koppel de SaveInstantiatedPrefabs methode aan de onClick event van de saveButton
        if (saveButton != null)
        {
            saveButton.clicked += SaveInstantiatedPrefabs ;
        }
        else
        {
            Debug.LogError("Save button is not assigned.");
        }
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
    public async void LoadWorldObjects()
        {
        if (SessionData.token != null)
        {
            var registerDto = new PostWorldObjectLoaderRequestDto()
            {
                worldId = SessionData.worldId,
            };
            string jsonData = JsonUtility.ToJson(registerDto);
            string url = $"https://avansict2228256.azurewebsites.net/Object2D/{SessionData.worldId}";
            var response = await PerformApiCall(url, "POST", jsonData, SessionData.token);
            Debug.Log(response);
        }
        else
        {
            Debug.LogError("SessionData token is null");
        }
        
    }

    public async void SaveInstantiatedPrefabs()
    {
        // Zoek alle gameobjecten in de scène
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Lijst om de gegevens van de geïnstantieerde prefabs op te slaan
        List<PrefabData> prefabDataList = new List<PrefabData>();

        foreach (GameObject obj in allObjects)
        {
            // Controleer of het object een prefab is (dit kan variëren afhankelijk van hoe je prefabs instantieert)
            if (IsPrefab(obj))
            {
                // Verzamel de gegevens die je wilt opslaan
                PrefabData data = new PrefabData();
                data.id = obj.name;
                data.environmentId = SessionData.worldId; // Voeg hier de omgevings-ID toe
                data.prefabId = prefabName.name; // Voeg hier de prefab-ID toe
                data.positionX = obj.transform.position.x;
                data.positionY = obj.transform.position.y;
                data.scaleX = obj.transform.localScale.x;
                data.scaleY = obj.transform.localScale.y;
                data.rotationZ = obj.transform.rotation.eulerAngles.z;
                data.sortingLayer = obj.GetComponent<SpriteRenderer>().sortingLayerID;
                // Voeg hier andere relevante gegevens toe

                prefabDataList.Add(data);
            }
        }

        // Serialiseer de lijst naar JSON
        string jsonData = JsonConvert.SerializeObject(prefabDataList, Formatting.Indented);

        // Sla de JSON-gegevens op in een bestand
        var registerDto = new PostWorldObjectAddRequestDto()
        {
            id = "<string>",
            environmentId = SessionData.worldId,
            prefabId = prefabName.name,
            positionX = 0,
            positionY = 0,
            scaleX = 0,
            scaleY = 0,
            rotationZ = 0,
            sortingLayer = 0
        };
        jsonData = JsonUtility.ToJson(registerDto);
        string url = $"https://avansict2228256.azurewebsites.net/Object2D";
        var response = await PerformApiCall(url, "POST", jsonData, SessionData.token);
    }

    private bool IsPrefab(GameObject obj)
    {
        // Dit is een eenvoudige controle. Mogelijk moet je dit aanpassen aan je specifieke behoeften
        return UnityEditor.PrefabUtility.IsPartOfPrefabAsset(obj);
    }
}
