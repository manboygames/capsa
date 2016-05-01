using UnityEngine;
using System.Collections;
using System;

public class LobbyMenu : MonoBehaviour {

    public Transform playerContainer;

    IEnumerator Start()
    {
        while (LobbyManager.Instance == null)
        {
            yield return null;
        }
    }

    public void SetPlayerLobbyPosition(GameObject lobbyPlayer)
    {
        lobbyPlayer.transform.SetParent(playerContainer);
        lobbyPlayer.transform.localScale = new Vector3(1, 1, 1);
    }

    public Transform GetPlayerLobbyParent()
    {
        return playerContainer;
    }

}
