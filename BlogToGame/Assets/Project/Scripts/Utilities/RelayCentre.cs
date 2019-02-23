using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Message
{
	None = 0,
	LoadDidCompleteNotification = 1,
	GameOverNotification = 2,
	GameWillBeginNotification = 3,
    GameLoaded = 5,
	DialogLoaded = 6,
	ReloadTriggers = 12,
	SaveGame = 13,
	LoadGame = 14,
	DialogStarted = 15,
	DialogFinished = 16,
	PauseGame = 17,
	MenuButtonHit = 18,
	SoundControllerLoaded = 19,
	CursorControllerLoaded = 20,
	TriggerFilterUpdated = 21,
	TimeOutCountdown = 22,
	TimeOutCancel = 23,
	ClickClick = 24,
	BackButtonHit = 28,
	NextTrailerPiece = 29,
	NewContentLoaded = 30,
	AnalyticsControllerLoaded = 31,
	ParagraphLimitReached = 32,
	FontBooterLoaded = 33,
	SettingsControllerLoaded = 34
}

public class MessageSubscriber
{
	public GameObject subscriber;
	public Message message;
	public Action simpleAction;
	public Action<object> action;
}

public class RelayCentre : MonoBehaviour {

	private static RelayCentre sharedInstance = null;

    public static RelayCentre instance
    {
        get
        {
            if (sharedInstance == null)
            {
                sharedInstance = GameObject.FindObjectOfType<RelayCentre>();

                if (!sharedInstance)
                {
                    Debug.LogError("No RelayCentre object found, there needs to be one RelayCentre object in scene");
                }
            }
            return sharedInstance;
        }
    }
	
	List<MessageSubscriber> messageSubscribers = new List<MessageSubscriber>();

	public static void addSubscriber(GameObject gObject, Message message, Action action)
	{
		if(instance.subscriberExists(gObject, message))
		{
			Debug.LogError("Objects can't subscribe to the same message multiple times");
			return;
		}
		
		MessageSubscriber mSubscriber = new MessageSubscriber();

		mSubscriber.subscriber = gObject;
		mSubscriber.simpleAction = action;
		mSubscriber.message = message;

		instance.messageSubscribers.Add(mSubscriber);
	}

	public static void addSubscriber(GameObject gObject, Message message, Action<object> action)
	{
		if(instance.subscriberExists(gObject, message))
		{
			Debug.LogError("Objects can't subscribe to the same message multiple times");
			return;
		}
		
		MessageSubscriber mSubscriber = new MessageSubscriber();

		mSubscriber.subscriber = gObject;
		mSubscriber.action = action;
		mSubscriber.message = message;

		instance.messageSubscribers.Add(mSubscriber);
	}

	public static void removeSubscriber(GameObject gObject, Message message)
	{
		for(int i = 0; i < instance.messageSubscribers.Count; i++)
		{
			if(instance.messageSubscribers[i].subscriber == gObject)
			{
				if(message == instance.messageSubscribers[i].message)
				{
					instance.messageSubscribers.Remove(instance.messageSubscribers[i]);
					break;
				}
			}
		}
	}

	bool subscriberExists(GameObject gObject, Message message)
	{
		for(int i = 0; i < instance.messageSubscribers.Count; i++)
		{
			if(instance.messageSubscribers[i].subscriber == gObject)
			{
				if(message == instance.messageSubscribers[i].message)
				{
					return true;
				}
			}
		}

		return false;
	}

	public static void postMessage(Message message)
	{
		for(int i = 0; i < instance.messageSubscribers.Count; i++)
		{
			if(message == instance.messageSubscribers[i].message)
			{
				instance.messageSubscribers[i].simpleAction();
			}
		}
	}

	public static void postMessage(Message message, object messageObject)
	{
		for(int i = 0; i < instance.messageSubscribers.Count; i++)
		{
			if(message == instance.messageSubscribers[i].message)
			{
				instance.messageSubscribers[i].action(messageObject);
			}
		}
	}


	
}
