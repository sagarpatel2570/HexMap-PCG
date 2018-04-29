using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Player : MonoBehaviour {

	public float moveSpeed = 6;
	public HexGrid hexGrid;

	public Transform startTransform;
	public Transform endTransform;

	Rigidbody rigidbody;
	Camera viewCamera;
	Vector3 velocity;

	void Awake () {
		rigidbody = GetComponent<Rigidbody> ();
		viewCamera = Camera.main;
		hexGrid.OnMapReady += HexGrid_OnMapReady;
	}

	void HexGrid_OnMapReady ()
	{
		GetComponent<Rigidbody> ().isKinematic = false;
	}

	void Update () {
		Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
		transform.LookAt (mousePos + Vector3.up * transform.position.y);
		velocity = (mousePos - transform.position).normalized * Input.GetAxisRaw ("Vertical") * moveSpeed;

		/*
		if (Input.GetKeyDown (KeyCode.Space)) {
			FindPath ();
		}
		*/
	}

	void FixedUpdate() {
		rigidbody.MovePosition (rigidbody.position + velocity * Time.fixedDeltaTime);
		HexCell cell = hexGrid.GetHexcellFromPosition (rigidbody.position);
		//hexGrid.RefreshMap (cell.room);
	}

	/*
	List<Node> nodes;

	void FindPath ()
	{
		 nodes = hexGrid.FindPath (startTransform.position, endTransform.position);
	}
	*/

	/*
	void OnDrawGizmos ()
	{
		if (nodes == null) {
			return;
		}

		for (int i = 0; i < nodes.Count - 1; i++) {
			Node node = nodes [i];
			Gizmos.color = Color.red;
			Gizmos.DrawSphere (node.hexCell.transform.localPosition, 3);
			Gizmos.color = Color.black;
			Gizmos.DrawLine (node.hexCell.transform.position, nodes [i + 1].hexCell.transform.position);
		}
		Gizmos.color = Color.red;	
		Gizmos.DrawSphere (nodes[nodes.Count - 1].hexCell.transform.localPosition, 3);
	}
	*/
}
