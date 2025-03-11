using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class ApiWorldLoaderClient : MonoBehaviour
{
    public List<GameObject> prefabs; // Lijst van beschikbare prefabs
    public List<PrefabData> instantiatedPrefabsData = new List<PrefabData>();
    public static ApiWorldLoaderClient instance { get; private set; }

    void Awake()
    {
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
            string prefabName = obj.name.Replace("(Clone)", "").Trim();
            int prefabIndex = prefabs.FindIndex(prefab => prefab.name == prefabName);
            if (prefabIndex != -1) // Controleer of de prefab is gevonden
            {
                PrefabData data = new PrefabData
                {
                    environmentId = SessionData.worldId,
                    prefabId = prefabIndex.ToString(),
                    positionX = obj.transform.position.x,
                    positionY = obj.transform.position.y,
                    scaleX = obj.transform.localScale.x,
                    scaleY = obj.transform.localScale.y,
                    rotationZ = obj.transform.rotation.eulerAngles.z,
                    sortingLayer = obj.GetComponent<SpriteRenderer>().sortingLayerID
                };
                instantiatedPrefabsData.Add(data);

                string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                string url = "https://avansict2228256.azurewebsites.net/Object2D";
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
        }

    }


    public async void LoadWorld()
    {
        string url = $"https://avansict2228256.azurewebsites.net/Object2D/{SessionData.worldId}";
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
