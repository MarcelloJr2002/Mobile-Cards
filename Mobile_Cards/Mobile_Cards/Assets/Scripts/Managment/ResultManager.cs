using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;
using DG.Tweening;
using System.Linq;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    public GameObject bustedSign;
    public GameObject winnerSign;
    public GameObject loseSign;
    public GameObject drawSign;
    int signIndex;
    private PhotonView photonView;
    private Vector3 signPosition = new Vector3(0, 150, 0);
    public Text[] betText;
    public Text[] MoneyText;

    public IEnumerator GetBlackJackResult(Dictionary<string, Player> playersList, Player dealerBot)
    {
        yield return new WaitForSeconds(3);
        double betMoney = 0;
        string playerId;
        int score;

        for (int i = 0; i < playersList.Count; i++)
        {
            playerId = playersList.Keys.ElementAt(i);
            score = playersList[playerId].score;
            if (playersList[playerId].Busted(score))
            {
                i++;
                playerId = playersList.Keys.ElementAt(i);
            }

            if (playersList[playerId].score > dealerBot.score)
            {
                betMoney += playersList[playerId].bet * 2;
                playersList[playerId].AddMoney(betMoney);
                signIndex = 1;
                photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);

                yield return new WaitForSeconds(3);
            }

            if (playersList[playerId].score < dealerBot.score && !dealerBot.Busted(dealerBot.score))
            {
                signIndex = 2;
                photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                yield return new WaitForSeconds(3);
            }

            if (playersList[playerId].score == dealerBot.score && !playersList[playerId].Busted(score) && !dealerBot.Busted(dealerBot.score))
            {

                if (playersList[playerId].IfBlackJack() && !dealerBot.IfBlackJack())
                {
                    betMoney = playersList[playerId].bet + playersList[playerId].bet * 1.5;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 1;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);

                    yield return new WaitForSeconds(3);
                }

                if (dealerBot.IfBlackJack() && !playersList[playerId].IfBlackJack())
                {
                    signIndex = 2;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);

                    yield return new WaitForSeconds(3);
                }

                if (playersList[playerId].IfBlackJack() && dealerBot.IfBlackJack())
                {
                    betMoney += playersList[playerId].bet;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);

                    yield return new WaitForSeconds(3);
                }

                else
                {
                    betMoney += playersList[playerId].bet;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);

                    yield return new WaitForSeconds(3);
                }
            }

        }

    }
    
    [PunRPC]
    public void ShowAndHideResultSign(int signIndex, Vector3 position)
    {
        GameObject signToShow = signIndex switch
        {
            0 => bustedSign,
            1 => winnerSign,
            2 => loseSign,
            3 => drawSign,
            _ => null
        };

        if (signToShow != null)
        {
            //ShowAndHideSign(signToShow, position);
            StartCoroutine(ShowAndHideSign(signToShow, position));
        }
    }

    private IEnumerator ShowAndHideSign(GameObject sign, Vector3 position)
    {
        yield return new WaitForSeconds(3);
        // Ustawiamy pozycję i aktywujemy znak
        sign.transform.position = position;
        sign.transform.SetAsLastSibling();
        sign.SetActive(true);

        // Resetujemy skalę do początkowej wartości
        sign.GetComponent<CanvasGroup>().alpha = 0; // Upewniamy się, że jest niewidoczne (wymaga CanvasGroup)

        // Animacja pojawiania się
        sign.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack); // Powiększenie z efektem
        sign.GetComponent<CanvasGroup>().DOFade(1, 0.5f); // Pojawianie się

        // Animacja zanikania po 2 sekundach
        DOVirtual.DelayedCall(3.0f, () =>
        {
            sign.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack); // Zmniejszenie
            sign.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
            {
                sign.SetActive(false); // Dezaktywacja znaku po zakończeniu animacji
            });
        });

        yield return new WaitForSeconds(5);
    }

    [PunRPC]
    public void UpdateMoneyText(double amount, float x)
    {
        UpdateMoney(amount, x);
    }

    [PunRPC]
    public void DisplayBet(double betValue, float x)
    {
        Debug.Log("Bet rpc: " + betValue);
        ShowBet(betValue, x);
    }

    public void UpdateMoney(double amount, float x)
    {
        for (int i = 0; i < MoneyText.Length; i++)
        {
            if (MoneyText[i].transform.position.x == x)
            {
                MoneyText[i].text = amount.ToString() + "$";
                break;
            }
        }
    }

    public void ShowBet(double betValue, float x)
    {
        for (int i = 0; i < betText.Length; i++)
        {
            if (betText[i].transform.position.x == x)
            {
                betText[i].text = betValue.ToString() + "$";
                break;
            }
        }
    }
}
