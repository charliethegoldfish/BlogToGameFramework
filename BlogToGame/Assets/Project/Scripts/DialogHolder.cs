using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Random = UnityEngine.Random;

//********************************************************************************************** */
//Paragraph
//********************************************************************************************** */

[System.Serializable]
public class Paragraph
{
	public string key = "";
	[TextAreaAttribute(3,10)]
	public List<string> sentences;
	public List<Triggers> triggers;

	//so we can quickly check to see if we need to filter this paragraph out
	public bool hasTrigger(Triggers trigger)
	{
		for (int i = 0; i < triggers.Count; i++) {
			if (trigger == triggers [i])
				return true;
		}
		return false;
	}

    //this is maybe a bit of a weird function
    public bool hasKeyWords(string keyWord, int countRequired)
    {
        int totalCount = 0;

        for (int i = 0; i < sentences.Count; i++)
        {
            int count = (sentences[i].Length - sentences[i].Replace(keyWord, "").Length) / keyWord.Length;
            totalCount += count;
        }

        if(totalCount >= countRequired)
        {
            return true;
        }

        return false;
    }
}

//********************************************************************************************** */
//Dialog Holder
//********************************************************************************************** */

public class DialogHolder : LoadingObject {

	private static DialogHolder sharedInstance = null;

	public static DialogHolder instance {
		get {
			if (sharedInstance == null) {
				sharedInstance = GameObject.FindObjectOfType<DialogHolder>();

				if (!sharedInstance) {
					Debug.LogError ("No DialogHolder object found, there needs to be one DialogHolder object in scene");
				}
			}
			return sharedInstance;
		}
	}

	public List<Paragraph> paragraphs;

	public List<Paragraph> paragraphsToReadFrom;

	public override void load(Action completion)
	{
		completion();
	}

	//just for instances where we are completely reloading the data
    public void clearData()
    {
        paragraphs.Clear();
    }

	public void clearReadableData()
    {
        paragraphsToReadFrom.Clear();
    }

	public void addParagraph(Paragraph newParagraph)
	{
		if (paragraphs == null) {
			paragraphs = new List<Paragraph> ();
		}

		paragraphs.Add (newParagraph);
	}

	public void addReadableParagraph(Paragraph newParagraph)
	{
		if (paragraphsToReadFrom == null) {
			paragraphsToReadFrom = new List<Paragraph> ();
		}

		paragraphsToReadFrom.Add (newParagraph);
	}

    public Paragraph grabRandomParagraph()
    {
		if(paragraphsToReadFrom.Count < 1)
		{
			//Charlie: if we're in demo mode we uh, might wanna reset the dialog
			// but we also don't wanna end up in an infinite loooooop
			if(GameController.instance.demoMode)
			{
				SaveController.instance.resetContent();
				RelayCentre.postMessage(Message.ReloadTriggers);

				if(paragraphsToReadFrom.Count < 1)
				{
					//look, I guess we're not finding one right now
					return null;
				}

				return findTheRandomParagraph();
			}
			
			return null;
		}

		return findTheRandomParagraph();
    }

	//this is mostly just for our trailer tech
	public Paragraph grabParagraphByKey(string key)
	{
		for(int i = 0; i < paragraphsToReadFrom.Count; i++)
		{
			if(key == paragraphsToReadFrom[i].key)
			{
				return paragraphsToReadFrom[i];
			}
		}

		return null;
	}

	public bool checkIfParagraphExists(string key)
	{
		if(grabParagraphByKey(key) != null)
		{
			return true;
		}
		
		return false;
	}

	Paragraph findTheRandomParagraph()
	{
		int index = Random.Range(0, paragraphsToReadFrom.Count);
		
		SaveController.instance.settings.addDialogKeyRead(paragraphsToReadFrom[index].key);
		RelayCentre.postMessage(Message.SaveGame);

		return paragraphsToReadFrom[index];
	}

	public void removeParagraph(Paragraph paragraph)
	{
		paragraphsToReadFrom.Remove(paragraph);
	}

	public int numberOfParagraphsContainingKey(string key)
	{
		int number = 0;

		for(int i = 0; i < paragraphs.Count; i++)
		{
			if(paragraphs[i].key.Contains(key))
			{
				number++;
			}
		}

		return number;
	}
}
