using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum BlogType
{
    Squarespace = 0,
    Tumblr = 1
}

[System.Serializable]
public class FilterableTriggers
{
    [Tooltip("Enter anything to search by to find the trigger")]
    public string key;
    public List<string> searchTerms;
    public Triggers trigger;
}

//Charlie: there is probs a bunch of ways this could be better and less, hard coded??
[ExecuteInEditMode]
public class TextLoader : LoadingObject {

    public BlogType typeOfBlogToLoad;

    [TextAreaAttribute(3,10)]
	public List<string> urlsToLoad;
	public string mainUrl;

    public List<FilterableTriggers> triggersToFilter;

	[HideInInspector] public bool loading = false;
	[HideInInspector] public bool writing = false;

    // private bool gameLoaded = false;

	public override void load(Action completion)
	{
		SettingsController.instance.setTriggerFilters();
        StartCoroutine(loadReadableDialog(completion));
        RelayCentre.addSubscriber(gameObject, Message.ReloadTriggers, reloadAndFilterTriggers);

        completion();
	}

	public void loadPages()
	{
		StartCoroutine (loadWebpage(urlsToLoad));
	}

    public void loadTumblrPages()
    {
        StartCoroutine (loadTumblrWebpage(urlsToLoad));
    }

    public void stopLoadingPages()
    {
        StopCoroutine(loadWebpage(urlsToLoad));
        StopCoroutine(loadTumblrWebpage(urlsToLoad));
        StopCoroutine(writeJsonToFile());
    }

    public void writeToFile()
    {
        StartCoroutine (writeJsonToFile());
    }

    public void readFromFile()
    {
        StartCoroutine (readJsonFromFile());
    }

	//********************************************************************************************** */
	//Load From Webpage 
	//********************************************************************************************** */
    public IEnumerator loadWebpage(List<string> theUrls)
    {
        Debug.Log("Data Loading: Started");
        loading = true;

        DialogHolder.instance.clearData();   

        for (int i = 0; i < theUrls.Count; i++)
        {

			#if UNITY_EDITOR
            float progress = (float)i / (float)theUrls.Count;
            string urlKey = grabKeyFromURL(theUrls[i], mainUrl);
            string loadingText = "Loading url " + urlKey + " (" + i + " / " + theUrls.Count + ")...";
            EditorUtility.DisplayProgressBar("Loading Blog Data", loadingText, progress);
			#endif

            WWW www = new WWW(theUrls[i]);
            yield return www;

            string textFromSite = "";

            if(string.IsNullOrEmpty(www.error))
            {
                textFromSite = System.Text.Encoding.UTF8.GetString(www.bytes, 3, www.bytes.Length - 3);
            }

            string[] textLines = textFromSite.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

            grabPost(textLines, theUrls[i]);
        }
        loading = false;
        Debug.Log("Data Loading: Finished");

		#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
		#endif
		
        StartCoroutine(writeJsonToFile());  
    }

    public IEnumerator loadTumblrWebpage(List<string> theUrls)
    {
         Debug.Log("Data Loading: Started");
        loading = true;

        DialogHolder.instance.clearData();   

        for (int i = 0; i < theUrls.Count; i++)
        {

			#if UNITY_EDITOR
            float progress = (float)i / (float)theUrls.Count;
            string urlKey = grabKeyFromURL(theUrls[i], mainUrl);
            string loadingText = "Loading url " + urlKey + " (" + i + " / " + theUrls.Count + ")...";
            EditorUtility.DisplayProgressBar("Loading Blog Data", loadingText, progress);
			#endif

            WWW www = new WWW(theUrls[i]);
            yield return www;

            string textFromSite = "";

            if(string.IsNullOrEmpty(www.error))
            {
                textFromSite = System.Text.Encoding.UTF8.GetString(www.bytes, 3, www.bytes.Length - 3);
            }

            string[] textLines = textFromSite.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

            grabTumblrPost(textLines, theUrls[i]);
        }
        loading = false;
        Debug.Log("Data Loading: Finished");

		#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
		#endif
		
		#if UNITY_EDITOR
        StartCoroutine(writeJsonToFile());
		#endif   
    }

