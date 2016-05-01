using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour {

    public List<TurnPlayer> players;
    public Button startButton;

    private int currentPlayerIndex;
    private TurnPlayer currentTurnPlayer;

    void Start()
    {
        foreach (var player in players)
        {
            player.Activate(false);
            player.onDestroy.AddListener(OnPlayerDestroyed);
        }
    }

    public void StartGame()
    {
        currentPlayerIndex = UnityEngine.Random.Range(0, players.Count);
        currentTurnPlayer = players[currentPlayerIndex];
        currentTurnPlayer.Activate(true);
        Destroy(startButton.gameObject);
    }

    public void NextPlayer()
    {
        if(players.Count < 2)
        {
            Debug.Log("Game Over");
        }

        if(currentTurnPlayer != null)
        {
            currentTurnPlayer.Activate(false);
        }
        else
        {
            currentPlayerIndex--;
        }

        currentTurnPlayer = null;

        while(currentTurnPlayer == null)
        {
            currentPlayerIndex++;

            if (currentPlayerIndex >= players.Count)
            {
                currentPlayerIndex = 0;
            }

            currentTurnPlayer = players[currentPlayerIndex];
        }

        currentTurnPlayer.Activate(true);
    }

    private void OnPlayerDestroyed(TurnPlayer player)
    {
        players.Remove(player);
    }
}
