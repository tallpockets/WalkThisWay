using UnityEngine;
using System.Collections;

public class CharNav : MonoBehaviour {

	//link to Animator component
	public Animator animController;

	//used to set anim controller parameters
	public enum MoveState {Idle, Walking}
	public MoveState moveState;

	//link to NavMeshAgent component
	public NavMeshAgent navAgent;




	// oq
	private NavMeshAgent agent;
	private float speed;
	private Animator anim;							// a reference to the animator on the character


	Quaternion lastRotation;
	Quaternion currentRotation;






	void Start ()
	{
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();	
	}





	void Update ()
	{


		speed = (Mathf.Clamp (((agent.remainingDistance / 4.0f) + 0.0f), 0, 2));
		anim.SetFloat ("MoveSpeed", speed);


		//Debug.Log (speed);

		//set animation speed based on navAgent 'Speed' var
		animController.speed = navAgent.speed;

		//character walks if there is a navigation path longer than the value set here, idle all other times
		if (navAgent.remainingDistance > 1.0f) {
			moveState = MoveState.Walking;
		}

		else
			moveState = MoveState.Idle;



		//send move state info to animator controller
		animController.SetInteger("MoveState", (int)moveState);
	}





	void OnAnimatorMove ()
	{



		//set the navAgent's velocity to the velocity of the animation clip currently playing
		navAgent.velocity = animController.deltaPosition / Time.deltaTime;


		//only perform if walking
		if (moveState == MoveState.Walking)
		{

			//smoothly rotate the character in the desired direction of motion
			Quaternion lookRotation = Quaternion.LookRotation(navAgent.desiredVelocity);
			currentRotation = Quaternion.RotateTowards(transform.rotation, lookRotation, navAgent.angularSpeed * Time.deltaTime);
			transform.rotation = Quaternion.Lerp( lastRotation, currentRotation, Time.deltaTime * 25);
		}


			lastRotation = navAgent.transform.rotation;

	}


}









