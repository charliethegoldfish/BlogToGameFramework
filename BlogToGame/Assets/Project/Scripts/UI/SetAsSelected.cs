using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SetAsSelected : MonoBehaviour {

	public void OnEnable()
	{
		EventSystem.current.SetSelectedGameObject(this.gameObject);
		Debug.Log("Hello, I am setting this I guess");
	}

	// void SetSelected(GameObject selectableObject) {
    // 	// Set the currently selected GameObject
    // 	EventSystem.current.SetSelectedGameObject(selectableObject);
	// }


}
