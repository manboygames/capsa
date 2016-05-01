using UnityEngine;
using System.Collections;
using AdvancedInspector;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour {

    private static CharacterManager instance;

    public static CharacterManager Instance
    {
        get
        {
            return instance;
        }
    }

    [Inspect,Group("Data"),ReadOnly]
    public string characterName;
    [Inspect, Group("Data"), ReadOnly]
    public int characterID;

    private int localPlayerID = -1;

    public int LocalPlayerID
    {
        get
        {
            return localPlayerID;
        }

        set
        {
            localPlayerID = value;
        }
    }

    public List<Character> characters;

    void Awake()
    {
        if(CharacterManager.instance != null)
        {
            Destroy(gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
