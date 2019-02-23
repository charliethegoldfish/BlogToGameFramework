using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : LoadingObject {

	public float controllerSpeed = 2f;

	[Header("Walking Settings")]
	public string walkingAudioKey;
	[Range(0.1f, 1f)]
	public float stepMaxDelay = 0.1f;

	public AudioSource audioSource;

	private NavMeshAgent agent;
	private Camera mainCam;

	private bool takingInput;
	private bool loaded;

	private FurnitureID furnitureId;
	private Vector3 cursorPosition;

	public override void load(Action completion)
	{
		agent = this.gameObject.GetComponent<NavMeshAgent> ();
		mainCam = Camera.main;

		//gosh does this look terrible
		cursorPosition = CursorController.instance.cursor.transform.position;

		loadObservers();

		loaded = true;

		completion();
	}

	void Update()
	{
		if(loaded)
		{
			cursorInput();
		}
		
		if (takingInput) {
			
			checkInput ();

			checkHighlighting();

			//Charlie: replace this with a specific key check I guess
			if(Input.GetKeyUp(KeyCode.Escape))
			{
				RelayCentre.postMessage(Message.MenuButtonHit);

				RelayCentre.postMessage(Message.TimeOutCancel);
			}
		}

		// if(loaded)
		// {
		// 	cursorInput();
		// }
		
		CursorController.instance.updateCursorPos(cursorPosition);

		
	}

	void checkInput()
	{
		// Charlie: replace this with uh, mouse click I think
		if(Input.GetMouseButtonUp(0))
		{
			RelayCentre.postMessage(Message.TimeOutCancel);

			furnitureId = null;
			StopCoroutine ("checkWalking");

			RaycastHit hit;
			Ray ray = mainCam.ScreenPointToRay (cursorPosition);

			if (Physics.Raycast (ray, out hit)) 
			{
				if(hit.collider == null)
				{
					return;
				} else if (hit.transform.tag == "Floor" || hit.transform.tag == "Furniture")
				{
					agent.SetDestination (hit.point);

					if (hit.transform.tag == "Furniture") {
					furnitureId = hit.transform.GetComponent<FurnitureID> ();
					}

					StartCoroutine ("checkWalking");
				}
			}
		}	
	}

	void checkHighlighting()
	{
		RaycastHit hit;
		Ray ray = mainCam.ScreenPointToRay (cursorPosition);

		FurnitureHandler.instance.turnOffAllHighlighting();
		
		if (Physics.Raycast (ray, out hit)) 
		{
			if(hit.collider == null)
			{
				return;
			} else if (hit.transform.tag == "Floor" || hit.transform.tag == "Furniture")
			{
				if (hit.transform.tag == "Furniture") {
				hit.transform.GetComponent<FurnitureID> ().highlight();
				}
			}
		}
	}

	// Charlie: re-writing this will no longer work for devices, 
	// so maybe consider re-writing it to do that sometime
	void cursorInput()
	{
		// if(GameController.instance.isDevice) return;
		
		cursorPosition = Input.mousePosition;
	}

	IEnumerator checkWalking()
	{
		//first we need to wait to move
		while (agent.velocity == Vector3.zero) {
			yield return null;
		}

		startWalkingSounds();

		//now we need to wait till we have stopped
		while (agent.velocity != Vector3.zero) {
			RelayCentre.postMessage(Message.TimeOutCancel);
			yield return null;
		}

		RelayCentre.postMessage(Message.TimeOutCancel);

		stopWalkingSounds();

		if(furnitureId != null)
		{
			//now we can interact
			interactWithFurniture (furnitureId);
			furnitureId = null;
		}
	}

	void interactWithFurniture(FurnitureID furniturePiece)
	{
		FurnitureType furnitureType;

		if (furniturePiece == null) {
			furnitureType = FurnitureType.generic;
		} else {
			furnitureType = furniturePiece.furnitureType;
		}

		DialogHandler.instance.startDialog (furnitureType);
	}

	void setInput(bool input)
	{
		takingInput = input;
	}

	void loadObservers()
	{
		RelayCentre.addSubscriber(gameObject, Message.GameWillBeginNotification, ()=> {
			setInput(true);
		});

		RelayCentre.addSubscriber(gameObject, Message.DialogStarted, ()=> {
			setInput(false);
		});

		RelayCentre.addSubscriber(gameObject, Message.DialogFinished, ()=> {
			setInput(true);
		});

		RelayCentre.addSubscriber(gameObject, Message.PauseGame, ()=> {
			setInput(false);
		});
	}

	//********************************************************************************************** */
	//Walking Sounds
	//********************************************************************************************** */
	IEnumerator walkingSounds()
	{
		float stepDelay = Random.Range(0.1f, stepMaxDelay);

		yield return new WaitForSeconds(stepDelay);

		playFootstep();

		while(audioSource.isPlaying)
		{
			yield return null;
		}

		if(!MenuController.instance.menu.activeSelf)
		{
			StartCoroutine("walkingSounds");
		}

		yield return null;
	}

	void startWalkingSounds()
	{
		StopCoroutine("walkingSounds");
		StartCoroutine("walkingSounds");
	}

	void stopWalkingSounds()
	{
		StopCoroutine("walkingSounds");
	}
	void playFootstep()
	{
		if(!SettingsController.instance.playSoundEffects()) return;
		AudioClip footstep = SoundController.instance.getAudioClip(walkingAudioKey);
		audioSource.PlayOneShot(footstep);
	}
}
