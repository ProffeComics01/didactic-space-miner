// DummyScene
// Loads a level from application controller global variables
// by Litobyte Softworks di Tommaso Lintrami
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Orbit : MonoBehaviour
{
	
	public Transform target;
	public float speed;
	public bool yaxis;
	public bool both;
	public bool xneg;
	
	
	void Awake()
	{
		
	}
	
	void Update()
	{
		if(yaxis){
			
			if(both){
				if(xneg){
					transform.RotateAround (target.position, Vector3.forward, speed * Time.deltaTime);
					transform.RotateAround (target.position, Vector3.down, speed * Time.deltaTime);
				}
				else{
					transform.RotateAround (target.position, Vector3.left, speed * Time.deltaTime);
					transform.RotateAround (target.position, Vector3.up, speed * Time.deltaTime);
				}
			}
			else{
				transform.RotateAround (target.position, Vector3.left, speed * Time.deltaTime);
			}
		}
		else{	transform.RotateAround (target.position, Vector3.up, speed * Time.deltaTime);}
	}

}