using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu"; // Escena a cargar después de la inicialización

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        // Asegurarse de que MenuSystems esté instanciado
        if (Controller_Menus.Instance == null)
        {
            Debug.Log("MenuSystems no encontrado. Asegúrate de que el prefab de MenuSystems esté en la escena.");
            // Opcional: Instanciar el prefab de MenuSystems si no está presente
            // GameObject menuSystemsPrefab = Resources.Load<GameObject>("Prefabs/MenuSystems");
            // Instantiate(menuSystemsPrefab);
        }

        // Asegurarse de que AudioManager esté instanciado
        if (AudioManager.instance == null)
        {
            Debug.Log("AudioManager no encontrado. Asegúrate de que el prefab de AudioManager esté en la escena.");
            // Opcional: Instanciar el prefab de AudioManager si no está presente
            // GameObject audioManagerPrefab = Resources.Load<GameObject>("Prefabs/AudioManager");
            // Instantiate(audioManagerPrefab);
        }

        // Cargar configuraciones de audio
        LoadAudioSettings();

        // Cargar configuraciones gráficas
        LoadGraphicsSettings();

        // Cargar configuraciones de controles (ya se manejan en ControlsSettings, pero podemos asegurarnos)
        LoadControlsSettings();

        // Cargar otras configuraciones si las tienes (por ejemplo, datos del jugador)
        LoadPlayerData();

        // Pasar a MainMenu
        SceneManager.LoadScene(mainMenuScene);
    }

    private void LoadAudioSettings()
    {
        if (AudioManager.instance != null)
        {
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            AudioManager.instance.SetMusicVolume(musicVolume);
            AudioManager.instance.SetSFXVolume(sfxVolume);
            Debug.Log($"Configuraciones de audio cargadas: MusicVolume={musicVolume}, SFXVolume={sfxVolume}");
        }
        else
        {
            Debug.LogWarning("AudioManager no está inicializado. No se pudieron cargar las configuraciones de audio.");
        }
    }

    private void LoadGraphicsSettings()
    {
        int qualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(qualityLevel, true);
        Debug.Log($"Configuraciones gráficas cargadas: QualityLevel={QualitySettings.names[qualityLevel]}");
    }

    private void LoadControlsSettings()
    {
        ControlsSettings controlsSettings = FindAnyObjectByType<ControlsSettings>();
        if (controlsSettings != null)
        {
            controlsSettings.InitializeActions();
            Debug.Log("Configuraciones de controles cargadas.");
        }
        else
        {
            Debug.LogWarning("ControlsSettings no encontrado. No se pudieron cargar las configuraciones de controles.");
        }
    }

    private void LoadPlayerData()
    {
        // Ejemplo: Cargar datos del jugador como el nivel actual o estadísticas
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        Debug.Log($"Datos del jugador cargados: CurrentLevel={currentLevel}");
        // Aquí podrías cargar más datos, como upgrades o estadísticas del jugador
    }
}
