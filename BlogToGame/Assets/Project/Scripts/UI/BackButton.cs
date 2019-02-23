using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour {

	public Button backButton;

	void OnEnable()
	{
		RelayCentre.addSubscriber(gameObject, Message.BackButtonHit, goBack);
	}

	void OnDisable()
	{
		RelayCentre.removeSubscriber(gameObject, Message.BackButtonHit);
	}

	void goBack()
	{
		backButton.onClick.Invoke();
	}
}
