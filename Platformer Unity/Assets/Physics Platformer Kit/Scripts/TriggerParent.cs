﻿using UnityEngine;
using System.Collections;

//this is a utility class. It holds collision information about this trigger, so another script can access that.
//for example: put this on an enemies "field of vision cone", and then the AIscript gets information like: has the player entered the field of vision?
public class TriggerParent : MonoBehaviour 
{
	public string[] tagsToCheck;			//if left empty, trigger will check collisions from everything. Othewise, it will only check these tags
	
	[HideInInspector]
	public bool collided, colliding;
	[HideInInspector]
	public GameObject hitObject;
	
	void Awake()
	{
		if(!collider || (collider && !collider.isTrigger))
			Debug.LogError ("'TriggerParent' script attached to object which does not have a trigger collider", transform);
	}
	
	//see if anything entered trigger, filer by tag, store the object
	void OnTriggerEnter (Collider other)
	{
		if (tagsToCheck.Length > 0 && !collided)
		{
			foreach (string tag in tagsToCheck)
			{
				if (other.tag == tag )
				{
					collided = true;
					hitObject = other.gameObject;
					break;
				}
					
			}
		}
		else
			collided = true;
			hitObject = other.gameObject;
	}
	
	//see if anything is constantly colliding with this trigger, filter by tag, store the object
	void OnTriggerStay (Collider other)
	{
		if (tagsToCheck.Length > 0)
		{
			foreach (string tag in tagsToCheck)
			{
				if (other.tag == tag )
				{
					hitObject = other.gameObject;
					colliding = true;
					break;
				}
			}
		}
		else
		{
			hitObject = other.gameObject;
			colliding = true;
		}
	}
	
	//this runs after the main code, and resets the info to false
	//so we know when something is no longer colliding with this trigger
	void LateUpdate()
	{
		colliding = false;
		collided = false;
		hitObject = null;
	}
}