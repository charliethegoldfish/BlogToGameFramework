using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffIfDemoMode : MonoBehaviour {

	// Use this for initialization
	void OnEnable()
	{
		if(GameController.instance.demoMode)
		{
			gameObject.SetActive(false);
		}
	}
}
