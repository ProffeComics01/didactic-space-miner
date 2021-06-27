/// GameController and highscore class, for highscore (offline AND online) management
/// Written by Tommaso Lintrami and the class ;-) 2014 International School Of Comics
/// 
/// Indico la lista dei namespace che mi servono:
/// UnityEngine, naturalmente, per accedere alle classi "MonoBehaviour"
/// System, per le liste IComparable che ci servono per ordinare i punteggi.
/// System.Collections per poter usare le Coroutines, e System.Collections.Generic, per poter accedere alle <List>
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Hazard
{
	public GameObject obj;
	public float delay = 1;
	public float xOffset;
}

[Serializable]
public class Wave
{
	public GameObject WaveBoss;
	public float nextWaveDelay;
	public Hazard[] hazards;

	//private List<GameObject> unitObjectList = new List<GameObject> ();
	private List<GameObject> activeUnitList = new List<GameObject> ();
	private bool cleared=false;
	private bool spawned=false;

	public bool IsCleared(){ return cleared;}
	public void Cleared(){	cleared=true;}
	public bool IsAllSpawned(){ return spawned;}
	public void Spawned(){	spawned=true;}

	// setta/getta la lista nemici attivi
	//void SetList(List<GameObject> list){unitObjectList=list;}
	//List<GameObject> GetList(){	return unitObjectList;}
	
	private int activeUnitCount=0;
	//private GameObject[] activeUnitList=new GameObject[count];
	
	public void ResetActiveList(){
		activeUnitList=new List<GameObject> ();
		activeUnitCount=0;
		cleared=false;
		spawned=false;
	}

	public List<GameObject> GetList(){
		return activeUnitList;
	}

	// Called at spawn
	public void NewUnit(GameObject obj, int id){
		//activeUnitList[id]=obj;
		activeUnitList.Add(obj);
		++activeUnitCount;
		Debug.Log("new unit#"+id+" added, new activeUnitCount:"+activeUnitCount);
	}

	public void AddUnitCount(int t){
		activeUnitCount += t;
	}

	public int GetActiveCount(){return activeUnitCount;}

	public bool CheckList(){
		bool flag=(activeUnitCount>0) ? false:true;
		return flag;
	}
	
	// funzione checklist che periodicamente controlla che la lista di nemici non sia ancora stata svuotata (distrutta)
	/*bool CheckList(){

		bool flag=false;

		for(int i=0; i<unitObjectList.Count; i++){
			if(unitObjectList[i]==null) {
				unitObjectList.RemoveAt(i);
				i-=1;
			}
		}
		if(unitObjectList.Count==0){
			flag=true;
		}
		return flag;
	}*/
}

// high scores table iComparable class. this type is used by the List of this type to order the scores
public class highscore : IComparable<highscore>
{	
	// il costruttore della classe (opzionale)
	public highscore(string playerName, int theScore)
	{
		this.score = theScore;
		this.name = playerName;
	}
	// i membri della classe
	public string name;
	public int score;
	// un metodo della classe (che noi non useremo, per ora)
	public int CompareTo(highscore other)
	{
		return this.score.CompareTo(other.score);
	}

}

/// <summary>
/// SC game controller.
/// </summary>
public class SCGameController : MonoBehaviour
{
	// Variabili publiche esposte nell'inspector
	// La GUI skin che voglio usare per tutti i comandi GUI invocati da questa classe
	public GUISkin miaSkin;

	public Wave[] waves;
	public VortexEffect VortexControl;
    public GameObject MainUIWindow;
    // Il boss di fine livello
    public GameObject theBoss;
	private GameObject theBigBossObj;
	// lista dei pericoli (oggetti e navi nemiche e miniboss) che possono essere "spawnati"
	public GameObject[] hazards;
	// lista dei powerups esistenti
	public GameObject[] powerUps;
	// Una lista di possibili clip di musica di fondo
	public AudioClip[] MusicBackgrounds;
	// I valori di spawn usati per rendere casuale il punto di partenza dei nemici
	public Vector3 spawnValues;
	// l'attesa all'avvio della partita (in secondi)
	public float startWait;

	// punteggio, frase di restart, scrittona GAME OVER, e punteggio migliore
	public Text scoreText;
	public Text score2Text;
	public Text hiScoreText;
	public Text networkText;
	public Text gameOverText;
	public Text titleText;

	// animazioni varie, scenario, boss iniziale, stazione(nascosta per ora)
	//public GameObject SceneryAnim;
	//public GameObject BossAnim;
	//public GameObject StationAnim;

	// Il nostro caro giocatore (1)
	public SCPlayerController thePlayer;
	// Il nostro caro giocatore (2)
	public SCPlayerController thePlayer2;  // faremo più avanti !

	// enum che elenca i possibili stati di gioco, menuStart, menuOpzioni, gameOver, hiscore table e in gioco(playTime)
	enum GameStates
	{
		menuStart = 0,
		menuOptions =1,
		gameOver =2,
		hiScoreTable =3,
		playTime =4,
		warpTime = 5,
		loadTime = 6
	}

	// booleane che indicano il tipo di gioco
	private bool isNetworkGame;
	[HideInInspector] public bool isTwoPlayersGame;
	private string addressField = "127.0.0.1";
	// stato di gioco
	private int GameState = 0;
	// online hi scores stuff, indirizzi URL, e chiave segreta
	private string secretKey = "SCSKBwuahahahahahah!"; // Edit this value and make sure it's the same as the one stored on the server
	private string addScoreURL = "http://www.litobyte.com/highscores/AddScore.php?"; //be sure to add a ? to your url
	private string highscoreURL = "http://www.litobyte.com/highscores/ShowScores.php?"; //be sure to add a ? to your url

	private int hiscore; // highest score to display on the top middle

	// Alloco la memoria per due liste dinamiche di tipo <highscore> che è una classe di tipo IComparable (vedi qualsiasi guida .net per il significato di IComparable)
	private List<highscore> hiScores = new List<highscore> ();
	private List<highscore> onlineHiscores = new List<highscore> ();

	// Alloco la memoria per una lista dinamica di nemici attivi di una stessa ondata
	private List<GameObject> nemiciAttiviOndata = new List<GameObject> ();

	// [obsolete] espongo nell'inspector publicamente il totale numero di ondate per ogni "livello", supponendo che ne avremo altri con scenari e nemici diversi (!?)
	// ora setto il numero ondate dalla larghezza della nuova struttura
	private int numeroOndateLivello;
	// Globale privata, numero intero che contiene il numero ondata corrente
	private int numeroOndataCorrente;
	
	// global che contiene il nome del giocatore, viene ricaricato da disco, se non trova nulla di salvato HasKey(), attribuisce: "anonymous" al nome giocatore
	private string playerName;
	private string playerName2;
	private string editingPlayerName;
	private string editingPlayerName2;


	// new GUI varaiables
	private Rect windowRect0= new Rect(100, 120, 600, 400);
	private Rect windowRect1= new Rect(200, 120, 400, 400);
	private Vector2 scrollPosition0,scrollPosition1;

	// map the camera effect we want to use dinamically
	private ColorCorrectionEffect correzioneColore;
	private bool pausedFlag;
	private int localLivelloCorrente;

