using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandTest : MonoBehaviour {

    public List<CardData> handA = new List<CardData>();
    public List<CardData> handB = new List<CardData>();

    public void CompareHands()
    {
        Debug.Log(HandEvaluator.CompareHand(handA, handB));
    }

    public void EvaluateHand()
    {
        Debug.Log(HandEvaluator.EvaluateHand(handA));
    }

}
