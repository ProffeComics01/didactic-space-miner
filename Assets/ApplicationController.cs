using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

public class ApplicationController : MonoBehaviour {

	[HideInInspector] public bool internetDisponibile;
	public string TestUrl="http://www.google.com";
	public float SoundLevel=0.5f;
	public float MusicLevel=0.85f;

	public PlayerInfo player1info, player2info;
	public struct PlayerInfo
	{
		public int score, health, livelloArma;
		public float speed;
		public bool shieldActive, supporterL_Active, supporterR_Active;
		public int maxNumOfBombs,numOfBombs;
	}
	private SCGameController gameController;

	void Awake () {
		DontDestroyOnLoad (this);
		PlayerInfo player1info = new PlayerInfo();
		PlayerInfo player2info = new PlayerInfo();
        //UnityEditor.EditorSettings.webSecurityEmulationHostUrl = "www.google.com";
    }
	protected AudioSource[] audioChannel = new AudioSource[2];
	private int currentAudioChannel = 0; // current audiosource used by the gameobject for audio CrossFade function

	float multipw {get; set;}
	float multiph {get; set;}

	public float Multipw
	{
		get { return multipw; }
		set { multipw = value; }
	}
	public float Multiph
	{
		get { return multiph; }
		set { multiph = value; }
	}
	// Use this for initialization
	void Start () {

		// Switch che determina la connettività di rete
		switch( Application.internetReachability )
		{
			case NetworkReachability.NotReachable:
				internetDisponibile = false;
				Debug.Log("network not even reachable");
				break;			
			case NetworkReachability.ReachableViaCarrierDataNetwork:
				Debug.Log("network reachable via data carrier testing..");
				StartCoroutine("CheckPeriodicoInternet");
				break;
			case NetworkReachability.ReachableViaLocalAreaNetwork:
				Debug.Log("network reachable via LAN testing..");
                //StartCoroutine(GetOnlineResponse());
                StartCoroutine("CheckPeriodicoInternet");				
				break;
		}

		livelloCorrente = 1;

		if(!PlayerPrefs.HasKey("player1ctrl")) PlayerPrefs.SetInt ("player1ctrl",0);
		if(!PlayerPrefs.HasKey("player2ctrl")) PlayerPrefs.SetInt ("player2ctrl",1);

		// Setup music background AudioChannels
		audioChannel[0] = (AudioSource) this.gameObject.GetComponent<AudioSource>();		
		audioChannel[1] = this.gameObject.AddComponent<AudioSource>();
		audioChannel[1].loop = true;
		audioChannel[1].playOnAwake = false;
		audioChannel[0].ignoreListenerVolume = true;
		audioChannel[1].ignoreListenerVolume = true;

		multipw = Screen.width / 800F;
		multiph = Screen.height / 600F;

		Debug.Log("multipw:" +multipw + " multipw:"+multiph );
		Debug.Log("aspect ratio:" +(float)Screen.width / (float)Screen.height);
	}

    private Dictionary<string, string> headerInfo;

