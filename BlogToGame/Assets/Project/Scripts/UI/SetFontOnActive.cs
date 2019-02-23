using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetFontOnActive : MonoBehaviour {

	public Text text;

	void OnEnable()
	{
		// Font currentFont = SettingsController.instance.getCurrentFont();
		// setFont(currentFont);
	}

	void setFont(Font font)
	{
		text.font = font;
	}
}
