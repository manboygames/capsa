using UnityEngine;
using System.Collections;

public class CardTest : MonoBehaviour {

    public void CardFlip()
    {
        GetComponent<Card>().Flip();
    }

}
