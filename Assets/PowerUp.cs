using UnityEngine;
using System.Collections;

public class PowerUp : MonoBehaviour  {

	public float speed;

    public float myRotationXSpeed = 100f;
	public float myRotationYSpeed = 100f;
	public float myRotationZSpeed = 100f;
    public bool isRotateX = false;
    public bool isRotateY = false;
    public bool isRotateZ = false;
	public GameObject getPowerFlash;
	//public Transform powerUpPosition;

	public int PowerUpType = -1;

	public enum PowerUpTypes
	{
		supporter = -5,
		speed = -4,
		life = -3,
		bomb = -2,
		shield = -1,
		DoublePlasma = 0,
		TriplePlasma = 1,
		Laser = 2,
		DoubleLaser = 3,
		TripleLaser = 4,
		Missile = 5,
		DualMissile = 6,
		TripleMissile = 7
	}

    // CHANGE TO ROTATE IN OPPOSITE DIRECTION
    private bool positiveRotation = false;

    private int pos0rNeg = 1;

    void Start ()
    {
		GetComponent<Collider>().isTrigger = true;
		if(positiveRotation == false)
		{
			pos0rNeg = -1;
		}
		// replace the need of "Done_Mover"
		GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }

	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Player")
		{

			Instantiate (getPowerFlash, transform.position, transform.rotation);

			switch(PowerUpType)
			{
				case (int)PowerUpTypes.shield:
					other.gameObject.GetComponent<SCPlayerController>().AddShield();
					break;
				case (int)PowerUpTypes.bomb:
					other.gameObject.GetComponent<SCPlayerController>().AddBomb();
					break;
				case (int)PowerUpTypes.life:
					other.gameObject.GetComponent<SCPlayerController>().AddLife();
					break;
				case (int)PowerUpTypes.speed:
					other.gameObject.GetComponent<SCPlayerController>().AddSpeed(2.5f);		
					break;
				case (int)PowerUpTypes.supporter:
					other.gameObject.GetComponent<SCPlayerController>().AddSupporter();	
					break;
				case (int)PowerUpTypes.DoublePlasma:
				case (int)PowerUpTypes.TriplePlasma:
				case (int)PowerUpTypes.Laser:
				case (int)PowerUpTypes.DoubleLaser:
				case (int)PowerUpTypes.TripleLaser:
				case (int)PowerUpTypes.Missile:
				case (int)PowerUpTypes.DualMissile:
				case (int)PowerUpTypes.TripleMissile:
					other.gameObject.GetComponent<SCPlayerController>().SetLivelloArma( PowerUpType+1 );
					break;
			}

			AudioSource.PlayClipAtPoint(GetComponent<AudioSource>().clip, transform.position);
			Destroy(gameObject); //,audio.clip.length+0.05f);
		}

	}
	
    void Update ()
    {
        // Toggles X Rotation
    if(isRotateX)    
       {
			transform.Rotate(myRotationXSpeed * Time.deltaTime * pos0rNeg, 0, 0);//rotates coin on X axis
          //Debug.Log("You are rotating in the X axis") ;
       }
       // Toggles Y Rotation
       if(isRotateY)    
       {
			transform.Rotate(0, myRotationYSpeed * Time.deltaTime * pos0rNeg, 0);//rotates coin on Y axis
          //Debug.Log("You are rotating in the X axis") ;
       }
          // Toggles Z Rotation
          if(isRotateZ)    
       {
			transform.Rotate(0, 0, myRotationZSpeed * Time.deltaTime * pos0rNeg);//rotates coin on Z axis
          //Debug.Log("You are rotating in the X axis") ;
       }
    }
}