    //********************************************************************************************** */
	//Grab the Post 
	//********************************************************************************************** */
    //Charlie: let's write a neeeeeew function to read the web stuff
    void grabPost(string[] textLines, string url, bool addToNewParagraphs = false)
    {
        string title = "";

        List<Triggers> triggers = new List<Triggers>();

        for (int i = 0; i < textLines.Length; i++)
        {
            if (textLines[i].Contains("<title>"))
            {
                title = formatTitle(textLines[i]);
            }

            string text = "";

            //Charlie: we want either the next line, or the one and a few after it?
            //I could write something that removes everything in between < >'s
            //including the < >'s too
            if (textLines[i].Contains("<div class=\"body entry-content\">"))
            {
                for(int lineIndex = i + 1; lineIndex < textLines.Length; lineIndex++)
                {
                    string check = textLines[lineIndex].Trim();
                    if(check == "</div>") break;
                    
                    List<String> bitsToRemove = partsToRemove(textLines[lineIndex], '<', '>');
                    text = removePartsOfString(textLines[lineIndex], bitsToRemove);
                    text = text.Trim();

                    if(text.Length < 3) continue;

                    string testText = text.ToLower();
                    if (testText.Contains("trigger warning"))
                    {
                        triggers = grabTriggerWarnings(testText);
                        //continue;
                    }

                    string[] textParagraphs = splitTextIntoParagraphs(text);

                    if(textParagraphs.Length < 1) continue;

                    if(textParagraphs.Length < 2 && textParagraphs[0].ToLower().Contains("trigger"))
                        continue;     
                    
                    formatText(textParagraphs, title, triggers, addToNewParagraphs);
                    
                }   
            }
        }

        if (title == "")
        {
            title = grabKeyFromURL(url, mainUrl);
        }

    }

    //********************************************************************************************** */
	//Grab the Post - from Tumblr
	//********************************************************************************************** */
    void grabTumblrPost(string[] textLines, string url, bool addToNewParagraphs = false)
    {
        string title = "";

        List<Triggers> triggers = new List<Triggers>();

        for (int i = 0; i < textLines.Length; i++)
        {
            string text = "";
            
            if (textLines[i].Contains("<h2 class=\"title\">"))
            {
                //Charlie: make this work for the tumblr format
                title = formatTumblrTitle(textLines[i]);
                
                //This is the line we WANT
                
            //Charlie: we are assuming the stuff we want is always in the same line here
            //WOOOOO WHAT COULD GO WRONG
            } else if (textLines[i].Contains("<div class=\"body-text\">"))
            {
                // Debug.Log("[Text Loader] We have decided upon this line: " + textLines[i]);
                
                List<String> bitsToRemove = partsToRemove(textLines[i], '<', '>');
                text = removePartsOfString(textLines[i], bitsToRemove);
                text = text.Trim();

                // Debug.Log("[Text Loader] this line now looks like this " + text);

                //Charlie: not entirely sure how this will work in this section
                string testText = text.ToLower();
                if (testText.Contains("trigger warning"))
                {
                    triggers = grabTriggerWarnings(testText);
                }

                // Debug.Log("[Text Loader] We have this many triggers: " + triggers.Count);

                string[] textParagraphs = splitTextIntoParagraphs(text);

                // Debug.Log("[Text Loader] We have this many paragraphs: " + textParagraphs.Length);

                //Charlie: should this, go here????
                if (title == "")
                {
                    title = grabKeyFromURL(url, mainUrl);
                }

                //Charlie: may need a tumblr sepcific one???
                formatText(textParagraphs, title, triggers, addToNewParagraphs);

                //Charlie: revist this perhaps, but otherwise we grab other posts too
                break;
            }
        }
    }