	private ApplicationController appCtrl;
	private float multipw,multiph;
    private Dictionary<string, string> headerInfo;

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += LevelWasLoaded;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= LevelWasLoaded;
    }

    // When level starts
    void Start ()
	{
		GameObject appCtrlGO = GameObject.Find ("ApplicationController");
		if (appCtrlGO)
		{
			appCtrl = appCtrlGO.GetComponent<ApplicationController> ();
			localLivelloCorrente = appCtrl.GetIndiceLivello();
			GetComponent<AudioSource>().volume=appCtrl.MusicLevel;
			multipw = appCtrl.Multipw;
			multiph = appCtrl.Multiph;
		}
		else
		{
			localLivelloCorrente = Application.loadedLevel;
			GetComponent<AudioSource>().volume=1f;
		}
        //Debug.Log ("newscreen:" +600*multipw+"x"+400*multiph );

        headerInfo = new Dictionary<string, string>
        {
            { "User-Agent", "LitobyteClient" }
        };

        windowRect0 = new Rect(100*multipw, 120*multiph, 600*multipw, 400*multiph);
		windowRect1= new Rect(200*multipw, 120*multiph, 400*multipw, 400*multiph);		
		GetComponent<AudioSource>().ignoreListenerVolume = true;


		// rimette a zero la chiave di registro dei punteggi online per cercare un refresh 
		PlayerPrefs.SetInt ("punteggiScaricati",0 ); // setta a 1 la chiave di registro per questa sessione
		if(PlayerPrefs.HasKey("player2Name"))
			playerName2=PlayerPrefs.GetString ("player2Name");
		else
			playerName2="defaultplayer2";
		if(PlayerPrefs.HasKey("playerName"))
			playerName=PlayerPrefs.GetString ("playerName");
		else
			playerName="defaultplayer";
		editingPlayerName2=playerName2;
		editingPlayerName=playerName;


		player1controls = PlayerPrefs.HasKey("player1ctrl") ? PlayerPrefs.GetInt ("player1ctrl"):0;
		player2controls = PlayerPrefs.HasKey("player2ctrl") ? PlayerPrefs.GetInt ("player2ctrl"):1;


		// Punteggi Locali (offline)
		hiscore=PlayerPrefs.GetInt ("hiscore");
		UpdateHiScoreScreen();
		UpdateScoreScreen ();

		// read in the first 10 scores
		for(int slotNum=0;slotNum<10;++slotNum){

			highscore hs = new highscore(PlayerPrefs.GetString (slotNum+"HScoreName"), PlayerPrefs.GetInt (slotNum+"HScore") );
			//hs.score = PlayerPrefs.GetInt ("hiscore"+slotNum);
			//hs.name = PlayerPrefs.GetString ("hiscorename"+slotNum);
			hiScores.Add ( hs );
		}
		//hiScores.Clear(); // pulisce una lista riportandola a null (zero elementi nella lista)
		//int quanti = hiScores.Count; // numero di elementi nella lista
		hiScores.Sort ();
		hiScores.Reverse ();


		numeroOndateLivello = waves.Length;
	}
	
	/// Update this instance each engine cycle
	void Update ()
	{
		if (GameState == (int)GameStates.playTime)
		{
            UpdateScoreScreen();
            UpdateHiScoreScreen();

            // gestione tasto ESC (tasto P) (pausa)
            if ( Input.GetKeyUp (KeyCode.Escape ) || Input.GetKeyUp (KeyCode.P ) ) {
				pausedFlag = !pausedFlag;
				if(pausedFlag)
				{
					Time.timeScale = 0f;
					//Screen.lockCursor = false;
					Cursor.visible = true;
					titleText.gameObject.SetActive (true);
				}
				else
				{
					Time.timeScale = 1f;
					//Screen.lockCursor = true;
					Cursor.visible = false;
					titleText.gameObject.SetActive (false);
				}
				
			}

		}

		// Cheats !
		if ( Debug.isDebugBuild && Application.isEditor )
		{
			if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnSelectedPowerUp (0);
			if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnSelectedPowerUp (1);
			if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnSelectedPowerUp (10);
			if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnSelectedPowerUp (11);
			if (Input.GetKeyDown(KeyCode.Alpha5)) SpawnSelectedPowerUp (12);
			if (Input.GetKeyUp(KeyCode.Alpha6)) SpawnSelectedPowerUp (5);
			if (Input.GetKeyUp(KeyCode.Alpha7)) SpawnSelectedPowerUp (6);
			if (Input.GetKeyUp(KeyCode.Alpha8)) SpawnSelectedPowerUp (7);
			if (Input.GetKeyUp(KeyCode.Alpha9)) SpawnSelectedPowerUp (8);
			if (Input.GetKeyUp(KeyCode.Alpha0)) SpawnSelectedPowerUp (9);
		}

		/*if (GameState == (int)GameStates.hiScoreTable)
		{
			if (Input.GetKeyDown (KeyCode.R) || Input.GetKeyDown (KeyCode.KeypadEnter) || Input.GetKeyDown (KeyCode.Return))
			{
				Application.LoadLevel (Application.loadedLevel);
			}
		}*/

		if (GameState == (int)GameStates.gameOver)
		{
			if (Input.GetKeyDown (KeyCode.Escape))
			{
				// Cambio stato a "hiScores" per non ripetere questa parte di codice 
				// e passare alla visualizzazione dei punteggi, non appena avrà caricato da internet
				GameState=(int)GameStates.hiScoreTable;
			}
		}

	}

	/// <summary>
	/// Spawns the whole level design list of waves and bosses.
	/// </summary>
	IEnumerator SpawnLevel ()
	{
		Vector3 spawnPosition;// = new Vector3 (UnityEngine.Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
		GameObject theHazard;
		
		yield return new WaitForSeconds (startWait);

		while (numeroOndataCorrente<numeroOndateLivello)
		{
			yield return new WaitForSeconds (waves[numeroOndataCorrente].nextWaveDelay);

			gameOverText.text = ("Wave: " + (numeroOndataCorrente + 1));		//reset the active unit list for this wave
			yield return new WaitForSeconds (2f); 								// show 2 seconds the Wave message
			gameOverText.text = "";												// reset the GUI text to display nothing.

			waves[numeroOndataCorrente].ResetActiveList();
			int currentSpawnCount=0;

			while(currentSpawnCount < waves[numeroOndataCorrente].hazards.Length)
			{
				yield return new WaitForSeconds ( waves[numeroOndataCorrente].hazards[currentSpawnCount].delay );

				spawnPosition = new Vector3 (waves[numeroOndataCorrente].hazards[currentSpawnCount].xOffset, spawnValues.y, spawnValues.z);
				theHazard = Instantiate ( waves[numeroOndataCorrente].hazards[currentSpawnCount].obj, spawnPosition, Quaternion.identity ) as GameObject;
				waves[numeroOndataCorrente].NewUnit( theHazard, currentSpawnCount );
				theHazard.GetComponent<Done_DestroyByContact>().SetWaveID(numeroOndataCorrente);

				++currentSpawnCount;
			}
			waves[numeroOndataCorrente].Spawned();

			yield return new WaitForSeconds (2f); // show 2 seconds "perfect" if is to, and waitfor the boss
			gameOverText.text = "";

			spawnPosition = new Vector3 (UnityEngine.Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
			Instantiate (waves[numeroOndataCorrente].WaveBoss, spawnPosition, Quaternion.identity );

			++numeroOndataCorrente;

			if(numeroOndataCorrente==numeroOndateLivello)
			{
				yield return new WaitForSeconds (startWait);
				spawnPosition = new Vector3 (0f, spawnValues.y, spawnValues.z-4f);
				theBigBossObj = Instantiate ( theBoss, spawnPosition, Quaternion.identity ) as GameObject;
				gameOverText.text = "";
				bigBossOnScreen = true;
				GetComponent<AudioSource>().Stop();
				GetComponent<AudioSource>().clip = MusicBackgrounds[2];
				GetComponent<AudioSource>().Play();
			}

		}

		// finchè il boss fine livello è a schermo
		while (bigBossOnScreen)
		{
			yield return new WaitForSeconds (1f);


			if(theBigBossObj==null)
			{
				VortexControl.enabled = true;
				bigBossOnScreen = false;
				GetComponent<AudioSource>().Stop();
				if(appCtrl) appCtrl.GetComponent<AudioSource>().PlayOneShot( MusicBackgrounds[3],2f );//if(appCtrl) appCtrl.audio.PlayDelayed( MusicBackgrounds[3],2f );
				GetComponent<AudioSource>().clip = MusicBackgrounds[0];
				GetComponent<AudioSource>().PlayDelayed(1f);
			}
		}

		// Vado in warp game state
		GameState = (int)GameStates.warpTime;

		// Ripulisco la scena per evitare distruzione del player durante il warp
		SceneCleanUp();

		// Durante il "warp" time
		while ( GameState == (int)GameStates.warpTime )
		{
			if(thePlayer.GetComponent<Rigidbody>().position.z > 6f) GameState = (int)GameStates.loadTime;
			Debug.Log ("SONO A: "+thePlayer.GetComponent<Rigidbody>().position.z);
			VortexControl.angle = Mathf.Clamp ( 60*thePlayer.GetComponent<Rigidbody>().position.z,0,360);
			yield return new WaitForSeconds (0.001f);
		}


		++localLivelloCorrente;
		if(appCtrl) appCtrl.SetIndiceLivello (localLivelloCorrente);

		if (isTwoPlayersGame) {

			// Player one
			appCtrl.player1info.score = thePlayer.GetScore();
			appCtrl.player1info.health = thePlayer.GetHealth();
			appCtrl.player1info.livelloArma = thePlayer.GetlivelloArma();
			appCtrl.player1info.numOfBombs = thePlayer.GetNumOfBombs();
			appCtrl.player1info.maxNumOfBombs = thePlayer.GetMaxNumOfBombs();
			appCtrl.player1info.speed = thePlayer.GetSpeed();
			appCtrl.player1info.shieldActive = thePlayer.Shield.GetComponent<Renderer>();
			appCtrl.player1info.supporterL_Active = thePlayer.GetSupporterL_Active();
			appCtrl.player1info.supporterR_Active = thePlayer.GetSupporterR_Active();
			
			// Player two
			appCtrl.player2info.score = thePlayer2.GetScore();
			appCtrl.player2info.health = thePlayer2.GetHealth();
			appCtrl.player2info.livelloArma = thePlayer2.GetlivelloArma();
			appCtrl.player2info.numOfBombs = thePlayer2.GetNumOfBombs();
			appCtrl.player2info.maxNumOfBombs = thePlayer2.GetMaxNumOfBombs();
			appCtrl.player2info.speed = thePlayer2.GetSpeed();
			appCtrl.player2info.shieldActive = thePlayer2.Shield.GetComponent<Renderer>();
			appCtrl.player2info.supporterL_Active = thePlayer2.GetSupporterL_Active();
			appCtrl.player2info.supporterR_Active = thePlayer2.GetSupporterR_Active();
			
		}
		else{
			// Sincronizzo le variabili del player controller con le globali di applicazione
			if(appCtrl)
			{

				//Debug.Log ( "Salvo le variabili del player nelle globali appcontroller");

				appCtrl.player1info.score = thePlayer.GetScore();
				appCtrl.player1info.health = thePlayer.GetHealth();
				appCtrl.player1info.livelloArma = thePlayer.GetlivelloArma();
				appCtrl.player1info.numOfBombs = thePlayer.GetNumOfBombs();
				appCtrl.player1info.maxNumOfBombs = thePlayer.GetMaxNumOfBombs();
				appCtrl.player1info.speed = thePlayer.GetSpeed();

				appCtrl.player1info.shieldActive = thePlayer.Shield.GetComponent<Renderer>();
				appCtrl.player1info.supporterL_Active = thePlayer.GetSupporterL_Active();
				appCtrl.player1info.supporterR_Active = thePlayer.GetSupporterR_Active();
			}

		}
		gameOverText.text = "Level "+localLivelloCorrente; // <1--- here will be "Level "+currentLevel;
		Debug.Log ("Dati salvati, score:" + appCtrl.player1info.score+ ", health:" + appCtrl.player1info.health + " Larma:"+ appCtrl.player1info.livelloArma);

		Application.LoadLevel (localLivelloCorrente);
	}

	
	//called whenever a creep is killed, or scores check if his wave it's cleared
	public bool CheckIsWaveCleared(int waveID){

		//reduce the acvitve unit of the corresponding wave
		waves[waveID].AddUnitCount(-1);

		Debug.Log("unit killed, new activeUnitCount:"+waves[waveID].GetActiveCount());

		//if all the units in that wave are spawned and the active unit count is 0, then the wave is cleared
		if(waves[waveID].IsAllSpawned() && waves[waveID].CheckList() ){

			waves[waveID].Cleared();			
			WaveCleared(waveID);

			//AudioManager.PlayWaveClearedSound();
			//gameControlCom.WaveDown();
			//CheckIsAllWaveCleared();

			return true;
		}
		else
		{
			return false;
		}
	}
			
	//check if all the wave in the waveList is cleared
	void CheckIsAllWaveCleared(){
		//nota: only execute if the currentWave is the last wave 
		if(numeroOndataCorrente==waves.Length){
			bool allCleared=true;
			foreach(Wave wave in waves){
				if(!wave.IsCleared()) allCleared=false;
			}
			if(allCleared) AllWavesCleared();
		}
	}

	//called when a wave is cleared
	void WaveCleared(int i){
		
		// Spawn an additional powerup if the wave was interally cleared by the player shoots
		//SpawnPowerUp(spawnPosition);

	}
			
	//called when all the wave in the SpawnManager is cleared
	void AllWavesCleared(){
		// Tutte le ondate sono state ripulite SUPER PERFECT BONUS A FINE LIVELLO!
		//Instantiate (theBoss, Vector3.zero, Quaternion.identity);
		//Debug.Log(this.gameObject.name+": my wave list is cleared !",this);
	}
	

	private bool bigBossOnScreen = false;
	/// <summary>
	/// Coroutine: Spawns the rocks of a level (until the bigBoss is there)
	/// </summary>
	IEnumerator SpawnRocks ()
	{
		Vector3 spawnPosition;
		while (!bigBossOnScreen)
		{

			spawnPosition = new Vector3 (UnityEngine.Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
			int pericolo = UnityEngine.Random.Range (0, 4); // sceglie uno dei primi 5 hazards (asteroidi)
			Instantiate (hazards [pericolo], spawnPosition, Quaternion.identity);
			yield return new WaitForSeconds (1.5f-( (float)localLivelloCorrente/10f) );

		}
	}

	////////////////////////////////////////////////////////////
	/// <summary>
	/// GUI STUFF, MAIN ONGUI CALL AND WINDOW DRAWING FUNCTIONS
	/// </summary>
	////////////////////////////////////////////////////////////
	private bool showOnLineScores;
	private bool sendButtonPressed;
	
    /*
	void OnGUI()
	{
		GUI.depth = 0;
		GUI.skin = miaSkin;

		// Aggiorna anche gli elementi non-UnityGUI GuiText in scena
		UpdateScoreScreen();
		UpdateHiScoreScreen();

		// Gestisce i vari stati di gioco, con relative interfacce utente
		switch( GameState )
		{
			// Menu iniziale
			case (int)GameStates.menuStart:
				windowRect1 = GUI.Window (0, windowRect1, MenuStartWindow, "");// qui è la finestra che si occupa di disegnare i controlli, vedere funzione MenuWindow
			break;
			// Menu opzioni
			case (int)GameStates.menuOptions:
				windowRect1 = GUI.Window (0, windowRect1, MenuOptionsWindow, "");// qui è la finestra che si occupa di disegnare i controlli, vedere funzione MenuOptionsWindow
			break;
			// Gestisce lo stato di gioco "game over", quando il player deve inviare a internet / salvare localmente il punteggio
			case (int)GameStates.gameOver:

				string buttonText = "Save score";
				GUI.BeginGroup( new Rect(100*multipw,150*multiph,600*multipw,400*multiph));
				GUI.Box( new Rect (0,0,600*multipw,400*multiph),"",miaSkin.window);

				if(isTwoPlayersGame)
				{
					GUI.Label (new Rect (120*multipw,56*multiph,300*multipw,32*multiph),"Insert your name Player1"); 
					editingPlayerName = GUI.TextField( new Rect (120*multipw,89*multiph,400*multipw,38*multiph), editingPlayerName, 50 );
					//GUI.TextField( new Rect (120,89,280,38), editingPlayerName, 32 );
					GUI.Label (new Rect (120*multipw,156*multiph,300*multipw,32*multiph),"Insert your name Player2"); 
					editingPlayerName2 = GUI.TextField( new Rect (120*multipw,189*multiph,400*multipw,38*multiph), editingPlayerName2, 50 );
				}
				else{
					GUI.Label (new Rect (120*multipw,136*multiph,300*multipw,32*multiph),"Insert your name Player1"); 
					editingPlayerName = GUI.TextField( new Rect (120*multipw,169*multiph,400*multipw,38*multiph), editingPlayerName, 50 );					
				}
			   
				if(appCtrl)
				{
					if(appCtrl.internetDisponibile)
					{
						buttonText = (isTwoPlayersGame) ? "Send scores" : "Send score";
					}
					else
					{
						buttonText = (isTwoPlayersGame) ? "Save scores" : "Save score";
					}
				}
				else
				{
					buttonText = (isTwoPlayersGame) ? "Save scores" : "Save score";
				}
				
				
				GUI.enabled = !sendButtonPressed;
						if ( GUI.Button (new Rect (400*multipw,276*multiph,100*multipw,40*multiph),"Cancel" ) )
				{
					// Cambio stato a "hiScores" per non ripetere questa parte di codice 
					// e passare alla visualizzazione dei punteggi, non appena avrà caricato da internet
					if(Application.loadedLevel>1)
					{
						Application.LoadLevel(1);
					}
					else
					{
						GameState=(int)GameStates.hiScoreTable;
						gameOverText.text = "";
						titleText.gameObject.SetActive(true);						
					}	
			}
			
						if ( GUI.Button (new Rect (120*multipw,276*multiph,180*multipw,40*multiph),buttonText ) )
				{
					int tmpscore;
					int tmpscore2=0;
					sendButtonPressed = true;
					// recupera il punteggio di player1;
					tmpscore = thePlayer.GetScore();
					// Aggiorna il nome del player nella globale, e salva su disco il cambiamento del giocatore locale
					playerName = editingPlayerName;
					PlayerPrefs.SetString("playerName",editingPlayerName);
					if(isTwoPlayersGame)
					{
						playerName2 = editingPlayerName2;
						PlayerPrefs.SetString("player2Name",editingPlayerName2);
						tmpscore2 = thePlayer2.GetScore();
					}
					// Cambio stato a "hiScores" per non ripetere questa parte di codice 
					// e passare alla visualizzazione dei punteggi, non appena avrà caricato da internet
					// Nota: una buona idea sarà spostare il cambio GameState, a invio punteggio avvenuto!
					
					
					if(appCtrl && appCtrl.internetDisponibile)
					{
						// Salva su disco come record locale(se nei primi 10) e riordina la lista ricreata da zero
						AddScore ( playerName, tmpscore );
						AddScore ( playerName2, tmpscore2 );	
						// Inizio la coroutine che invia i punteggi via internet
						StartCoroutine( PostScores(playerName, tmpscore ) );
						if(isTwoPlayersGame) StartCoroutine( PostScores2(playerName2, tmpscore2 ) );
						networkText.text = "Sending scores to server..";
					}
					else
					{
						// Salva su disco come record locale(se nei primi 10) e riordina la lista ricreata da zero
						AddScore ( playerName,tmpscore );
						if(isTwoPlayersGame)	AddScore ( playerName2, tmpscore2 );
						
						if(Application.loadedLevel>1)
						{
							Application.LoadLevel(1);
						}
						else
						{
							GameState=(int)GameStates.hiScoreTable;
							gameOverText.text = "";
							titleText.gameObject.SetActive(true);						
						}		
						
					}



				}

				GUI.enabled = true;
				GUI.EndGroup();
				//GUI.Label (  new Rect (40,2+inc*36,500,28), ascore.name+"..."+ascore.score);
				//GUI.Label (new Rect (360,236,500,28),"Post your high score to internet"); 
			break;
		
		// Gestisce lo stato "punteggi" dove leggi la tabella punteggi migliori e puoi ricominciare una nuova partita
		case (int)GameStates.hiScoreTable:

			if(appCtrl && appCtrl.internetDisponibile)
			{
				if(showOnLineScores)
				{
					//GUI.Box( new Rect (0,0,600,400),"Online scores");
					windowRect0 = GUI.Window (0, windowRect0, OnlineScoresWindow, "");
				}
				else
				{
					//GUI.Box( new Rect (0,0,600,400),"Local scores");
					windowRect0 = GUI.Window (1, windowRect0, LocalScoresWindow, "");
				}
								GUI.BeginGroup ( new Rect(0,0,100*multipw,100*multiph));
				// End the group we started above. This is very important to remember!
				GUI.EndGroup ();


				if(showOnLineScores)
				{
					if (GUI.Button (  new Rect (372*multipw,512*multiph,300*multipw,40*multiph),"Show Offline Scores") )
					{
						showOnLineScores = false;
					}
				}
				else{
					if (GUI.Button (  new Rect (372*multipw,512*multiph,300*multipw,40*multiph),"Show Online Scores") )
					{
						showOnLineScores = true;
					}
				}
			}
			else
			{
				//GUI.Box( new Rect (0,0,600,400),"(offline)Local scores");
				windowRect0 = GUI.Window (1, windowRect0, LocalScoresWindow, "");
				//now adjust to the group. (0,0) is the topleft corner of the group.
				GUI.BeginGroup ( new Rect(0,0,100*multipw,100*multiph));
				// End the group we started above. This is very important to remember!
				GUI.EndGroup ();

			}
			if (GUI.Button (  new Rect (152*multipw,512*multiph,200*multipw,40*multiph),"MAIN MENU") )
			{
				GameState = (int)GameStates.menuStart;
			}



			break;

		default:
		// Stato rimanente ovvero "in-game" il game controller non stampa nulla a GUI, a meno che... ? (ditelo voi :-P)
			break;
		}

	}
    */

	//////////////////////////////////////
	/// 	Main Menu - start window
	//////////////////////////////////////
	void MenuStartWindow ( int windowID  )
	{
		GUILayout.BeginVertical();
		GUILayout.Label ("All your base are belong to us!", "HeaderText");
		GUILayout.Space(80);
		if(GUILayout.Button ( "START 1P GAME", GUILayout.Height(40*multiph) ) )
		{
			// we fold in a function all the shit
			StartGame(false);
		}
		GUILayout.Space(4);
		if(GUILayout.Button ( "START 2P GAME", GUILayout.Height (40*multiph) ) )
		{
			// we folder in a function all the shit
			StartGameDouble();
		}
	
#if (UNITY_EDITOR_OSX || UNITY_EDITOR || UNITY_STANDALONE || UNITY_STANDALONE_OSX )
		GUILayout.Space(4);
		if(GUILayout.Button ( "HOST GAME", GUILayout.Height (40*multiph) ) )
		{
			//NetworkManager.Host(7777);
			isNetworkGame = false;
			StartGame(true);
			//NetworkManager.LoadLevel("Level2");
		}
#endif
		GUILayout.Space(4);
		GUILayout.BeginHorizontal();
		if(GUILayout.Button ( "JOIN:", GUILayout.Height (40*multiph),GUILayout.MaxWidth (100*multipw) ) )
		{
			if (addressField != "")
			{
				//NetworkManager.Connect(addressField, 7777);
				StartGame(true);
			}

		}
		GUILayout.FlexibleSpace();
		addressField = GUILayout.TextField ( addressField, GUILayout.Height (40*multiph), GUILayout.MinWidth(220*multipw) );
		GUILayout.EndHorizontal();

		GUILayout.Space(4);
		if (GUILayout.Button ( "OPTIONS", GUILayout.Height (40*multiph) ) )
		{
			GameState = (int)GameStates.menuOptions;
		}
		GUILayout.Space(4);
		if (GUILayout.Button ( "SCORES", GUILayout.Height (40*multiph) ) )
		{
			GameState = (int)GameStates.hiScoreTable;
		}
#if (!UNITY_WEBPLAYER && !UNITY_EDITOR_OSX && !UNITY_EDITOR )
		GUILayout.Space(4);
		if (GUILayout.Button ( "QUIT", GUILayout.Height (40*multiph)) )
		{
			Application.Quit();
		}
#endif
		
		GUILayout.EndVertical();		
		// Make the windows be draggable.
		//GUI.DragWindow ( new Rect(0,0,10000,10000));
	}

	private int player1controls=0; // keyboard;
	private int player2controls=1; // mouse;

	//private string[] controls_strings = new string[] { "keyboard", "mouse", "joypad" };
	public GUIContent[] controls_icons;

	////////////////////////////////////////
	/// 		options window
	////////////////////////////////////////
	void MenuOptionsWindow ( int windowID  )
	{
		GUILayout.BeginVertical();

		GUILayout.Label ("All your base are belong to us!", "HeaderText");
		GUILayout.Space(10);
		GUILayout.Label ( "1 PLAYER CONTROLS", GUILayout.Height(32));
		player1controls=GUILayout.SelectionGrid(player1controls,controls_icons,3,"ImgButtons",GUILayout.Height(40));
		/*GUILayout.Space(10);
		GUILayout.Label ( "2 PLAYERS CONTROLS", GUILayout.Height(32));
		player2controls=GUILayout.SelectionGrid(player2controls,controls_icons,3,"ImgButtons",GUILayout.Height(40));*/

		GUILayout.Space(60);
		GUILayout.Label ("Audio levels", "HeaderText");
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.Label(new GUIContent("Sound","ADJUST SOUND FX GLOBAL VOLUME"));
		appCtrl.SoundLevel =  GUILayout.HorizontalSlider( appCtrl.SoundLevel, 0,1f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label(new GUIContent("Music","ADJUST BACKGROUND MUSIC VOLUME") );
		appCtrl.MusicLevel =  GUILayout.HorizontalSlider( appCtrl.MusicLevel, 0,1f);
		GUILayout.EndHorizontal();
		/*if (GUILayout.Button ( "AUDIO OPTIONS", GUILayout.Height(40)) )
		{
			//GameState = (int)GameStates.menuStart;
		}
		if (GUILayout.Button ( "GAME OPTIONS", GUILayout.Height(40)) )
		{
			//GameState = (int)GameStates.menuStart;
		}*/
		if (GUILayout.Button ( "MAIN MENU", GUILayout.Height(40)) )
		{
			GameState = (int)GameStates.menuStart;

		}

		GUILayout.EndVertical();		
		// Make the windows be draggable.
		//GUI.DragWindow ( new Rect(0,0,10000,10000));


		if(GUI.changed){
			
			// Set audio volumes on the Master AudioListener
			AudioListener.volume = appCtrl.SoundLevel;

			// Set audio volumes on the BGM channels
			GetComponent<AudioSource>().volume = appCtrl.MusicLevel;
			//appCtrl.SetChannelsVolume();

			PlayerPrefs.SetInt ("player1ctrl",player1controls);
			PlayerPrefs.SetInt ("player2ctrl",player2controls);
			
			PlayerPrefs.SetFloat("SoundLevel", appCtrl.SoundLevel);
			PlayerPrefs.SetFloat("MusicLevel", appCtrl.MusicLevel);
		}

	}
    public void SetControlPlayerOne(int ctrls)
    {
        player1controls = ctrls;
        SettingsChanged();
    }
    public void SetControlPlayerTwo(int ctrls)
    {
        player2controls = ctrls;
        SettingsChanged();
    }
    void SettingsChanged()
    {
        // Set audio volumes on the Master AudioListener
        AudioListener.volume = appCtrl.SoundLevel;

        // Set audio volumes on the BGM channels
        GetComponent<AudioSource>().volume = appCtrl.MusicLevel;
        //appCtrl.SetChannelsVolume();

        PlayerPrefs.SetInt("player1ctrl", player1controls);
        PlayerPrefs.SetInt("player2ctrl", player2controls);

        PlayerPrefs.SetFloat("SoundLevel", appCtrl.SoundLevel);
        PlayerPrefs.SetFloat("MusicLevel", appCtrl.MusicLevel);
    }
	////////////////////////////////////////
	/// Draw the online scores window.
	////////////////////////////////////////
	void OnlineScoresWindow ( int windowID  )
	{
		GUILayout.BeginVertical();
		//GUILayout.Label("ONLINE HIGHSCORES");
		GUILayout.Label ("sorted by score", "HeaderText");
		scrollPosition0 = GUILayout.BeginScrollView( scrollPosition0, GUILayout.Height (332));

		int inc=0;
		foreach(highscore ascore in onlineHiscores)
		{
			++inc;
			GUILayout.BeginHorizontal();
			GUILayout.Label ( inc+". "+ascore.name, GUILayout.Width(400) );
			GUILayout.FlexibleSpace();
			GUILayout.Label ( " "+ascore.score,"LabelRight" );
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();


		
		
		GUILayout.EndVertical();		
		// Make the windows be draggable.
		//GUI.DragWindow ( new Rect(0,0,10000,10000));
	}

	////////////////////////////////////////
	/// Draw the Locals scores window.
	/// ////////////////////////////////////////
	void LocalScoresWindow ( int windowID  ){	
		GUILayout.BeginVertical();
		//GUILayout.Label("LOCAL HIGHSCORES");
		GUILayout.Label ("sorted by score", "HeaderText");
				
		scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, GUILayout.Height (332));
		int inc=0;
		foreach(highscore ascore in hiScores)
		{
			++inc;
			GUILayout.BeginHorizontal();
			GUILayout.Label ( inc+". "+ascore.name, GUILayout.Width(320) );
			//GUILayout.FlexibleSpace();
			GUILayout.Label ( " "+ascore.score,"LabelRight" );
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();

		GUILayout.EndVertical();
		//GUI.DragWindow ( new Rect(0,0,10000,10000));
	}
	
	/// Starts the game.
	/// Initialize all the variables, setup the first level ship
	public void StartGame(bool restartFlag)
	{
		networkText.text = "";
		gameOverText.text = "";
		isTwoPlayersGame = false;
		titleText.gameObject.SetActive(false);
		numeroOndataCorrente = 0;
		bigBossOnScreen = false;
		//if(restartFlag==false) thePlayer.ResetInfo(); // Qui le strutture del ShipInfo del player  non sono ancora state create!!!
		thePlayer.transform.position = new Vector3 (0f, 0f, -3f);

		//SceneryAnim.animation.Play ();
		//BossAnim.animation.Play ();
		//StationAnim.animation.Play ();

		UpdateScoreScreen ();
		UpdateHiScoreScreen ();

		thePlayer.gameObject.SetActive(true);
		thePlayer.PlayerBars.gameObject.SetActive(true);


		GetComponent<AudioSource>().clip = MusicBackgrounds[0];
		GetComponent<AudioSource>().Play ();
		Screen.lockCursor = true;

		GameState=(int)GameStates.playTime;

		StartCoroutine (SpawnLevel () );
		StartCoroutine (SpawnRocks () );

		if(restartFlag) thePlayer.SetPlayerInfo (); else thePlayer.ResetInfo();

		//StartCoroutine (SpawnWaves ());
	}

	/// <summary>
	/// Starts the game double.
	/// </summary>
	public void StartGameDouble()
	{
        thePlayer.ResetInfo();
        thePlayer2.ResetInfo();

        networkText.text = "";
		gameOverText.text = "";
		titleText.gameObject.SetActive(false);
		numeroOndataCorrente = 0;
		bigBossOnScreen = false;
		isTwoPlayersGame = true;

		//SceneryAnim.animation.Play ();
		//BossAnim.animation.Play ();
		//StationAnim.animation.Play ();
		
		UpdateScoreScreen ();
		UpdateHiScoreScreen ();
		
		thePlayer.gameObject.SetActive(true);
		thePlayer.PlayerBars.gameObject.SetActive(true);
		//thePlayer.ResetInfo();

		if(thePlayer2)
		{
			thePlayer2.gameObject.SetActive(true);
			thePlayer2.PlayerBars.gameObject.SetActive(true);
			//thePlayer2.ResetInfo();
		}
		else
		{
			Debug.LogError("Errore: Il player2 non è presente in scena");
			// To do: togliere il debug logError e instanziare un prefab del player2
		}

		GetComponent<AudioSource>().clip = MusicBackgrounds[0];
		GetComponent<AudioSource>().Play ();
		Screen.lockCursor = true;
		
		GameState=(int)GameStates.playTime;

		thePlayer.transform.position = new Vector3 (-3f, 0f, -3f);
		thePlayer2.transform.position = new Vector3 (3f, 0f, -3f);

		StartCoroutine (SpawnLevel ());
		StartCoroutine (SpawnRocks () );
	}

	/// <summary>
	/// Clean up of the objects(enemies, shots, and so forth) from scene for a restart.
	/// </summary>
	void SceneCleanUp()
	{
		GameObject[] allshots = GameObject.FindGameObjectsWithTag("Shot");
		foreach(GameObject go in allshots)
		{
			Destroy(go);
		}
		GameObject[] allEshots = GameObject.FindGameObjectsWithTag("EnemyShot");
		foreach(GameObject go in allEshots)
		{
			Destroy(go);
		}
		GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
		foreach(GameObject go in allEnemies)
		{
			Destroy(go);
		}
		GameObject[] allSupporters = GameObject.FindGameObjectsWithTag("Supporter");
		foreach(GameObject go in allSupporters)
		{
			Destroy(go);
		}
	}

	void SpawnSelectedPowerUp( int potenziamento )
	{
		GameObject powerup;
		Vector3 pos = new Vector3 (0f, 0f, 16f);

		// assegna il powerup dal indice passato come parametro
		powerup = powerUps [potenziamento];
		// istanzia il powerup alla posizione predefinita
		Instantiate (powerup, pos, powerup.transform.rotation);
	}

	/// Spawns the power up.
	/// Position Vector3 where to spawnthe powerup
	public void SpawnPowerUp( Vector3 pos )
	{
		GameObject powerup;
		int livelloArmaPlayer,livelloArmaPlayer2,livelloArmaConfronto;
		// Considera anche player 2 per spawnare sempre un powerup utile a entrambi

		livelloArmaPlayer = (thePlayer) ? thePlayer.GetlivelloArma () : 0;
		livelloArmaPlayer2 = (thePlayer2) ? thePlayer2.GetlivelloArma () : 0;
		livelloArmaConfronto = (livelloArmaPlayer >= livelloArmaPlayer2) ? livelloArmaPlayer : livelloArmaPlayer2;

		// Vogliamo fare si che le armi gia in possesso vengano escluse, ma che solo la successiva venga instanziata, per cui:
		int potenziamento = UnityEngine.Random.Range (0, powerUps.Length);

		if(potenziamento<8) // è un potenziamento armi ( se il numero di armi cambia cambiare il valore di controllo qui !!!)
		{
			// Operatore ternario, ci risparmia la fatica di scrivere 4 linee di codice e fa esattamente la stessa cosa di qui in basso
			potenziamento = potenziamento > (livelloArmaConfronto-1) ? 
				livelloArmaConfronto : UnityEngine.Random.Range (8, powerUps.Length);

			// versione "basic" dello stesso assegnamento, fatto con gli if else
			if(potenziamento>livelloArmaConfronto-1)
				potenziamento = livelloArmaConfronto;
			else
				potenziamento = UnityEngine.Random.Range (8, powerUps.Length);
		}

		// assegna finalmente il powerup corretto
		powerup = powerUps [potenziamento];
		Instantiate (powerup, pos, powerup.transform.rotation);
	}
	
	// Aggiorna guiText a schermo coi punteggi
	void UpdateScoreScreen ()
	{	
		scoreText.text = "player 1: " + thePlayer.GetScore();
		if(thePlayer2) score2Text.text = "player 2: " + thePlayer2.GetScore();	
	}
	public void UpdateHiScoreScreen ()	
	{ 
		hiScoreText.text = "hi-score: " + hiscore;	
	}
	
	/// Games over. Lanciata quanto il player o entrambi i player(2 giocatori) sono stati distrutti
	public void GameOver ()
	{
		gameOverText.text = "Game Over";

		if(thePlayer.GetScore() > hiscore) hiscore = thePlayer.GetScore();

		if(thePlayer2)
		{
			if(thePlayer2.GetScore()> hiscore && thePlayer2.GetScore()>thePlayer.GetScore() ) hiscore = thePlayer2.GetScore();
		}

		PlayerPrefs.SetInt ("hiscore",hiscore);
		PlayerPrefs.Save ();
		UpdateHiScoreScreen();

		sendButtonPressed = false;
		//SceneryAnim.animation.Stop ();
		//BossAnim.animation.Stop ();
		//StationAnim.animation.Stop ();

		StopAllCoroutines();
		SceneCleanUp();

        // since we stopped all the coroutines, 
        // we want to start again the periodic internet check
        StartCoroutine (appCtrl.CheckPeriodicoInternet () );

        // Vecchio codice che uccideva player
        if (thePlayer && thePlayer.gameObject.activeSelf) thePlayer.gameObject.SetActive(false);
		if(thePlayer2 && thePlayer2.gameObject.activeSelf) thePlayer2.gameObject.SetActive(false);

		GameState = (int)GameStates.gameOver;

        MainUIWindow.SetActive(true);

        GetComponent<AudioSource>().clip = MusicBackgrounds[1];
		GetComponent<AudioSource>().Play ();
		Screen.lockCursor = false;
		Cursor.visible = true;
	}

	// Online/Offline Hiscores stuff
	
	// Cerca di ordinare con un iterazione "furba" i punteggi nei 10 slots offline, viene dal tutorial della pagina che vi ho linkato, 
	// noi non useremo questo metodo, li lasceremo alla rinfusa e li ordineremo solo in visualizzazione attraverso la lista
	void AddScore (string name, int score){
		int newScore;
		string newName;
		int oldScore;
		string oldName;
		newScore = score;
		newName = name;
		for(int i=0;i<10;i++){
			if(PlayerPrefs.HasKey(i+"HScore")){
				if(PlayerPrefs.GetInt(i+"HScore")<newScore){ 
					// new score is higher than the stored score
					oldScore = PlayerPrefs.GetInt(i+"HScore");
					oldName = PlayerPrefs.GetString(i+"HScoreName");
					PlayerPrefs.SetInt(i+"HScore",newScore);
					PlayerPrefs.SetString(i+"HScoreName",newName);
					newScore = oldScore;
					newName = oldName;
				}
				else
				{
					// Non sei entrato nella Top Ten, non salva nessun punteggio locale (solo 10 slots di punteggi offline)
					// Qui potrebbe essere un buon punto per postare online il punteggio se internet è presente, automaticamente
					// Oppure si potrebbe gestire semplicemente nella ONGui()

				}
			}else{

				// Lo slot nel registro non è mai stato creato creato e setta il punteggio
				PlayerPrefs.SetInt(i+"HScore",newScore);
				PlayerPrefs.SetString(i+"HScoreName",newName);
				newScore = 0;
				newName = "";
			}
		}
		// ho finito, ripeto la procedura che popola la Lista hiScores (punteggi locali) per la visualizzazione
		hiScores.Clear ();
		for(int slotNum=0;slotNum<10;++slotNum){
			highscore hs = new highscore(PlayerPrefs.GetString (slotNum+"HScoreName"), PlayerPrefs.GetInt (slotNum+"HScore") );
			//hs.score = PlayerPrefs.GetInt ("hiscore"+slotNum);
			//hs.name = PlayerPrefs.GetString ("hiscorename"+slotNum);
			hiScores.Add ( hs );
		}
		//hiScores.Clear(); // pulisce una lista riportandola a null (zero elementi nella lista)
		//int quanti = hiScores.Count; // numero di elementi nella lista
		hiScores.Sort ();
		hiScores.Reverse ();
	}

	// Coroutines methods

	// Online scores stuff
	// remember to use StartCoroutine when calling this function!
	IEnumerator PostScores(string name, int score)
	{
		//This connects to a server side php script that will add the name and score to a MySQL DB.
		// Supply it with a string representing the players name and the players score.
		string hash = Tools.Md5Sum(name + score + secretKey);
		string post_url = addScoreURL + "name=" + WWW.EscapeURL(name) + "&score=" + score + "&hash=" + hash;
		
		// Post the URL to the site and create a download object to get the result.
		WWW hs_post = new WWW(post_url, null, headerInfo);
		yield return hs_post; // Wait until the download is done

		if (hs_post.error != null)
		{
			networkText.text ="error posting the high score";
			print("There was an error posting the high score: " + hs_post.error);
		}
		else
		{
			networkText.text ="";
		}
		hs_post = null;

		if(Application.loadedLevel>1)
		{
			Application.LoadLevel(1);
		}
		else
		{
			GameState=(int)GameStates.hiScoreTable;
			gameOverText.text = "";
			titleText.gameObject.SetActive(true);
			yield return new WaitForSeconds(3f);			
			GetOnLineScores_callsub();
		}
	}

	// Online scores stuff
	// remember to use StartCoroutine when calling this function!
	IEnumerator PostScores2(string name, int score)
	{
		yield return new WaitForSeconds(2f);
		//This connects to a server side php script that will add the name and score to a MySQL DB.
		// Supply it with a string representing the players name and the players score.

		string hash = Tools.Md5Sum(name + score + secretKey);

		string post_url = addScoreURL + "name=" + WWW.EscapeURL(name) + "&score=" + score + "&hash=" + hash;
		
		// Post the URL to the site and create a download object to get the result.
		WWW hs_post = new WWW(post_url, null, headerInfo);
		yield return hs_post; // Wait until the download is done

		if (hs_post.error != null)
		{
			networkText.text ="error posting the high score";
			print("There was an error posting the high score: " + hs_post.error);
		}
		else
		{
			networkText.text ="";
		}
		hs_post = null;

		if(Application.loadedLevel>1)
		{
			Application.LoadLevel(1);
		}
		else
		{
			GameState=(int)GameStates.hiScoreTable;
			gameOverText.text = "";
			titleText.gameObject.SetActive(true);
			yield return new WaitForSeconds(3f);			
			GetOnLineScores_callsub();
		}	



	}

	// Commodity function that stops eventual running "GetOnlineScores" coroutine and launch a new one getting the first 100 scores
	public void GetOnLineScores_callsub ()
	{
		StopCoroutine ("GetOnlineScores");
		StartCoroutine("GetOnlineScores",100);
	}

    // Get the scores from the MySQL DB to store them in a List of IComparable <highscore>
    IEnumerator GetOnlineScores( int numofScores)
	{
		string HugeString;
		string Nome;	// 
		int Punteggio; //
		string get_url = highscoreURL +"nos=100&rh="+UnityEngine.Random.value;
        networkText.text = "Loading Online Scores";

		if(numofScores==0) numofScores=10;

		WWW hs_get = new WWW( get_url, null, headerInfo);

		yield return hs_get;
		
		if (hs_get.error != null)
		{
			networkText.text = "error getting scores: " + hs_get.error;
			print("There was an error getting "+get_url+": " + hs_get.error);
		}
		else
		{
			networkText.text = "Hiscores downloaded";
			Debug.Log( "Hiscores have been downloaded successfully");
			showOnLineScores = true;
			onlineHiscores.Clear (); // ripulisce la lista dei punteggi online
			PlayerPrefs.SetInt ("punteggiScaricati",1 ); // setta a 1 la chiave di registro per questa sessione
			// fill the hiscore List parsing the www answer string
			HugeString = hs_get.text;

			string[] Scores = HugeString.Split( '.' );

			for(int i = 0; i<(Scores.Length-1); ++i)
			{
				string[] tmpscore = Scores[i].Split(':');
				Nome = tmpscore[0];
				Punteggio = int.Parse ( tmpscore[1] );
				highscore hs = new highscore(Nome, Punteggio );
				onlineHiscores.Add (hs);
			}

			//hiScores.Clear(); // pulisce una lista riportandola a null (zero elementi nella lista)
			//int quanti = hiScores.Count; // numero di elementi nella lista
			onlineHiscores.Sort ();
			onlineHiscores.Reverse ();
			yield return new WaitForSeconds(0.5f);
			networkText.text = "";
		}
		hs_get = null;
	}

	/// <summary>
	/// The paused boolean is used to...
	/// </summary>
	/*private bool paused;
	IEnumerator OnApplicationPause(bool pauseStatus)
	{
		yield return new WaitForFixedUpdate ();
		paused = pauseStatus;

		if(paused){

			PlayerPrefs.SetInt ("hiscore",hiscore);
			PlayerPrefs.Save ();
		}
	}*/

	void OnApplicationQuit()
	{
		PlayerPrefs.SetInt ("hiscore",hiscore);
		PlayerPrefs.Save ();
	}

	void LevelWasLoaded(Scene scene, LoadSceneMode mode) {

		Debug.Log ("Si, ho appena caricato il livello gioco num:"+scene.buildIndex);
		if (scene.buildIndex > 1){
			if(isTwoPlayersGame)
			{
				StartGameDouble();
				//thePlayer.SetPlayerInfo ();
				//thePlayer2.SetPlayerInfo ();
			}
			else
			{
				StartGame(true);
				//thePlayer.SetPlayerInfo ();
			}
		}
	}

	public void SetGameState (int gstate){GameState = gstate;}
	public int GetGameState (){ return GameState;}

	// Camera Shake Globals
	private float shakeSpeed = 50;
	private	float startingShakeDistance = 0.8f;
	private	float decreasePercentage = -.1f;

	////////////////////////////////////////////////////////
	//
	//	 Studio 2:  sfoltiamo tutto questo codice disordinato
	//	 Sfruttando un Overload del metodo ShakeCamera ()
	//
	///////////////////////////////////////////////////////
	/// 
	/// 
	/// Shake Camera base method
	/// come potete vedere non ci sono parametri passati alla funzione

	IEnumerator ShakeCamera ()
	{
		float hitTime = Time.time;
		Vector3 originalCamPosition = new Vector3(0f,10f,5f);//Camera.main.transform.localPosition;
		int shake = 10;
		float shakeDistance = 1.05f; //startingShakeDistance
		
		while (shake>0) {
			// Make timers always start at 0 
			float timer = (Time.time - hitTime) * shakeSpeed;
			Camera.main.transform.localPosition = new Vector3(originalCamPosition.x + Mathf.Sin(timer) * shakeDistance/10,originalCamPosition.y,originalCamPosition.z + Mathf.Sin(timer) * (shakeDistance/8) );
			// See if we've gone through an entire sine wave cycle, reset distance timer if so and do less distance next cycle
			if (timer > Mathf.PI * 2) {
				hitTime = Time.time;
				shakeDistance = Mathf.Lerp( 1.05f,0,decreasePercentage);
				decreasePercentage = Mathf.Lerp(-0.1f,-0.5f,.01f);
				shake--;
			}
			yield return new WaitForEndOfFrame();
		}
		Camera.main.transform.localPosition = originalCamPosition;

	}
	/// <summary>
	/// Shakes the camera method overload
	/// </summary>
	/// <returns>The camera.</returns>
	/// <param name="numOfShakes">Number of shakes.</param>
	IEnumerator ShakeCamera (int numOfShakes)
	{

		float hitTime = Time.time;
		Vector3 originalCamPosition = new Vector3(0f,10f,5f);//Camera.main.transform.localPosition;
		float shakeDistance = startingShakeDistance;

		while (numOfShakes>0) {
			// Make timers always start at 0 
			float timer = (Time.time - hitTime) * shakeSpeed;
			Camera.main.transform.localPosition = new Vector3(originalCamPosition.x + Mathf.Sin(timer) * shakeDistance/10,originalCamPosition.y,originalCamPosition.z + Mathf.Sin(timer) * (shakeDistance/8) );
			// See if we've gone through an entire sine wave cycle, reset distance timer if so and do less distance next cycle
			if (timer > Mathf.PI * 2) {
				hitTime = Time.time;
				shakeDistance = Mathf.Lerp(startingShakeDistance,0,decreasePercentage);
				decreasePercentage = Mathf.Lerp(-0.1f,-0.5f,.01f);
				numOfShakes--;
			}
			yield return new WaitForEndOfFrame();
		}
		Camera.main.transform.localPosition = originalCamPosition;
	}
	/// <summary>
	/// Shakes the camera.
	/// </summary>
	/// <returns>The camera.</returns>
	/// <param name="numOfShakes">Number of shakes.</param>
	/// <param name="decreasePercentage">Decrease percentage.</param>
	IEnumerator ShakeCamera (int numOfShakes, float decreasePercentage,float shakeSpeed)
	{
		
		float hitTime = Time.time;
		Vector3 originalCamPosition = new Vector3(0f,10f,5f);//Camera.main.transform.localPosition;
		float shakeDistance = startingShakeDistance;
		
		while (numOfShakes>0) {
			// Make timers always start at 0 
			float timer = (Time.time - hitTime) * shakeSpeed;
			Camera.main.transform.localPosition = new Vector3(originalCamPosition.x + Mathf.Sin(timer) * shakeDistance/10,originalCamPosition.y,originalCamPosition.z + Mathf.Sin(timer) * (shakeDistance/8) );
			// See if we've gone through an entire sine wave cycle, reset distance timer if so and do less distance next cycle
			if (timer > Mathf.PI * 2) {
				hitTime = Time.time;
				shakeDistance = Mathf.Lerp(startingShakeDistance,0,decreasePercentage);
				decreasePercentage = Mathf.Lerp(-0.1f,-0.5f,.01f);
				numOfShakes--;
			}
			yield return new WaitForEndOfFrame();
		}
		Camera.main.transform.localPosition = originalCamPosition;
	}

	IEnumerator ShakeCamera (GameObject theShield)
	{
		float hitTime = Time.time;
		Vector3 originalCamPosition = new Vector3(0f,10f,5f);//Camera.main.transform.localPosition;
		int shake = 10;
		float shakeDistance = 1.05f;
		SCPlayerController thisPlayer = theShield.transform.parent.GetComponent<SCPlayerController>();

		while (shake>0) {
			// Make timers always start at 0 
			float timer = (Time.time - hitTime) * shakeSpeed;
			Camera.main.transform.localPosition = new Vector3(originalCamPosition.x + Mathf.Sin(timer) * shakeDistance/10,originalCamPosition.y,originalCamPosition.z + Mathf.Sin(timer) * (shakeDistance/8) );

			//Camera.main.transform.localPosition.Set( originalCamPosition.x + Mathf.Sin(timer) * shakeDistance/10,originalCamPosition.y,originalCamPosition.z + Mathf.Sin(timer) * (shakeDistance/8) );
			// See if we've gone through an entire sine wave cycle, reset distance timer if so and do less distance next cycle
			if (timer > Mathf.PI * 2) {
				hitTime = Time.time;
				shakeDistance = Mathf.Lerp( 1.05f,0,decreasePercentage);
				decreasePercentage = Mathf.Lerp(-0.1f,-0.5f,.01f);
				theShield.transform.localScale += new Vector3(shakeDistance/50,shakeDistance/50,shakeDistance/50);

				shake--;
			}
			yield return new WaitForEndOfFrame();
		}

		Camera.main.transform.localPosition = originalCamPosition;

		if(thisPlayer.shieldsHP>0)
		{
			--thisPlayer.shieldsHP;
			theShield.GetComponent<Renderer>().material.color = new Color (0f,1f,1f,(float)thisPlayer.shieldsHP/5);
		}

		if(thisPlayer.shieldsHP==0)
		{
			theShield.GetComponent<Renderer>().enabled = false;
			theShield.GetComponent<Collider>().enabled = false;
			theShield.transform.localScale = new Vector3(4f,4f,4f);
			theShield.GetComponent<Renderer>().material.color = new Color (0f,1f,1f,1f);
		}
	}

	public void ShakeShieldCamera(GameObject theShield)	{ StartCoroutine( ShakeCamera(theShield) ); }

	public void DestructiveShakeCamera(	) 	
	{	
		StartCoroutine(ShakeCamera(20,0.1f,50) );
	}
	public void CustomShakeCamera(int numeroScosse, float percDimin, float potenza)	{	
		StartCoroutine(ShakeCamera(numeroScosse,percDimin,potenza) );
	}
	public void MicroshakeTheCamera(){
		StartCoroutine(ShakeCamera(3) );
	}
	public void ShakeTheCamera() {		
		StartCoroutine(ShakeCamera() );
	}

	public void SpawnFoe(Vector3 spawnPosition)
	{
		Instantiate ( hazards[UnityEngine.Random.Range (4, hazards.Length-1 )], spawnPosition, Quaternion.identity ) ;
	}
	
}