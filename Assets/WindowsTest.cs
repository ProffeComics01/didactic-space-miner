using UnityEngine;
using System.Collections;

public class WindowTest : MonoBehaviour {
	bool doWindow0= true;
	bool doWindow1= true;
	bool doWindow2= true;
	
	GUISkin mySkin;
	Texture bgImage;
	
	private Rect windowRect0= new Rect(0, 20, 600, 400);
	private Rect windowRect1= new Rect(350, 20, 350, 500);
	private Rect windowRect2= new Rect(700, 20, 350, 500);
	
	private Vector2 scrollPosition0;
	private Vector2 scrollPosition1;
	private float HorSliderValue= 0.5f;
	private float VertSliderValue= 0.5f;
	private bool Toggle0= false;
	private bool Toggle1= false;
	private bool Toggle2= false;
	
	void  DoMyWindow0 ( int windowID  ){
		GUILayout.BeginVertical();
		GUILayout.Space(8);
		GUILayout.Label("WINDOW 1 TITLE");
		GUILayout.Label ("SCROLLVIEW WITHOUT SCROLL", "HeaderText");		
		scrollPosition0 = GUILayout.BeginScrollView(scrollPosition0, GUILayout.Height (110));
		GUILayout.Label ("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam tincidunt, ante id pretium faucibus, libero lorem feugiat odio, at euismod est leo a eros. ", "PlainText");
		GUILayout.EndScrollView();	
		GUILayout.Space(10);
		GUILayout.Label ("TOGGLE BUTTONS", "HeaderText");
		Toggle0 = GUILayout.Toggle(Toggle0, "Toggle Button 1");
		Toggle1 = GUILayout.Toggle(Toggle1, "Toggle Button 2");
		Toggle2 = GUILayout.Toggle(Toggle2, "Toggle Button 3");
		GUILayout.Space(10);
		GUILayout.Label ("TEXTFIELD", "HeaderText");
		GUILayout.TextField("Lorem ipsum dolor sit amet, consectetur adipiscing elit.");
		GUILayout.Label ("TEXTAREA", "HeaderText");
		GUILayout.TextArea("Lorem ipsum dolor sit amet, consectetur adipiscing elit.");
		
		
		GUILayout.EndVertical();		
		// Make the windows be draggable.
		GUI.DragWindow ( new Rect(0,0,10000,10000));
	}
	
	void  DoMyWindow1 ( int windowID  ){	
		GUILayout.Space(8);
		GUILayout.Label("WINDOW 2 TITLE");
		GUILayout.BeginVertical();
		GUILayout.Label ("SCROLLVIEW WITH SCROLL", "HeaderText");
		GUILayout.BeginHorizontal();		
		scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, GUILayout.Height (250));
		GUILayout.Label ("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam tincidunt, ante id pretium faucibus, libero lorem feugiat odio, at euismod est leo a eros. Donec sit amet justo ac odio venenatis imperdiet vel nec turpis. Vestibulum dictum imperdiet elit, sit amet condimentum justo placerat et. Sed commodo vulputate nunc at ultricies. Morbi suscipit metus sit amet libero sollicitudin placerat. Fusce orci arcu, tincidunt at faucibus vel, pharetra lacinia velit. Donec et lectus tellus. Praesent malesuada enim nec ligula fermentum euismod. Nunc dolor arcu, iaculis eu volutpat vitae, porttitor ut purus.", "PlainText");
		GUILayout.EndScrollView();
		GUILayout.EndHorizontal();
		GUILayout.Space(8);
		GUILayout.Label ("SLIDERS", "HeaderText");
		HorSliderValue = GUILayout.HorizontalSlider(HorSliderValue, 0.0f, 1.1f);
		VertSliderValue = GUILayout.VerticalSlider(VertSliderValue, 0.0f, 1.1f, GUILayout.Height(80));	
		GUILayout.EndVertical();
		GUI.DragWindow ( new Rect(0,0,10000,10000));
	}
	
	//bringing it all together
	void  DoMyWindow2 ( int windowID  ){
		GUILayout.Space(8);
		GUILayout.BeginVertical();
		GUILayout.Label("WINDOW 3 TITLE");	
		
		GUILayout.Label ("HEADER TITLE", "HeaderText");
		GUILayout.Label ("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam tincidunt, ante id pretium faucibus, libero lorem feugiat odio, at euismod est leo a eros.", "PlainText");
		GUILayout.Space(8);		
		
		GUILayout.Label ("PLAIN TEXT", "HeaderText");
		GUILayout.Label ("Lorem ipsum dolor sit amet, consectetur adipiscing elit.", "PlainText");
		
		GUILayout.Space(190);
		GUILayout.Label ("BUTTONS", "HeaderText");
		GUILayout.BeginHorizontal();
		GUILayout.Button("CANCEL");        
		GUILayout.Button("OK");
		GUILayout.EndHorizontal();
		
		GUILayout.EndVertical();
		
		GUI.DragWindow ( new Rect(0,0,10000,10000));
	}
	
	void  OnGUI (){
		GUI.skin = mySkin;
		
		GUI.DrawTexture( new Rect(0,0,Screen.width,Screen.height), bgImage);  
		
		if (doWindow0)
			windowRect0 = GUI.Window (0, windowRect0, DoMyWindow0, "");
		//now adjust to the group. (0,0) is the topleft corner of the group.
		GUI.BeginGroup ( new Rect(0,0,100,100));
		// End the group we started above. This is very important to remember!
		GUI.EndGroup ();
		
		if (doWindow1)
			windowRect1 = GUI.Window (1, windowRect1, DoMyWindow1, "");
		//now adjust to the group. (0,0) is the topleft corner of the group.
		GUI.BeginGroup ( new Rect(0,0,100,100));
		// End the group we started above. This is very important to remember!
		GUI.EndGroup ();
		
		if (doWindow2)
			windowRect2 = GUI.Window (2, windowRect2, DoMyWindow2, "");
		//now adjust to the group. (0,0) is the topleft corner of the group.
		GUI.BeginGroup ( new Rect(0,0,100,100));
		// End the group we started above. This is very important to remember!
		GUI.EndGroup ();
	}
}