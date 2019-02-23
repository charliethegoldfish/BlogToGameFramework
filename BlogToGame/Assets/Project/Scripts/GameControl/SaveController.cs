using System.Collections;

using System.Collections.Generic;
using System.IO;
using UnityEngine;

using System;

//********************************************************************************************** */
//Save Settings 
//********************************************************************************************** */

[System.Serializable]
public class BoolSetting
{
	public string key;
	public bool setting;
}

[System.Serializable]
public class IntSetting
{
	public string key;
	public int setting;
}

// [System.Serializable]

//Charlie: Hey, maybe make most of the variables private aye
[System.Serializable]
public class SaveSettings
{
	[Header("Dialog Keys")]
	public List<string> dialogKeysRead = new List<string>();

	public void addDialogKeyRead(string key)
	{
		dialogKeysRead.Add(key);
	}

	public void resetDialogRead()
	{
		dialogKeysRead.Clear();
		RelayCentre.postMessage(Message.ReloadTriggers);
	}

	//********************************************************************************************** */
	//Trigger Settings 
	//********************************************************************************************** */
	[Header("Trigger Settings")]
	public List<Triggers> triggersToFilter;
	public void resetTriggersToFilter(List<Triggers> triggers)
	{
		triggersToFilter = triggers;
	}

	//********************************************************************************************** */
	//Bool Settings 
	//********************************************************************************************** */
	[Header("Bool Settings")]
	public List<BoolSetting> boolSettings = new List<BoolSetting>();
	public void setBool(string key, bool setting)
	{
		for(int i = 0; i < boolSettings.Count; i++)
		{
			if(key.ToLower() == boolSettings[i].key.ToLower())
			{
				boolSettings[i].setting = setting;
				return;
			}
		}
		
		BoolSetting newSetting = new BoolSetting();
		newSetting.key = key;
		newSetting.setting = setting;

		boolSettings.Add(newSetting);
		
		Debug.Log("Couldn't find a setting with the name " + key);
	}

	public bool findSetting(string key)
	{
		for(int i = 0; i < boolSettings.Count; i++)
		{
			if(key.ToLower() == boolSettings[i].key.ToLower())
			{
				return boolSettings[i].setting;
			}
		}

		for(int i = 0; i < triggersToFilter.Count; i++)
		{
			if(key.ToLower() == triggersToFilter[i].ToString().ToLower()) return true;
		}

		Debug.Log("Couldn't find a setting with the name " + key);
		return false;
	}

	public List<BoolSetting> settingsStartingWith(string key)
	{
		List<BoolSetting> settings = new List<BoolSetting>();

		for(int i = 0; i < boolSettings.Count; i++)
		{
			if(boolSettings[i].key.ToLower().StartsWith(key))
			{
				settings.Add(boolSettings[i]);
			}
		}

		return settings;
	}

	//********************************************************************************************** */
	//Key Settings 
	//********************************************************************************************** */
	public class KeyForValueSettings
	{
		public string key;
		public string value;
	}

	public List<KeyForValueSettings> keyForValue = new List<KeyForValueSettings>();

	public string getSettingForKey(string key)
	{
		for(int i = 0; i < keyForValue.Count; i++)
		{
			if(key.ToLower() == keyForValue[i].key.ToLower())
			{
				return keyForValue[i].value;
			}
		}

		return null;
	}

	public void setSettingWithKey(string key, string value)
	{
		for(int i = 0; i < keyForValue.Count; i++)
		{
			if(key.ToLower() == keyForValue[i].key.ToLower())
			{
				keyForValue[i].value = value;
				return;
			}
		}
		
		KeyForValueSettings newSetting = new KeyForValueSettings();
		newSetting.key = key;
		newSetting.value = value;

		keyForValue.Add(newSetting);
		
		Debug.Log("Couldn't find a setting with the name " + key);
	}

	//********************************************************************************************** */
	//New Paragraphs
	//********************************************************************************************** */
	[Header("New Paragraphs Downloaded")]
	public List<Paragraph> newParagraphs = new List<Paragraph>();

	public List<string> newUrlsUsed = new List<string>();

