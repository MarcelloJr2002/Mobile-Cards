using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseGameManager : MonoBehaviourPunCallbacks
{
    public CardDealer dealer;
    
    public virtual void GameStart()
    {
        dealer.SetNumberOfPlayers(PhotonNetwork.CurrentRoom.PlayerCount);
    }

    public virtual void EndGame()
    {
        StartCoroutine(ClearCardsFromScene(2.0f));
    }

    public virtual void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        dealer.SetNumberOfPlayers(PhotonNetwork.CurrentRoom.PlayerCount);
    }

    public virtual void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        dealer.SetNumberOfPlayers(PhotonNetwork.CurrentRoom.PlayerCount);
    }

    private IEnumerator ClearCardsFromScene(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (GameObject card in GameObject.FindGameObjectsWithTag("CardsModels"))
        {
            Destroy(card);
        }
    }
}
