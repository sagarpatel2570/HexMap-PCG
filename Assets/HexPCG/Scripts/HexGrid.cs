using UnityEngine;
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
		/// we maintain a list of room which have combine so that we can actually ignore them from rendering
		List<int> indexRoomToIgnore = new List<int> ();

		foreach (HexRoom room in rooms) {

			if (indexRoomToIgnore.Contains ((rooms.FindIndex (r => r == room)))) {
				continue;
			}

			HexRoom currentRoom = room;
			/// add the neighbour rooms until the combination of room has minimum cell count
			while (currentRoom.cells.Count <= minCellInRoom) {

				/// find the neighbour with max cell so that we do not go in merge room after room
				HexRoom neighbourRoomWithMaxCell = currentRoom.GetNeighbourWIthMaxCells ();
				/// add all the cell's neighbour rooms to current cell room
				neighbourRoomWithMaxCell.AddNeighbourRoomFrom (currentRoom);
				/// also add all of it's cell
				neighbourRoomWithMaxCell.AddCellFromRoom (currentRoom);

				indexRoomToIgnore.Add (rooms.FindIndex (r => r == currentRoom));
				/// we remove the index from the indexroomtoignore list if the neighbourRoomWithMaxCell room index is in that list
				if (indexRoomToIgnore.Contains (rooms.FindIndex (r => r == neighbourRoomWithMaxCell))) {
					indexRoomToIgnore.Remove (rooms.FindIndex (r => r == neighbourRoomWithMaxCell));
				}
				currentRoom = neighbourRoomWithMaxCell;
			}

			/// we remove unwanted walls and door from the combine rooms
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

	/// <summary>
	/// Generates the mesh for each rooms.
	/// we ignore the rooms' which we merge
	/// </summary>
	/// <param name="roomIndexToIgnore">Room index to ignore.</param>
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

	/// Maze Generation
	/// if we visit  cell for the first time  we will add it to our list and remove it form the last index (same as stack) once's it is fully initialize
	/// if it is visited first time we either create passage or door's
	/// if visited second time we check if it has same region type if so we create a passage also if both have different room we merge them
	/// else it has differnet region so we add wall
	/// if it doesn't have neighbour we simmple put walls
	/// 
    void ProcessNextStep (List<HexCell> hexCellsList)
    {
        HexCell currentCell = hexCellsList[hexCellsList.Count - 1];
       
        if (currentCell.IsFullyInitialize == true)
        {
            hexCellsList.RemoveAt(hexCellsList.Count - 1);
            return;
        }
		/// we should avoid the direction which is already initialize with some type wall, door or passage so that
		/// we can make sure that all of it direction's are visited
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
					/// add all the cell's neighbour rooms to current cell room
					currentCell.room.AddNeighbourRoomFrom (neighbour.room);
					/// also add all of it's cell
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

	/// Create the hex and store its neighbour information
	/// For more infor about hexmap
	/// https://catlikecoding.com/unity/tutorials/hex-map/part-1/
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

	/// <summary>
	/// Find the shortest path from position A to Position B considering region cost into account
	/// </summary>
	/// <returns>The path.</returns>
	/// <param name="startpos">Startpos.</param>
	/// <param name="endPos">End position.</param>
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

	/// <summary>
	/// Refreshs the map.
	/// It will disable all the room except the room that you passed
	/// </summary>
	/// <param name="room">Room.</param>
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