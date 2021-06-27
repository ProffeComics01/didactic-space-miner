using UnityEngine;
using System.Collections;


public class SCSupportShip : MonoBehaviour
{
	public Transform shotSpawn;
	public Transform shotSpawn1;
	public Transform shotSpawn2;

	void Update ()
	{
	}

	void Shoot(SCPlayerController thePlayer) // invok with a message from player
	{
		int livelloArma = thePlayer.GetlivelloArma ();
		switch(livelloArma)
		{
		case 0:
			Instantiate(thePlayer.bullets[0], shotSpawn.position, Quaternion.identity );
			break;
		case 1: // Double Plasma
			Instantiate(thePlayer.bullets[0], shotSpawn1.position, Quaternion.identity);
			Instantiate(thePlayer.bullets[0], shotSpawn2.position, Quaternion.identity);
			break;
		case 2: // Triple Plasma
			Instantiate(thePlayer.bullets[0], shotSpawn.position, Quaternion.identity);
			Instantiate(thePlayer.bullets[0], shotSpawn1.position, Quaternion.identity);
			Instantiate(thePlayer.bullets[0], shotSpawn2.position, Quaternion.identity);
			break;
		case 3: // Laser
			Instantiate(thePlayer.bullets[3], shotSpawn.position, Quaternion.identity);
			break;
		case 4: // Double Laser
			Instantiate(thePlayer.bullets[3], shotSpawn1.position, Quaternion.identity);
			Instantiate(thePlayer.bullets[3], shotSpawn2.position, Quaternion.identity);
			break;
		case 5: // Triple Laser
		case 6: // Missile
		case 7: // Missile
		case 8: // Missile
			Instantiate(thePlayer.bullets[3], shotSpawn.position, Quaternion.identity);
			Instantiate(thePlayer.bullets[3], shotSpawn1.position, Quaternion.identity);
			Instantiate(thePlayer.bullets[3], shotSpawn2.position, Quaternion.identity);
			break;
		}
	}

}
