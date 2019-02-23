using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogBoxController : MonoBehaviour {

	public Text textObject;
	Paragraph currentParagraph;
	int dialogIndex = 0;
	bool writingText = false;
	bool waitingForNextClick = false;

	bool dialogOver = false;

	IEnumerator typeTextCached;

	bool trailerActive;

	void Update()
	{
		// if(!GameController.instance.gamePlaying) return;

		if(Input.GetMouseButtonUp(0))
		{
			click();
		}

		if(Input.GetKeyUp(KeyCode.Escape))
		{
			dialogOver = true;
			click();
		}
	}

	void click()
	{
		RelayCentre.postMessage(Message.TimeOutCancel);

		if(trailerActive)
		{
			if(dialogOver)
			{
				string dialogBoxToUse = SettingsController.instance.getDialogBoxToUse();
				MenuController.instance.hideDialogScreen(dialogBoxToUse);
				RelayCentre.postMessage(Message.DialogFinished);
				RelayCentre.postMessage(Message.NextTrailerPiece);
				trailerActive = false;
				return;
			}
		}
		
		if (writingText)
        {
            if(typeTextCached != null) StopCoroutine(typeTextCached);
            textObject.text = currentParagraph.sentences[dialogIndex];
            writingText = false;
			waitingForNextClick = true;
            return;
        }

		if(dialogOver)
		{
			string dialogBoxToUse = SettingsController.instance.getDialogBoxToUse();
			MenuController.instance.hideDialogScreen(dialogBoxToUse);
			RelayCentre.postMessage(Message.DialogFinished);
			RelayCentre.postMessage(Message.NextTrailerPiece);
			return;
		}

		if(waitingForNextClick)
		{
			dialogIndex++;
			waitingForNextClick = false;
			displaySentence();
			return;
		}
	}

	//we'll do more with this later
	public void startDialog(Paragraph paragraph)
	{
		dialogIndex = 0;
		currentParagraph = paragraph;
		writingText = false;
		waitingForNextClick = false;
		dialogOver = false;
		RelayCentre.postMessage(Message.DialogStarted);

		string dialogBoxToUse = SettingsController.instance.getDialogBoxToUse();
		MenuController.instance.loadDialogScreen(dialogBoxToUse);
		displaySentence();

		RelayCentre.postMessage(Message.TimeOutCancel);
	}

	public void noMoreDialog()
	{
		dialogIndex = 0;
		writingText = false;
		waitingForNextClick = false;
		RelayCentre.postMessage(Message.DialogStarted);

		string dialogBoxToUse = SettingsController.instance.getDialogBoxToUse();
		MenuController.instance.loadDialogScreen(dialogBoxToUse);

		dialogOver = true;

		string[] splitText = new string[1];
		splitText[0] = "Oh, I don't have anything else to say";
		typeTextCached = typeText(splitText);
		StartCoroutine(typeTextCached);
	}

	void displaySentence()
	{
		if(dialogIndex >= currentParagraph.sentences.Count)
		{
			string dialogBoxToUse = SettingsController.instance.getDialogBoxToUse();
			MenuController.instance.hideDialogScreen(dialogBoxToUse);

			RelayCentre.postMessage(Message.DialogFinished);
			RelayCentre.postMessage(Message.NextTrailerPiece);
			return;
		}
		
		string textToSplit = currentParagraph.sentences[dialogIndex];
		string[] splitText = textToSplit.Split(' ');

		typeTextCached = typeText(splitText);
		StartCoroutine(typeTextCached);
	}

	IEnumerator typeText(string[] text)
	{
		writingText = true;
		textObject.text = "";

		string soundSetToUse = "";

		float check = Random.Range(0f, 1f);
		if(check >= 0.5)
		{
			soundSetToUse = "TypeWriterClick";
		} else
		{
			//soundSetToUse = "TypeWriterClickVintage";
			soundSetToUse = "TypeWriterClick";
		}

		for(int i = 0; i < text.Length; i++)
		{
			for(int a = 0; a < text[i].Length; a++)
			{
				textObject.text += text[i][a];
				yield return null;
			}
			textObject.text += " ";

			float value = Random.Range(0f, 1f);

			//we need it to always play on the first word and last, then a chance of playing
			if(i == 1)
			{
				SoundController.instance.playAudioEntry(soundSetToUse);
			} else if (i == text.Length - 1)
			{
				SoundController.instance.playAudioEntry(soundSetToUse);
			} else if(value < 0.6)
			{
				SoundController.instance.playAudioEntry(soundSetToUse);
			} else if(value < 0.7)
			{
				SoundController.instance.playAudioEntry("TypeWriterClickFlavour");
			}
			
			yield return null;
		}
		
		writingText = false;
		waitingForNextClick = true;
	}

	public void typeTitleText()
	{
		StartCoroutine(typeSpecificText("In My Mind..."));
		trailerActive = true;
	}

	IEnumerator typeSpecificText(string text)
	{
		writingText = true;
		dialogOver = false;

		textObject.text = "";
		string soundSetToUse = "";

		for(int i = 0; i < text.Length; i++)
		{
			if(text[i] == ' ')
			{
				soundSetToUse = "TypeWriterSpace";

			} else 
			{
				soundSetToUse = "TypeWriterClick";
			}

			textObject.text += text[i];

			SoundController.instance.playAudioEntry(soundSetToUse);

			float time = Random.Range(0.1f, 0.3f);

			yield return new WaitForSeconds(time);
		}

		dialogOver = true;

		yield return null;
	}
}