	public bool urlHasBeenUsed(string url)
	{
		for(int i = 0; i < newUrlsUsed.Count; i++)
		{
			if(url == newUrlsUsed[i]) return true;
		}

		return false;
	}

	public void addUrlsWeUsed(List<string> urls)
	{
		for(int i = 0; i < urls.Count; i++)
		{
			newUrlsUsed.Add(urls[i]);
		}
	}

	//********************************************************************************************** */
	//Other keys to keep track of
	//********************************************************************************************** */
	[Header("Int Settings")]
	public List<IntSetting> intSettings;
	public void setInt(string key, int setting)
	{
		for(int i = 0; i < intSettings.Count; i++)
		{
			if(key.ToLower() == intSettings[i].key.ToLower())
			{
				intSettings[i].setting = setting;
				return;
			}
		}
		
		IntSetting newSetting = new IntSetting();
		newSetting.key = key;
		newSetting.setting = setting;

		intSettings.Add(newSetting);
		
		Debug.Log("Couldn't find a setting with the name " + key);
	}

	public void incrementInt(string key)
	{
		for(int i = 0; i < intSettings.Count; i++)
		{
			if(key.ToLower() == intSettings[i].key.ToLower())
			{
				intSettings[i].setting += 1;
				return;
			}
		}

		IntSetting newSetting = new IntSetting();
		newSetting.key = key;
		newSetting.setting = 1;

		intSettings.Add(newSetting);
		
		Debug.Log("Couldn't find a setting with the name " + key);
	}

	public int findIntSetting(string key, int defaultValue = 0)
	{
		int value = defaultValue;

		for(int i = 0; i < intSettings.Count; i++)
		{
			if(key.ToLower() == intSettings[i].key.ToLower())
			{
				return intSettings[i].setting;
			}
		}

		return value;
	}
}

//********************************************************************************************** */
//Save Controller 
//********************************************************************************************** */
public class SaveController : LoadingObject {
	public SaveSettings settings = new SaveSettings();

	private static SaveController sharedInstance = null;
	public static SaveController instance {
		get {
			if (sharedInstance == null) {
				sharedInstance = GameObject.FindObjectOfType<SaveController>();

				if (!sharedInstance) {
					Debug.LogError ("No SaveController object found, there needs to be one SaveController object in scene");
				}
			}
			return sharedInstance;
		}
	}

	public override void load(Action completion)
	{
		loadSettings();

		RelayCentre.addSubscriber(gameObject, Message.SaveGame, saveSettings);
		RelayCentre.addSubscriber(gameObject, Message.LoadGame, loadSettings);

		completion();
	}

	public void saveSettings()
	{
		using(StreamWriter jFile = new StreamWriter(Application.persistentDataPath + "/saveData.json"))
		{
			string line = JsonUtility.ToJson(settings);
			jFile.Write(line);
		}
	}

	public void loadSettings()
	{
		string path = Application.persistentDataPath + "/saveData.json";
		
		if(!File.Exists(path))
		{
			SettingsController.instance.toggleSoundEffects(true);
			
			settings.setBool("Font_Arial", false);
			settings.setBool("Font_OpenDyslexic", false);
			settings.setBool("Font_Telegrama", true);

			settings.setBool("DialogBox_Bottom", true);
			settings.setBool("DialogBox_Fullscreen", false);

			SettingsController.instance.toggleGameLoaded(true);
			
			saveSettings();
			return;
		} 
		using(StreamReader jFile = new StreamReader(path))
		{
			string contents = jFile.ReadToEnd();
			if(string.IsNullOrEmpty(contents)) return;
			settings = JsonUtility.FromJson<SaveSettings>(contents);
		}
	}

	public void resetContent()
	{
		settings.resetDialogRead();
		saveSettings();
	}

	//********************************************************************************************** */
	//Clear out the save settings for debug purposes
	//********************************************************************************************** */
	public void clearSettings()
	{
		settings.dialogKeysRead.Clear();
		settings.triggersToFilter.Clear();
		settings.boolSettings.Clear();
		saveSettings();
	}

}
