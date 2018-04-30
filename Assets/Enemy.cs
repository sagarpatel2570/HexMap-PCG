using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Enemy : MonoBehaviour {

	public float speed;
	public float turnSpeed;
	public Transform endTransform;

	Path path;
	int lineIndex;
	bool previousSide;
	bool currentSide;
	Vector2 perpendicularPointOnLine;
	HexGrid hexGrid;

	void Start () {
		hexGrid = GameObject.FindObjectOfType<HexGrid> ();
	}
	
	void Update()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			FindPath ();
		}

		if(path == null)
		{
			return;
		}

		Vector2 targetPos = FollowPath();
		Vector3 targetDir = Seek(targetPos.V2ToV3());
		if(targetDir != Vector3.zero)
		{
		    Rotate(targetDir);
		}
		Move(targetPos.V2ToV3());
	}

	Vector2 FollowPath()
	{
		// find the  shortest perpendicular to the line ..
		Line line = path.lines[lineIndex];
		currentSide = line.GetSide(transform.position.V3ToV2());
		if(currentSide != previousSide)
		{
			lineIndex++;
			if(lineIndex >= path.lines.Length)
			{
				lineIndex = path.lines.Length - 1;
				currentSide = line.GetSide(transform.position.V3ToV2());
				previousSide = currentSide;
			}
			line = path.lines[lineIndex];
			perpendicularPointOnLine = line.starPoint;
			previousSide = currentSide = line.GetSide(transform.position.V3ToV2());
		}
		previousSide = currentSide;
		perpendicularPointOnLine = line.PerpendicularPointOnLine (transform.position.V3ToV2());
		Vector2 targetPosition = perpendicularPointOnLine + (line.endPoint - line.starPoint).normalized * 0.5f;
		return (targetPosition);
	}

	Vector3 Seek(Vector3 targetPos)
	{
		return (targetPos - transform.position).normalized;
	}

	void Rotate(Vector3 targetDir)
	{
		Quaternion targetRotation = Quaternion.LookRotation (targetDir);
		transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

	}

	void Move(Vector3 targetPos)
	{
		transform.Translate (Vector3.forward * Time.deltaTime * speed ,Space.Self);
	}

	public List<Node> nodes;

	void FindPath ()
	{
		Stopwatch watch = new Stopwatch ();
		watch.Start ();

		nodes = hexGrid.FindPath (transform.position, endTransform.position);

		watch.Stop ();
		UnityEngine.Debug.Log ("Path finded in " + watch.ElapsedMilliseconds);

		Vector2[] waypoints = new Vector2[nodes.Count];
		for (int i = 0; i < nodes.Count; i++) {
			Vector3 pos = nodes [i].hexCell.transform.position;
			waypoints [i] = new Vector2 (pos.x, pos.z);
		}
		lineIndex = 0;
		previousSide = false;
		currentSide = false;
		perpendicularPointOnLine = Vector2.zero;
		path = null;
		path = new Path (waypoints, 10);
	}

	void OnDrawGizmos ()
	{
		if (nodes == null) {
			return;
		}

		if (path != null) {
			path.DrawGizmos ();
		}

		/*
		for (int i = 0; i < nodes.Count - 1; i++) {
			Node node = nodes [i];
			Gizmos.color = Color.red;
			Gizmos.DrawSphere (node.hexCell.transform.localPosition, 3);
			Gizmos.color = Color.black;
			Gizmos.DrawLine (node.hexCell.transform.position, nodes [i + 1].hexCell.transform.position);
		}
		Gizmos.color = Color.red;	
		Gizmos.DrawSphere (nodes[nodes.Count - 1].hexCell.transform.localPosition, 3);
		*/
	}

}
