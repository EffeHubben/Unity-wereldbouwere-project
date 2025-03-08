using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class ApiClient : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorText;
    public TMP_Text loginWarning;

    public static ApiClient instance { get; private set; }
    public PostLoginResponseDto responseDto { get; private set; }
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

    public async void Register()
    {
        bool wachtwoordValidatie = await WachtwoordValidatieAsync(passwordInput.text);
        if (wachtwoordValidatie)
        {
            var registerDto = new PostRegisterRequestDto()
            {
                email = emailInput.text.ToString(),
                password = passwordInput.text.ToString()
            };

            string jsonData = JsonUtility.ToJson(registerDto);

            var response = await PerformApiCall("https://avansict2228256.azurewebsites.net/account/register", "POST", jsonData);
            Debug.Log(response);
            Debug.Log(emailInput.text);
            Debug.Log(passwordInput.text);
        }
    }

    public async void Login()
    {
        var loginDto = new PostLoginRequestDto()
        {
            email = emailInput.text.ToString(),
            password = passwordInput.text.ToString()
        };


        string jsonData = JsonUtility.ToJson(loginDto);

        var response = await PerformApiCall("https://avansict2228256.azurewebsites.net/account/login", "POST", jsonData);
        loginWarning.text = "Email of wachtwoord is onjuist.";
        bool responseSuccess = response.ToString().Contains("token");
        if (responseSuccess)
        {
            loginWarning.text = "Email of wachtwoord is juist.";
        }

        var responseDto = JsonUtility.FromJson<PostLoginResponseDto>(response); 
        //token = responseDto.token.ToString();
        Debug.Log(responseDto.accessToken);
        string userId = await GetUserId(responseDto.accessToken);

        if (!string.IsNullOrEmpty(userId))
        {
            SessionData.ownerUserId = userId; // Opslaan in SessionData
            SessionData.token = responseDto.accessToken; // Opslaan in SessionData
        }
        else
        {
            Debug.LogError("Failed to retrieve user ID");
        }
        Debug.Log(SessionData.ownerUserId);
        Debug.Log(response);
        Debug.Log(emailInput.text);
        Debug.Log(passwordInput.text);
    }

    public async Task<string> GetUserId(string token)
    {
        var response = await PerformApiCall("https://avansict2228256.azurewebsites.net/wereldbouwer/GetUserId", "GET", null, token);

        if (!string.IsNullOrEmpty(response))
        {
            // Assuming the response is a plain string containing the userId
            return response;
        }
        else
        {
            Debug.LogError("Empty response from GetUserId API");
        }

        return null;
    }

    public async Task <bool> WachtwoordValidatieAsync(string wachtwoord)
    {
        errorText.text = "";
        string password = wachtwoord;
        if (password.Length < 10)
        {
            errorText.text += "Wachtwoord moet minimaal 10 karakters lang zijn.";
        }

        if (!Regex.IsMatch(password, "[A-Z]"))
        {
            errorText.text += "Wachtwoord moet minstens 1 hoofdletter bevatten.";
        }

        if (!Regex.IsMatch(password, "[a-z]"))
        {
            errorText.text += "Wachtwoord moet minstens 1 kleine letter bevatten.";
        }

        if (!Regex.IsMatch(password, "[0-9]"))
        {
            errorText.text += "Wachtwoord moet minstens 1 cijfer bevatten.";
        }

        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
        {
            errorText.text += "Wachtwoord moet minstens 1 niet-alfanumeriek teken bevatten.";
        }

        if(errorText.text == "")
        {
            return true;
        }
        return false;
    }
}
