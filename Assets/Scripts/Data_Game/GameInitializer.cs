using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "Main_Menu"; 

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        if (Controller_Menus.Instance == null)
        {
            Debug.Log("MenuSystems no encontrado. Asegúrate de que el prefab de MenuSystems esté en la escena.");
            GameObject menuSystemsPrefab = Resources.Load<GameObject>("Prefabs/MenuSystems");
            Instantiate(menuSystemsPrefab);
        }

        // Asegurarse de que AudioManager esté instanciado
        if (AudioManager.instance == null)
        {
            Debug.Log("AudioManager no encontrado. Asegúrate de que el prefab de AudioManager esté en la escena.");
            // Opcional: Instanciar el prefab de AudioManager si no está presente
            GameObject audioManagerPrefab = Resources.Load<GameObject>("Prefabs/AudioManager");
            Instantiate(audioManagerPrefab);
        }
        // Asegurarse de que ControlsSettings esté instanciado
        if (VFXManager.Instance == null)
        {
            Debug.Log("VFXManager no encontrado. Asegúrate de que el prefab de VFXManager esté en la escena.");
        }

        // Cargar configuraciones de audio
        LoadAudioSettings();

        // Cargar configuraciones gráficas
        LoadGraphicsSettings();

        LoadVFXSettings();

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
            bool musicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
            bool sfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

            AudioManager.instance.SetMusicVolume(musicVolume);
            AudioManager.instance.SetSFXVolume(sfxVolume);
            AudioManager.instance.SetMusicMute(musicMuted);
            AudioManager.instance.SetSFXMute(sfxMuted);
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

    private void LoadVFXSettings()
    {
        if (VFXManager.Instance != null)
        {
            bool vfxEnabled = PlayerPrefs.GetInt("VFXEnabled", 1) == 1;
            float vfxIntensity = PlayerPrefs.GetFloat("VFXIntensity", 1f);

            VFXManager.Instance.SetVFXEnabled(vfxEnabled);
            VFXManager.Instance.SetVFXIntensity(vfxIntensity);

            Debug.Log($"Configuraciones de VFX cargadas: VFXEnabled={vfxEnabled}, VFXIntensity={vfxIntensity}");
        }
        else
        {
            Debug.LogWarning("VFXManager no está inicializado. No se pudieron cargar las configuraciones de VFX.");
        }
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
