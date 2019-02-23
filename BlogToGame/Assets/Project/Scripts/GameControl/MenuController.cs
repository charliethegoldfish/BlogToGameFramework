using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[System.Serializable]
public class MenuScreen
{
    public string screenName = "";
    public GameObject screenObject;
    public GameObject selectableButton;
}

[System.Serializable]
public class ToggleSetting
{
    public string key;
    public Toggle[] toggles;
}

[System.Serializable]
public class AvailableFont
{
    public string key;
    public Font font;
}

public class MenuController : LoadingObject {

    private static MenuController sharedInstance = null;

    public static MenuController instance
    {
        get
        {
            if (sharedInstance == null)
            {
                sharedInstance = GameObject.FindObjectOfType<MenuController>();

                if (!sharedInstance)
                {
                    Debug.LogError("No MenuController object found, there needs to be one MenuController object in scene");
                }
            }
            return sharedInstance;
        }
    }

    public GameObject menu;
    public GameObject menuButton;

    [Header("Screens")]
    public MenuScreen[] menuScreens;

    public MenuScreen[] dialogScreens;

    [Header("Settings")]
    public ToggleSetting[] toggleSettings;

    [Header("Fonts To Choose From")]
    public AvailableFont[] availableFonts;

    [Header("Text Objects For Font Switching")]
    public List<TextSettings> textObjects;

    [Header("Sound Effects")]
    public string buttonPressKey;
    public string selectionPressKey;

    [Header("Controller Support")]
    public GameObject firstSelectable;

    public override void load(Action completion)
    {
        menu.SetActive(true);
        menuButton.SetActive(false);
        loadMenuScreen("Loading");

        RelayCentre.addSubscriber(gameObject, Message.GameLoaded, loadMainMenu);
        RelayCentre.addSubscriber(gameObject, Message.PauseGame, loadMainMenu);
        RelayCentre.addSubscriber(gameObject, Message.SettingsControllerLoaded, loadSettings);

        completion();
    }

    void loadSettings()
    {
        for(int i = 0; i < toggleSettings.Length; i++)
        { 
            for(int a = 0; a < toggleSettings[i].toggles.Length; a++)
            {
                toggleSettings[i].toggles[a].isOn = SaveController.instance.settings.findSetting(toggleSettings[i].key);
            }
        }
    }

    public void loadMainMenu()
    {
        //turnOffEverythingInMenu();
        menu.SetActive(true);
        menuButton.SetActive(false);
        loadMenuScreen("MainMenu");

        //Charlie: do a check for a controller though aye?
        // EventSystem.current.SetSelectedGameObject(firstSelectable);
    }

    public void loadMenuScreen(string screenName)
    {
        turnOffEverythingInMenu();
        loadScreen(screenName, menuScreens);
    }

    public void loadDialogScreen(string screenName)
    {
        loadScreen(screenName, dialogScreens);
    }

    public void hideDialogScreen(string screenName)
    {
        hideScreen(screenName, dialogScreens);
    }

    public MenuScreen getDialogScreen(string screenName)
    {
        for (int i = 0; i < dialogScreens.Length; i++)
        {
            if (dialogScreens[i].screenName == screenName)
            {
                return dialogScreens[i];
            }
        }
        Debug.LogError("Can't find the screen of name" + screenName);
        return null;
    }

    //********************************************************************************************** */
	//Trigger Settings 
	//********************************************************************************************** */
    public void setSelfHarmTrigger(bool filterOut)
    {
        SettingsController.instance.setTriggerSettings("SelfHarm", filterOut);
        if(GameController.instance.gameHasLoaded) SettingsController.instance.setTriggerFilters();
        loadSettings();
        SaveController.instance.saveSettings();
        RelayCentre.postMessage(Message.TriggerFilterUpdated);
    }

    public void setSuicideTrigger(bool filterOut)
    {
        SettingsController.instance.setTriggerSettings("Suicide", filterOut);
        if(GameController.instance.gameHasLoaded) SettingsController.instance.setTriggerFilters();
        loadSettings();
        SaveController.instance.saveSettings();
        RelayCentre.postMessage(Message.TriggerFilterUpdated);
    }

    public void setEatingDisorderTrigger(bool filterOut)
    {
        SettingsController.instance.setTriggerSettings("EatingDisorder", filterOut);
        if(GameController.instance.gameHasLoaded) SettingsController.instance.setTriggerFilters();
        loadSettings();
        SaveController.instance.saveSettings();
        RelayCentre.postMessage(Message.TriggerFilterUpdated);
    }

    public void setWithdrawalsTrigger(bool filterOut)
    {
        SettingsController.instance.setTriggerSettings("Withdrawals", filterOut);
        if(GameController.instance.gameHasLoaded) SettingsController.instance.setTriggerFilters();
        loadSettings();
        SaveController.instance.saveSettings();
        RelayCentre.postMessage(Message.TriggerFilterUpdated);
    }
    public void leaveTriggerSettings()
    {
        SettingsController.instance.setTriggerFilters();
        RelayCentre.postMessage(Message.ReloadTriggers);
    }

    //********************************************************************************************** */
	//Dialog Box Settings 
	//********************************************************************************************** */
    public void setDialogBoxBottom(bool setting)
    {
        SettingsController.instance.setDialogBoxSettings("DialogBox_Bottom", setting);

    }

    public void setDialogBoxFullscreen(bool setting)
    {
        SettingsController.instance.setDialogBoxSettings("DialogBox_Fullscreen", setting);
    }

