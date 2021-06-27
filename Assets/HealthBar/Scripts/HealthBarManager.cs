using UnityEngine;
using System.Collections.Generic;

public class HealthBarManager : MonoBehaviour
{
	//The extra offset added to the first bar.
	public int pixelOffset = 4;
	//The height of the coloured section of each bar.
	public int barHeight = 5;
	//spacing between the colour section of each bar.
	public int barSpacing = 4;
	//The border added to each bar (this intrudes into spacing)
	public int borderSize = 1;
	//The colors used for each bar.
	public Color[] colors;
	//The color of the border which is added around each bar.
	public Color borderColor = Color.white;
	//This should just be a white image.
	public Texture2D baseTexture;

	public void Show (HealthBar bar)
	{
		if (!display.Contains (bar))
			display.Add (bar);
	}

	public void Hide (HealthBar bar)
	{
		if (display.Contains (bar))
			display.Remove (bar);
	}
	
	static HealthBarManager instance;

	public static HealthBarManager Instance {
		get {
			if (instance == null) {
				var g = GameObject.Find ("/HealthBarManager");
				//var g = GameObject.Find ("/AGameController");
				
				if (g == null) {
					return null;
					//Debug.LogError ("Could not find HealthBarManager.");
				} else {
					instance = g.GetComponent<HealthBarManager> ();
					if (instance == null)
						return null;
//						Debug.LogError ("Could not find HealthBarManager component.");
				}
			}
			return instance;
		}
	}

	void OnGUI ()
	{
		foreach (var i in display.ToArray())
			Draw (i);
	}

	void Draw (HealthBar bar)
	{
		Rect rect;
		Vector3 p;
		Vector3 w;
		//var e = bar.renderer.bounds.extents;
		if(!bar) return;
		Vector3 e = bar.GetComponent<Collider>().bounds.extents;

		p = Camera.main.WorldToScreenPoint (bar.transform.position + new Vector3 (-e.x, e.y, 0));
		w = Camera.main.WorldToScreenPoint (bar.transform.position + new Vector3 (e.x, e.y, 0));
		rect = new Rect ( p.x - w.x/p.x, Screen.height - p.y - pixelOffset, 0, barHeight);
		
		for (int i = 0; i < bar.values.Length; i++) {
			//Dont want borders? Comment out the next three lines.
			GUI.color = borderColor;
			var borderRect = new Rect (rect.x - borderSize, rect.y - borderSize, (w.x - p.x) + (borderSize * 2), rect.height + (borderSize * 2));
			GUI.DrawTexture (borderRect, baseTexture);
			
			GUI.DrawTexture (rect, baseTexture);
			GUI.color = colors[i % colors.Length];
			rect.width = (w.x - p.x) * bar.values[i];
			GUI.DrawTexture (rect, baseTexture);
			rect.y += barHeight + barSpacing;
		}
		
	}

	List<HealthBar> display = new List<HealthBar> ();
	
}
