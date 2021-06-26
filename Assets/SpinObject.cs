using UnityEngine;
using System.Collections;

public class SpinObject : MonoBehaviour  {

    public float myRotationXSpeed = 100f;
	public float myRotationYSpeed = 100f;
	public float myRotationZSpeed = 100f;
    
    public bool isRotateX = false;
    public bool isRotateY = false;
    public bool isRotateZ = false;
    // CHANGE TO ROTATE IN OPPOSITE DIRECTION
    private bool positiveRotation = false;

    private int pos0rNeg = 1;

    void Start ()
    {
       if(positiveRotation == false)
       {
              pos0rNeg = -1;
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
