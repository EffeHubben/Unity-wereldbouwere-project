using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;
using JetBrains.Annotations;

public class ApiWorldLoaderClient : MonoBehaviour
{
    public List<GameObject> prefabs; // Lijst van beschikbare prefabs
    public List<PrefabData> instantiatedPrefabsData = new List<PrefabData>();
    public static ApiWorldLoaderClient instance { get; private set; }

    void Awake()
    {
        LoadWorld();
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
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

    public async void SaveWorld()
    {
        DeleteDatabaseObjects();
        instantiatedPrefabsData.Clear();
        GameObject[] instantiatedObjects = GameObject.FindGameObjectsWithTag("Instantiated");
        List<PrefabData> currentData = new List<PrefabData>();

        // Haal de huidige data op van de API.
        string url = $"https://localhost:7015/Object2D/{SessionData.worldId}";
        string response = await PerformApiCall(url, "GET", null, SessionData.token);

        if (response != null)
        {
            currentData = JsonConvert.DeserializeObject<List<PrefabData>>(response);
        }

        foreach (GameObject obj in instantiatedObjects)
        {
            string prefabName = obj.name.Replace("(Clone)", "");
            if (prefabName != null) // Controleer of de prefab is gevonden
            {
                PrefabData newData = new PrefabData
                {
                    environmentId = SessionData.worldId,
                    prefabId = prefabName,
                    positionX = obj.transform.position.x,
                    positionY = obj.transform.position.y,
                    scaleX = obj.transform.localScale.x,
                    scaleY = obj.transform.localScale.y,
                    rotationZ = obj.transform.rotation.eulerAngles.z,
                    sortingLayer = obj.GetComponent<SpriteRenderer>().sortingLayerID
                };
                //instantiatedPrefabsData.Add(data);
                PrefabData existingData = currentData.Find(data => data.id == newData.id);

                if (existingData != null)
                {
                    // Update de bestaande data.
                    newData.id = existingData.id; // Belangrijk: Behoud de bestaande ID.
                    string updateJsonData = JsonConvert.SerializeObject(newData, Formatting.Indented);
                    string updateUrl = $"https://localhost:7015/Object2D/UpdateObject2D";
                    string updateResponse = await PerformApiCall(updateUrl, "PUT", updateJsonData, SessionData.token);

                    if (updateResponse != null)
                    {
                        Debug.Log("Object data updated in API: " + updateResponse);
                    }
                    else
                    {
                        Debug.LogError("Failed to update object data in API.");
                    }
                }
                else
                {
                    // Voeg nieuwe data toe.
                    string postJsonData = JsonConvert.SerializeObject(newData, Formatting.Indented);
                    string postUrl = "https://localhost:7015/Object2D";
                    string postResponse = await PerformApiCall(postUrl, "POST", postJsonData, SessionData.token);

                    if (postResponse != null)
                    {
                        Debug.Log("Object data saved to API: " + postResponse);
                    }
                    else
                    {
                        Debug.LogError("Failed to save Object data to API.");
                    }
                }


                if (response != null)
                {
                    Debug.Log("World data saved to API: " + response);
                }
                else
                {
                    Debug.LogError("Failed to save world data to API.");
                }
            }
        }

    }



    public async void LoadWorld()
    {
        string url = $"https://localhost:7015/Object2D/{SessionData.worldId}";
        string response = await PerformApiCall(url, "GET", null, SessionData.token);

        GameObject[] instantiatedObjects = GameObject.FindGameObjectsWithTag("Instantiated");
        foreach (GameObject instantiatedPrefab in instantiatedObjects)
        {
            Destroy(instantiatedPrefab);
        }

        if (response != null)
        {
            List<PrefabData> loadedData = JsonConvert.DeserializeObject<List<PrefabData>>(response);
            if (loadedData != null)
            {
                foreach (PrefabData data in loadedData)
                {
                    // Probeer de prefabnaam (string) om te zetten naar de enum PrefabNames
                    if (Enum.TryParse(data.prefabId, out PrefabNames prefabName))
                    {
                        // Zoek de index van de prefab in de lijst 'prefabs'
                        int prefabIndex = (int)prefabName;

                        if (prefabIndex >= 0 && prefabIndex < prefabs.Count)
                        {
                            GameObject prefab = prefabs[prefabIndex]; // Haal de prefab op uit de lijst
                            if (prefab != null)
                            {
                                GameObject instantiatedObject = Instantiate(prefab, new Vector3(data.positionX, data.positionY, 0), Quaternion.Euler(0, 0, data.rotationZ));
                                instantiatedObject.transform.localScale = new Vector3(data.scaleX, data.scaleY, 1);
                                instantiatedObject.GetComponent<SpriteRenderer>().sortingLayerID = data.sortingLayer;
                                instantiatedObject.tag = "Instantiated";
                                MenuPanel.items.Add(instantiatedObject);

                                // Stel de MenuPanel referentie in
                                DragDrop dragDrop = instantiatedObject.GetComponent<DragDrop>();
                                if (dragDrop != null)
                                {
                                    dragDrop.menuPanel = GameObject.Find("LeftPanel").GetComponent<MenuPanel>(); // Zoek het MenuPanel component op LeftPanel
                                }
                            }
                            else
                            {
                                Debug.LogError("Prefab not found at index: " + prefabIndex);
                            }
                        }
                        else
                        {
                            Debug.LogError("Invalid prefab index: " + prefabIndex);
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid prefab name: " + data.prefabId);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Failed to load world data from API.");
        }
    }

    public async void DeleteWorldObjects()
    {
        GameObject[] instantiatedObjects = GameObject.FindGameObjectsWithTag("Instantiated");
        foreach (GameObject instantiatedPrefab in instantiatedObjects)
        {
            Destroy(instantiatedPrefab);
        }

        string url = $"https://avansict2228256.azurewebsites.net/Object2D/environment/{SessionData.worldId}";
        string response = await PerformApiCall(url, "DELETE", null, SessionData.token);

        Debug.Log(response);
    }

    public async void DeleteDatabaseObjects()
    {
        GameObject[] instantiatedObjects = GameObject.FindGameObjectsWithTag("Instantiated");

        string url = $"https://localhost:7015/Object2D/environment/{SessionData.worldId}";
        string response = await PerformApiCall(url, "DELETE", null, SessionData.token);

        Debug.Log(response);
    }



}
