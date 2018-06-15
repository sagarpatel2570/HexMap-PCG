using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Enemy : MonoBehaviour {

	public float speed;
	public float turnSpeed;
	public float acceleration;
	public Transform endTransform;
	public float slowDownDistance;

	Path path;
	int lineIndex;
	float currentSpeed;
	bool previousSide;
	bool currentSide;
	Vector2 perpendicularPointOnLine;
	HexGrid hexGrid;
	List<Node> nodes;
	List<HexCell> endHexCells = new List<HexCell> ();

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

		Vector2 targetPos = FollowPath();  /// get the targetpos
		Vector3 targetDir = Seek(targetPos.V2ToV3()); /// get the target direction
		if(targetDir != Vector3.zero)
		{
		    Rotate(targetDir); /// rotate the transform according to turn speed
		}
		Move(targetPos.V2ToV3()); /// finally move the transform
	}

	/// <summary>
	/// we have created a perpendiculate line at the certain distance from endpoint 
	/// so if the player crosses that point it will seek the endpoint of next line 
	/// this way we get smooth turn movement 
	/// </summary>
	/// <returns>The path.</returns>
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
			previousSide = currentSide = line.GetSide(transform.position.V3ToV2());
		}
		previousSide = currentSide;
		return (line.endPoint);
	}


	/// <summary>
	/// Seek the specified targetPos.
	/// </summary>
	/// <param name="targetPos">Target position.</param>
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
		float actualSpeed = GetMovementSpeedAcoordingToRegion ();
		transform.Translate (Vector3.forward * Time.deltaTime * actualSpeed ,Space.Self);
	}

	/// <summary>
	/// Gets the movement speed acoording to region.
	/// It will slow down the speed with the factor of movement cost which is defined in the region scriptable object
	/// </summary>
	/// <returns>The movement speed acoording to region.</returns>
	float GetMovementSpeedAcoordingToRegion ()
	{
		HexCell currentCell = hexGrid.GetHexcellFromPosition (transform.localPosition);
		float maxSpeedInRegion = speed / currentCell.room.region.movementCost;
		currentSpeed = Mathf.MoveTowards (currentSpeed, maxSpeedInRegion, Time.deltaTime * acceleration);
		float actualSpeed = currentSpeed;
		if (endHexCells.Contains (currentCell)) 
		{
			float distanceRemaining = Vector2.Distance (endHexCells [endHexCells.Count - 1].transform.position.V3ToV2 (), transform.position.V3ToV2 ());

			float percentDistanceRemaning = distanceRemaining / slowDownDistance;

			actualSpeed = Mathf.Lerp (currentSpeed, 0, (1 - percentDistanceRemaning));
		}
		return actualSpeed;
	}

	/// <summary>
	/// Adds the end hexcell according to the slowdown distance  for slow down.
	/// </summary>
	void AddEndHexcellForSlowDown ()
	{
		float totalDistance = nodes.Count * HexMetrics.outerRadius;
		int nodesToConsiderForStopping = Mathf.CeilToInt (slowDownDistance / HexMetrics.outerRadius);
		if (endHexCells != null && endHexCells.Count > 0)
		{
			endHexCells.Clear ();
		}
		for (int i = 0; i < nodesToConsiderForStopping; i++) 
		{
			if (i >= nodes.Count) 
			{
				break;
			}
			int index = nodes.Count - (i + 1);
			endHexCells.Add (nodes [index].hexCell);
		}

		endHexCells.Reverse ();
	}

	/// <summary>
	/// Finds the shortest path and will create a path for movement
	/// </summary>
	void FindPath ()
	{
		Stopwatch watch = new Stopwatch ();
		watch.Start ();

		nodes = hexGrid.FindPath (transform.position, endTransform.position);

		watch.Stop ();
		UnityEngine.Debug.Log ("Path finded in " + watch.ElapsedMilliseconds);

		if (nodes == null)
			return;

		AddEndHexcellForSlowDown ();

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
