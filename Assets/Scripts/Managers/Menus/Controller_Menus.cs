using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller_Menus : MonoBehaviour
{
    public static Controller_Menus Instance { get; private set; } // Singleton

    [Header("Scene Names")]
    [SerializeField] private string sceneMenu = "MainMenu"; // Escena del men� principal
    [SerializeField] private string sceneRetry = "GameScene"; // Escena para reintentar el nivel
    [SerializeField] private List<string> gameScenes = new List<string> { "Prueba" }; // Lista de escenas de juego

    [Header("Menu Panels")]
    [SerializeField] private MenuPanel pauseMenuPanel; // Panel del men� de pausa
    [SerializeField] private MenuPanel optionsMenuPanel; // Panel del men� de opciones
    [SerializeField] private MenuPanel controlsMenuPanel; // Panel del submen� de controles
    [SerializeField] private MenuPanel audioMenuPanel; // Panel del submen� de audio
    [SerializeField] private MenuPanel graphicsMenuPanel; // Panel del submen� de gr�ficos
    [SerializeField] private MenuPanel quitPanel; // Panel de confirmaci�n de salida

    [Header("Dependencies")]
    private PlayerInputHadler inputHandler; // Script de manejo de input (null en MainMenu)
    [SerializeField] private MenuEventSystemHadler menuEventSystem; // Sistema de eventos para la UI
    [SerializeField] private CanvasGroup fadePanel; // Panel de transici�n para el fade

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 1f; // Duraci�n del fade en segundos

    private bool isPaused = false; // Estado de pausa del juego
    private MenuPanel currentMenuPanel; // Panel actualmente activo
    private Stack<MenuPanel> menuStack; // Pila para manejar la navegaci�n entre men�s
    private bool isGameScene = false; // Indica si estamos en una escena de juego
    private bool hasInputHandler = false; // Para evitar buscar si ya lo encontramos

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        menuStack = new Stack<MenuPanel>();
        InitializeMenuPanels();

        if (fadePanel != null)
        {
            DontDestroyOnLoad(fadePanel.gameObject);
            fadePanel.alpha = 0f; // Asegurarse de que el panel de transici�n est� invisible al inicio
        }
    }

    private void Start()
    {
        // Detectar la escena actual
        UpdateSceneContext();
        StartCoroutine(TryFindInputHandler());


        if (isGameScene)
        {
            // Cerrar todos los men�s al inicio
            CloseAllMenus();
            ResumeGame(); // Asegurarse de que el juego comience sin pausa en escenas de juego
        }
        else
        {
            // En el MainMenu, no necesitamos pausar el juego
            Cursor.visible = true;
            menuEventSystem.enabled = false;
            CloseAllMenus();
        }

        // Iniciar con un fade in desde negro
        if (fadePanel != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateSceneContext();
        hasInputHandler = false; // Resetear para buscar en la nueva escena
        StartCoroutine(TryFindInputHandler());

        // Hacer un fade in al cargar la nueva escena
        if (fadePanel != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private void Update()
    {
        // Solo manejar el input de pausa en escenas de juego
        if (isGameScene && inputHandler != null && inputHandler.OptionsInput)
        {
            TogglePause();
            inputHandler.UseOptionsInput();
        }
    }

    // Detectar si estamos en una escena de juego o en el men� principal
    private void UpdateSceneContext()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        isGameScene = gameScenes.Contains(currentScene);
        Debug.Log($"Escena actual: {currentScene}, �Es escena de juego? {isGameScene}");

        // Asegurarse de que el inputHandler est� asignado solo en escenas de juego
        if (!isGameScene)
        {
            inputHandler = null;
        }
    }

    private IEnumerator TryFindInputHandler()
    {
        // Intentar buscar el inputHandler varias veces para manejar retrasos en la instanciaci�n
        for (int i = 0; i < 5; i++)
        {
            if (hasInputHandler) yield break; // Salir si ya lo encontramos

            inputHandler = FindAnyObjectByType<PlayerInputHadler>();
            if (inputHandler != null)
            {
                Debug.Log($"PlayerInputHadler encontrado en la escena: {SceneManager.GetActiveScene().name} (intento {i + 1})");
                hasInputHandler = true;

                // Notificar a ControlsSettings para inicializar las acciones
                ControlsSettings controlsSettings = FindAnyObjectByType<ControlsSettings>();
                if (controlsSettings != null)
                {
                    controlsSettings.InitializeActions();
                }
                yield break;
            }

            Debug.Log($"PlayerInputHadler no encontrado en la escena: {SceneManager.GetActiveScene().name} (intento {i + 1})");
            yield return new WaitForSeconds(0.1f); // Esperar un breve momento antes de volver a intentar
        }

        Debug.LogWarning("No se encontr� PlayerInputHadler despu�s de varios intentos en la escena: " + SceneManager.GetActiveScene().name);
    }
    // M�todo para que el PlayerInputHadler se registre autom�ticamente
    public void RegisterInputHandler(PlayerInputHadler newInputHandler)
    {
        inputHandler = newInputHandler;
        hasInputHandler = true;
        Debug.Log($"PlayerInputHadler registrado en MenuSystems: {SceneManager.GetActiveScene().name}");

        // Notificar a ControlsSettings para inicializar las acciones
        ControlsSettings controlsSettings = FindAnyObjectByType<ControlsSettings>();
        if (controlsSettings != null)
        {
            controlsSettings.InitializeActions();
        }
    }

    // M�todo para desregistrar el inputHandler (opcional, por si necesitas limpiarlo)
    public void UnregisterInputHandler()
    {
        inputHandler = null;
        hasInputHandler = false;
        Debug.Log("PlayerInputHadler desregistrado de MenuSystems");
    }

    // Inicializar los paneles de men�
    private void InitializeMenuPanels()
    {
        pauseMenuPanel?.Initialize();
        optionsMenuPanel?.Initialize();
        controlsMenuPanel?.Initialize();
        audioMenuPanel?.Initialize();
        graphicsMenuPanel?.Initialize();
        quitPanel?.Initialize();
    }

    // Cambiar entre pausa y reanudar
    private void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    // Pausar el juego
    private void PauseGame()
    {
        isPaused = true;
        //Time.timeScale = 0f;
        if (inputHandler != null)
        {
            inputHandler.OnPause();
        }
        Cursor.visible = true;

        OpenMenu(pauseMenuPanel);
        menuEventSystem.enabled = true;

        AudioManager.instance.PauseMusic();
        AudioManager.instance.PlaySFX("MenuOpen");
    }

    // Reanudar el juego
    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (inputHandler != null)
        {
            inputHandler.OnGame();
        }
        Cursor.visible = false;

        CloseAllMenus();
        menuEventSystem.enabled = false;

        AudioManager.instance.ResumeMusic();
        AudioManager.instance.PlaySFX("MenuClose");
    }

    // Abrir un men�
    public void OpenMenu(MenuPanel menuPanel)
    {
        if (menuPanel == null) return;

        // Si hay un men� abierto, cerrarlo primero
        if (currentMenuPanel != null && currentMenuPanel != menuPanel)
        {
            StartCoroutine(CloseMenuCoroutine(currentMenuPanel));
        }

        // Abrir el nuevo men�
        menuStack.Push(menuPanel);
        currentMenuPanel = menuPanel;
        StartCoroutine(OpenMenuCoroutine(menuPanel));

        AudioManager.instance.PlaySFX("ButtonClick");
    }

    // Abrir directamente el men� de opciones (para MainMenu)
    public void OpenOptionsDirectly()
    {
        if (isGameScene)
        {
            PauseGame(); // En escenas de juego, abrir el men� de pausa primero
        }
        else
        {
            // En MainMenu, abrir directamente el men� de opciones
            Debug.Log("Abriendo directamente el men� de opciones en MainMenu");
            Cursor.visible = true;
            menuEventSystem.enabled = true;

            // Asegurarse de que todos los dem�s paneles est�n cerrados
            CloseAllMenus();

            // Abrir el men� de opciones
            OpenMenu(optionsMenuPanel);
        }
    }

    // Cerrar el men� actual y volver al anterior
    private void CloseCurrentMenu()
    {
        if (menuStack.Count <= 1)
        {
            if (isGameScene)
            {
                ResumeGame();
            }
            else
            {
                CloseAllMenus();
                menuEventSystem.enabled = false;
                Cursor.visible = true; // En MainMenu, mantener el cursor visible
            }
            return;
        }

        StartCoroutine(CloseMenuCoroutine(currentMenuPanel));
        menuStack.Pop();

        currentMenuPanel = menuStack.Peek();
        StartCoroutine(OpenMenuCoroutine(currentMenuPanel));

        AudioManager.instance.PlaySFX("ButtonClick");
    }

    // Cerrar todos los men�s
    private void CloseAllMenus()
    {
        pauseMenuPanel?.CloseImmediate();
        optionsMenuPanel?.CloseImmediate();
        controlsMenuPanel?.CloseImmediate();
        audioMenuPanel?.CloseImmediate();
        graphicsMenuPanel?.CloseImmediate();
        quitPanel?.CloseImmediate();

        menuStack.Clear();
        currentMenuPanel = null;
    }

    // Corrutina para abrir un men� con animaci�n
    private IEnumerator OpenMenuCoroutine(MenuPanel menuPanel)
    {
        menuPanel.Open();
        yield return new WaitForSecondsRealtime(menuPanel.AnimationDuration);
    }

    // Corrutina para cerrar un men� con animaci�n
    private IEnumerator CloseMenuCoroutine(MenuPanel menuPanel)
    {
        menuPanel.Close();
        yield return new WaitForSecondsRealtime(menuPanel.AnimationDuration);
    }
    public void Button_ChangeScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(LoadSceneWithTransition(sceneName));
        }
        else
        {
            Debug.LogWarning("No hay escena asignada.");
        }
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        // Fade out
        if (fadePanel != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime; // Usar unscaledDeltaTime para que el fade funcione incluso si Time.timeScale = 0
                fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
            fadePanel.alpha = 1f; // Asegurarse de que est� completamente opaco
        }

        // Detener m�sica y efectos de sonido
        Time.timeScale = 1f;
        AudioManager.instance.StopMusic();
        AudioManager.instance.StopAllSFX();

        // Cargar la nueva escena
        SceneManager.LoadScene(sceneName);

        // El fade in se manejar� en OnSceneLoaded
    }

    private IEnumerator FadeIn()
    {
        // Fade in desde negro
        if (fadePanel != null)
        {
            float elapsedTime = 0f;
            fadePanel.alpha = 1f; // Asegurarse de que est� completamente opaco al inicio
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                fadePanel.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }
            fadePanel.alpha = 0f; // Asegurarse de que est� completamente transparente
        }
    }

    // M�todos de los botones
    public void Button_Resume()
    {
        ResumeGame();
    }

    public void Button_Options()
    {
        OpenMenu(optionsMenuPanel);
    }

    public void Button_Controls()
    {
        OpenMenu(controlsMenuPanel);
    }

    public void Button_Audio()
    {
        OpenMenu(audioMenuPanel);
    }

    public void Button_Graphics()
    {
        OpenMenu(graphicsMenuPanel);
    }

    public void Button_Back()
    {
        CloseCurrentMenu();
    }

    public void Button_QuitPanel()
    {
        OpenMenu(quitPanel);
    }

    public void Button_Quit()
    {
        Application.Quit();
        Debug.Log("Ha salido del juego");
    }

    public void Button_CancelQuit()
    {
        CloseCurrentMenu();
    }
}
