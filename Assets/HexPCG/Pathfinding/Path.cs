using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Given a list of position this class will create lines
/// </summary>
public class Path 
{
	public Line[] lines;

	public Path  (Vector2[] wayPoints,float distanceToPerpendicularLineFromEndpoint,bool isCyclic = false)
	{
		if (!isCyclic) 
		{
			lines = new Line[wayPoints.Length - 1];
			for (int i = 0; i < lines.Length; i++) 
			{
				lines [i] = new Line (wayPoints [i], wayPoints [i + 1], distanceToPerpendicularLineFromEndpoint);
			}
		} else
		{
			lines = new Line[wayPoints.Length ];

			for (int i = 0; i < lines.Length; i++)
			{
				lines [i] = new Line (wayPoints [i], wayPoints [(i + 1)%wayPoints.Length], distanceToPerpendicularLineFromEndpoint);
			}
		}
	}

	public void DrawGizmos () 
	{
		if (lines != null) 
		{
			for (int i = 0; i < lines.Length; i++) 
			{
				Gizmos.color = Color.white;
				Gizmos.DrawSphere (lines [i].starPoint.V2ToV3(), 3);
				Gizmos.DrawSphere (lines [i].endPoint.V2ToV3(),3);

				Gizmos.color = Color.red;
				Gizmos.DrawLine (lines [i].starPoint.V2ToV3(), lines [i].endPoint.V2ToV3());
				Gizmos.color = Color.white;
				lines [i].DrawGizmos ();
			}
		}
	}
}
