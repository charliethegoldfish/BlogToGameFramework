using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionNumber : MonoBehaviour {

	public Text text;

	void OnEnable()
	{
		if(text != null)
		{
			text.text = Application.version;
		} else
		{
			text = gameObject.GetComponent<Text>();
		}
	}
}
