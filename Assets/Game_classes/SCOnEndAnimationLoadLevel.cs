using UnityEngine;
using UnityEngine.SceneManagement;

public class SCOnEndAnimationLoadLevel : MonoBehaviour {

	private ApplicationController appCtrl;

	void Awake()
	{
		GameObject appCtrlGO = GameObject.Find ("ApplicationController");
		if (appCtrlGO)
		{
			appCtrl = appCtrlGO.GetComponent<ApplicationController> ();
		}
		else
		{
			Debug.Log ("Errore nella scena introduttiva, application controller mancante!");
		}
	}


	void OnEndAnimation()
	{
		if(appCtrl)
			SceneManager.LoadScene( appCtrl.GetIndiceLivello() );
		else
			Debug.Log ("Errore nella scena introduttiva, application controller mancante!");
	}


}
