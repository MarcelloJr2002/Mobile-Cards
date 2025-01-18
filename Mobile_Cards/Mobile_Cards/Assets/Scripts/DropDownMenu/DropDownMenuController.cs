using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DropDownMenuController : MonoBehaviourPunCallbacks
{
    public GameObject dropDownMenu;
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button mainMenuBtn;
    public UnityEngine.UI.Button closeMenuBtn;
    private bool isOpen = false;

    private void Start()
    {
        dropDownMenu.SetActive(false);
        menuButton.onClick.AddListener(ToggleMenu);
        closeMenuBtn.onClick.AddListener(ToggleMenu);
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;
        dropDownMenu.SetActive(isOpen);
    }

    
}
