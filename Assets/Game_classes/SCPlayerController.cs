using UnityEngine;
using System.Collections;
// The Serializable attribute lets you embed a generic (non monobehaviour) class with sub properties in the inspector.
[System.Serializable]
public class Done_Boundary
{
	public float xMin, xMax, zMin, zMax;
}
// La classe del nostro caro player controller
public class SCPlayerController : MonoBehaviour
{
	public Done_Boundary boundary;
	public AudioClip[] weaponsAudio;
	public GameObject supporter;
	public GameObject supporterR;
	public GameObject Shield;
	public GameObject Mine;
	public GameObject Smoke;
	public GameObject DetonationForce;
	public Transform shotSpawn;
	public Transform shotSpawn1;
	public Transform shotSpawn2;
	public Transform supporterSpawn;
	public Transform supporterSpawn2;
	public Transform shieldSpawn;
	public Transform mineSpawn;
	public float fireRate;
	public Texture2D bombIcon;
	public Texture2D lifeQuad;
	public GameObject shipLevel1,shipLevel2,shipLevel3;
	public GameObject fakesteroid;

	private float nextFire; // tiene il tempo per il prossimo sparo (laser  o plasma)
	private float nextMissile; // tiene il tempo per il prossimo missile
	private int score;
	private int health;
	public int startHealth=5;
	private int maxHealth;
	[HideInInspector]public int shieldsHP;

	private int numOfBombs;
	public int startNumOfBombs = 3;

	// ShipInfo, globali recuperati dalle strutture ShipInfo delle 3 navette
	private int maxNumOfBombs;	// numero massimo che una nave giocatore può portare (questo numero limita il numero di powerup bomba che si possono raccogliere attivamente, 
								// e in seguito(to do!), condizionerà ache la non uscita di tal power up(per render piu facile il gioco)
	private float baseSpeed;	// la speed di base della navetta corrente
	private float speedBonus;	// la speed bonus ottenuta con i powerups
	private float speed;		// la speed attuale (speed base+bonus speed powerup)
	private float tilt;			// ammontare di tilt attuale per questa nave
	private float maxSpeed = 14f;// massima velocità raggiungibile (baseSpeed+speedBonus)

	private int livelloArma;	// livello armamenti/nave corrente
	private int playerCtrl=1;   // tipo di controllo, tastiera, mouse, joystick, 0-2
	public HealthBar PlayerBars;
	public GameObject [] bullets;

	private GameObject flame1,flame2,flame3;
	private GameObject AidRightGO, AidLeftGO; // supporters ships

	public struct ShipInfo
	{
		public float speed;
		public float tilt;
		public int maxNumOfBombs;
		//public float colliderRadius;

		public ShipInfo( float sp, float til,int maxNbomb) //, float colRad)
		{
			this.speed = sp;
			this.tilt = til;
			this.maxNumOfBombs = maxNbomb;
			//this.colliderRadius = colRad;
		}
	}
	// le strutture di tipo ShipInfo ship1data, etc...contengono i 3 assetti delle 3 navette con cui si combatte via via
	public ShipInfo ship1data,ship2data,ship3data;
	private SCGameController gameController;
	private ApplicationController appCtrl;

