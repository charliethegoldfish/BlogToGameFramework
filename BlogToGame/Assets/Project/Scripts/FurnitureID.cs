using UnityEngine;
using System.Collections;

public class FurnitureID : MonoBehaviour {

	public FurnitureType furnitureType;

	
	// public GameObject[] theObjects;
	
	// #if UNITY_STANDALONE || UNITY_EDITOR
	// void OnMouseEnter()
	// {
	// 	highlight();
		
	// }

	// void OnMouseExit()
	// {
	// 	turnOffHighlight();
		
	// }

	public void highlight()
	{
		if(MenuController.instance.menu.activeSelf) return;

		// for(int i = 0; i < theObjects.Length; i++)
		// {
		// 	theObjects[i].layer = 9;
		// }
		
		CursorController.instance.changeToInterationCursor();
	}

	public void turnOffHighlight()
	{
		if(MenuController.instance.menu.activeSelf) return;

		// for(int i = 0; i < theObjects.Length; i++)
		// {
		// 	theObjects[i].layer = 8;
		// }

		CursorController.instance.changeToDefaultCursor();
	}
	// #endif
}
