using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{

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
        var registerDto = new PostRegisterRequestDto() { 
        email = "tk.tsoieeeeee@student.avans.nl",
        password = "VaO|E`SF(wp<\\dX1[@Fy|-)\"g[~=KkXd({D~/8!G%{7-1v*\\U-Yj*lzy\"7~G\\\"F#2$<.bH1Hlh#Ot4,2%l})4*<jkiN*KT!3VTn>g~P{%H%An4B7d^2M8cyGzI=nyBx;+~4)%=n</!WAu9wKT{gpv#GK`RN@.vK\"GrU.-)!H,6C)-|Y:nN~Pscdf.'YCxxyi|NCT,Cz7gY/!1-cjVQ&7@>S<a(Tj(~%j%}mQNQ>P.mBB!/h`)F]Kn3zu/atoN'UA2c0sx_]8~hz;${GoR%n67Nw5<S&|]mh;dWX5/ywd@[q\"4X~-`Y,+,sKz{q<cR*\\VR~LYR6o^g7-\"\"?uUMx6We):/5].5{H\"~{pzk>1};$Z|zI-MC-AmMm31GFfa@9P-qRR<hSvT8KOk5v`(Q=pJ-]jRA-L=qUgUPH3A4%,j/8EgfY{E1/e{<@wdm?BNqw;8)~Y)fi5?ch!Qt^KV2d@6-a<OUSMdWq`<o/pf:({zK3$}~b>@v\\'4\"dK'.o4pK~O%A"
        };


        string jsonData = JsonUtility.ToJson(registerDto);

        var response = await PerformApiCall("https://avansict123456.azurewebsites.net/account/register", "POST", jsonData);
        Debug.Log(response);
    }

    public async void Login()
    {
        var loginDto = new PostLoginRequestDto()
        {
            email = "tk.tsoieeeeee@student.avans.nl",
            password = "VaO|E`SF(wp<\\dX1[@Fy|-)\"g[~=KkXd({D~/8!G%{7-1v*\\U-Yj*lzy\"7~G\\\"F#2$<.bH1Hlh#Ot4,2%l})4*<jkiN*KT!3VTn>g~P{%H%An4B7d^2M8cyGzI=nyBx;+~4)%=n</!WAu9wKT{gpv#GK`RN@.vK\"GrU.-)!H,6C)-|Y:nN~Pscdf.'YCxxyi|NCT,Cz7gY/!1-cjVQ&7@>S<a(Tj(~%j%}mQNQ>P.mBB!/h`)F]Kn3zu/atoN'UA2c0sx_]8~hz;${GoR%n67Nw5<S&|]mh;dWX5/ywd@[q\"4X~-`Y,+,sKz{q<cR*\\VR~LYR6o^g7-\"\"?uUMx6We):/5].5{H\"~{pzk>1};$Z|zI-MC-AmMm31GFfa@9P-qRR<hSvT8KOk5v`(Q=pJ-]jRA-L=qUgUPH3A4%,j/8EgfY{E1/e{<@wdm?BNqw;8)~Y)fi5?ch!Qt^KV2d@6-a<OUSMdWq`<o/pf:({zK3$}~b>@v\\'4\"dK'.o4pK~O%A"
        };


        string jsonData = JsonUtility.ToJson(loginDto);

        var response = await PerformApiCall("https://avansict123456.azurewebsites.net/account/login", "POST", jsonData);
        var responseDto = JsonUtility.FromJson<PostLoginResponseDto>(response);
        Debug.Log(response);
    }
}