    //********************************************************************************************** */
	//Get a list of parts within 2 characters
	//********************************************************************************************** */
    List<string> partsToRemove(string text, char startChar, char endChar)
    {
        List<string> bits = new List<string>();
        
        for(int a = 0; a < text.Length; a++)
        {
            //Uh I guess we loop through till we find the opening <
            //this is SOO BAAD AAAH (lol)
            if(text[a] == startChar)
            {
                 bool foundNextChar = false;
                for(int b = a; b < text.Length; b++)
                {
                    if(text[b] == endChar && !foundNextChar)
                    {
                        //Charlie: let's store them for later so we can like, remove 'em later??
                        int startIndex = a;
                        int endIndex = b;
                        int length = endIndex - startIndex + 1;
                        string bit = text.Substring(startIndex, length);

                        bits.Add(bit);
                        foundNextChar = true;
                    }
                }
            }
        }

        return bits;
    }
    
    //********************************************************************************************** */
	// Remove certain parts of string
	//********************************************************************************************** */
    string removePartsOfString(string text, List<string> removableBits, string replacement = "")
    {
        for(int i = 0; i < removableBits.Count; i++)
        {
            if(removableBits[i] == "<br>" || removableBits[i] == "</p>" ||
            removableBits[i] == "<p>" || removableBits[i] == "<br/>")
            {
                continue;
            }
            text = text.Replace(removableBits[i], replacement);
        }

        return text;
    }

    //********************************************************************************************** */
	// Find trigger warnings
	//********************************************************************************************** */

    List<Triggers> grabTriggerWarnings(string text)
    {
        List<Triggers> triggers = new List<Triggers>();
        string lowerText = text.ToLower();

        for(int i = 0; i < triggersToFilter.Count; i++)
        {
            FilterableTriggers filterableTrigger = triggersToFilter[i];
            bool addFilter = false;

            for(int t = 0; t < filterableTrigger.searchTerms.Count; t++)
            {
                if(lowerText.Contains(filterableTrigger.searchTerms[t]))
                {
                    if(checkIfSingleWord(lowerText, filterableTrigger.searchTerms[t]))
                    {
                        addFilter = true;
                    } 
                }
            }

            if(addFilter)
            {
                triggers.Add(filterableTrigger.trigger);
            }
        }

        return triggers;
    }

    //we're just going to use this to make sure it's not part of another word
    bool checkIfSingleWord(string text, string wordsLookingFor)
    {
        int charIndexToCheckBefore = text.IndexOf(wordsLookingFor, StringComparison.Ordinal);

        if(charIndexToCheckBefore > 0)
        {
            int charIndexToCheck = charIndexToCheckBefore - 1;
            char charToCheck = text[charIndexToCheck];

            if(!char.IsLetter(charToCheck)) return true;

        } else
        {
            return true;
        }
        
        return false;
    }

    //********************************************************************************************** */
	// Format the text
	//********************************************************************************************** */
    string[] splitTextIntoParagraphs(string text)
    {
        string[] textPieces = text.Split(new string[] { "<br>", "</p>", "<p>", "<br/>" }, System.StringSplitOptions.None);
        return textPieces;
    }

    //TODO maybe make this a co-routine?? If it seems to be hanging while playing?
    //but then we are calling like a billion co-routines? Is that bad? Maybe? Probably? *shrug*
    void formatText(string[] textPieces, string title, List<Triggers> triggers, bool addToNewParagraphs = false)
    {
        List<Paragraph> extraParagraphs = new List<Paragraph>();
        
        for (int i = 0; i < textPieces.Length; i++)
        {
            if (textPieces[i].Length <= 0)
                continue;

            if(textPieces[i].ToLower().Contains("trigger warning"))
                continue;

            Paragraph tempPara = new Paragraph();

            int keyNumber = DialogHolder.instance.numberOfParagraphsContainingKey(title) + 1;

            tempPara.key = title + "_" + keyNumber;
            tempPara.sentences = splitIntoSentences(textPieces[i]);
            tempPara.triggers = triggers;
            if (tempPara.sentences.Count <= 0)
                return;

            if(!addToNewParagraphs)
            {
                DialogHolder.instance.addParagraph(tempPara);

            } else
            {
               SaveController.instance.settings.newParagraphs.Add(tempPara);
               extraParagraphs.Add(tempPara);
            }
            
        }
        
        if(extraParagraphs.Count > 0)
        {
            StartCoroutine(addParagraphsAsReadable(extraParagraphs));
        }
    }

