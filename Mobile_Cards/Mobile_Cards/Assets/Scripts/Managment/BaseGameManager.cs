using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseGameManager : MonoBehaviourPunCallbacks
{
    //protected Dictionary<string, Player> players = new Dictionary<string, Player>();

    public virtual void GameStart()
    {
        //Do uzupelnienia
    }

    public virtual void EndGame()
    {
        StartCoroutine(ClearCardsFromScene(2.0f));
    }

    public virtual void InitializePlayer(Photon.Realtime.Player photonPlayer)
    {

        /*Player player = new(photonPlayer);
        photonPlayer.NickName = "PhotonPlayer"; //potem dam opcje wpisywania nazwy gracza
        players.Add(photonPlayer.NickName, player);
        players[photonPlayer.NickName] = player;
        Debug.Log(players.Count);
        Debug.Log(photonPlayer.NickName);
        //Debug.Log(players[photonPlayer.NickName].position);*/
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        /*if(PhotonNetwork.CurrentRoom.PlayerCount > 3)
        {
            PhotonNetwork.LeaveRoom();

        }
        
        Player player = new(newPlayer);
        newPlayer.NickName = "PhotonPlayer2"; //potem dam opcje wpisywania nazwy gracza
        players.Add(newPlayer.NickName, player);
        players[newPlayer.NickName] = player;
        Debug.Log(players.Count);

        if (players.Count >= 2)
        {
            GameStart();
        }*/
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        /*players.Remove(otherPlayer.NickName);

        if(players.Count < 2)
        {
            EndGame();
        }*/
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
