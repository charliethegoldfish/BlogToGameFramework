using System.Collections;
using UnityEngine;
using System;

public class LoadingObject : MonoBehaviour {

	public bool completed;

	// Use this for initialization
	public virtual void load(Action completion)
	{
		completion();
	}
}
