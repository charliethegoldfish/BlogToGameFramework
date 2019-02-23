using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextSettings : MonoBehaviour {

	public Text text;

	public void setFont(Font newFont)
	{
		if(text == null) 
		{
			Debug.Log("We've got null text here", this);
			return;
		}
			
		text.font = newFont;
	}

	public void setFontSize(int size)
	{
		text.fontSize = size;
	}
}
