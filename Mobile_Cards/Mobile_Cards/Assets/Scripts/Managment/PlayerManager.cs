using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class PlayerManager : MonoBehaviour
{
    public InputField inputField;
    public Text infoText;

    public void ShowText(string text)
    {
        // Ustaw początkową przezroczystość na 0 (tekst niewidoczny)
        infoText.color = new Color(infoText.color.r, infoText.color.g, infoText.color.b, 0);
        infoText.text = text;

        // Animacja pojawienia się
        infoText.DOFade(1, 0.5f) // Fade-in w 0.5 sekundy
            .OnComplete(() =>
            {
                // Po 2 sekundach zniknij
                infoText.DOFade(0, 0.5f).SetDelay(2f); // Fade-out w 0.5 sekundy po opóźnieniu 2 sekund
            });
    }

    public void CreatePlayer()
    {
        if(inputField.text == "")
        {
            infoText.text = "Input field cannot be empty";
            ShowText(infoText.text);
        }

        else if(ContainsPolishCharacters(inputField.text))
        {
            infoText.text = "You can't use polish letters!";
            ShowText(infoText.text);
        }

        else
        {
            Globals.localPlayerId = inputField.text;
            Debug.Log(Globals.localPlayerId);

            if (GameModeManager.Instance.selectedMode == GameModeManager.GameMode.Photon)
            {
                SceneManager.LoadScene("CreateAndJoinRoom");
                Debug.Log(Globals.localPlayerId);
            }

            else
            {
                SceneManager.LoadScene("BTConnect");
            }
        }
    }

    public bool ContainsPolishCharacters(string input)
    {
        // Wyrażenie regularne sprawdzające polskie znaki
        string polishCharsPattern = "[ąćęłńóśźżĄĆĘŁŃÓŚŹŻ]";
        return Regex.IsMatch(input, polishCharsPattern);
    }
}
