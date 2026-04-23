using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Text;
using TMPro;

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
    [Header("Configuración Game Over")]
    public CanvasGroup grupoGameOver; 
    public float velocidadFade = 1.0f;

    public string username;
    public string password;
    public static GameManager Instance { get; set; }

    [Header("Configuración API")]
    public string urlApi = "https://localhost:7164/api/auth";
    [Header("Estado del Jugador")]
    public PlayerData jugadorActivo;
    public GameObject loading;
    public GameObject error;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
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
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    public IEnumerator RegistrarUsuario(string user, string pass)
    {
        LoginRequest datos = new LoginRequest { username = user, password = pass };
        string json = JsonUtility.ToJson(datos);

        using (UnityWebRequest request = new UnityWebRequest(urlApi + "/register", "POST"))
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
                error.SetActive(true);
                StartCoroutine(MostrarYEsconder());
            }
            if (loading != null)
            {
                loading.SetActive(false);
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

                // 1. Actualizamos el dato en el Manager
                jugadorActivo.nivelActual = nivel;
                SceneManager.LoadScene("PantallaCarga");
            }
            else
            {
                Debug.LogError("Error API al guardar: " + request.error);
                SceneManager.LoadScene("PantallaCarga");
            }
        }
    }
    private IEnumerator EfectoFadeInGameOver()
    {
        // 1. Activa el objeto (por si estaba desactivado en la jerarquía)
        grupoGameOver.gameObject.SetActive(true);

        // 2. Asegura que empiece totalmente transparente
        grupoGameOver.alpha = 0;

        // 3. Desactiva interacciones para que el botón no funcione antes de verse
        grupoGameOver.interactable = false;
        grupoGameOver.blocksRaycasts = false;

        float tiempo = 0;
        while (tiempo < 1f)
        {
            // El factor (1.0f / velocidadFade) ajusta el tiempo a segundos reales
            tiempo += Time.deltaTime * (1.0f / velocidadFade);

            // Esto cambia el alpha del PANEL y de todo lo que tenga dentro a la vez
            grupoGameOver.alpha = Mathf.Lerp(0, 1, tiempo);

            yield return null;
        }

        // 4. Finaliza activando los clicks
        grupoGameOver.alpha = 1;
        grupoGameOver.interactable = true;
        grupoGameOver.blocksRaycasts = true;
    }

    // UTILIDADES
    public void PantallaDeCarga()
    {
        jugadorActivo = null;
        SceneManager.LoadScene("PantallaCarga");
    }
    public void GameOver()
    {
        if (grupoGameOver != null)
        {
            StartCoroutine(EfectoFadeInGameOver());
        }
    }
    private IEnumerator MostrarYEsconder()
    {
        // 1. Hacemos el objeto visible
        if (error != null)
        {
            error.SetActive(true);
        }

        // 2. Esperamos los segundos indicados
        yield return new WaitForSeconds(5f);

        // 3. Lo hacemos invisible
        if (error != null)
        {
            error.SetActive(false);
        }
    }
    public void RecibirUsernamePassword(string user, string pass)
    {
        username = user;
        password = pass;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PantallaCarga")
        {
            StartCoroutine(CargarJuego(username, password));
        }        
    }
}