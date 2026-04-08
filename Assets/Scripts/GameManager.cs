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
public class ErrorResponse
{
    public string message;
}

[System.Serializable]
public class PlayerData
{
    public int id;
    public string username;
    public int nivelActual;
}

[System.Serializable]
public class LoginRequest { public string username; public string password; }

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

    }

    public IEnumerator CargarJuego(string user, string pass)
    {
        LoginRequest datos = new LoginRequest { username = user, password = pass };
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

    public IEnumerator RegistrarUsuario(string user, string pass)
    {
        LoginRequest datos = new LoginRequest { username = user, password = pass };
        string json = JsonUtility.ToJson(datos);

        using (UnityWebRequest request = new UnityWebRequest(urlApi + "/Auth/register", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("<color=green>¡Usuario creado!</color>");
            }
            else
            {
                // 1. Manejo por código numérico
                if (request.responseCode == 409)
                {
                    MostrarMensajeEnPantalla("El usuario ya está registrado.");
                }
                else if (request.responseCode == 400)
                {
                    // 2. Intentar leer detalle del JSON
                    try
                    {
                        var error = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                        MostrarMensajeEnPantalla("Error: " + error.message);
                    }
                    catch
                    {
                        MostrarMensajeEnPantalla("Datos inválidos.");
                    }
                }
                else
                {
                    MostrarMensajeEnPantalla("Error de conexión: " + request.error);
                }
            }
        }
    }

    // Método auxiliar para feedback visual al usuario
    void MostrarMensajeEnPantalla(string msg)
    {
        // Aquí podrías asignar el texto a un objeto de UI Text
        Debug.Log("<color=red>FEEDBACK:</color> " + msg);
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