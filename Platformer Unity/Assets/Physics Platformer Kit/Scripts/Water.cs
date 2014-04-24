﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//class to handle buoyancy of objects whilst in a "Water" tagged object
[RequireComponent(typeof(BoxCollider))]
public class Water : MonoBehaviour 
{
	public AudioClip splashSound;						//played when objects enter water
	public Vector3 force = new Vector3(0, 16.5f, 0);	//pushForce of the water. This is a vector3 so you can have force in any direction, for example a current or river
	public bool effectPlayerDrag;						//should the players rigidbody be effected by the drag/angular drag values of the water?
	public float resistance = 0.4f;						//the drag applied to rigidbodies in the water (but not player)
	public float angularResistance = 0.2f;				//the angular drag applied to rigidbodies in the water (but not player)
	
	private Dictionary<GameObject, float> dragStore = new Dictionary<GameObject, float>();
	private Dictionary<GameObject, float> angularStore = new Dictionary<GameObject, float>();
	
	void Awake()
	{
		if(tag != "Water")
		{
			tag = "Water";
			Debug.LogWarning("'Water' script attached to an object not tagged 'Water', it been assigned the tag 'Water'", transform);
		}
		collider.isTrigger = true;
	}
	
	//apply buoyancy
	void OnTriggerStay(Collider other)
	{
		//get surface position
		float surface = transform.position.y + collider.bounds.extents.y;
		if(other.rigidbody)
		{
			//get object depth
			float depth = surface - other.transform.position.y;
			//if below surface, push object
			if(depth > 0.4f)
				other.rigidbody.AddForce(force, ForceMode.Force);
			//if we are near the surface, add less force, this prevents objects from "jittering" up and down on the surface
			else
				other.rigidbody.AddForce (force * (depth * 2), ForceMode.Force);
		}
	}
	
	//sets drag on objects entering water
	void OnTriggerEnter(Collider other)
	{
		//rigidbody entered water?
		if(other.rigidbody)
		{
			if(splashSound)
			{
				float volume = other.rigidbody.velocity.magnitude/5;
				AudioSource.PlayClipAtPoint(splashSound, other.transform.position, volume);
			}
			Rigidbody r = other.rigidbody;
			//stop if we arent effecting player
			if (r.tag == "Player" && !effectPlayerDrag)
				return;
	
			//store objects default drag values
			dragStore.Add (r.gameObject, r.drag);
			angularStore.Add(r.gameObject, r.angularDrag);
			
			//apply new drag values to object
			r.drag = resistance;
			r.angularDrag = angularResistance;
		}
		else if(splashSound)
			AudioSource.PlayClipAtPoint(splashSound, other.transform.position);
	}
	
	//reset drag on objects leaving water
	void OnTriggerExit(Collider other)
	{
		//rigidbody entered water?
		if(other.rigidbody)
		{
			Rigidbody r = other.rigidbody;
			//stop if we arent effecting player
			if(r.tag == "Player" && !effectPlayerDrag)
				return;
			
			//see if we've stored this objects default drag values
			if (dragStore.ContainsKey(r.gameObject) && angularStore.ContainsKey(r.gameObject))
			{
				//restore values
				r.drag = dragStore[r.gameObject];
				r.angularDrag = angularStore[r.gameObject];
				//remove stored values for this object
				dragStore.Remove(r.gameObject);
				angularStore.Remove (r.gameObject);
			}
			else
			{
				//restore default values incase we cant find it in list (for whatever reason)
				r.drag = 0f;
				r.angularDrag = 0.05f;
				print ("Object left water: couldn't get drag values, restored to defaults");
			}
		}
	}
}