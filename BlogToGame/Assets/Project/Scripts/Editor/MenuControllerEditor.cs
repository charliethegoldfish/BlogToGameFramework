#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MenuController))]
public class MenuControllerEditor : Editor {

	public override void OnInspectorGUI ()
	{
		MenuController mC = target as MenuController;

		if(GUILayout.Button("Find Text Objects"))
		{
			grabTextObjects(mC);
			Debug.Log("we've hit the fucking button");
		}

		for(int i = 0; i < mC.availableFonts.Length; i++)
		{
			if(GUILayout.Button("Change font to " + mC.availableFonts[i].key))
			{
				mC.menu.gameObject.SetActive(false);
				mC.changeFont(mC.availableFonts[i].key);
				mC.menu.gameObject.SetActive(true);
			}
		}

		DrawDefaultInspector();
	}

	public void grabTextObjects(MenuController menu)
    {
        menu.textObjects.Clear();

		// Debug.Log("we're in the fucking function");

		for(int i = 0; i < menu.menuScreens.Length; i++)
		{
			menu.loadMenuScreen(menu.menuScreens[i].screenName);
			TextSettings[] objects = GameObject.FindObjectsOfType(typeof(TextSettings)) as TextSettings[];

			for(int a = 0; a < objects.Length; a++)
			{
				//objects[a].grabTextComponent();
				menu.textObjects.Add(objects[a]);
			}
		}

		//menu.loadMainMenu();

		for(int i = 0; i < menu.dialogScreens.Length; i++)
		{
			menu.loadDialogScreen(menu.dialogScreens[i].screenName);

			TextSettings[] objects = GameObject.FindObjectsOfType(typeof(TextSettings)) as TextSettings[];

			for(int a = 0; a < objects.Length; a++)
			{
				//objects[a].grabTextComponent();
				menu.textObjects.Add(objects[a]);
			}

			menu.hideDialogScreen(menu.dialogScreens[i].screenName);
		}
    }
	
}
#endif