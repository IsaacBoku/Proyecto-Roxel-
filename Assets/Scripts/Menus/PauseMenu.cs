using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public UIManager uiManager;
    public GameObject menuUI;
    public PlayerInput playerInput;

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePauseMenu();
        }
    }
    public void TogglePauseMenu()
    {
        bool isActive = menuUI.activeSelf;
        menuUI.SetActive(!isActive);

        if(isActive)
        {
            uiManager.gameObject.SetActive(false);
        }
        else
        {
            uiManager.gameObject.SetActive(true);
        }
    }

}
