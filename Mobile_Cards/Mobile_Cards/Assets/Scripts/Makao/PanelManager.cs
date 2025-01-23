using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    public GameObject togglePrefab; 
    public GameObject togglePanel;  
    public Text headerText;       
    public ToggleGroup toggleGroup;
    public Transform toggleContainer;
    private Makao makao;

    void Awake()
    {
        makao = FindObjectOfType<Makao>();
        if (makao == null)
        {
            Debug.LogError("Makao instance not found in scene!");
        }
    }


    public void CreateTogglesForValues(List<int> values, int requestedValue)
    {
        // Ustaw nagłówek
        headerText.text = "Choose request card";

        // Usuń istniejące toggles w panelu, jeśli są
        foreach (Transform child in toggleContainer)
        {
            if (child.GetComponent<Toggle>() != null)
            {
                Destroy(child.gameObject); // Usuwa tylko obiekty, które mają komponent Toggle
            }
        }

        togglePanel.SetActive(true);


        // Tworzenie toggle dla każdej wartości
        foreach (int value in values)
        {
            GameObject toggleObj = Instantiate(togglePrefab, toggleContainer); // Dodanie do panelu
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            Text toggleText = toggleObj.GetComponentInChildren<Text>();

            if (toggle != null && toggleText != null)
            {
                toggle.group = toggleGroup; // Przypisz do grupy, aby zachować Radio Button
                toggleText.text = value.ToString(); // Ustaw tekst toggle
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        Debug.Log($"Value {value} selected");
                        requestedValue = value;
                        makao.requestedValue = requestedValue;
                        makao.photonView.RPC("SetValue", RpcTarget.AllBuffered, requestedValue);
                        Debug.Log($"Requested value: {requestedValue}");
                    }

                    else if (!toggleGroup.AnyTogglesOn()) // Jeśli wszystko odznaczone
                    {
                        requestedValue = 0;
                        makao.requestedValue = requestedValue;
                        makao.photonView.RPC("SetValue", RpcTarget.AllBuffered, requestedValue);
                        Debug.Log("No toggle selected, default value applied");
                    }
                });
            }
        }

        if(toggleContainer.childCount > 0)
        {
            Transform firstToggleTransform = toggleContainer.GetChild(toggleContainer.childCount - 1);
            Toggle firstToggle = firstToggleTransform.GetComponent<Toggle>();
            Text labelText = firstToggleTransform.GetComponentInChildren<Text>();
            requestedValue = int.Parse(labelText.text);
            makao.requestedValue = requestedValue;
            makao.photonView.RPC("SetValue", RpcTarget.AllBuffered, requestedValue);
            Debug.Log(requestedValue);
        }
    }

    public void CreateTogglesForColors(List<Card.CardColor> colors, string requestedColor)
    {
        // Ustaw nagłówek
        headerText.text = "Choose request color";

        // Usuń istniejące toggles w panelu, jeśli są
        foreach (Transform child in toggleContainer)
        {
            if (child.GetComponent<Toggle>() != null)
            {
                Destroy(child.gameObject); // Usuwa tylko obiekty, które mają komponent Toggle
            }
        }


        togglePanel.SetActive(true);
        //requestedColor = "";

        // Tworzenie toggle dla każdego koloru
        foreach (Card.CardColor color in colors)
        {
            GameObject toggleObj = Instantiate(togglePrefab, toggleContainer); // Dodanie do panelu
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            Text toggleText = toggleObj.GetComponentInChildren<Text>();

            if (toggle != null && toggleText != null)
            {
                toggle.group = toggleGroup; // Przypisz do grupy, aby zachować Radio Button
                toggleText.text = color.ToString(); // Ustaw tekst toggle
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        Debug.Log($"Color {color} selected");
                        requestedColor = color.ToString();
                        makao.requestedColor = requestedColor;
                        makao.photonView.RPC("SetColor", RpcTarget.AllBuffered, requestedColor);
                        Debug.Log($"Requested color: {requestedColor}");
                    }

                    else if (!toggleGroup.AnyTogglesOn()) // Jeśli wszystko odznaczone
                    {
                        requestedColor = "";
                        makao.requestedColor = requestedColor;
                        makao.photonView.RPC("SetColor", RpcTarget.AllBuffered, requestedColor);
                        Debug.Log("No toggle selected, default color applied");
                    }
                });
            }
        }
        Transform firstToggleTransform = toggleContainer.GetChild(toggleContainer.childCount - 1); // Pierwsze dziecko kontenera
        Toggle firstToggle = firstToggleTransform.GetComponent<Toggle>();
        Text labelText = firstToggleTransform.GetComponentInChildren<Text>();
        requestedColor = labelText.text;
        makao.requestedColor = requestedColor;
        makao.photonView.RPC("SetColor", RpcTarget.AllBuffered, requestedColor);
        Debug.Log(requestedColor);
    }

    public void OnCloseButtonClicked()
    {
        togglePanel.SetActive(false);
        foreach (Transform child in toggleContainer)
        {
            if (child.GetComponent<Toggle>() != null)
            {
                Destroy(child.gameObject); // Usuwa tylko obiekty, które mają komponent Toggle
            }
        }

    }

    [PunRPC]
    public void SetColor(string color)
    {
        makao.requestedColor = color;
    }

    [PunRPC]
    public void SetValue(int value)
    {
        makao.requestedValue = value;
    }
}
