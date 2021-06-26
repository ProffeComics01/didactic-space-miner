using UnityEngine;
using System.Collections;

public class RepulsorCapsule : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	void OnCollisionEnter(Collision other)
	{
		if(other.gameObject.tag=="Player" || other.gameObject.tag=="Player2")
		{
			other.rigidbody.AddForceAtPosition(other.contacts[0].normal*100f ,other.contacts[0].point);
		}
		
	}
}