    //********************************************************************************************** */
	// Split into sentences
	//********************************************************************************************** */
    List<string> splitIntoSentences(string text)
    {
        text = fixPunctuation(text);

        //here we split the string, but keep the punctuation at the end (to the left of split)
        string [] textPieces = Regex.Split(text, @"(?<=[.!?])");
        List<string> temp = new List<string>();

        for (int i = 0; i < textPieces.Length; i++)
        {
            textPieces[i] = textPieces[i].Trim();

            if (textPieces[i].Length <= 0)
                continue;

            if (textPieces[i].Length <= 3)
            {
                if(textPieces[i].StartsWith(".") || textPieces[i].StartsWith("!")
                || textPieces[i].StartsWith("?"))
                {
                    continue;
                }    
            }

            if (textPieces[i].Contains("<a href"))
                continue;

            temp.Add(textPieces[i]);
        }

        return temp;
    }

    //Charlie: we could probably write this to use the format tumblr functionality??
    string formatTitle(string text)
    {
        string title = "";
        text = text.Replace("<title>", "").Replace("—", "").Replace("Charlie Francis Cassidy", "").Replace("</title>", "");

        title = fixPunctuation(text);
        title = title.TrimEnd();

        return title;
    }
//                                             <h2 class="title"><a href="https://blogtogame.tumblr.com/post/179812251635/intro">Intro</a></h2>
    string formatTumblrTitle(string text)
    {
        string title = "";
        
        List<String> bitsToRemove = partsToRemove(text, '<', '>');
        title = removePartsOfString(text, bitsToRemove);
        title = title.Trim();

        title = fixPunctuation(title);
        title = title.TrimEnd();

        return title;
    }

    string fixPunctuation(string text)
    {
        string newText = text.Replace("&rsquo;", "'").Replace("&nbsp;", " ").Replace("&ldquo;", "\"");
        newText = newText.Replace("&rdquo;", "\"").Replace("<span>", "").Replace("</span>", "");
        newText = newText.Replace("&hellip;", "...");

        return newText;
    }

    string grabKeyFromURL(string url, string mainUrl)
    {
        string key = "";
        key = url.Replace(mainUrl, "");
        return key;
    }

	//********************************************************************************************** */
	//Write to File
	//********************************************************************************************** */
	public IEnumerator writeJsonToFile()
	{
		Debug.Log("Data Writing: Started");
		writing = true;
		
		List<Paragraph> paragraphs = DialogHolder.instance.paragraphs;

		using(StreamWriter jFile = new StreamWriter("Assets/Resources/DialogText.json"))
		{
			for(int i = 0; i < paragraphs.Count; i++)
			{
				#if UNITY_EDITOR
				//Progress bar
				float progress = (float)i / (float)paragraphs.Count;
				string loadingText = "Writing paragraph " + paragraphs[i].key + " (" + i + " / " + paragraphs.Count + ")...";
				EditorUtility.DisplayProgressBar("Writing Blog Data", loadingText, progress);
				#endif

				//Actual writing code
				string line = JsonUtility.ToJson(paragraphs[i]);
				jFile.WriteLine(line);
				//Debug.Log("Json line written - " + line);

				yield return new WaitForSeconds(0.1f);
			}
		}
		writing = false;
        Debug.Log("Data Writing: Finished");

		#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
		#endif
	}

	//********************************************************************************************** */
	//Load From File
	//********************************************************************************************** */
	public IEnumerator readJsonFromFile()
	{
		Debug.Log("Data Reading: Started");
		loading = true;

		using(StreamReader jFile = new StreamReader("Assets/Resources/DialogText.json"))
		{
			string contents = jFile.ReadToEnd();

			string[] lines = contents.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
			
			for(int i = 0; i < lines.Length; i++)
			{
				#if UNITY_EDITOR
				//Progress bar
				float progress = (float)i / (float)lines.Length;
				string loadingText = "Reading file " + " (" + i + " / " + lines.Length + ")...";
				EditorUtility.DisplayProgressBar("Reading Blog Data", loadingText, progress);
				#endif
				
				if(string.IsNullOrEmpty(lines[i]))
				{
					continue;
				}

				//Actual reading code
				Paragraph paragraph = JsonUtility.FromJson<Paragraph>(lines[i]);
				DialogHolder.instance.addParagraph(paragraph);

				yield return new WaitForSeconds(0.1f);
			}
		}
		loading = false;
        Debug.Log("Data Reading: Finished");

		#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
		#endif
	}

