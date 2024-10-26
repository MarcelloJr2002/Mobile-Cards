using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DropDownMenuController : MonoBehaviour
{
    public GameObject dropDownMenu;
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button mainMenuBtn;
    private bool isOpen = false;

    private void Start()
    {
        dropDownMenu.SetActive(false);
        menuButton.onClick.AddListener(ToggleMenu);
        mainMenuBtn.onClick.AddListener(OnMainMenuClicked);
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;
        dropDownMenu.SetActive(isOpen);
    }

    public void OnMainMenuClicked()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("MainMenu");
    }
}
