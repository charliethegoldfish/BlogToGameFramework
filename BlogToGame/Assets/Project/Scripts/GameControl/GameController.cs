using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class LoadObjects
{
	public string key;
	public LoadingObject loadingObject;

	[HideInInspector]
	public bool completed = false;
	
	[HideInInspector]
	public bool loaded = false;
}

public class GameController : MonoBehaviour {

	private static GameController sharedInstance = null;

    public static GameController instance
    {
        get
        {
            if (sharedInstance == null)
            {
                sharedInstance = GameObject.FindObjectOfType<GameController>();

                if (!sharedInstance)
                {
                    Debug.LogError("No GameController object found, there needs to be one GameController object in scene");
                }
            }
            return sharedInstance;
        }
    }

	public LoadObjects[] loadingObjects;
	List<SegmentToLoad> segmentsToLoad;

	[Header("General In Game Things")]
	public bool gameHasLoaded;

	[Header("To Set Build For Demo Purposes")]
	public bool demoMode;
	public float secondsTillReset = 600f;
	[HideInInspector]
	public bool gamePlaying = false;

	float timer;

	// Use this for initialization
	void Start () {

		//let's get loadin'
		StartCoroutine("loadingProcess");
	}

	void switchMenu()
	{
		if(gamePlaying)
		{
			gamePlaying = false;
			RelayCentre.postMessage(Message.PauseGame);
		}
		//TODO: maybe also add to switch from menu back into game
	}

	void loadNext()
	{
		for(int i = 0; i < loadingObjects.Length; i++)
		{
			if(loadingObjects[i].completed)
			{
				// Debug.Log("already completed " + loadingObjects[i].key);
				continue;
			} 
			
			int index = i;
			loadingObjects[i].loadingObject.load(()=> {
				loadingObjects[index].completed = true;
				LoadingBarController.instance.segmentLoaded(grabSegmentFromKey(loadingObjects[index].key));
				// Debug.Log("we've completed " + loadingObjects[index].key);
				loadNext();
			});
			// Debug.Log("And breaking");
			return; //this should break the function until we have completed everything
		}

		// Debug.Log("we've completed everything");

		gameHasLoaded = true;

		RelayCentre.postMessage(Message.LoadDidCompleteNotification);
		RelayCentre.postMessage(Message.GameLoaded);

		Debug.Log ("Booting: We have finished loading");
	}

	IEnumerator loadingProcess()
	{
		Debug.Log ("Booting: We have started loading");

		Cursor.visible = false;

		addObservers();

		segmentsToLoad = new List<SegmentToLoad>();

		for(int a = 0; a < loadingObjects.Length; a++)
		{
			SegmentToLoad segment = new SegmentToLoad();
			segment.objectLoading = loadingObjects[a].loadingObject.gameObject;
			segment.name = loadingObjects[a].key;
			segment.index = a;

			segmentsToLoad.Add(segment);
		}

		LoadingBarController.instance.initLoading(segmentsToLoad);
		
		loadNext();

		yield return null;
	}

	void Update()
	{
		// Debug.Log("Tiiiiime is " + timer);

		if(!gamePlaying) return;
		
		timer += Time.deltaTime;

		if(timer >= secondsTillReset)
		{
			SaveController.instance.resetContent();
			MenuController.instance.loadMainMenu();
		}
	}

	void addObservers()
	{
		RelayCentre.addSubscriber(gameObject, Message.GameWillBeginNotification, ()=> {
			gamePlaying = true;
		});

		RelayCentre.addSubscriber(gameObject, Message.MenuButtonHit, switchMenu);
		RelayCentre.addSubscriber(gameObject, Message.TimeOutCancel, ()=>
		{
			timer = 0;
		});
	}

	SegmentToLoad grabSegmentFromKey(string key)
	{
		for(int i = 0; i < segmentsToLoad.Count; i++)
		{
			if(key == segmentsToLoad[i].name) return segmentsToLoad[i];
		}

		return null;
	}
}
