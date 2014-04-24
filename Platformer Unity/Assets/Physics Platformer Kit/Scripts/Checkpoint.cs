using UnityEngine;

//simple class to add to checkpoint triggers
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AudioSource))]
public class Checkpoint : MonoBehaviour 
{
	public Color activeColor = Color.green;	//color when checkpoint is activated
	public float activeColorOpacity = 0.4f;	//opacity when checkpoint is activated
	
	private Health health;
	private Color defColor;
	private GameObject[] checkpoints;
	
	//setup
	void Awake()
	{
		if(tag != "Respawn")
		{
			tag = "Respawn";
			Debug.LogWarning ("'Checkpoint' script attached to object without the 'Respawn' tag, tag has been assigned automatically", transform);	
		}
		collider.isTrigger = true;
		
		if(renderer)
			defColor = renderer.material.color;
		activeColor.a = activeColorOpacity;
	}
	
	//more setup
	void Start()
	{
		checkpoints = GameObject.FindGameObjectsWithTag("Respawn");
		health = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
		if(!health)
			Debug.LogError("For Checkpoint to work, the Player needs 'Health' script attached", transform);
	}
	
	//set checkpoint
	void OnTriggerEnter(Collider other)
	{
		if(other.transform.tag == "Player" && health)
		{
			//set respawn position in players health script
			health.respawnPos = transform.position;
			
			//toggle checkpoints
			if(renderer.material.color != activeColor)
			{
				foreach (GameObject checkpoint in checkpoints)
					checkpoint.renderer.material.color = defColor;
				audio.Play();
				renderer.material.color = activeColor;
			}
		}
	}
}