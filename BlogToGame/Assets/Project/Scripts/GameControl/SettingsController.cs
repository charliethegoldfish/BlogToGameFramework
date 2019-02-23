using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System;

[System.Serializable]
public class TriggerSettings
{
	public string key = "";
	public Triggers trigger;
	public bool filterOut;
	public string textDisplay;
}

public class SettingsController : LoadingObject {

	private static SettingsController sharedInstance = null;

	public static SettingsController instance {
		get {
			if (sharedInstance == null) {
				sharedInstance = GameObject.FindObjectOfType<SettingsController>();

				if (!sharedInstance) {
					Debug.LogError ("No SettingsController object found, there needs to be one SettingsController object in scene");
				}
			}
			return sharedInstance;
		}
	}

	public List<TriggerSettings> triggerSettings;

	private List<Triggers> triggersToFilter = new List<Triggers>();

	public override void load(Action completion)
	{
		fillTriggerSettingsList();
		
		RelayCentre.postMessage(Message.SettingsControllerLoaded);
		completion();
	}

	//********************************************************************************************** */
	//Trigger Settings 
	//********************************************************************************************** */

	public void fillTriggerSettingsList()
	{
		triggerSettings = new List<TriggerSettings>();
		//we want to loop through and add each enum value to the List
		//eventually we need to also set the settings from file
		foreach (Triggers trigger in System.Enum.GetValues(typeof(Triggers)))
		{
			TriggerSettings newTrigger = new TriggerSettings();
			newTrigger.trigger = trigger;
			newTrigger.key = trigger.ToString();
			newTrigger.textDisplay = Regex.Replace(newTrigger.key, "(\\B[A-Z])", " $1");

			triggerSettings.Add(newTrigger);
			Debug.Log("Trigger Settings: we have added a new trigger - " + newTrigger.key);
		}
	}

	public void setTriggerSettings(string key, bool filterOut)
	{
		for(int i = 0; i < triggerSettings.Count; i++)
		{
			if(key == triggerSettings[i].key)
			{
				triggerSettings[i].filterOut = filterOut;
				return;
			}
		}
		Debug.LogError("TriggerSetting: Couldn't find trigger - " + key);
	}

	public void setTriggerFilters()
	{
		triggersToFilter.Clear();

		for(int i = 0; i < triggerSettings.Count; i++)
		{
			if(triggerSettings[i].filterOut)
			{
				triggersToFilter.Add(triggerSettings[i].trigger);
			}
		}

		SaveController.instance.settings.resetTriggersToFilter(triggersToFilter);
		RelayCentre.postMessage(Message.SaveGame);
	}

	public List<Triggers> getTriggersToFilter()
	{
		return triggersToFilter;
	}

	//********************************************************************************************** */
	//Sound Settings 
	//********************************************************************************************** */
	public void toggleSoundEffects(bool soundEffectToggle)
	{
		SaveController.instance.settings.setBool("soundEffects", soundEffectToggle);
		SaveController.instance.saveSettings();
	}

	public void toggleMusic(bool musicToggle)
	{
		SaveController.instance.settings.setBool("music", musicToggle);
		SaveController.instance.saveSettings();
	}

	public bool playSoundEffects()
	{
		return SaveController.instance.settings.findSetting("soundEffects");
	}

	public bool playMusic()
	{
		return SaveController.instance.settings.findSetting("music");
	}

	//********************************************************************************************** */
	//Font Settings 
	//********************************************************************************************** */
	public Font getCurrentFont()
	{
		List<BoolSetting> fontSettings = SaveController.instance.settings.settingsStartingWith("font_");
		
		for(int i = 0; i < fontSettings.Count; i++)
		{
			if(fontSettings[i].setting)
			{
				string fontName = fontSettings[i].key.Replace("Font_", "");
				MenuController.instance.findFont(fontName);
			}
		}

		return null;
	}

	//********************************************************************************************** */
	//Dialog Box Settings 
	//********************************************************************************************** */
	public void setDialogBoxSettings(string key, bool setting)
	{
		SaveController.instance.settings.setBool(key, setting);
		SaveController.instance.saveSettings();
	}

	public string getDialogBoxToUse()
	{
		bool fullscreen = SaveController.instance.settings.findSetting("DialogBox_Fullscreen");

		if(fullscreen) return "FullscreenDialog";
		
		return "NormalDialog";
	}

	//********************************************************************************************** */
	//Other Settings 
	//********************************************************************************************** */
	public bool gameHasBeenLoadedBefore()
	{
		return SaveController.instance.settings.findSetting("gameHasBeenLoadedBefore");
	}

	public void toggleGameLoaded(bool toggle)
	{
		SaveController.instance.settings.setBool("gameHasBeenLoadedBefore", toggle);
	}

	public bool hasShownTriggerWarning()
	{
		return SaveController.instance.settings.findSetting("shownTriggerWarning");
	}

	public void toggleShownTriggerWarning(bool toggle)
	{
		SaveController.instance.settings.setBool("shownTriggerWarning", toggle);
	}

	//we will make this a bit more secure when we legit have iap hooked up
	public bool hasPaid()
	{
		return SaveController.instance.settings.findSetting("hasPaid");
	}

	public void setHasPaid(bool toggle)
	{
		SaveController.instance.settings.setBool("hasPaid", toggle);
	}
}
