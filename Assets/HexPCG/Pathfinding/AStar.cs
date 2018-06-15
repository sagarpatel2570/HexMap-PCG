using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

/// <summary>
/// A star.  Reference link https://www.redblobgames.com/pathfinding/a-star/introduction.html
/// I used Priority Queue To store the cost to reach neighbour 
/// This is the link https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
/// </summary>
public static class AStar {

	static List<Node> path = new List<Node>();

	public static List<Node> DoPathFind_AStar (Node startNode,Node endNode,Node[] hexMapNodes) {

		// if startnode is equal to endnode
		// no need of pathfinding
		if (startNode == endNode) {
			Debug.LogError ("start and end positions are same");
			return null;
		}

		// clear everything 
		path.Clear ();
		foreach (Node n in hexMapNodes) {
			n.cameFrom = null;
		}

		SimplePriorityQueue<Node> nodes = new SimplePriorityQueue<Node> ();
		nodes.Enqueue (startNode,startNode.costSoFar);

		while (nodes.Count > 0) {
			Node currentNode = nodes.Dequeue ();
			if (currentNode == endNode) {
				// we found the path break the loop !!!
				RetracePath(endNode,startNode);
				return path;
			}
				
			for (int i = 0; i < 6; i++) {

				HexCell neighbourCell = currentNode.hexCell.GetNeighbor ((HexDirection)i);
				if (neighbourCell == null) {
					continue;
				}

				Node n = neighbourCell.node;
				float neighbourCost = 0;
				/// if it is wall we should not be able to proceed hence adding max value
				if (currentNode.hexCell.edgeTypes [i] == EdgeType.WALL) {
					neighbourCost = float.MaxValue;
					/// for door's there should be a perfect calculation which consite's it speed and wait time into account in movement cost TODO !!!
				}else if(currentNode.hexCell.edgeTypes [i] == EdgeType.DOOR) {
					neighbourCost = 5;
				}

				float newCost = currentNode.costSoFar + ((n.movementCost == 0) ? 10000 : n.movementCost ) + neighbourCost;
				if (n.cameFrom == null ||  newCost < n.costSoFar) {
					n.costSoFar = newCost;
					float priority = n.costSoFar + HeuristicDistance (endNode, n) ;

					if (n.cameFrom != null) {
						if (nodes.Contains (n)) {
							nodes.UpdatePriority (n, priority);
						} else {
							nodes.Enqueue (n, priority);
						}
					} else {
						nodes.Enqueue (n, priority);
					}
					n.cameFrom = currentNode;
				}
			}
		}

		// if we reach her mean's we were unable to find path 
		Debug.LogError("Unable to find Path");
		return null;

	}
		
	static void RetracePath (Node endNode,Node startNode) {
		
		path.Add (endNode);
		Node cameFrom = endNode.cameFrom;
		while (cameFrom != startNode) {
			path.Add (cameFrom);
			cameFrom = cameFrom.cameFrom;
		}
		path.Add (startNode);

		path.Reverse ();
	}

	static float HeuristicDistance (Node a , Node b ){
		return a.hexCell.coordinates.DistanceTo (b.hexCell.coordinates);
	}
}

/// <summary>
/// Node. This is simple data for hexcell which store's movement cost 
/// and the neighbour it came from
/// and cost so far to reach that node
/// </summary>
public class Node {
	public HexCell hexCell;
	public float movementCost;
	public Node cameFrom;
	public float costSoFar;

	public Node (HexCell cell){
		this.hexCell = cell;
		movementCost = 1;
	}
}
