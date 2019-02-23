using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilterTriggersText : MonoBehaviour {

	// public Toggle[] triggers;

	public GameObject clarificationObject;
	public Text clarificationText;

	bool setup = false;

	void OnEnable()
	{
		if(!setup)
		{
			RelayCentre.addSubscriber(gameObject, Message.TriggerFilterUpdated, updateText);
			setup = true;
		}

		updateText();
	}

	void updateText()
	{
		List<Triggers> triggersToFilter;
		triggersToFilter = SaveController.instance.settings.triggersToFilter;

		List<string> triggerDisplayText = new List<string>();
		
		//grab everything we want to display
		for(int i = 0; i < triggersToFilter.Count; i++)
		{
			Triggers trigger = triggersToFilter[i];

			for(int t = 0; t < SettingsController.instance.triggerSettings.Count; t++)
			{
				TriggerSettings triggerSetting = SettingsController.instance.triggerSettings[t];
				if(trigger.ToString() == triggerSetting.key)
				{
					triggerDisplayText.Add(triggerSetting.textDisplay);
				}
			}
		}

		if(triggerDisplayText.Count > 0)
		{
			string textToDisplay = "";

			for(int i = 0; i < triggerDisplayText.Count; i++)
			{
				textToDisplay += triggerDisplayText[i];

				int wordsLeft = triggerDisplayText.Count - i;
				if(wordsLeft > 1)
				{
					textToDisplay += ", ";
				}
			}

			clarificationText.text = textToDisplay;
			
			clarificationObject.SetActive(true);
		} else
		{
			clarificationObject.SetActive(false);
		}		

	}

}
