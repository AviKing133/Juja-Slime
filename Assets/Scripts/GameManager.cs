using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Text;

[System.Serializable]
public class UpdateNivelRequest
{
    public string username;
    public int nuevoNivel;
}

[System.Serializable]
public class PlayerData
{
    public int id;
    public string username;
    public int nivelActual;
}

[System.Serializable]
public class LoginRequest { public string Username; public string Password; }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    [Header("Configuración API")]
    public string urlApi = "https://localhost:7164/api/auth";
    [Header("Estado del Jugador")]
    public PlayerData jugadorActivo;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(SoloComprobarYCargar("Miguelito", "locura"));
    }

    private IEnumerator SoloComprobarYCargar(string user, string pass)
    {
        LoginRequest datos = new LoginRequest { Username = user, Password = pass };
        string json = JsonUtility.ToJson(datos);

        using (UnityWebRequest request = new UnityWebRequest(urlApi + "/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string respuestaCruda = request.downloadHandler.text;
                Debug.Log("<color=orange>JSON RECIBIDO DE LA API:</color> " + respuestaCruda);

                // --- CLAVE: Guardamos los datos en jugadorActivo ---
                jugadorActivo = JsonUtility.FromJson<PlayerData>(respuestaCruda);

                Debug.Log($"<color=green>RESULTADO UNITY:</color> {jugadorActivo.username} está en nivel {jugadorActivo.nivelActual}");

                if (jugadorActivo.nivelActual > 0)
                {
                    // Solo cargamos si no estamos ya en ese nivel para evitar bucles
                    string nombreNivel = "Nivel" + jugadorActivo.nivelActual;
                    if (SceneManager.GetActiveScene().name != nombreNivel)
                        SceneManager.LoadScene(nombreNivel);
                }
            }
            else
            {
                Debug.LogError("Fallo Login: " + request.responseCode + " - " + request.error);
            }
        }
    }

    public void GuardarProgreso(int proximoNivel)
    {
        if (jugadorActivo != null && !string.IsNullOrEmpty(jugadorActivo.username))
        {
            StartCoroutine(ProcesoUpdateNivel(jugadorActivo.username, proximoNivel));
        }
        else
        {
            Debug.LogError("No se puede guardar: No hay jugador activo.");
        }
    }

    private IEnumerator ProcesoUpdateNivel(string user, int nivel)
    {
        UpdateNivelRequest datos = new UpdateNivelRequest { username = user, nuevoNivel = nivel };
        string json = JsonUtility.ToJson(datos);

        Debug.Log("<color=yellow>Intentando guardar nivel...</color> " + json);

        using (UnityWebRequest request = new UnityWebRequest(urlApi + "/update-nivel", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("<color=green>API OK:</color> Nivel actualizado en MySQL.");
                jugadorActivo.nivelActual = nivel;
                SceneManager.LoadScene("Nivel" + nivel);
            }
            else
            {
                Debug.LogError("Error API al guardar: " + request.error);
                SceneManager.LoadScene("Nivel" + nivel); // Cargamos igual para no romper el flujo
            }
        }
    }
}