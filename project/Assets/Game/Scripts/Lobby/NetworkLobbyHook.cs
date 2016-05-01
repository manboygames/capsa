using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook
{

    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayerGO, GameObject gamePlayerGO)
    {
        LobbyPlayer lobbyPlayer = lobbyPlayerGO.GetComponent<LobbyPlayer>();

        /*Character lobbyCharacter = lobbyPlayerGO.GetComponentInChildren<Character>();
        GameCharacter gameCharacter = gamePlayerGO.GetComponentInChildren<GameCharacter>();*/

        GamePlayer gamePlayer = gamePlayerGO.GetComponent<GamePlayer>();
        gamePlayer.playerID = LobbyManager.Instance.lobbyPlayers.IndexOf(lobbyPlayer);
        gamePlayer.totalPlayers = LobbyManager.Instance.numPlayers;
        gamePlayer.transform.position = new Vector3(100, 100, 100);
    }

}
