using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogHandler : MonoBehaviour {

	private static DialogHandler sharedInstance = null;

	public static DialogHandler instance {
		get {
			if (sharedInstance == null) {
				sharedInstance = GameObject.FindObjectOfType<DialogHandler>();

				if (!sharedInstance) {
					Debug.LogError ("No DialogHandler object found, there needs to be one DialogHandler object in scene");
				}
			}
			return sharedInstance;
		}
	}

	public void startDialog(FurnitureType furnitureType)
	{
		//string dialogText = "hey dialog here";
		DialogBoxController dialog;

		string dialogBoxToUse = SettingsController.instance.getDialogBoxToUse();

		MenuScreen dialogScreen = MenuController.instance.getDialogScreen(dialogBoxToUse);
		dialog = dialogScreen.screenObject.GetComponent<DialogBoxController>();

		if(dialog != null)
		{
			Paragraph paragraph = DialogHolder.instance.grabRandomParagraph();

			if(paragraph == null)
			{
				dialog.noMoreDialog();
				return;
			}

			dialog.startDialog(paragraph);

			DialogHolder.instance.removeParagraph(paragraph);

			SaveController.instance.settings.incrementInt("NumberOfParagraphsRead");
		}
	}
}
