using UnityEngine;
using System.Collections;

public class SpinMine : MonoBehaviour {

	void Start() {
		GetComponent<Rigidbody>().AddForce(0, 0, 350);
	}
}
