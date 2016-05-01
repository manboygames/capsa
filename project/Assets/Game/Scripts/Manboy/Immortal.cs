using UnityEngine;
using System.Collections;

public class Immortal : MonoBehaviour {
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

}
