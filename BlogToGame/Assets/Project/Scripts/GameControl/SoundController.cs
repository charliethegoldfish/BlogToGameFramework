using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Random = UnityEngine.Random;

[System.Serializable]
public class SoundEntry
{
	public string key;
	public List<AudioClip> audioClips;

	public AudioClip grabClip()
	{
		if(audioClips.Count < 1)
		{
			Debug.LogWarning("No audio clip assigned to " + key);
			return null;
		}

		int index = Random.Range(0, audioClips.Count);
		return audioClips[index];
	}
}

public class SoundController : LoadingObject {

	public List<SoundEntry> soundLibrary;
	public AudioSource musicSource;
	public AudioSource playerSource;
	public AudioSource[] audioSourceList;

	private static SoundController sharedInstance = null;

    public static SoundController instance
    {
        get
        {
            if (sharedInstance == null)
            {
                sharedInstance = GameObject.FindObjectOfType<SoundController>();

                if (!sharedInstance)
                {
                    Debug.LogError("No SoundController object found, there needs to be one SoundController object in scene");
                }
            }
            return sharedInstance;
        }
    }

	public override void load(Action completion)
	{
		audioSourceList = gameObject.GetComponentsInChildren<AudioSource>();
		completion();
	}
	public AudioClip getAudioClip(string key)
	{
		for(int i = 0; i < soundLibrary.Count; i++)
		{
			if(key == soundLibrary[i].key) return soundLibrary[i].grabClip();
		}

		Debug.LogWarning("There is no audio entry for " + key);
		return null;
	}

	public void playAudioEntry(string key)
	{
		//if we don't want to play sound effects, then I guess we shouldn't aye
		if(!SettingsController.instance.playSoundEffects()) return;
		
		AudioClip clip = getAudioClip(key);
		if(clip == null) return;

		AudioSource source = grabNextAudioSource();
		if(source == null) return;

		source.clip = clip;
		source.Play();
	}

	private AudioSource grabNextAudioSource()
	{
		for(int i = 0; i < audioSourceList.Length; i++)
		{
			if(audioSourceList[i].isPlaying) continue;

			return audioSourceList[i];
		}

		Debug.LogWarning("We have no available audio sources");
		return null;
	}
	
}
