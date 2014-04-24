using UnityEngine;
using System.Collections;

//this allows the player to pick up/throw, and also pull certain objects
//you need to add the tags "Pickup" or "Pushable" to these objects
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMove))]
public class Throwing : MonoBehaviour 
{
	public string joystickGrab = "Grab";
	public AudioClip pickUpSound;								//sound when you pickup/grab an object
	public AudioClip throwSound;								//sound when you throw an object
	public GameObject grabBox;									//objects inside this trigger box can be picked up by the player (think of this as your reach)
	public float gap = 0.5f;									//how high above player to hold objects
	public Vector3 throwForce = new Vector3(0, 5, 7);			//the throw force of the player
	public float rotateToBlockSpeed = 3;						//how fast to face the "Pushable" object you're holding/pulling
	public float checkRadius = 0.5f;							//how big a radius to check above the players head, to see if anything is in the way of your pickup
	[Range(0.1f, 1f)]											//new weight of a carried object, 1 means no change, 0.1 means 10% of its original weight													
	public float weightChange = 0.3f;							//this is so you can easily carry objects without effecting movement if you wish to
	[Range(10f, 1000f)]
	public float holdingBreakForce = 45, holdingBreakTorque = 45;//force and angularForce needed to break your grip on a "Pushable" object youre holding onto
	public Animator animator;									//object with animation controller on, which you want to animate (usually same as in PlayerMove)
	public int armsAnimationLayer;								//index of the animation layer for "arms"
	
	[HideInInspector]
	public GameObject heldObj;
	private Vector3 holdPos;
	private FixedJoint joint;
	private float timeOfPickup, timeOfThrow, defRotateSpeed;
	private Color gizmoColor;
	
	
	private PlayerMove playerMove;
	private CharacterMotor characterMotor;
	private TriggerParent triggerParent;
	private RigidbodyInterpolation objectDefInterpolation;
	
	
	//setup
	void Awake()
	{
		//create grabBox is none has been assigned
		if(!grabBox)
		{
			grabBox = new GameObject();
			grabBox.AddComponent<BoxCollider>();
			grabBox.collider.isTrigger = true;
			grabBox.transform.parent = transform;
			grabBox.transform.localPosition = new Vector3(0f, 0f, 0.5f);
			grabBox.layer = 2;	//ignore raycast by default
			Debug.LogWarning("No grabBox object assigned to 'Throwing' script, one has been created and assigned for you", grabBox);
		}
		
		playerMove = GetComponent<PlayerMove>();
		characterMotor = GetComponent<CharacterMotor>();
		defRotateSpeed = playerMove.rotateSpeed;
		//set arms animation layer to animate with 1 weight (full override)
		if(animator)
			animator.SetLayerWeight(armsAnimationLayer, 1);
	}
	
	//throwing/dropping
	void Update()
	{
		//when we press grab button, throw object if we're holding one
		if (Input.GetButtonDown (joystickGrab) && heldObj && Time.time > timeOfPickup + 0.1f)
		{
			if(heldObj.tag == "Pickup" || heldObj.tag == "Player") 
				ThrowPickup();
		}
		//set animation value for arms layer
		if(animator)
			if(heldObj && (heldObj.tag == "Pickup" || heldObj.tag == "Player"))
				animator.SetBool ("HoldingPickup", true);
			else
				animator.SetBool ("HoldingPickup", false);
		
			if(heldObj && heldObj.tag == "Pushable")
				animator.SetBool ("HoldingPushable", true);
			else
				animator.SetBool ("HoldingPushable", false);
		
		//when grab is released, let go of any pushable objects were holding
		if (heldObj && heldObj.tag == "Pushable")
		{
			characterMotor.RotateToDirection(heldObj.transform.position, rotateToBlockSpeed, true);
			if(Input.GetButtonUp (joystickGrab))
			{
				DropPushable();
			}
			
			if(!joint)
			{
				DropPushable();
				print ("'Pushable' object dropped because the 'holdingBreakForce' or 'holdingBreakTorque' was exceeded");
			}	
		}
	}
	
