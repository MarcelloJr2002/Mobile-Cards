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

        headerText.text = "Choose request card";


        foreach (Transform child in toggleContainer)
        {
            if (child.GetComponent<Toggle>() != null)
            {
                Destroy(child.gameObject); 
            }
        }

        togglePanel.SetActive(true);



        foreach (int value in values)
        {
            GameObject toggleObj = Instantiate(togglePrefab, toggleContainer); 
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            Text toggleText = toggleObj.GetComponentInChildren<Text>();

            if (toggle != null && toggleText != null)
            {
                toggle.group = toggleGroup; 
                toggleText.text = value.ToString();
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        Debug.Log($"Value {value} selected");
                        requestedValue = value;
                        makao.requestedValue = requestedValue;
                        makao.photonView.RPC("SetValue", RpcTarget.All, requestedValue);
                        Debug.Log($"Requested value: {requestedValue}");
                    }

                    else if (!toggleGroup.AnyTogglesOn()) 
                    {
                        requestedValue = 0;
                        makao.requestedValue = requestedValue;
                        makao.photonView.RPC("SetValue", RpcTarget.All, requestedValue);
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

            if(labelText.text == "" || labelText.text == null)
            {
                requestedValue = 0;
            }

            else
            {
                requestedValue = int.Parse(labelText.text);
            }
            makao.requestedValue = requestedValue;
            makao.photonView.RPC("SetValue", RpcTarget.AllBuffered, requestedValue);
            Debug.Log(requestedValue);
        }
    }

    public void CreateTogglesForColors(List<Card.CardColor> colors, string requestedColor)
    {

        headerText.text = "Choose request color";


        foreach (Transform child in toggleContainer)
        {
            if (child.GetComponent<Toggle>() != null)
            {
                Destroy(child.gameObject); 
            }
        }


        togglePanel.SetActive(true);
        //requestedColor = "";


        foreach (Card.CardColor color in colors)
        {
            GameObject toggleObj = Instantiate(togglePrefab, toggleContainer);
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            Text toggleText = toggleObj.GetComponentInChildren<Text>();

            if (toggle != null && toggleText != null)
            {
                toggle.group = toggleGroup; 
                toggleText.text = color.ToString(); 
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

                    else if (!toggleGroup.AnyTogglesOn()) 
                    {
                        requestedColor = "";
                        makao.requestedColor = requestedColor;
                        makao.photonView.RPC("SetColor", RpcTarget.AllBuffered, requestedColor);
                        Debug.Log("No toggle selected, default color applied");
                    }
                });
            }
        }
        Transform firstToggleTransform = toggleContainer.GetChild(toggleContainer.childCount - 1);
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
                Destroy(child.gameObject);
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
