using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Line  
{

	/// <summary>
	/// if gradient is divided by zero we use this value to avoid inifinity
	/// </summary>
	const float verticalLineGradient = 1e5f;

	/// <summary>
	/// The start point of line.
	/// </summary>
	public Vector2 starPoint;
	/// <summary>
	/// The end point of line
	/// </summary>
	public Vector2 endPoint;

	/// <summary>
	/// The gradient/slope which is  perpendicular to line. 
	/// </summary>
	public float gradientPerpendicularToLine;
	/// <summary>
	/// The gradient/slope of  line.
	/// </summary>
	public float gradientLine;

	/// <summary>
	/// These are the point which is perpendicular to the line  at "distanceToPerpendicularLineFromEndpoint"
	/// </summary>
	Vector2 pointOnLine_1;
	Vector2 pointOnLine_2;

	public Line (Vector2 startPoint,Vector2 endPoint,float distanceToPerpendicularLineFromEndpoint) 
	{
		this.starPoint = startPoint;
		this.endPoint = endPoint;

		float dx = endPoint.x - startPoint.x;
		float dy = endPoint.y - startPoint.y;

		if (dx == 0) 
		{
			gradientLine = verticalLineGradient;
		} 
		else 
		{
			gradientLine = dy / dx;
		}

		if (gradientLine == 0)
		{
			gradientPerpendicularToLine = verticalLineGradient;
		}
		else
		{
			gradientPerpendicularToLine = -1 / gradientLine;
		}

		pointOnLine_1 = endPoint + (startPoint- endPoint).normalized * distanceToPerpendicularLineFromEndpoint + new Vector2 (1, gradientPerpendicularToLine).normalized;
		pointOnLine_2 = endPoint + (startPoint- endPoint).normalized * distanceToPerpendicularLineFromEndpoint - new Vector2 (1, gradientPerpendicularToLine).normalized;
	}

	public float DistanceFromPointToLine( Vector2 c)
	{
		float s1 = -endPoint.y + starPoint.y;
		float s2 = endPoint.x - starPoint.x;
		return Mathf.Abs((c.x - starPoint.x) * s1 + (c.y - starPoint.y) * s2) / Mathf.Sqrt(s1*s1 + s2*s2);
	}

	public bool GetSide(Vector2 p) 
	{
		return (p.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
	}

	public void DrawGizmos () 
	{
		Gizmos.DrawLine (pointOnLine_1.V2ToV3(), pointOnLine_2.V2ToV3());
	}

}

public static class ExtentionMethod
{
	public static Vector3 V2ToV3(this Vector2 pos)
	{
		return new Vector3 (pos.x, 2, pos.y);
	}

	public static Vector2 V3ToV2(this Vector3 pos)
	{
		return new Vector2 (pos.x, pos.z);
	}
}
