using UnityEngine;
using System.Collections;

public class KGFSingleton : MonoBehaviour {

    private static KGFSingleton instance;

    public static KGFSingleton Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if(KGFSingleton.Instance != null)
        {
            Destroy(gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
