using UnityEngine;
using System.Collections;

public class ShowStates : MonoBehaviour {

	public Animator[] mCharacters;
	
	public enum Status
	{
		Idle,
		Win,
		Lose
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.F1)) SetStatus(Status.Idle);
		else if(Input.GetKeyDown(KeyCode.F2)) SetStatus(Status.Win);
		else if(Input.GetKeyDown (KeyCode.F3)) SetStatus(Status.Lose);
	}

	public void SetStatus (Status status)
	{
		switch(status)
		{
		case Status.Idle:
			foreach(Animator ch in mCharacters)ch.SetTrigger("Idle");
			break;
		
		case Status.Win:
			foreach(Animator ch in mCharacters) ch.SetTrigger("Win");
			break;

		case Status.Lose:
			foreach(Animator ch in mCharacters) ch.SetTrigger("Lose");
			break;
		}
	}
}
