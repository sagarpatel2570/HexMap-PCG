using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

	public HexRoom room;

	Mesh hexMesh;
    [NonSerialized] List<Vector3> vertices;
    [NonSerialized] List<Color> colors;
    [NonSerialized] List<int> triangles;

    Mesh wallMesh;
    [NonSerialized] List<Vector3> wallVertices;
    [NonSerialized] List<Color> wallColors;
    [NonSerialized] List<int> wallTriangles;

    MeshCollider meshCollider;

    public MeshCollider wallMeshCollider;
    public MeshFilter wallMeshFilter;
    public GameObject doorPrefab;


    void Awake () {
		GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        wallMeshFilter.mesh = wallMesh = new Mesh();
		meshCollider = gameObject.AddComponent<MeshCollider>();
		hexMesh.name = "Hex Mesh";
	}

	public void Triangulate  (HexRoom room) {
		this.room = room;
        Clear();
		for (int i = 0; i < room.cells.Count; i++) {
			Triangulate(room.cells[i]);
		}
        Apply();
    }

    public void Apply()
    {
        hexMesh.SetVertices(vertices);
        hexMesh.SetColors(colors);
        hexMesh.SetTriangles(triangles, 0);
        hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;

        wallMesh.SetVertices(wallVertices);
        wallMesh.SetColors(wallColors);
        wallMesh.SetTriangles(wallTriangles, 0);
        wallMesh.RecalculateNormals();
        wallMeshCollider.sharedMesh = wallMesh;
    }

    public void Clear()
    {
        hexMesh.Clear();
        vertices = new List<Vector3>();
        colors = new List<Color>();
        triangles = new List<int>();

        wallMesh.Clear();
        wallVertices = new List<Vector3>();
        wallColors = new List<Color>();
        wallTriangles = new List<int>();
    }

    void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        AddTriangle(center, v1, v2);
        AddTriangleColor(room.region.regionColor);

		TriangulateConnection(direction, cell, v1, v2);

		/*
        if (direction <= HexDirection.SE)
        {
            TriangulateConnection(direction, cell, v1, v2);
        }
        */
        
        HexCell neighbor = cell.GetNeighbor(direction);
        
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;


		if (cell.edgeTypes[(int)direction] == EdgeType.WALL )
        {
            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
            AddConnectionWall(v1, v2, v3, v4, cell);

            Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
            if (nextNeighbor != null)
            {
                v5.y = nextNeighbor.transform.localPosition.y;
            }
            AddConnectionWallTriangle(v2, v4, v5, cell);

        }
    }

    void TriangulateConnection( HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2 )
    {
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;

        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor != null)
        {
            v3.y = v4.y = neighbor.transform.localPosition.y;
        }

		if (direction <= HexDirection.SE) {
			AddQuad (v1, v2, v3, v4);
			AddQuadColor (
				room.region.regionColor,
				room.region.regionColor,
				room.region.regionColor,
				room.region.regionColor
			);
		}
       
        if (cell.edgeTypes[(int)direction] == EdgeType.WALL)
        {
            AddConnectionWall(v1, v2, v3, v4, cell);
        }

        if (cell.edgeTypes[(int)direction] == EdgeType.DOOR)
        {
            GenerateDoor(v1,v2,v3,v4);
        }

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        HexCell previousNeighbour = cell.GetNeighbor(direction.Previous());

        Vector3 v2Top = v2 + Vector3.up * HexMetrics.wallHeight;
        Vector3 v4Top = v4 + Vector3.up * HexMetrics.wallHeight;

		if (direction <= HexDirection.NW )
        {
            Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
			if (nextNeighbor != null) 
			{
				v5.y = nextNeighbor.transform.localPosition.y;
			}
			if (direction <= HexDirection.E) {
				AddTriangle (v2, v4, v5);
				AddTriangleColor (room.region.regionColor);
			}
			if (cell.edgeTypes[(int)direction] == EdgeType.WALL || cell.edgeTypes[(int)direction] == EdgeType.DOOR)
			{
				AddConnectionWallTriangle (v2, v4, v5, cell);
			}

			/*
            if (cell.edgeTypes[(int)direction] == EdgeType.WALL || 
                cell.edgeTypes[(int)direction.Next()] == EdgeType.WALL ||
                cell.GetNeighbor(direction).edgeTypes[(int)direction.Opposite().Previous()] == EdgeType.WALL
                )
            {
                AddConnectionWallTriangle(v2, v4, v5, cell);
            }
            */
        }
    }

    void AddConnectionWallTriangle(Vector3 v2, Vector3 v4, Vector3 othervertices, HexCell cell)
    {
        Vector3 v2Top = v2 + Vector3.up * HexMetrics.wallHeight;
        Vector3 v4Top = v4 + Vector3.up * HexMetrics.wallHeight;
        Vector3 otherverticesTop = othervertices + Vector3.up * HexMetrics.wallHeight;

        AddWallQuad(v2, othervertices, v2Top, otherverticesTop);
        AddWallQuadColor(cell.wallColor, cell.wallColor, cell.wallColor, cell.wallColor);

        AddWallQuad(v4, v2, v4Top, v2Top);
        AddWallQuadColor(cell.wallColor, cell.wallColor, cell.wallColor, cell.wallColor);

        AddWallQuad(othervertices, v4, otherverticesTop, v4Top);
        AddWallQuadColor(cell.wallColor, cell.wallColor, cell.wallColor, cell.wallColor);

        AddWallTriangle(v2Top, v4Top, otherverticesTop);
        AddWallTriangleColor(cell.wallColor);
    }

    void AddConnectionWall(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, HexCell cell)
    {
        Vector3 v1Top = v1 + Vector3.up * HexMetrics.wallHeight;
        Vector3 v2Top = v2 + Vector3.up * HexMetrics.wallHeight;
        Vector3 v3Top = v3 + Vector3.up * HexMetrics.wallHeight;
        Vector3 v4Top = v4 + Vector3.up * HexMetrics.wallHeight;

        AddWallQuad(v1, v2, v1Top, v2Top);
        AddWallQuadColor(cell.wallColor, cell.wallColor, cell.wallColor, cell.wallColor);

        AddWallQuad(v4, v3, v4Top, v3Top);
        AddWallQuadColor(cell.wallColor, cell.wallColor, cell.wallColor, cell.wallColor);

        AddWallQuad(v1Top, v2Top, v3Top, v4Top);
        AddWallQuadColor(cell.wallColor, cell.wallColor, cell.wallColor, cell.wallColor);
    }

    public void GenerateDoor(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {

        Vector3 v1Top = v1 + Vector3.up * HexMetrics.wallHeight;
        Vector3 v2Top = v2 + Vector3.up * HexMetrics.wallHeight;
        Vector3 v3Top = v3 + Vector3.up * HexMetrics.wallHeight;
        Vector3 v4Top = v4 + Vector3.up * HexMetrics.wallHeight;

        Mesh doorMesh = new Mesh();
        List<Vector3> doorVertices = new List<Vector3>();
        List<int> doorTriangles = new List<int>();
        GameObject door = Instantiate(doorPrefab, this.transform);
        door.GetComponent<MeshFilter>().mesh = doorMesh;

        v1 = Perturb(v1);
        v2 = Perturb(v2);
        v3 = Perturb(v3);
        v4 = Perturb(v4);
        v1Top = Perturb(v1Top);
        v2Top = Perturb(v2Top);
        v3Top = Perturb(v3Top);
        v4Top = Perturb(v4Top);

        door.transform.position = v1;
		door.transform.right = (v4 - v2).normalized;
        v1 = door.transform.InverseTransformPoint(v1);
        v2 = door.transform.InverseTransformPoint(v2);
        v3 = door.transform.InverseTransformPoint(v3);
        v4 = door.transform.InverseTransformPoint(v4);
        v1Top = door.transform.InverseTransformPoint(v1Top);
        v2Top = door.transform.InverseTransformPoint(v2Top);
        v3Top = door.transform.InverseTransformPoint(v3Top);
        v4Top = door.transform.InverseTransformPoint(v4Top);


        AddDoorQuad(v1, v2, v1Top, v2Top, doorVertices, doorTriangles);

        AddDoorQuad(v2, v4, v2Top, v4Top, doorVertices, doorTriangles);

        AddDoorQuad(v4, v3, v4Top, v3Top, doorVertices, doorTriangles);

        AddDoorQuad(v3, v1, v3Top, v1Top, doorVertices, doorTriangles);

        AddDoorQuad(v1Top, v2Top, v3Top, v4Top, doorVertices, doorTriangles);

        doorMesh.SetVertices(doorVertices);
        doorMesh.SetTriangles(doorTriangles,0);

    }

    Vector3 Perturb(Vector3 position)
    {
        Vector4 sample = HexMetrics.SampleNoise(position);
        position.x += (sample.x * 2f - 1f) * HexMetrics.cellPerturbStrength;
        position.z += (sample.z * 2f - 1f) * HexMetrics.cellPerturbStrength;
        return position;
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));
        vertices.Add(Perturb(v4));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }

    void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));
        triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

	void AddTriangleColor (Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}

    void AddWallQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = wallVertices.Count;
        wallVertices.Add(Perturb(v1));
        wallVertices.Add(Perturb(v2));
        wallVertices.Add(Perturb(v3));
        wallVertices.Add(Perturb(v4));
        wallTriangles.Add(vertexIndex);
        wallTriangles.Add(vertexIndex + 2);
        wallTriangles.Add(vertexIndex + 1);
        wallTriangles.Add(vertexIndex + 1);
        wallTriangles.Add(vertexIndex + 2);
        wallTriangles.Add(vertexIndex + 3);
    }

    void AddWallQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        wallColors.Add(c1);
        wallColors.Add(c2);
        wallColors.Add(c3);
        wallColors.Add(c4);
    }

    void AddWallTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = wallVertices.Count;
        wallVertices.Add(Perturb(v1));
        wallVertices.Add(Perturb(v2));
        wallVertices.Add(Perturb(v3));
        wallTriangles.Add(vertexIndex);
        wallTriangles.Add(vertexIndex + 1);
        wallTriangles.Add(vertexIndex + 2);
    }

    void AddWallTriangleColor(Color color)
    {
        wallColors.Add(color);
        wallColors.Add(color);
        wallColors.Add(color);
    }

    void AddDoorQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,List<Vector3> doorVertices,List<int> doorTriangles)
    {
        int vertexIndex = doorVertices.Count;
        doorVertices.Add(v1);
        doorVertices.Add((v2));
        doorVertices.Add((v3));
        doorVertices.Add((v4));
        doorTriangles.Add(vertexIndex);
        doorTriangles.Add(vertexIndex + 2);
        doorTriangles.Add(vertexIndex + 1);
        doorTriangles.Add(vertexIndex + 1);
        doorTriangles.Add(vertexIndex + 2);
        doorTriangles.Add(vertexIndex + 3);
    }
    
}