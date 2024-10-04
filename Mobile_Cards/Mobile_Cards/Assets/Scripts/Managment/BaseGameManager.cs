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
        dealer.numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public virtual void EndGame()
    {

    }

    public virtual void OnPlayerEnteredRoom(Player newPlayer)
    {
        dealer.numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public virtual void OnPlayerLeftRoom(Player otherPlayer)
    {
        dealer.numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
    }
}
