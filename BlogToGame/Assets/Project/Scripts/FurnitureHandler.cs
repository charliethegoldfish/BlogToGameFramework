using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum FurnitureType
{
	generic = 0,
	chair = 10,
	tv = 15,
	bench = 20,
	bookcase = 25
}

public class FurnitureHandler : MonoBehaviour {

	private static FurnitureHandler sharedInstance = null;

	public static FurnitureHandler instance {
		get {
			if (sharedInstance == null) {
				sharedInstance = GameObject.FindObjectOfType<FurnitureHandler>();

				if (!sharedInstance) {
					Debug.LogError ("No FurnitureHandler object found, there needs to be one FurnitureHandler object in scene");
				}
			}
			return sharedInstance;
		}
	}

	public List<FurnitureID> furniture;

	public void turnOffAllHighlighting()
	{
		for(int i = 0; i < furniture.Count; i++)
		{
			furniture[i].turnOffHighlight();
		}
	}

}
