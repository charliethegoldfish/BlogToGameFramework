#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextLoader))]
public class TextLoaderEditor : Editor {
	public override void OnInspectorGUI ()
	{
		TextLoader loader = target as TextLoader;
	    if (GUILayout.Button ("Load Posts")) {

            if(!loader.loading)
            {
                if(loader.typeOfBlogToLoad == BlogType.Squarespace)
                {
                    loader.loadPages();
                } else if(loader.typeOfBlogToLoad == BlogType.Tumblr)
                {
                    loader.loadTumblrPages();
                }
                
            } else
            {
                Debug.LogError("Already loading");
            }
		}

        if (GUILayout.Button("Stop Loading"))
        {
            loader.stopLoadingPages();

             EditorUtility.ClearProgressBar();
             loader.loading = false;
             loader.writing = false;
        }

        if (GUILayout.Button ("Write To File")) {

            if(!loader.writing)
            {
                loader.writeToFile();
            } else
            {
                Debug.LogError("Already writing");
            }
		}

        if (GUILayout.Button ("Load From File")) {

            if(!loader.loading)
            {
                loader.readFromFile();
            } else
            {
                Debug.LogError("Already loading");
            } 
		}

            DrawDefaultInspector ();

		EditorUtility.SetDirty(target);
	} 
}
#endif