	void Awake ()
	{
		// crea le 3 strutture ShipInfo per le 3 navette player
		ship1data = new ShipInfo (6f, 5f, 3); //, 0.35f);
		ship2data = new ShipInfo (5f, 4f, 5); //, 0.5f);
		ship3data = new ShipInfo (4f, 3f, 10); //, 1f);
		
		//SetShipInfo (ship1data);

		GameObject appCtrlGO = GameObject.Find ("ApplicationController");
		if (appCtrlGO)
		{
			appCtrl = appCtrlGO.GetComponent<ApplicationController> ();
		}
		else
		{
			Debug.LogWarning ( " Attenzione, il game è stato lanciato senza application controller");
		}


		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null)
		{
			gameController = gameControllerObject.GetComponent <SCGameController>();
		}
		if (gameController == null)
		{
			Debug.Log ("Cannot find 'GameController' script");
		}
	}

	void Start()
	{

	}

	private float moveHorizontal=0f;
	private float moveVertical=0f;

    private Vector3 worldPosition;

    // update input and shit
    void Update ()
	{

		// Aggiornamento delle barrette del giocatore (il controllo if(PlayerBars) serve a prevenire un crash quando le barrette non esistessero per qualche ragione 
		//	(per esempio, state cambiando la GUI con dell'altro vostro codice e buttando via le barrette) o vi siete dimenticati di mettere l'oggetto barrette del player in scena
		// o non esistono perchè ancora disattivate (stato di game over)
		if(PlayerBars)
		{
			PlayerBars.values[0] = (float)health/maxHealth; // health
			//PlayerBars.values[1] = (float)livelloArma/8f; // weapon power
			PlayerBars.values[1] = (float)shieldsHP/5f;
			PlayerBars.values[2] = (float)speed/maxSpeed; // speed
		}


		// Il movimento viene gestito, come gli Input nella FixedUpdate piuttosto che l'Update 
		// perchè vengono utilizzati i Rigidbody e lo spostamento della fisica, piuttosto che i Transform (fisica e input vanno gestiti sempre nella fixed update!)
		
		//  tipo di controllo input del player (0-2)
		switch(playerCtrl)
		{
		case 0: // TASTIERA
			
			moveHorizontal = Input.GetAxis ("Horizontal");
			moveVertical = Input.GetAxis ("Vertical");
			
			if ( (Input.GetKeyUp(KeyCode.K) || Input.GetKeyUp(KeyCode.RightShift)  || Input.GetKeyUp(KeyCode.LeftShift) ||  Input.GetKeyUp(KeyCode.Space)  ) 
			    && Time.time > nextFire && numOfBombs>0) 
			{
				nextFire = Time.time + fireRate;
				--numOfBombs;
				Instantiate (Mine, shieldSpawn.position, shieldSpawn.rotation);
			}
			if ( (Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Time.time > nextFire) Shoot();
			
			break;
		case 1: // MOUSE
            moveHorizontal = Input.GetAxis("Mouse X");
            moveVertical = Input.GetAxis("Mouse Y");
            if(moveHorizontal>0.1f || moveVertical > 0.01f)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Camera.main.nearClipPlane;
                worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

                journeyLength = Vector3.Distance(GetComponent<Rigidbody>().position, worldPosition);
            }

            if (Input.GetButtonUp ("Fire2") && Time.time > nextFire && numOfBombs>0) 
			{
				nextFire = Time.time + fireRate;
				--numOfBombs;
				Instantiate (Mine, shieldSpawn.position, shieldSpawn.rotation);
			}
			// Tasto fire primario, bottone sinistro del mouse, CTRL, joystick A  (guardare gli Input Settings di progetto per ulteriori info
			if ( Input.GetButton("Fire1") && Time.time > nextFire) Shoot(); //if (Input.GetMouseButton(0) && Time.time > nextFire) Shoot();
			break;
			
		case 2:  // TO DO: JoyStick controls, dovremo differenziare i vari nomi chiave Fire1, Fire2, e anche qui Horizontal e Vertical per gestire a parte il controllo
			moveHorizontal = Input.GetAxis ("HorizontalJoy");
			moveVertical = Input.GetAxis ("VerticalJoy");
			
			if (Input.GetButtonUp ("Fire2") && Time.time > nextFire && numOfBombs>0) 
			{
				nextFire = Time.time + fireRate;
				--numOfBombs;
				Instantiate (Mine, shieldSpawn.position, shieldSpawn.rotation);
			}
			
			// Tasto fire primario, bottone sinistro del mouse, CTRL, joystick A  (guardare gli Input Settings di progetto per ulteriori info
			if ( Input.GetButton("Fire1") && Time.time > nextFire) Shoot(); //if (Input.GetMouseButton(0) && Time.time > nextFire) Shoot();
			
			break;
			
		}
	}
    private float startTime;
    private float journeyLength;

    void FixedUpdate ()
	{
        if ( gameController.GetGameState() == 4) // playTime, enable controller movement
		{
            //  tipo di controllo input del player (0-2)
            switch (playerCtrl)
            {

                case 0: // TASTIERA
                    // Applica il movimento orizzontale e verticale del controller al rigidbody della navetta
                    Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
                    if (gameController.GetGameState() == 5) // warpTime, control movement to move player at center and forward to the black hole
                    {
                        if (GetComponent<Rigidbody>().position.x >= 0.1f) moveHorizontal = -1f; else if (GetComponent<Rigidbody>().position.x <= -0.1f) moveHorizontal = 1f; else moveHorizontal = 0f;
                        movement = new Vector3(moveHorizontal, 0.0f, 0.5f);
                    }

                    GetComponent<Rigidbody>().velocity = movement * speed;
                    GetComponent<Rigidbody>().position = new Vector3
                        (
                            Mathf.Clamp(GetComponent<Rigidbody>().position.x, boundary.xMin, boundary.xMax),
                            0.0f,
                            Mathf.Clamp(GetComponent<Rigidbody>().position.z, boundary.zMin, boundary.zMax)
                            );
                    GetComponent<Rigidbody>().rotation = Quaternion.Euler(0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -tilt);

                    break;
                case 1: // MOUSE
                    // Applica il movimento orizzontale e verticale del controller al rigidbody della navetta
                    movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
                    float distCovered = (Time.time - startTime) * speed;
                    float fracJourney = distCovered / journeyLength;
                    GetComponent<Rigidbody>().position = Vector3.Lerp(GetComponent<Rigidbody>().position, worldPosition, fracJourney);

                    //GetComponent<Rigidbody>().velocity = movement * speed;
                    // constrain the rigidbody into the boundary square
                    GetComponent<Rigidbody>().position = new Vector3
                        (
                            Mathf.Clamp(GetComponent<Rigidbody>().position.x, boundary.xMin, boundary.xMax),
                            0.0f,
                            Mathf.Clamp(GetComponent<Rigidbody>().position.z, boundary.zMin, boundary.zMax)
                            );
                    // set tilt rotation according to horizontal velocity
                    GetComponent<Rigidbody>().rotation = Quaternion.Euler(0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -tilt);
                    break;

                case 2:  // TO DO: JoyStick controls, dovremo differenziare i vari nomi chiave Fire1, Fire2, e anche qui Horizontal e Vertical per gestire a parte il controllo

                    // Applica il movimento orizzontale e verticale del controller al rigidbody della navetta
                    movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

                    if (gameController.GetGameState() == 5) // warpTime, control movement to move player at center and forward to the black hole
                    {
                        if (GetComponent<Rigidbody>().position.x >= 0.1f) moveHorizontal = -1f; else if (GetComponent<Rigidbody>().position.x <= -0.1f) moveHorizontal = 1f; else moveHorizontal = 0f;
                        movement = new Vector3(moveHorizontal, 0.0f, 0.5f);
                    }

                    GetComponent<Rigidbody>().velocity = movement * speed;
                    GetComponent<Rigidbody>().position = new Vector3
                        (
                            Mathf.Clamp(GetComponent<Rigidbody>().position.x, boundary.xMin, boundary.xMax),
                            0.0f,
                            Mathf.Clamp(GetComponent<Rigidbody>().position.z, boundary.zMin, boundary.zMax)
                            );
                    GetComponent<Rigidbody>().rotation = Quaternion.Euler(0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -tilt);

                    break;

            }

		}

	}

	//////////////////////////////////////////
	// Shoot procedure, folder for commodity
	//////////////////////////////////////////
	void Shoot()
	{
		GameObject nemicoVicino;
		GameObject g;
		GameObject[] ae;
		nextFire = Time.time + fireRate;

		switch(livelloArma)
		{
		case 0:
			Instantiate(bullets[0], shotSpawn.position, Quaternion.identity );
			break;
		case 1: // Double Plasma
			Instantiate(bullets[0], shotSpawn1.position, Quaternion.identity);
			Instantiate(bullets[0], shotSpawn2.position, Quaternion.identity);
			break;
		case 2: // Triple Plasma
			Instantiate(bullets[0], shotSpawn.position, Quaternion.identity);
			Instantiate(bullets[0], shotSpawn1.position, Quaternion.identity);
			Instantiate(bullets[0], shotSpawn2.position, Quaternion.identity);
			break;
		case 3: // Laser
			Instantiate(bullets[3], shotSpawn.position, Quaternion.identity);
			break;
		case 4: // Double Laser
			Instantiate(bullets[3], shotSpawn1.position, Quaternion.identity);
			Instantiate(bullets[3], shotSpawn2.position, Quaternion.identity);
			break;
		case 5: // Triple Laser
			Instantiate(bullets[3], shotSpawn.position, Quaternion.identity);
			Instantiate(bullets[3], shotSpawn1.position, Quaternion.identity);
			Instantiate(bullets[3], shotSpawn2.position, Quaternion.identity);
			break;
		case 6: // Missile
			Instantiate(bullets[3], shotSpawn.position, Quaternion.identity);
			Instantiate(bullets[3], shotSpawn1.position, Quaternion.identity);
			Instantiate(bullets[3], shotSpawn2.position, Quaternion.identity);
			// Random choose one of the enemy on scree. Sceglie a caso uno dei nemici a schermo
			if(Time.time>nextMissile){
				nextMissile = Time.time + 1.0f;
				ae = GameObject.FindGameObjectsWithTag("Enemy") as GameObject[];
				if(ae.Length>0)
				{
					g=Instantiate(bullets[2], transform.position+new Vector3(0f,-1f,0f), Quaternion.identity) as GameObject;
					g.GetComponent<SCMissile>().Initiate( ae[Random.Range(0,ae.Length)].transform ,0f);
				}
				else
				{
					Debug.Log("No enemy on screen, fire for the glory");
					g=Instantiate(bullets[2], shotSpawn.position, Quaternion.identity) as GameObject;
					g.GetComponent<SCMissile>().Initiate( GameObject.Find ("Fakesteroid").transform,0f );
				}
			}
			break;
		case 7: // Triple Laser+ double Missile
			Instantiate(bullets[3], shotSpawn.position, Quaternion.identity);
			Instantiate(bullets[3], shotSpawn1.position, Quaternion.identity);
			Instantiate(bullets[3], shotSpawn2.position, Quaternion.identity);
			if(Time.time>nextMissile){
				nextMissile = Time.time + 0.75f;
				// Sceglie il più vicino dei nemici a schermo
				ae = GameObject.FindGameObjectsWithTag("Enemy") as GameObject[];
				
				if(ae.Length>0)
				{
					g=Instantiate(bullets[2], transform.position+new Vector3(0f,-1f,0f), Quaternion.identity) as GameObject;
					g.GetComponent<SCMissile>().Initiate( ae[Random.Range(0,ae.Length)].transform ,0f);
					
					GameObject g2=Instantiate(bullets[2], shotSpawn.position, Quaternion.identity) as GameObject;
					g2.GetComponent<SCMissile>().Initiate( ae[Random.Range(0,ae.Length)].transform ,0.375f);
				}
				else
				{
					Debug.Log("No enemy on screen, FIRE for the glory");
					g = Instantiate(bullets[2], shotSpawn.position, Quaternion.identity) as GameObject;
					g.GetComponent<SCMissile>().Initiate( GameObject.Find ("Fakesteroid").transform,0f );
				}
			}
			break;
		case 8: // Triple Laser+ triple Missile
			
			Instantiate(bullets[3], shotSpawn.position, Quaternion.identity);
			Instantiate(bullets[3], shotSpawn1.position, Quaternion.identity);
			Instantiate(bullets[3], shotSpawn2.position, Quaternion.identity);
			if(Time.time>nextMissile){
				nextMissile = Time.time + 0.5f;
				// Sceglie il più vicino dei nemici a schermo
				nemicoVicino = GetNearestEnemy(); 
				
				if(nemicoVicino)
				{
					g=Instantiate(bullets[2], shotSpawn.position, Quaternion.identity) as GameObject;
					g.GetComponent<SCMissile>().Initiate( nemicoVicino.transform,0f );
					
					GameObject g2=Instantiate(bullets[2], shotSpawn1.position, Quaternion.identity) as GameObject;
					g2.GetComponent<SCMissile>().Initiate( nemicoVicino.transform,0.125f );
					
					GameObject g3=Instantiate(bullets[2], shotSpawn2.position, Quaternion.identity) as GameObject;
					g3.GetComponent<SCMissile>().Initiate( nemicoVicino.transform,0.25f );
				}
				else
				{
					Debug.Log("No enemy on screen, fire for the glory");
					g=Instantiate(bullets[2], shotSpawn.position, Quaternion.identity) as GameObject;
					g.GetComponent<SCMissile>().Initiate( GameObject.Find ("Fakesteroid").transform,0f );
				}
			}
			break;
		}
		if (AidRightGO) AidRightGO.SendMessage("Shoot",this);
		if (AidLeftGO) AidLeftGO.SendMessage("Shoot",this);
		
		GetComponent<AudioSource>().Play ();
	}

	void OnGUI()
	{
		//GUI.skin = miaSkin;
		int player2OffsetX = 480; // offset per disegnare le etichette valori energia, scudi arma per il player2 a destra
		GUI.depth = -1;
		if(this.name=="Player") player2OffsetX = 0;

		GUI.Label (new Rect (130+player2OffsetX, Screen.height-48, 200, 20), "health:"+health);
		GUI.Label (new Rect (130+player2OffsetX, Screen.height-34, 200, 20), "shields:"+shieldsHP);
		GUI.Label (new Rect (130+player2OffsetX, Screen.height-20, 200, 20), "speed:"+speed);

		if(this.name=="Player"){
			// player da sinistra verso destra
			for(int i=0;i<numOfBombs;++i)
			{
				GUI.DrawTexture(new Rect(5+i*28,Screen.height-72,28,28),bombIcon);
			}
		}
		else{
			// player2 da destra riempie verso sinistra
			for(int i=0;i<numOfBombs;++i)
			{
				GUI.DrawTexture(new Rect((Screen.height-5)-i*28,Screen.height-72,28,28),bombIcon);
			}
		}
	}

	
	void HitByMissile(Transform hittingMissile){


		DecreaseHealth(1);
		if (hittingMissile.GetComponent<SCMissile>().hitEffect != null) Instantiate(hittingMissile.GetComponent<SCMissile>().hitEffect, transform.position, transform.rotation);

		if(health == 0 )
		{
			gameController.ShakeTheCamera();
			if(gameController.isTwoPlayersGame)
			{
				gameObject.SetActive(false);
				GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
				if(players.Length==0) gameController.GameOver();
				//if(players[0].gameObject.activeSelf==false && players[1].gameObject.activeSelf==false) gameController.GameOver();
			}
			else{
				gameController.GameOver();
			}
			Destroy (hittingMissile.gameObject);

		}
		else
		{
			gameController.MicroshakeTheCamera();
			GetComponent<Rigidbody>().AddExplosionForce (5000f,transform.position+new Vector3(0.25f,0,1f), 50f,1f,ForceMode.Acceleration);
			//other.rigidbody.AddExplosionForce (500f,other.transform.position+new Vector3(1f,0,0), 10f,1f,ForceMode.VelocityChange);
			Destroy (hittingMissile.gameObject);
			
		}

		//if (playerExplosion != null) Instantiate(playerExplosion, other.transform.position, other.transform.rotation);

	}

	/// <summary>
	/// Sets the current ship info.
	/// </summary>
	void SetShipInfo( ShipInfo shipdata )
	{
		baseSpeed = shipdata.speed;
		tilt = shipdata.tilt;
		maxNumOfBombs = shipdata.maxNumOfBombs;
		//this.GetComponent<SphereCollider>().radius = shipdata.colliderRadius;
		speed = Mathf.Clamp (baseSpeed + speedBonus, 1,maxSpeed);
	}
	
	/// <summary>
	/// Resets the info.
	/// </summary>
	public void ResetInfo()
	{
		Debug.LogWarning("RESETTO LE INFO PER UNA NUOVA PARTITA");

		GetComponent<Renderer>().enabled = true; // ship 1 show
		shipLevel1.SetActive(false); // enabling ship 1
		shipLevel3.SetActive(false); // ship 3 disactivate
		this.GetComponent<CapsuleCollider>().enabled = false; // special capsule collider disabling
		GetComponent<Collider>().enabled = true; // enable base collider (good for 1 and 2)

		speedBonus = 0f;

		// restore first ship metadata

		SetShipInfo (ship1data); // speed, tilt, and max num of bombs

		score = 0;
		health = startHealth;
		maxHealth = startHealth * 2;
		fireRate = 0.5f;
		numOfBombs = 3;
		livelloArma = 0;

		// Resetto gli scudi
		Shield.GetComponent<Renderer>().enabled = false;
		Shield.GetComponent<Collider>().enabled = false;
		shieldsHP = 0;

		// Assegno il giusto tipo di controlli input per i giocatori, assegnati nel menu opzioni
		if(this.name=="Player")
		{
			playerCtrl = PlayerPrefs.GetInt ("player1ctrl");
		}
		else
		{
			playerCtrl = PlayerPrefs.GetInt ("player2ctrl");
		}

	}

	public void SetPlayerInfo ( )
	{
		if(appCtrl){
			Debug.LogWarning("APPLICO LE INFO PER UN NEXT LEVEL O PER UN CONTINUE",this);

			score = appCtrl.player1info.score;
			health = appCtrl.player1info.health;

			//SetScore ( appCtrl.player1info.score );
			//SetSpeed (appCtrl.player1info.speed  );
			//SetHealth ( appCtrl.player1info.health );
			SetLivelloArma ( appCtrl.player1info.livelloArma );
			SetShieldsEnabled ( appCtrl.player1info.shieldActive );
			SetupSupportShips ();

			speed = appCtrl.player1info.speed;

		}

		// Assegno il giusto tipo di controlli input per i giocatori, assegnati nel menu opzioni
		if(this.name=="Player")
		{
			playerCtrl = PlayerPrefs.GetInt ("player1ctrl");
		}
		else
		{
			playerCtrl = PlayerPrefs.GetInt ("player2ctrl");
		}
	}

	/// <summary>
	/// Gets the nearest enemy.
	/// </summary>
	/// <returns>The nearest enemy.</returns>
	public GameObject GetNearestEnemy()
	{
		float distanzaNemico;
		float ultimaDistanza=1000f;

		//GameObject nemico = new GameObject(); <---- ERRORE! Questa istruzione crea un nuovo game object in scena!

		GameObject nemico = null as GameObject;
		GameObject[] nemiciAttivi = GameObject.FindGameObjectsWithTag("Enemy");
		int totaleNemiciAttivi = nemiciAttivi.Length;
		//if(Debug.isDebugBuild) Debug.Log ("Nemici a video: " + totaleNemiciAttivi);

		foreach( GameObject nemicoAttivo in nemiciAttivi)
		{
			distanzaNemico = Vector3.Distance( transform.position, nemicoAttivo.transform.position );
			if(distanzaNemico<ultimaDistanza)
			{
				nemico = nemicoAttivo;
				ultimaDistanza = distanzaNemico;
			}
		}
		if (nemico == null || nemico.transform.position == Vector3.zero) {
			nemico = fakesteroid;
		}

		return nemico;
	}

	/// Aumenta il livello arma, e cambia nave di conseguenza
	public void SetLivelloArma (int valore)
	{
		if(valore>livelloArma) livelloArma = valore;
		//fireRate = 0.25f*livelloArma;

		GetComponent<AudioSource>().clip = weaponsAudio[livelloArma-1];

		switch(livelloArma)
		{
			// plasma gun
		case 1:
			fireRate = 0.25f;
			SetShipInfo (ship1data);
			break;
		case 2:
			fireRate = 0.2f;
			SetShipInfo (ship1data);
			break;
			// laser gun
		case 3:
			fireRate = 0.2f;
			GetComponent<Renderer>().enabled = true;
			shipLevel1.SetActive(true);
			shipLevel1.GetComponent<Animation>().Play ();
			SetShipInfo( ship2data );
			break;
		case 4:
			fireRate = 0.175f;
			GetComponent<Renderer>().enabled = true;
			shipLevel1.SetActive(true);
			shipLevel1.GetComponent<Animation>().Play ();
			SetShipInfo( ship2data );
			break;
		case 5:
			fireRate = 0.15f;//1.25f-Mathf.Clamp01(0.25f*livelloArma);
			GetComponent<Renderer>().enabled = true;
			shipLevel1.SetActive(true);
			shipLevel1.GetComponent<Animation>().Play ();
			SetShipInfo( ship2data );
			break;
		case 6:
			fireRate = 0.15f;
			//renderer.enabled = false;
			GetComponent<Collider>().enabled = false;
			shipLevel3.SetActive(true);
			shipLevel1.SetActive(false);
			GetComponent<CapsuleCollider>().enabled = true;
			SetShipInfo( ship3data );
			break;
		case 7:
			fireRate = 0.15f;
			GetComponent<Collider>().enabled = false;
			shipLevel3.SetActive(true);
			shipLevel1.SetActive(false);
			GetComponent<CapsuleCollider>().enabled = true;
			SetShipInfo( ship3data );
			break;
		case 8:
			fireRate = 0.15f;
			GetComponent<Collider>().enabled = false;
			shipLevel3.SetActive(true);
			shipLevel1.SetActive(false);
			GetComponent<CapsuleCollider>().enabled = true;
			SetShipInfo( ship3data );
			break;
		}

	}
	// Supporters active getters
	public bool GetSupporterL_Active(){ if(AidLeftGO) return true; else return false; }
	public bool GetSupporterR_Active(){	if(AidRightGO) return true; else return false; }

	/// Adds (show) the shield.
	public void AddShield()
	{
		shieldsHP = 5;
		Shield.GetComponent<Renderer>().enabled = true;
		Shield.GetComponent<Collider>().enabled = true;
		Shield.transform.localScale = new Vector3(4f,4f,4f);
	}

	/// Sets the shields enabled.
	public void SetShieldsEnabled(bool flag)
	{
		Shield.GetComponent<Renderer>().enabled = flag;
		Shield.GetComponent<Collider>().enabled = flag;
		if(flag){
			Shield.transform.localScale = new Vector3(4f,4f,4f);
			shieldsHP = 5;
		}
		else
		{
			shieldsHP = 0;
		}
	}
	public void SetupSupportShips()
	{
		if( appCtrl ){
			if( appCtrl.player1info.supporterL_Active ){
				AidLeftGO = Instantiate (supporter, supporterSpawn.position, supporterSpawn.rotation) as GameObject; 
				AidLeftGO.transform.parent = transform;
			}
			if( appCtrl.player1info.supporterR_Active ){
				AidRightGO = Instantiate (supporterR, supporterSpawn2.position, supporterSpawn2.rotation) as GameObject; 
				AidRightGO.transform.parent = transform;
			}
		}
	}

	/// Adds the speed considering the bonus
	public void AddSpeed(float incremento)
	{	

		if ((speedBonus + incremento) < maxSpeed) speedBonus+=incremento;
		speed = baseSpeed + speedBonus;
		
		//if (speed > maxSpeed) speed -= incremento; 
	}
	
	/// Adds one bomb.
	public void AddBomb(){
		if(numOfBombs < maxNumOfBombs) ++numOfBombs;
	}

	/// Adds the supporter ship if none, or just 1 present.
	public void AddSupporter()
	{
		if (!AidRightGO && !AidLeftGO) {
			AidLeftGO = Instantiate (supporter, supporterSpawn.position, supporterSpawn.rotation) as GameObject; 
			AidLeftGO.transform.parent = transform;
		} else if (!AidRightGO && AidLeftGO) {
			AidRightGO = Instantiate (supporterR, supporterSpawn2.position, supporterSpawn2.rotation) as GameObject; 
			AidRightGO.transform.parent = transform;
		} else if (AidRightGO && !AidLeftGO) {
			AidLeftGO = Instantiate (supporter, supporterSpawn.position, supporterSpawn.rotation) as GameObject; 
			AidLeftGO.transform.parent = transform;
		} else {
			Debug.Log ("Supporter Ships both present!");
		}

	}
	
	/// Adds the smoke.
	public void AddSmoke()
	{
		if(health==3){
			flame1 = Instantiate(Smoke, shotSpawn1.transform.position, shotSpawn1.transform.rotation) as GameObject;
			flame1.transform.parent = this.transform;
		}
		else if(health==2){
			flame2 = Instantiate(Smoke, shotSpawn2.transform.position, shotSpawn2.transform.rotation) as GameObject;
			flame2.transform.parent = this.transform;
		}
		else if(health==1)
		{
			flame3 = Instantiate(Smoke, shieldSpawn.transform.position, shieldSpawn.transform.rotation) as GameObject;
			flame3.transform.parent = this.transform;
		}

	}
	/// <summary>
	/// Decreases the health.
	/// </summary> 
	/// <param name="val">Value.</param>
	public void DecreaseHealth( int val ){	
		health -= val;
	}
	/// <summary>
	/// Gets the health.
	/// </summary>
	/// <returns>The health.</returns>
	public float GetSpeed (){return speed;}
	public int GetHealth (){return health;}
	public int GetMaxNumOfBombs (){return maxNumOfBombs;}
	public int GetNumOfBombs () {return numOfBombs;}

	public void AddScore (int addscore){score+=addscore;}
	public int GetScore (){return score;}
	/// Get livello the arma.
	public int GetlivelloArma (){return livelloArma;}

	// Setta il punteggio del player
	public void SetScore (int punteggioDaSettare) { score = punteggioDaSettare; }
	// Setta la velocità del player
	public void SetSpeed (float speedDaSettare)	{ speed = speedDaSettare; }
	// Setta il punteggio del player
	public void SetHealth (int saluteDaSettare)	{ health = saluteDaSettare; }

	/// Adds one life unit to health.
	public void AddLife()	{		
		
		if(health<maxHealth)
			++health;	 
		
		/*if(health==4)
		{
			if(flame1){
				flame1.transform.parent = null;
				Destroy(flame1);
			}
		}
		else if(health==3)
		{
			if(flame2)
			{
				flame2.transform.parent  = null;
				Destroy(flame2);
			}
		}
		else if(health==2)
		{
			if(flame3){
				flame3.transform.parent  = null;
				Destroy(flame3);
			}
		}*/
	}

	void OnDestroy()
	{
		Debug.LogWarning("UAAARGH: "+this.name+" Destroyed, application has been quit, or changed level or a bug!",this);
	}
}
