using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class ApiWorldLoaderClient : MonoBehaviour
{
    public List<PrefabData> instantiatedPrefabsData = new List<PrefabData>();
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

    public async void SaveWorld()
    {
        instantiatedPrefabsData.Clear();
        GameObject[] instantiatedObjects = GameObject.FindGameObjectsWithTag("Instantiated");

        foreach (GameObject obj in instantiatedObjects)
        {
            PrefabData data = new PrefabData();
            data.id = "<string>";
            data.environmentId = SessionData.worldId;
            data.prefabId = obj.name.Replace("(Clone)", "");
            data.positionX = obj.transform.position.x;
            data.positionY = obj.transform.position.y;
            data.scaleX = obj.transform.localScale.x;
            data.scaleY = obj.transform.localScale.y;
            data.rotationZ = obj.transform.rotation.eulerAngles.z;
            data.sortingLayer = obj.GetComponent<SpriteRenderer>().sortingLayerID;
            instantiatedPrefabsData.Add(data);
        }

        // Gebruik de API om de werelddata op te slaan
        await SaveWorldDataToApi(instantiatedPrefabsData);
    }

    private async Task SaveWorldDataToApi(List<PrefabData> data)
    {
        string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
        string url = "https://avansict2228256.azurewebsites.net/Object2D"; // Vervang door de juiste URL
        string response = await PerformApiCall(url, "POST", jsonData, SessionData.token);

        if (response != null)
        {
            Debug.Log("World data saved to API: " + response);
        }
        else
        {
            Debug.LogError("Failed to save world data to API.");
        }
    }

    public async void LoadWorld()
    {
        string url = $"https://avansict2228256.azurewebsites.net/Object2D/{SessionData.worldId}"; // Vervang door de juiste URL
        string response = await PerformApiCall(url, "GET", null, SessionData.token);

        if (response != null)
        {
            List<PrefabData> loadedData = JsonConvert.DeserializeObject<List<PrefabData>>(response);
            if (loadedData != null)
            {
                foreach (PrefabData data in loadedData)
                {
                    GameObject prefab = Resources.Load<GameObject>(data.prefabId);
                    if (prefab != null)
                    {
                        GameObject instantiatedObject = Instantiate(prefab, new Vector3(data.positionX, data.positionY, 0), Quaternion.Euler(0, 0, data.rotationZ));
                        instantiatedObject.transform.localScale = new Vector3(data.scaleX, data.scaleY, 1);
                        instantiatedObject.GetComponent<SpriteRenderer>().sortingLayerID = data.sortingLayer;
                        instantiatedObject.tag = "Instantiated";
                    }
                    else
                    {
                        Debug.LogError("Prefab not found: " + data.prefabId);
                    }
                }
                Debug.Log("World loaded from API.");
            }
        }
        else
        {
            Debug.LogError("Failed to load world data from API.");
        }
    }
}