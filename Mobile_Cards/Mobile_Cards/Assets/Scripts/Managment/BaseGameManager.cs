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

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount > 3)
        {
            PhotonNetwork.LeaveRoom();

        }
        Debug.Log("Player joined room");
    }


    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        //players.Remove(otherPlayer.NickName);
        /*
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
