using UnityEngine;

//add this to your "level goal" trigger
//[RequireComponent(typeof(Collider))]
public class Goal : MonoBehaviour 
{
	public float lift;				//the lifting force applied to player when theyre inside the goal
	public float loadDelay;			//how long player must stay inside the goal, before the game moves onto the next level
	public int nextLevelIndex;	//scene index of the next level
	
	private float counter;
	
	//when player is inside trigger for enough time, load next level
	//also lift player upwards, to give the goal a magical sense
	void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.tag == "Player")
		{
			Application.LoadLevel (nextLevelIndex);
		}
	}
}