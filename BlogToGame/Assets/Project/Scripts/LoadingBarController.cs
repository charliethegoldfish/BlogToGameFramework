using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SegmentToLoad {

    public string name = ""; //Charlie: maybe rename this sometime
    public GameObject objectLoading;
    public Action completion; //do we actually need this?
    public int index;
    
   // int innerSegmentsToLoad = 0;
}

public class LoadingBarController : MonoBehaviour {


	private static LoadingBarController sharedInstance = null;

    public static LoadingBarController instance
    {
        get
        {
            if (sharedInstance == null)
            {
                sharedInstance = GameObject.FindObjectOfType<LoadingBarController>();

                if (!sharedInstance)
                {
                    Debug.LogError("No LoadingBarController object found, there needs to be one LoadingBarController object in scene");
                }
            }
            return sharedInstance;
        }
    }

	public Image barImage;
    public Text loadText;
    bool loading = false;

	float totalLoadAmount;
	float currentLoadProgress;
    float extraLoadDetail;

    List<SegmentToLoad> segmentsToLoad;

    public void Update()
    {
        if(loading)
        {
            barImage.fillAmount = (currentLoadProgress + extraLoadDetail)/totalLoadAmount;
        }
    }

    public void initLoading(List<SegmentToLoad> segments)
    {
        if(!loading)
        {
            loadText.text = segments[0].name;
            barImage.type = Image.Type.Filled;
            barImage.fillMethod = Image.FillMethod.Horizontal;
            currentLoadProgress = 0f;
            extraLoadDetail = 0f;
            barImage.fillAmount = 0f;
            loading = true;

            segmentsToLoad = segments;
            totalLoadAmount = (float)segmentsToLoad.Count;
        }
    }

    //some stuff we're loading might take longer than others, so we should try to show progress for that
    public void updateLoading(int totalSegmentsToLoad, int progressInSegments)
    {
        extraLoadDetail = (float)progressInSegments/(float)totalSegmentsToLoad;
        
    }

    void segmentLoaded(SegmentToLoad segment, int index)
    {
        extraLoadDetail = 0;
        currentLoadProgress += 1;

        index++;

        if(index < segmentsToLoad.Count)
        {
            loadText.text = segmentsToLoad[index].name;
        } else
        {
            loadText.text = "Completed!";
            loading = false;
            barImage.fillAmount = 1f;
        }
    }

    public void segmentLoaded(SegmentToLoad segment)
    {
        extraLoadDetail = 0;
        currentLoadProgress += 1;

        int index = segment.index + 1;

        if(index < segmentsToLoad.Count)
        {
            loadText.text = segmentsToLoad[index].name;
        } else
        {
            loadText.text = "Completed!";
            loading = false;
            barImage.fillAmount = 1f;
        }
    }

    int getSegmentIndex(string segmentKey)
    {
        for(int i = 0; i < segmentsToLoad.Count; i++)
        {
            if(segmentKey == segmentsToLoad[i].name)
            {
                return segmentsToLoad[i].index;
            }
        }

        //a fail case/end case
        return segmentsToLoad.Count;
    }
}
