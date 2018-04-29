using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public Transform playerTransform;
	public float speed ;

	void Start () {
		
	}
	
	void LateUpdate () {
		Vector3 targetPos = new Vector3 (
			playerTransform.position.x,
			transform.position.y,
			playerTransform.position.z
		);

		transform.position = Vector3.Lerp (transform.position,
			targetPos,
			speed * Time.deltaTime
		);
	}
}
