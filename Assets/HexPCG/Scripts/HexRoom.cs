using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HexRoom  {

	public HexRegion region;
    public List<HexCell> cells = new List<HexCell>();

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

    public void AddCell (HexCell cell)
    {
        cells.Add(cell);
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
}
