﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Diagnostics;

public class HexGrid : MonoBehaviour {

	public event System.Action OnMapReady;

	public int width = 6;
	public int height = 6;

	public HexCell cellPrefab;
	public HexMesh hexMeshPrefab;

    public HexRegions hexRegions;
    public Texture2D noiseSource;

	public int seed = 1000;
	[Range(0,1)]
	public float passagePercent;
	public int minCellInRoom = 3;
	public bool removeSmallRegion;

    HexCell[] cells;
	Node[] nodes;
    List<HexRoom> rooms = new List<HexRoom>();
	Dictionary<HexRoom,GameObject> roomDictionary = new Dictionary<HexRoom, GameObject>();

	void Awake ()
	{
		SetRandomValueInRangeZeroToOneForRegions ();
	}

	void OnEnable()
	{
		HexMetrics.noiseSource = noiseSource;
	}

	void Start ()
	{
		Random.seed = seed;
        HexMetrics.noiseSource = noiseSource;

		cells = new HexCell[height * width];
		nodes = new Node[height * width];

		for (int z = 0, i = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				CreateCell(x, z, i++);
			}
		}

		Stopwatch watch = new Stopwatch ();
		watch.Start ();

        GenerateHexMap();
        
		watch.Stop ();
		UnityEngine.Debug.Log ("Map Finished in " + watch.ElapsedMilliseconds);
	}

    void GenerateHexMap ()
    {
        List<HexCell> hexCellsList = new List<HexCell>();
        hexCellsList.Add(cells[0]);
        HexRoom hexRoom = new HexRoom(RegionType.NONE,hexRegions);
        rooms.Add(hexRoom);        
        cells[0].initialize = true;
        cells[0].room = hexRoom;
        hexRoom.AddCell(cells[0]);

        while (hexCellsList.Count > 0)
        {
            ProcessNextStep(hexCellsList);
        }

		List<int> roomIndexToRemove = null;

		if (removeSmallRegion) {
			roomIndexToRemove = new List<int> ();
			roomIndexToRemove = RemoveSmallRegions ();
		}

		GenerateMeshForRooms( roomIndexToRemove);

		if (OnMapReady != null) {
			OnMapReady ();
		}
	}

	List<int> RemoveSmallRegions ()
	{
		List<int> indexRoomToIgnore = new List<int> ();

		foreach (HexRoom room in rooms) {

			if (indexRoomToIgnore.Contains ((rooms.FindIndex (r => r == room)))) {
				continue;
			}

			HexRoom currentRoom = room;
			while (currentRoom.cells.Count <= minCellInRoom) {
				
				HexRoom neighbourRoomWithMaxCell = currentRoom.GetNeighbourWIthMaxCells ();
				// FIXME !! this is a quick fix 
				if (neighbourRoomWithMaxCell == currentRoom) {
					UnityEngine.Debug.LogError ("this should not happen");
					neighbourRoomWithMaxCell = currentRoom.neighbourRoom [1];
				}
				neighbourRoomWithMaxCell.AddNeighbourRoomFrom (currentRoom);
				neighbourRoomWithMaxCell.AddCellFromRoom (currentRoom);

				indexRoomToIgnore.Add (rooms.FindIndex (r => r == currentRoom));
				if (indexRoomToIgnore.Contains (rooms.FindIndex (r => r == neighbourRoomWithMaxCell))) {
					indexRoomToIgnore.Remove (rooms.FindIndex (r => r == neighbourRoomWithMaxCell));
				}
				currentRoom = neighbourRoomWithMaxCell;
			}

			foreach (HexCell cell in currentRoom.cells) {
				for (int i = 0; i < 6; i++) {
					HexDirection direction = (HexDirection)i;
					HexCell neighbour = cell.GetNeighbor (direction);
					if (neighbour != null ) {
						if (neighbour.room.region.type == cell.room.region.type) {
							cell.SetPassage (direction, EdgeType.PASSAGE, cell.room);
							neighbour.SetPassage (direction.Opposite (), EdgeType.PASSAGE, cell.room);
						} else {
							if (cell.edgeTypes [i] == EdgeType.PASSAGE) {
								cell.SetPassage (direction, EdgeType.WALL, cell.room);
								neighbour.SetPassage (direction.Opposite (), EdgeType.WALL, cell.room);
							}
						}
					}
				}
			}
		}
		return indexRoomToIgnore;
	}

	void GenerateMeshForRooms (List<int> roomIndexToIgnore)
	{
		for (int i = 0; i < rooms.Count; i++) {

			if (roomIndexToIgnore != null && roomIndexToIgnore.Contains (i)) {
				continue;
			}

			HexRoom room = rooms [i];
			HexMesh hexRoomMesh = Instantiate (hexMeshPrefab, this.transform);
			hexRoomMesh.Triangulate (room);
			roomDictionary.Add (room, hexRoomMesh.gameObject);
		}
	}

    void ProcessNextStep (List<HexCell> hexCellsList)
    {
        HexCell currentCell = hexCellsList[hexCellsList.Count - 1];
       
        if (currentCell.IsFullyInitialize == true)
        {
            hexCellsList.RemoveAt(hexCellsList.Count - 1);
            return;
        }

        HexDirection randomDirection = currentCell.GetRandomUnVisitedDirection();
        HexCell neighbour = currentCell.GetNeighbor(randomDirection);

        if (neighbour != null)
        {
            if (!neighbour.initialize)
            {
               
                if (Random.value <= passagePercent)
                {
                    currentCell.SetPassage(randomDirection, EdgeType.PASSAGE, currentCell.room);
					neighbour.SetPassage (randomDirection.Opposite (), EdgeType.PASSAGE, currentCell.room);
					currentCell.room.AddCell (currentCell.GetNeighbor(randomDirection));
                }
                else
                {
					HexRoom nextRoom = new HexRoom(currentCell.room.region.type, hexRegions);
					currentCell.SetPassage(randomDirection, EdgeType.DOOR, currentCell.room);
					neighbour.SetPassage (randomDirection.Opposite (), EdgeType.DOOR, nextRoom);

					currentCell.room.AddNeighbour (nextRoom);
					nextRoom.AddNeighbour (currentCell.room);

                    rooms.Add(nextRoom);
                    nextRoom.AddCell(neighbour);
                }
                hexCellsList.Add(neighbour);
			}else if (currentCell.room.region.type == neighbour.room.region.type ) {

				currentCell.SetPassage (randomDirection, EdgeType.PASSAGE, currentCell.room);
				neighbour.SetPassage (randomDirection.Opposite(), EdgeType.PASSAGE, currentCell.room);
				if (currentCell.room != neighbour.room) {
					rooms.Remove (neighbour.room);
					currentCell.room.AddNeighbourRoomFrom (neighbour.room);
					currentCell.room.AddCellFromRoom (neighbour.room);
				}

			}else
            {
                currentCell.SetPassage(randomDirection, EdgeType.WALL, null);
				neighbour.SetPassage (randomDirection.Opposite(), EdgeType.WALL, null);
            }
        }
        else
        {
            currentCell.SetPassage(randomDirection, EdgeType.WALL,null);
        }
    }

    void CreateCell(int x, int z, int i)
    {
        Vector3 position = Vector3.zero;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.node = new Node (cell);
		nodes [i] = cell.node;

        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - width]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - width]);
                if (x < width - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
                }
            }
        }
    }

	void SetRandomValueInRangeZeroToOneForRegions ()
	{
		float sumOfAllRandomValue = 0;
		foreach (HexRegion region in hexRegions.regions) {
			sumOfAllRandomValue += region.chanceOfOccurence;
		}

		float previousMaxValue = 0;
		foreach (HexRegion region in hexRegions.regions) {
			region.SetMinMaxValue (sumOfAllRandomValue, previousMaxValue);
			previousMaxValue += region.chanceOfOccurence / sumOfAllRandomValue; 
		}
	}

	public List<Node> FindPath (Vector3 startpos,Vector3 endPos)
	{
		HexCell startCell = GetHexcellFromPosition (startpos);
		HexCell endCell = GetHexcellFromPosition (endPos);

		Node startNode = startCell.node;
		Node endNode = endCell.node;

		return AStar.DoPathFind_AStar (startNode, endNode, nodes);
	}

	public HexCell GetHexcellFromPosition (Vector3 pos)
	{
		HexCoordinates coordinates = HexCoordinates.FromPosition(pos);
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		HexCell cell = cells[index];
		return cell;
	}

	public void RefreshMap (HexRoom room)
	{
		foreach (HexRoom r in roomDictionary.Keys) {
			if (r == room) {
				roomDictionary [r].SetActive (true);
			} else {
				roomDictionary [r].SetActive (false);
			}
		}
	}
}