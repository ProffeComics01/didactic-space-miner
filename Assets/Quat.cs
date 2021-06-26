using UnityEngine;
using System.Collections;

public class Quat : MonoBehaviour {
	public Transform target;
	void Update() {
		float angle = Quaternion.Angle(transform.rotation, target.rotation);
	}
}