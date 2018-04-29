using System.Collections.Generic;
using UnityEngine;

public enum EdgeType
{
    PASSAGE,
    WALL,
    DOOR,
    NONE
}

public class HexCell : MonoBehaviour {

	public HexCoordinates coordinates;
	public Color color;
    public Color wallColor;
    public EdgeType[] edgeTypes;
    public GameObject doorPrefab;
    public HexRoom room;

	public Node node;

    public bool initialize;

    public bool IsFullyInitialize
    {
        get
        {
            return initializedEdgeCount == edgeTypes.Length;
        }
    }
    [SerializeField]
	HexCell[] neighbors;
    public int initializedEdgeCount;


    public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}

	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

    public EdgeType Getpassage (HexDirection direction)
    {
        return edgeTypes[(int)direction];
    }

    
    public void SetPassage(HexDirection direction,EdgeType type,HexRoom room)
    {
        edgeTypes[(int)direction] = type;

        if (!initialize)
        {
            initialize = true;
            if (room != null)
            {
                this.room = room;
            }
        }
        initializedEdgeCount++;

    }

    public HexDirection GetRandomUnVisitedDirection()
    {
        List<int> neighbourUnInitialize = new List<int>();
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (edgeTypes[i] != EdgeType.NONE) {
                continue;   
            }
            neighbourUnInitialize.Add(i);
        }
        int randomDir = Random.Range(0, neighbourUnInitialize.Count);
        return (HexDirection)neighbourUnInitialize[randomDir];
    }

    /*
    public void GenerateDoor (Vector3 v1,Vector3 v2,Vector3 v3,Vector3 v4,int direction)
    {
        GameObject door = Instantiate(doorPrefab, this.transform);
        door.transform.position = doorPos;
        door.transform.localScale = new Vector3(Mathf.Round(HexMetrics.innerRadius), HexMetrics.wallHeight,- HexMetrics.blendFactor * HexMetrics.outerRadius * 2);
        door.transform.eulerAngles = new Vector3(0, (direction ) * 60  +( -150), 0);
        doors.Add(door);
    }
    */
}