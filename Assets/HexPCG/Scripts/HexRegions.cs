using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HexRegions : ScriptableObject {
	public List<HexRegion> regions;
}

[System.Serializable]
public class HexRegion
{
	public string name;
	public RegionType type;
	public float movementCost;
	public Color regionColor;
	[Range(0,1)]
	public float chanceOfOccurence;

	private float minValue;
	private float maxValue;

	public void SetMinMaxValue (float totalvalue,float minValue)
	{
		this.minValue = minValue;
		this.maxValue = chanceOfOccurence / totalvalue + minValue;
	}

	public bool IsInRange (float value)
	{
		return (minValue <= value && maxValue >= value);
	}
}

public enum RegionType
{
	GROUND,
	ROCK,
	WATER,
	GRASS,
	ICE,
	NONE
}