    /// <summary>
    /// Coroutine: Checks periodico presenza internet.
    /// quando internet è disponibile e i punteggi non sono mai stati scaricati, li recupera
    /// </summary>
    public IEnumerator CheckPeriodicoInternet()
	{
		bool gameControllerCaricato=false;
		GameObject gameControllerObj;
        headerInfo = new Dictionary<string, string>
        {
            { "User-Agent", "LitobyteClient" }
        };

        //using (WWW www = new WWW(TestUrl))
        using (WWW www = new WWW(TestUrl, null, headerInfo))
       {
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log(www.error);
                internetDisponibile = false;
            }
            else
            {
                //Debug.Log(www.text);
                internetDisponibile = true;
            }
            Debug.Log("Connesione internet attiva: " + internetDisponibile);

            while (true)
            {
                gameControllerObj = GameObject.Find("Game Controller");
                if (gameControllerObj != null) gameControllerCaricato = true;

                if (gameControllerCaricato)
                {
                    gameController = gameControllerObj.GetComponent<SCGameController>();
                    break;
                }
                yield return new WaitForSeconds(0.01f);
            }

            // Ulteriore controllo di sicurezza
            if (!gameController) Debug.LogError("Attenzione, il livello " + livelloCorrente + " è sprovvisto di SCGameController");

            // Se internet è disponibile, avvio la coroutine che recupera dal web i punteggi
            if (internetDisponibile && PlayerPrefs.GetInt("punteggiScaricati") == 0)
            {
                gameController.GetOnLineScores_callsub();// Try to retrieve online scores
            }

            yield return new WaitForSeconds(30.0f);
       }
		
	}

    IEnumerator GetOnlineResponse()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(TestUrl))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError )
            {
                Debug.Log(www.error);
                internetDisponibile = false;
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);
                internetDisponibile = true;
                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;
            }
        }
    }

    private bool requestFinished;
    private bool requestErrorOccurred;
    private string lastRequestURL;
    private List<string> lastRequestParameters;

    IEnumerator GetRequest(string uri)
    {
        requestFinished = false;
        requestErrorOccurred = false;

        UnityWebRequest request = UnityWebRequest.Get(uri);
        yield return request.SendWebRequest();

        requestFinished = true;
        if (request.isNetworkError)
        {
            Debug.Log("Something went wrong, and returned error: " + request.error);
            requestErrorOccurred = true;
        }
        else
        {
            // Show results as text
            Debug.Log(request.downloadHandler.text);

            if (request.responseCode == 200)
            {
                Debug.Log("Request finished successfully!");
            }
            else if (request.responseCode == 401) // an occasional unauthorized error
            {
                Debug.Log("Error 401: Unauthorized. Resubmitted request!");
                StartCoroutine(GetRequest( GenerateRequestURL(lastRequestURL, lastRequestParameters) ));
                requestErrorOccurred = true;
            }
            else
            {
                Debug.Log("Request failed (status:" + request.responseCode + ")");
                requestErrorOccurred = true;
            }

            if (!requestErrorOccurred)
            {
                yield return null;
                // process results
            }
        }
    }

    private string oauth_consumerKey, oauth_consumerSecret;

    string GenerateRequestURL(string in_url, List<string> paramaters, string HTTP_Method = "GET")
    {
        OAuth_CSharp oauth = new OAuth_CSharp(oauth_consumerKey, oauth_consumerSecret);
        string requestURL = oauth.GenerateRequestURL(in_url, HTTP_Method, paramaters);

        return requestURL;
    }
    private int livelloCorrente;
	public int GetIndiceLivello(){ return livelloCorrente; }
	public void SetIndiceLivello ( int indiceLivello ){ livelloCorrente = indiceLivello; }

	// Audio stuff
	public void SetChannelsVolume()
	{
		audioChannel[0].volume = MusicLevel;
		audioChannel[1].volume = MusicLevel;
	}
	void CrossFadeAudio(AudioClip incomingClip)
	{
		StopCoroutine( "CrossFader");
		AudioSource olderChannel = audioChannel[currentAudioChannel];
		if(currentAudioChannel==0) currentAudioChannel=1; else currentAudioChannel = 0;
		audioChannel[currentAudioChannel].clip = incomingClip;
		audioChannel[currentAudioChannel].volume = 0f;
		olderChannel.volume = MusicLevel;
		//if(incomingClip==battleWon || incomingClip==battleLost)	audioChannel[currentAudioChannel].loop = false; else audioChannel[currentAudioChannel].loop = true;
		
		StartCoroutine( "CrossFader",olderChannel );
	}
	IEnumerator CrossFader (AudioSource olderChannel)
	{
		float CrossProgress = 0f; // 100 x 0.01f steps
		audioChannel[currentAudioChannel].Play();
		while(true)
		{
			CrossProgress += 0.001f;
			if(olderChannel.volume > 0.0f) olderChannel.volume -= 0.025f;
			if(audioChannel[currentAudioChannel].volume < MusicLevel) audioChannel[currentAudioChannel].volume += 0.025f;
			//Debug.Log("CrossFade audio progress: "+ CrossProgress);
			if(CrossProgress>=1f) break;
			yield return new WaitForSeconds(0.001f);
		}
		olderChannel.Stop();
		audioChannel[currentAudioChannel].volume = MusicLevel;
	}

}