    //********************************************************************************************** */
	//Sound Settings 
	//********************************************************************************************** */
    public void setSoundEffects(bool toggle)
    {
        SettingsController.instance.toggleSoundEffects(toggle);
    }

    public void setMusic(bool toggle)
    {
        SettingsController.instance.toggleMusic(toggle);
    }

    //********************************************************************************************** */
	//Sound Effects
	//********************************************************************************************** */
    public void buttonClickSoundFX()
    {
        SoundController.instance.playAudioEntry(buttonPressKey);
    }

    public void selectionClickSoundFX()
    {
        SoundController.instance.playAudioEntry(selectionPressKey);
    }

    //********************************************************************************************** */
	//Settings 
	//********************************************************************************************** */
    public void resetContent()
    {
       //Charlie: maybe hook this up with notifications
       SaveController.instance.resetContent();
    }

    //********************************************************************************************** */
	//Accessability 
	//********************************************************************************************** */
    public Font findFont(string key)
    {
        for(int i = 0; i < availableFonts.Length; i++)
        {
            if(key.ToLower() == availableFonts[i].key.ToLower())
            {
                return availableFonts[i].font;
            }
        }
        Debug.Log("Couldn't find the font named " + key);
        return null;
    }

    public void changeFont(string key)
    {
        Font newFont = findFont(key);

        for(int i = 0; i < textObjects.Count; i++)
        {
            textObjects[i].setFont(newFont);
        }
    }

    public void changeFontSize(int size)
    {
         for(int i = 0; i < textObjects.Count; i++)
        {
            textObjects[i].setFontSize(size);
        }
    }

    void setObjectsActive(List<GameObject> objects, bool active)
    {
        for(int i = 0; i < objects.Count; i++)
        {
            objects[i].SetActive(active);
        }
    }

    public void changeFontToArial(bool selected)
    {
        if(!selected) return;
        
        changeFont("Arial");
        SaveController.instance.settings.setBool("Font_Arial", true);
        SaveController.instance.settings.setBool("Font_OpenDyslexic", false);
        SaveController.instance.settings.setBool("Font_Telegrama", false);
        RelayCentre.postMessage(Message.SaveGame);

        // controllerMapObject.the
    }

    public void changeFontToDyslexic(bool selected)
    {
        if(!selected) return;
        
        changeFont("OpenDyslexic");
        SaveController.instance.settings.setBool("Font_Arial", false);
        SaveController.instance.settings.setBool("Font_OpenDyslexic", true);
        SaveController.instance.settings.setBool("Font_Telegrama", false);
        RelayCentre.postMessage(Message.SaveGame);
    }

    public void changeFontToTelegrama(bool selected)
    {
        if(!selected) return;

        changeFont("Telegrama");
        SaveController.instance.settings.setBool("Font_Arial", false);
        SaveController.instance.settings.setBool("Font_OpenDyslexic", false);
        SaveController.instance.settings.setBool("Font_Telegrama", true);
        RelayCentre.postMessage(Message.SaveGame);
    }

     //Charlie: probably also set which font we're using from save settings

     //********************************************************************************************** */
	 //Control Remapping
	 //********************************************************************************************** */

     //Controller Remapping

    //  Opening the screen: ControlMapper.Open();
    //  Closing the screen: ControlMapper.Close();
     public void openControlMapper()
     {
        loadMenuScreen("ControllerMapping");
     }

     public void closeControlMapper()
     {
         loadMenuScreen("Settings");
     }

    //********************************************************************************************** */
	//Load screens 
	//********************************************************************************************** */

    //we'll want to use this exact code across all screens we want to bring up
    private bool loadScreen(string screenName, MenuScreen[] screens)
    {
        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i].screenName == screenName)
            {
                screens[i].screenObject.SetActive(true);
                //screens[i].screenObject.SendMessage("SetSelected", SendMessageOptions.DontRequireReceiver);
                return true;
            }
        }
        Debug.LogError("Can't find the screen of name" + screenName);
        return false;
    }

    private bool hideScreen(string screenName, MenuScreen[] screens)
    {
        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i].screenName == screenName)
            {
                screens[i].screenObject.SetActive(false);
                return true;
            }
        }
        Debug.LogError("Can't find the screen of name " + screenName);
        return false;
    }

    private void turnOffEverythingInMenu()
    {
        for(int i = 0; i < menuScreens.Length; i++)
        {
            if(menuScreens[i].screenObject == null) continue;
            menuScreens[i].screenObject.SetActive(false);
        }
    }

    public void closeMenu()
    {
        menu.SetActive(false);
        menuButton.SetActive(true);
    }

    public void playGame()
    {
        if(!SettingsController.instance.hasShownTriggerWarning())
        {
            //Charlie: - do something to hook this in with demo builds so it always shows
            if(!GameController.instance.demoMode) SettingsController.instance.toggleShownTriggerWarning(true);
            loadMenuScreen("TriggerWarning");
            SaveController.instance.saveSettings();
        } else
        {
            startGame();
        }
    }
    
    //GAME PLAY STUFF I GUESS
    public void startGame()
    {
        closeMenu();
        StartCoroutine("startTheGame");
        RelayCentre.postMessage(Message.TimeOutCancel);
    }

    public void pauseGame()
    {
        RelayCentre.postMessage(Message.PauseGame);
    }

    public void exitGame()
    {
        Application.Quit();
    }

    //lil hack to delay us starting to take input
    //pretty sure I fixed this, but hey
    private IEnumerator startTheGame()
    {
        yield return null;
        RelayCentre.postMessage(Message.GameWillBeginNotification);
    }
}
