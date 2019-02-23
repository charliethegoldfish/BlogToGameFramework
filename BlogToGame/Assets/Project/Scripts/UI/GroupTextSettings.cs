using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupTextSettings : MonoBehaviour {

	public Text[] textObjects;

	void OnEnable()
	{
		Debug.Log("hey I am being called, texty text text");
		textObjects = this.GetComponentsInChildren<Text>();

		// setFont();
	}

	public void setFont(Font newFont)
	{
		if(textObjects.Length <= 0) 
		{
			Debug.Log("We've got null text here", this);
			return;
		}

		for(int i = 0; i < textObjects.Length; i++)
		{
			textObjects[i].font = newFont;
		}

		
	}

	public void setFontSize(int size)
	{
		if(textObjects.Length <= 0) 
		{
			Debug.Log("We've got null text here", this);
			return;
		}

		for(int i = 0; i < textObjects.Length; i++)
		{
			textObjects[i].fontSize = size;
		}
	}
}