	//pickup/grab
	void OnTriggerStay(Collider other)
	{
		//if grab is pressed and an object is inside the players "grabBox" trigger
		if(Input.GetButton(joystickGrab))
		{
			//pickup
			if((other.tag == "Pickup" || other.tag == "Player") && heldObj == null && timeOfThrow + 0.2f < Time.time)
				LiftPickup(other);
			//grab
			if(other.tag == "Pushable" && heldObj == null && timeOfThrow + 0.2f < Time.time)
				GrabPushable(other);
		}
	}
			
	private void GrabPushable(Collider other)
	{
		heldObj = other.gameObject;
		objectDefInterpolation = heldObj.rigidbody.interpolation;
		heldObj.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		AddJoint ();
		//set limits for when player will let go of object
		joint.breakForce = holdingBreakForce;
		joint.breakTorque = holdingBreakTorque;
		//stop player rotating in direction of movement, so they can face the block theyre pulling
		playerMove.rotateSpeed = 0;
	}
	
	private void LiftPickup(Collider other)
	{
		//get where to move item once its picked up
		Mesh otherMesh = other.GetComponent<MeshFilter>().mesh;
		holdPos = transform.position;
		holdPos.y += (collider.bounds.extents.y) + (otherMesh.bounds.extents.y) + gap;
		
		//if there is space above our head, pick up item (layermask index 2: "Ignore Raycast", anything on this layer will be ignored)
		if(!Physics.CheckSphere(holdPos, checkRadius, 2))
		{
			gizmoColor = Color.green;
			heldObj = other.gameObject;
			objectDefInterpolation = heldObj.rigidbody.interpolation;
			heldObj.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			heldObj.transform.position = holdPos;
			heldObj.transform.rotation = transform.rotation;
			AddJoint();
			//here we adjust the mass of the object, so it can seem heavy, but not effect player movement whilst were holding it
			heldObj.rigidbody.mass *= weightChange;
			//make sure we don't immediately throw object after picking it up
			timeOfPickup = Time.time;
		}
		//if not print to console (look in scene view for sphere gizmo to see whats stopping the pickup)
		else
		{
			gizmoColor = Color.red;
			print ("Can't lift object here. If nothing is above the player, make sure triggers are set to layer index 2 (ignore raycast by default)");
		}
	}
	
	private void DropPushable()
	{
		heldObj.rigidbody.interpolation = objectDefInterpolation;
		Destroy (joint);
		playerMove.rotateSpeed = defRotateSpeed;
		heldObj = null;
		timeOfThrow = Time.time;
	}
	
	public void ThrowPickup()
	{
		if(throwSound)
		{
			audio.volume = 1;
			audio.clip = throwSound;
			audio.Play ();
		}
		Destroy (joint);
		heldObj.rigidbody.interpolation = objectDefInterpolation;
		heldObj.rigidbody.mass /= weightChange;
		heldObj.rigidbody.AddRelativeForce (throwForce, ForceMode.VelocityChange);
		heldObj = null;
		timeOfThrow = Time.time;
	}
	
	//connect player and pickup/pushable object via a physics joint
	private void AddJoint()
	{
		if (heldObj)
		{
			if(pickUpSound)
			{
				audio.volume = 1;
				audio.clip = pickUpSound;
				audio.Play ();
			}
			joint = heldObj.AddComponent<FixedJoint>();
			joint.connectedBody = rigidbody;
		}
	}
	
	//draws red sphere if something is in way of pickup (select player in scene view to see)
	void OnDrawGizmosSelected()
	{
		Gizmos.color = gizmoColor;
		Gizmos.DrawSphere (holdPos, checkRadius);
	}
}

/* NOTE: to check if the player is able to lift an object, and that nothing is above their head, a sphereCheck is used. (line 100)
 * this has a layermask set to layer index 2, by default: "Ignore Raycast", so to lift objects properly, any triggers need to be set to this layer
 * else the sphereCheck will collide with the trigger, and the script will think we cannot lift an object here, as there is something above our head */