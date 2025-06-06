﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Controller_Menus : MonoBehaviour
{
    public static Controller_Menus Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private List<string> gameScenes = new List<string> { "Prueba" };

    [Header("Menu Panels")]
    [SerializeField] private MenuPanel pauseMenuPanel;
    [SerializeField] private MenuPanel optionsMenuPanel;
    [SerializeField] private MenuPanel controlsMenuPanel;
    [SerializeField] private MenuPanel audioMenuPanel;
    [SerializeField] private MenuPanel graphicsMenuPanel;
    [SerializeField] private MenuPanel quitPanel;

    [Header("Dependencies")]
    private IMenuInputHandler inputHandler;
    [SerializeField] private MenuEventSystemHadler menuEventSystem;
    [SerializeField] private CanvasGroup fadePanel;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Settings Tabs")]
    [SerializeField] private List<SettingsTab> settingsTabs = new List<SettingsTab>();
    private SettingsTab currentTab;

    private bool isPaused = false;
    private MenuPanel currentMenuPanel;
    private Stack<MenuPanel> menuStack;
    private bool isGameScene = false;
    private bool hasInputHandler = false;
    private float lastTabSwitchTime = 0f;
    private const float tabSwitchCooldown = 0.2f;

    [System.Serializable]
    public class SettingsTab
    {
        public string tabName; 
        public Button tabButton;
        public GameObject tabContent;
    }

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
            return;
        }

        menuStack = new Stack<MenuPanel>();
        InitializeMenuPanels();

        if (fadePanel != null)
        {
            DontDestroyOnLoad(fadePanel.gameObject);
            fadePanel.alpha = 0f;
        }
    }

    private void Start()
    {
        UpdateSceneContext();
        StartCoroutine(InitializeScene());
    }

    private IEnumerator InitializeScene()
    {
        yield return StartCoroutine(TryFindInputHandler());

        if (isGameScene)
        {
            CloseAllMenus();
            ResumeGame();
            Debug.Log("Inicialización completa: Escena de juego, mapa Gameplay activado");
        }
        else
        {
            Cursor.visible = true;
            EnsureUIMode();
            CloseAllMenus();
            Debug.Log("Inicialización completa: Escena de Main Menu, mapa UI activado");
        }

        if (isGameScene && fadePanel != null)
        {
            StartCoroutine(FadeIn());
        }

        ControlsSettings controlsSettings = FindAnyObjectByType<ControlsSettings>();
        if (controlsSettings != null)
        {
            controlsSettings.InitializeActions();
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
        hasInputHandler = false;
        StartCoroutine(InitializeSceneAfterLoad());
    }

    private IEnumerator InitializeSceneAfterLoad()
    {
        if (isGameScene)
        {
            yield return StartCoroutine(TryFindInputHandler());
        }

        if (isGameScene)
        {
            CloseAllMenus();
            ResumeGame();
            Debug.Log("Escena de juego cargada, cambiado a mapa Gameplay");
        }
        else
        {
            EnsureUIMode();
            SelectMainMenuButton();
            Debug.Log("Escena de Main Menu cargada, mantenido mapa UI");
        }

        if (fadePanel != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private void Update()
    {
        if (inputHandler != null)
        {
            if (isGameScene)
            {
                if (inputHandler.OptionsInput)
                {
                    TogglePause();
                    inputHandler.UseOptionsInput();
                }

                if (isPaused)
                {
                    HandleUIInput();
                    HandleTabNavigation();
                    MaintainUISelection();
                }
            }
            else if (currentMenuPanel == optionsMenuPanel)
            {
                HandleUIInput();
                HandleTabNavigation();
                MaintainUISelection();
            }
        }
    }

    private void HandleTabNavigation()
    {
        if (currentMenuPanel != optionsMenuPanel || settingsTabs.Count == 0) return;

        if (Time.unscaledTime < lastTabSwitchTime + tabSwitchCooldown) return;

        if (inputHandler.QInput)
        {
            SwitchToNextOrPreviousTab(false); // Pestaña anterior
            inputHandler.UseQInput();
            lastTabSwitchTime = Time.unscaledTime;
        }
        else if (inputHandler.RInput)
        {
            SwitchToNextOrPreviousTab(true); // Pestaña siguiente
            inputHandler.UseRInput();
            lastTabSwitchTime = Time.unscaledTime;
        }
    }

    private void SwitchToNextOrPreviousTab(bool next)
    {
        if (settingsTabs.Count == 0) return;

        int currentIndex = settingsTabs.IndexOf(currentTab);
        int newIndex = next ? currentIndex + 1 : currentIndex - 1;

        if (newIndex < 0)
            newIndex = settingsTabs.Count - 1;
        else if (newIndex >= settingsTabs.Count)
            newIndex = 0;

        SwitchTab(settingsTabs[newIndex]);
    }

    private void MaintainUISelection()
    {
        if(EventSystem.current.currentSelectedGameObject == null && currentMenuPanel != null)
        {
            Button defaultButton = currentMenuPanel.panelObject.GetComponentInChildren<Button>();
            if(defaultButton != null)
            {
                EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
            }
        }
    }

    private void HandleUIInput()
    {
        if (inputHandler.SubmitInput)
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                var button = EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                    Debug.Log("Botón de UI confirmado con Submit");
                }
            }
            inputHandler.UseSubmitInput();
        }

        if (inputHandler.CancelInput)
        {
            CloseCurrentMenu();
            Debug.Log("Menú cerrado con Cancel");
            inputHandler.UseCancelInput();
        }

        if (inputHandler.NavigateInput.magnitude > 0.1f)
        {
            Debug.Log("Navegación en UI: " + inputHandler.NavigateInput);
        }
    }

    private void UpdateSceneContext()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        isGameScene = gameScenes.Contains(currentScene);
        Debug.Log($"Escena actual: {currentScene}, ¿Es escena de juego? {isGameScene}");
    }

    private IEnumerator TryFindInputHandler()
    {
        for (int i = 0; i < 10; i++)
        {
            if (hasInputHandler) yield break;

            inputHandler = FindAnyObjectByType<PlayerInputHadler>();
            if (inputHandler != null)
            {
                Debug.Log($"PlayerInputHandler encontrado en la escena: {SceneManager.GetActiveScene().name} (intento {i + 1})");
                hasInputHandler = true;

                ControlsSettings controlsSettings = FindAnyObjectByType<ControlsSettings>();
                if (controlsSettings != null)
                {
                    controlsSettings.InitializeActions();
                }
                yield break;
            }

            Debug.Log($"PlayerInputHandler no encontrado en la escena: {SceneManager.GetActiveScene().name} (intento {i + 1})");
            yield return new WaitForSeconds(0.2f);
        }

        Debug.LogWarning("No se encontró PlayerInputHandler después de varios intentos en la escena: " + SceneManager.GetActiveScene().name);
    }

    public void RegisterInputHandler(IMenuInputHandler newInputHandler)
    {
        inputHandler = newInputHandler;
        hasInputHandler = true;
        Debug.Log($"InputHandler registrado en MenuSystems: {SceneManager.GetActiveScene().name}");
    }

    public void UnregisterInputHandler()
    {
        inputHandler = null;
        hasInputHandler = false;
        Debug.Log("InputHandler desregistrado de MenuSystems");
    }

    private void InitializeMenuPanels()
    {
        pauseMenuPanel?.Initialize();
        optionsMenuPanel?.Initialize();
        controlsMenuPanel?.Initialize();
        audioMenuPanel?.Initialize();
        graphicsMenuPanel?.Initialize();
        quitPanel?.Initialize();
    }
    private void InitializeTabs()
    {
        foreach (var tab in settingsTabs)
        {
            if (tab.tabContent != null)
            {
                tab.tabContent.SetActive(false);
            }
            if (tab.tabButton != null)
            {
                tab.tabButton.onClick.RemoveAllListeners();
                tab.tabButton.onClick.AddListener(() => SwitchTab(tab));
            }
        }

        if (settingsTabs.Count > 0)
        {
            SwitchTab(settingsTabs[0]);
        }
    }

    public void SwitchTab(SettingsTab tab)
    {
        StartCoroutine(SwitchTabCoroutine(tab));
    }

    private IEnumerator SwitchTabCoroutine(SettingsTab tab)
    {
        if (currentTab != null && currentTab.tabContent != null)
        {
            var previousCanvasGroup = currentTab.tabContent.GetComponent<CanvasGroup>();
            if (previousCanvasGroup == null)
            {
                previousCanvasGroup = currentTab.tabContent.AddComponent<CanvasGroup>();
            }

            float elapsedTime = 0f;
            while (elapsedTime < 0.2f)
            {
                elapsedTime += Time.unscaledDeltaTime;
                previousCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / 0.2f);
                yield return null;
            }
            currentTab.tabContent.SetActive(false);
            previousCanvasGroup.alpha = 1f;

            var previousButtonImage = currentTab.tabButton.GetComponent<Image>();
            if (previousButtonImage != null)
            {
                previousButtonImage.color = Color.gray;
            }
        }

        currentTab = tab;
        if (currentTab.tabContent != null)
        {
            currentTab.tabContent.SetActive(true);
            var currentCanvasGroup = currentTab.tabContent.GetComponent<CanvasGroup>();
            if (currentCanvasGroup == null)
            {
                currentCanvasGroup = currentTab.tabContent.AddComponent<CanvasGroup>();
            }
            currentCanvasGroup.alpha = 0f;

            float elapsedTime2 = 0f;
            while (elapsedTime2 < 0.2f)
            {
                elapsedTime2 += Time.unscaledDeltaTime;
                currentCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime2 / 0.2f);
                yield return null;
            }
            currentCanvasGroup.alpha = 1f;
        }

        var currentButtonImage = currentTab.tabButton.GetComponent<Image>();
        if (currentButtonImage != null)
        {
            currentButtonImage.color = Color.red;
        }

        Selectable firstSelectable = tab.tabContent.GetComponentInChildren<Selectable>();
        if (firstSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
        }

        AudioManager.instance.PlaySFX("ButtonClick");
    }

    public List<SettingsTab> GetSettingsTabs()
    {
        return settingsTabs;
    }

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

    private void PauseGame()
    {
        isPaused = true;
        if (inputHandler != null)
        {
            inputHandler.OnPause();
        }
        Cursor.visible = true;

        OpenMenu(pauseMenuPanel);
        menuEventSystem.enabled = true;

        AudioManager.instance.PauseMusic();
        AudioManager.instance.PlaySFX("MenuOpen");
        Debug.Log("Juego pausado, menú de pausa abierto");
    }

    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (inputHandler != null)
        {
            inputHandler.OnGame();
            Debug.Log("Mapa Gameplay activado en ResumeGame");
        }
        Cursor.visible = false;

        CloseAllMenus();
        menuEventSystem.enabled = false;

        AudioManager.instance.ResumeMusic();
        AudioManager.instance.PlaySFX("MenuClose");
        Debug.Log("Juego reanudado, todos los menús cerrados");
    }

    public void OpenMenu(MenuPanel menuPanel)
    {
        if (menuPanel == null) return;

        if (currentMenuPanel != null && currentMenuPanel != menuPanel)
        {
            StartCoroutine(CloseAndOpenMenuCoroutine(currentMenuPanel, menuPanel));
        }
        else
        {
            menuStack.Push(menuPanel);
            currentMenuPanel = menuPanel;
            StartCoroutine(OpenMenuCoroutine(menuPanel));
        }

        if (menuPanel == optionsMenuPanel)
        {
            InitializeTabs();
        }

        AudioManager.instance.PlaySFX("ButtonClick");
    }

    private IEnumerator CloseAndOpenMenuCoroutine(MenuPanel menuToClose, MenuPanel menuToOpen)
    {
        yield return StartCoroutine(CloseMenuCoroutine(menuToClose));

        menuStack.Push(menuToOpen);
        currentMenuPanel = menuToOpen;
        yield return StartCoroutine(OpenMenuCoroutine(menuToOpen));
    }

    public void OpenOptionsDirectly()
    {
        if (isGameScene)
        {
            PauseGame();
        }
        else
        {
            Debug.Log("Abriendo directamente el menú de opciones en MainMenu");
            Cursor.visible = true;
            EnsureUIMode();
            menuEventSystem.enabled = true;

            CloseAllMenus();
            OpenMenu(optionsMenuPanel);
        }
    }

    private void CloseCurrentMenu()
    {
        if (menuStack.Count == 0)
        {
            if (isGameScene)
            {
                ResumeGame();
            }
            else
            {
                CloseAllMenus();
                EnsureUIMode();
                menuEventSystem.enabled = true;
                Cursor.visible = true;
                SelectMainMenuButton();
                Debug.Log("Todos los menús cerrados, mapa UI mantenido para Main Menu");
            }
            return;
        }

        StartCoroutine(CloseMenuCoroutine(currentMenuPanel));
        menuStack.Pop();

        if (menuStack.Count > 0)
        {
            currentMenuPanel = menuStack.Peek();
            StartCoroutine(OpenMenuCoroutine(currentMenuPanel));
        }
        else
        {
            if (isGameScene)
            {
                ResumeGame();
            }
            else
            {
                CloseAllMenus();
                EnsureUIMode();
                menuEventSystem.enabled = true;
                Cursor.visible = true;
                SelectMainMenuButton();
                Debug.Log("Menú cerrado, mapa UI mantenido para Main Menu");
            }
        }

        AudioManager.instance.PlaySFX("ButtonClick");
        Debug.Log("Menú actual cerrado");
    }
    private IEnumerator CloseAllMenusWithAnimation()
    {
        List<Coroutine> closeCoroutines = new List<Coroutine>();
        if (pauseMenuPanel != null && pauseMenuPanel.panelObject != null) closeCoroutines.Add(StartCoroutine(CloseMenuCoroutine(pauseMenuPanel)));
        if (optionsMenuPanel != null && optionsMenuPanel.panelObject != null) closeCoroutines.Add(StartCoroutine(CloseMenuCoroutine(optionsMenuPanel)));
        if (controlsMenuPanel != null && controlsMenuPanel.panelObject != null) closeCoroutines.Add(StartCoroutine(CloseMenuCoroutine(controlsMenuPanel)));
        if (audioMenuPanel != null && audioMenuPanel.panelObject != null) closeCoroutines.Add(StartCoroutine(CloseMenuCoroutine(audioMenuPanel)));
        if (graphicsMenuPanel != null && graphicsMenuPanel.panelObject != null) closeCoroutines.Add(StartCoroutine(CloseMenuCoroutine(graphicsMenuPanel)));
        if (quitPanel != null && quitPanel.panelObject != null) closeCoroutines.Add(StartCoroutine(CloseMenuCoroutine(quitPanel)));

        float maxAnimationDuration = 0f;
        MenuPanel[] allPanels = { pauseMenuPanel, optionsMenuPanel, controlsMenuPanel, audioMenuPanel, graphicsMenuPanel, quitPanel };
        foreach (MenuPanel panel in allPanels)
        {
            if (panel != null && panel.panelObject != null && panel.AnimationDuration > maxAnimationDuration)
            {
                maxAnimationDuration = panel.AnimationDuration;
            }
        }

        if (maxAnimationDuration > 0)
        {
            yield return new WaitForSecondsRealtime(maxAnimationDuration);
        }

        menuStack.Clear();
        currentMenuPanel = null;
        Debug.Log("Todos los menús cerrados con animación");
    }
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
        Debug.Log("Todos los menús cerrados");
    }

    private IEnumerator OpenMenuCoroutine(MenuPanel menuPanel)
    {
        if (menuPanel == null)
        {
            Debug.LogWarning("OpenMenuCoroutine: menuPanel es nulo.");
            yield break;
        }

        menuPanel.Open();

        if (menuPanel.AnimationDuration <= 0)
        {
            yield return null;
        }
        else
        {
            yield return new WaitForSecondsRealtime(menuPanel.AnimationDuration);
        }
    }

    private IEnumerator CloseMenuCoroutine(MenuPanel menuPanel)
    {
        if (menuPanel == null)
        {
            Debug.LogWarning("CloseMenuCoroutine: menuPanel es nulo.");
            yield break;
        }

        if (menuPanel.panelObject == null)
        {
            yield break;
        }

        menuPanel.Close();

        if (menuPanel.AnimationDuration <= 0)
        {
            yield return null;
        }
        else
        {
            yield return new WaitForSecondsRealtime(menuPanel.AnimationDuration);
        }

        if (menuPanel.panelObject != null)
        {
            menuPanel.panelObject.SetActive(false);
        }

    }

    public void Button_ChangeScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(LoadSceneWithTransition(sceneName));
            CloseAllMenus();
        }
        else
        {
            Debug.LogWarning("No hay escena asignada.");
        }
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        if (fadePanel != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
            fadePanel.alpha = 1f;
        }

        Time.timeScale = 1f;
        AudioManager.instance.StopMusic();
        AudioManager.instance.StopAllSFX();

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeIn()
    {
        if (fadePanel != null)
        {
            float elapsedTime = 0f;
            fadePanel.alpha = 1f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                fadePanel.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }
            fadePanel.alpha = 0f;
        }
    }

    private void EnsureUIMode()
    {
        if (inputHandler != null)
        {
            inputHandler.OnPause();
            Debug.Log("Mapa UI activado en EnsureUIMode");
        }
        menuEventSystem.enabled = true;
        Cursor.visible = true;
        Debug.Log("Modo UI asegurado para escena no de juego");
    }

    private void SelectMainMenuButton()
    {
        Controller_MainMenu mainMenu = FindFirstObjectByType<Controller_MainMenu>();
        if (mainMenu != null)
        {
            mainMenu.SelectInitialButton();
            Debug.Log("Botón inicial del Main Menu seleccionado");
        }
        else
        {
            Debug.LogWarning("Controller_MainMenu no encontrado para seleccionar botón inicial");
        }
    }

    public void Button_Resume()
    {
        StartCoroutine(ResumeWithAnimation());
    }
    private IEnumerator ResumeWithAnimation()
    {
        yield return StartCoroutine(CloseAllMenusWithAnimation());

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