	private IEnumerator loadDialog()
	{
		DialogHolder.instance.clearData();
		Debug.Log("Data Reading: Started");
		loading = true;

        TextAsset dialogText = Resources.Load("DialogText") as TextAsset;

        string contents = dialogText.text;

        string[] lines = contents.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        
        List<Paragraph> paragraphs = new List<Paragraph>();
        List<Paragraph> extraParagraphs = SaveController.instance.settings.newParagraphs;
        
        for(int i = 0; i < lines.Length; i++)
        {
            if(string.IsNullOrEmpty(lines[i]))
            {
                continue;
            }

            //Actual reading code
            Paragraph paragraph = JsonUtility.FromJson<Paragraph>(lines[i]);
            paragraphs.Add(paragraph);
        }

        for(int i = 0; i < extraParagraphs.Count; i++)
        {
            paragraphs.Add(extraParagraphs[i]);
        }

        StartCoroutine(addParagraphsToDialog(paragraphs));

        yield return null;

            // LoadingBarController.instance.updateLoading(lines.Length + extraParagraphs.Count, i);

        // LoadingBarController.instance.updateLoading(lines.Length + extraParagraphs.Count, lines.Length + i);	
	}

    private IEnumerator loadReadableDialog(Action completion = null)
    {
        DialogHolder.instance.clearReadableData();

        List<Paragraph> paragraphs = DialogHolder.instance.paragraphs;
        List<Paragraph> extraParagraphs = SaveController.instance.settings.newParagraphs;

        for(int i = 0; i < extraParagraphs.Count; i++)
        {
            paragraphs.Add(extraParagraphs[i]);
        }

        //do something here to work out if we add them to readable paragraphs
        StartCoroutine(addParagraphsAsReadable(paragraphs, completion));

        yield return null;
    }

    IEnumerator addParagraphsToDialog(List<Paragraph> paragraphs)
    {
        for(int i = 0; i < paragraphs.Count; i++)
        {

            DialogHolder.instance.addParagraph(paragraphs[i]); 

            yield return null;
        }

        loading = false;
        Debug.Log("Data Reading: Finished");
    }

    IEnumerator addParagraphsAsReadable(List<Paragraph> paragraphs, Action completion = null)
    {
        for(int i = 0; i < paragraphs.Count; i++)
        {
            //we should filter out triggers
            List<Triggers> filterOut = SettingsController.instance.getTriggersToFilter();
            bool remove = false;
            for(int t = 0; t < filterOut.Count; t++)
            {

                 Debug.Log("Hello a trigger here is " + filterOut[t]);
                if(paragraphs[i].hasTrigger(filterOut[t]))
                {
                    remove = true;
                   
                    break;
                }
                //Charlie: okay like, maybe have these once the game has fucking loaded aye
                // if(gameLoaded) yield return null;
            }

            if(!remove)
            {
                List<string> keysUsed = SaveController.instance.settings.dialogKeysRead;

                for(int a = 0; a < keysUsed.Count; a++)
                {
                    if(keysUsed[a] == paragraphs[i].key) remove = true;

                    // if(gameLoaded) yield return null;
                }

                if(!remove) DialogHolder.instance.addReadableParagraph(paragraphs[i]);
            }

            LoadingBarController.instance.updateLoading(paragraphs.Count, i);

            // yield return null;
        }
        
        yield return null;

        loading = false;
        Debug.Log("Data Reading: Finished");

        RelayCentre.postMessage(Message.DialogLoaded);

        if(completion != null)
        {
            completion();
        }
        
    }


	//********************************************************************************************** */
	//Reload Stuff
	//********************************************************************************************** */
	public void reloadAndFilterTriggers()
	{
		DialogHolder.instance.clearReadableData();
		// StartCoroutine(loadDialog());
        StartCoroutine(loadReadableDialog());
	}
}
