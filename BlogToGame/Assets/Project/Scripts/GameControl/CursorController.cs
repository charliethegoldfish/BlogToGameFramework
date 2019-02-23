using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CursorController : LoadingObject {

	public GameObject cursor;

	[Header("Cursor Types")]
	public GameObject defaultCursor;
	public GameObject interactionCursor;

	

	private static CursorController sharedInstance = null;

	public static CursorController instance {
		get {
			if (sharedInstance == null) {
				sharedInstance = GameObject.FindObjectOfType<CursorController>();

				if (!sharedInstance) {
					Debug.LogError ("No CursorController object found, there needs to be one CursorController object in scene");
				}
			}
			return sharedInstance;
		}
	}
	public override void load(Action completion)
	{
		changeToDefaultCursor();

		RelayCentre.addSubscriber(gameObject, Message.LoadDidCompleteNotification, ()=>
		{
			setCursorActiveState(true);
		});
		completion();
	}

	public void setCursorActiveState(bool activeState)
	{
		cursor.SetActive(activeState);
	}

	public void updateCursorPos(Vector2 newPos)
	{
		transform.position = newPos;
	}

	public void changeToDefaultCursor()
	{
		interactionCursor.SetActive(false);
		defaultCursor.SetActive(true);
	}

	public void changeToInterationCursor()
	{
		defaultCursor.SetActive(false);
		interactionCursor.SetActive(true);
	}

}
