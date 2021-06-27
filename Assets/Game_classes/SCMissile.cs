using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SCMissile : MonoBehaviour
{

	private Transform target;
	public float speed=10f;

	public GameObject hitEffect;
	public AudioClip soundEffect;
	public float soundEffectRange=10f;

	public bool fixedCurveX=false;
	public float maxCurveX=30.0f;
	public float maxCurveY=15.0f;

	public bool randCoeffX=true;
	public bool randCoeffY=true;
	public float coeffX=1.0f;	//coeff indicate number of half-cycles of the sinwave in effect for x-axis
	public float coeffY=1.0f;	//coeff indicate number of half-cycles of the sinwave in effect for y-axis

	private float maxRange=15f;
	private float curveAngleX=15f;
	private float curveAngleY=15f;

	private float triggerpoint=0.15f;
	private float targetDist;
	private float initialTargetDist;
	private Vector3 lastTargetPosition;
	private bool hitTarget=false;

	private Quaternion dir;
	private float pi=Mathf.PI;
	private Transform thisTransform;


	void Awake(){
		thisTransform=transform;
	}

	public void Initiate(Transform t, float delay){

		if(Debug.isDebugBuild) Debug.Log ("MISSILE FIRED! Target:"+t.name);
		hitTarget=false;
		//this.gameObject.active=true;
		
		if(t!=null){

			target=t;
			lastTargetPosition=target.position;
			
			if( target.gameObject.GetComponent<BoxCollider>()!=null ){
				BoxCollider col = target.gameObject.GetComponent<BoxCollider>() as BoxCollider;
				triggerpoint = (col.size.x+col.size.z)/4f;
			}
			
			initialTargetDist=Vector3.Distance(thisTransform.position, lastTargetPosition);
			
			if(!fixedCurveX) 
				curveAngleX=(initialTargetDist/maxRange)*Random.Range(0f, maxCurveX);
			else if(fixedCurveX) 
				curveAngleX=(initialTargetDist/maxRange)*Random.Range(maxCurveX-10f, maxCurveX+10f);
			
			curveAngleY=(initialTargetDist/maxRange)*Random.Range(-maxCurveY, maxCurveY);
			
			if(randCoeffX)	coeffX=Random.Range(0.5f, 1.5f);
			if(randCoeffY)	coeffY=Random.Range(0.5f, 1.5f);
			
			StartCoroutine( "DelayUpdate",delay );
			
		}
		else{
			Destroy(gameObject);
		}
		
		
	}

	IEnumerator DelayUpdate (float delay) {


		yield return new WaitForSeconds(delay);

		while(true){

			if( Vector3.Distance(thisTransform.position, lastTargetPosition)<triggerpoint && !hitTarget) Hit();
			
			if(target!=null) {
				targetDist=Vector3.Distance(thisTransform.position, target.transform.position);
				dir=Quaternion.LookRotation(target.transform.position-thisTransform.position);
				thisTransform.rotation=dir;
				lastTargetPosition=target.transform.position;
			}
			else{

				targetDist=Vector3.Distance(thisTransform.position, lastTargetPosition);
				dir=Quaternion.LookRotation(lastTargetPosition-thisTransform.position);
				thisTransform.rotation=dir;

				//target = null;

				GameObject[] ae = GameObject.FindGameObjectsWithTag("Enemy") as GameObject[];
				if(ae.Length>0)
				{
					target = ae[Random.Range(0,ae.Length)].transform;
				}
				else
				{
					target = GameObject.Find ("Fakesteroid").transform;
				}
			}
			thisTransform.rotation.eulerAngles.Set ( -90.0f+(curveAngleX*Mathf.Sin(coeffX*pi*((initialTargetDist-targetDist)/initialTargetDist)+pi/2f)), thisTransform.rotation.eulerAngles.y, (curveAngleY*Mathf.Sin(coeffY*pi*((initialTargetDist-targetDist)/initialTargetDist))));
			//thisTransform.rotation.eulerAngles.Set ( -90.0f+(curveAngleX*Mathf.Sin(coeffX*pi*((initialTargetDist-targetDist)/initialTargetDist)+pi/2f)),(curveAngleY*Mathf.Sin(coeffY*pi*((initialTargetDist-targetDist)/initialTargetDist))), thisTransform.rotation.eulerAngles.z);
			//thisTransform.rotation.eulerAngles.x -= -90.0f+(curveAngleX*Mathf.Sin(coeffX*pi*((initialTargetDist-targetDist)/initialTargetDist)+pi/2f));
			//thisTransform.rotation.eulerAngles.y -= (curveAngleY*Mathf.Sin(coeffY*pi*((initialTargetDist-targetDist)/initialTargetDist)));
			
			thisTransform.Translate(Vector3.forward*speed*Time.deltaTime);		
			//thisTransform.Translate( Vector3.up*Mathf.Min(targetDist*0.65f, speed*Time.deltaTime) );
			
			yield return new WaitForEndOfFrame();
		}
	}


	void Hit(){

		StopCoroutine("DelayUpdate");
		hitTarget=true;

		if(soundEffect!=null){
			GameObject audioObject = new GameObject();
			audioObject.AddComponent<AudioSource>();
			audioObject.GetComponent<AudioSource>().minDistance=soundEffectRange;
			audioObject.GetComponent<AudioSource>().clip=soundEffect;
			audioObject.GetComponent<AudioSource>().loop=false;
			//audioObject.audio.volume = whoShotMe.towerSfxVolume;
			audioObject.GetComponent<AudioSource>().Play();
			Destroy(audioObject, soundEffect.length);
		}

		if(target!=null)
		{

			if(target.name=="Fakesteroid") Destroy(gameObject); else target.SendMessage ("HitByMissile",transform);
			//if(target.tag == "Enemy") target.SendMessage ("HitByMissile",transform); else if(target.name=="Fakesteroid") Destroy(gameObject);
			//if(target.tag == "Player") target.SendMessage ("HitByMissile",transform); else if(target.name=="Fakesteroid") Destroy(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

}
