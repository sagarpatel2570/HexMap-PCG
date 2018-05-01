using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexRoom  {

	public HexRegion region;
    public List<HexCell> cells = new List<HexCell>();
	public List<HexRoom> neighbourRoom = new List<HexRoom> ();

    public HexRoom (RegionType regionType,HexRegions hexRegions)
    {
		float randomValue = Random.value;
		HexRegion currentRegion = null;

		foreach (HexRegion region in hexRegions.regions) {
			if (region.IsInRange(randomValue)) {
				currentRegion = region;
				break;
			}
		}
		if(currentRegion.type == regionType)
        {
			currentRegion = hexRegions.regions [((int)(currentRegion.type) + 1) % hexRegions.regions.Count];
        }

		this.region = currentRegion;
        
    }

	public void AddNeighbour (HexRoom room)
	{
		if (!neighbourRoom.Contains (room) ) {
			neighbourRoom.Add (room);
		}
	}

    public void AddCell (HexCell cell)
    {
		cells.Add (cell);

		cell.node.movementCost = region.movementCost;
    }

	public void AddCellFromRoom (HexRoom room) {

		int totalCellsInRoom = room.cells.Count;
		for (int i = 0; i < totalCellsInRoom; i++) {
			HexCell cell = room.cells [i];
			AddCell(cell);
			cell.room = this;
		}
	}

	public void AddNeighbourRoomFrom(HexRoom room)
	{
		foreach (HexRoom r in room.neighbourRoom.ToArray()) {
			if (!neighbourRoom.Contains (r) && r != this) {
				neighbourRoom.Add (r);
			}

			if (r.neighbourRoom.Contains (room)) {
				r.neighbourRoom.RemoveAll (x => x == room);
				r.neighbourRoom.Add (this);
			}
		}
	}

	public HexRoom GetNeighbourWIthMaxCells ()
	{
		neighbourRoom.RemoveAll (r => r == this);
		neighbourRoom.Sort (delegate (
			HexRoom r1,HexRoom r2
		) {
			return r2.cells.Count.CompareTo(r1.cells.Count);
		});
		return neighbourRoom [0];
	}
}
