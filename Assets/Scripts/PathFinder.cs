using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class PathFinder
{
    private const int STRAIGHT_MOVE_COST = 10;
    private const int DIAGONAL_MOVE_COST = 14;
    private static Node[,] generatedNodesArray;

    private class Node
    {
        public Vector3 position;
        public List<Node> neighbors;
        public float gCost;
        public float hCost;
        public float FCost => gCost + hCost;
        public Node parent;
        public NodeType nodeType;

        public Node(Vector3 pos)
        {
            position = pos;
            neighbors = new List<Node>();
            gCost = 0f;
            hCost = 0f;
            parent = null;
        }
    }

    public enum NodeType
    {
        Grass,
        Lava
    }

    public static List<Vector3> FindPath(int startI,int startJ, int targetI, int targetJ, Tile[,] modelArray)
    {
        int gridX = modelArray.GetLength(0);
        int gridZ = modelArray.GetLength(1);
        generatedNodesArray = new Node[gridX, gridZ];

        AssignNodes(modelArray);
        Node startNode = GetNode(startI, startJ);
        Node targetNode = GetNode(targetI, targetJ);

        List<Node> openList = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];

            currentNode.hCost = CalculateHeuristicCost(currentNode); //error was here

            for (int i = 0; i < openList.Count; i++)
            {

                if (openList[i].FCost < currentNode.FCost || openList[i].FCost == currentNode.FCost && openList[i].hCost < currentNode.hCost)
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbour in currentNode.neighbors)
            {
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                neighbour.gCost = currentNode.gCost + CalculateDistance(currentNode, neighbour);

                bool isDiagonal = false;
                if(System.Math.Abs(currentNode.position.x - neighbour.position.x) == System.Math.Abs(currentNode.position.z - neighbour.position.z))
                {
                    isDiagonal = true;
                }

                float hCost = CalculateHeuristicCost(neighbour);

                float newMovementCostToNeighbor = currentNode.gCost + Vector3.Distance(currentNode.position, neighbour.position);

                if (newMovementCostToNeighbor < neighbour.gCost || !openList.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbor;
                    neighbour.hCost = hCost;
                    neighbour.parent = currentNode;

                    if (!openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                    }
                }
            }
        }

        // Path not found
        return null;
    }

    private static void AssignNodes(Tile[,] tiles)
    {
        int gridX = tiles.GetLength(0);
        int gridZ = tiles.GetLength(1);

       

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridZ; j++)
            {
                Tile tile = tiles[i, j];

                Node node = new Node(tile.position); // Assign position from the Tile to the Node

                // Assign other properties to the node if needed
                node.nodeType = (NodeType)GetTileType(tile); 

                generatedNodesArray[i, j] = node;
            }
        }

        // Assign neighbors to each node based on the adjacent tiles
        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridZ; j++)
            {
                Node currentNode = generatedNodesArray[i, j];

                // Add the neighboring nodes
                if (i > 0)
                    currentNode.neighbors.Add(generatedNodesArray[i - 1, j]); // Left neighbor
                if (i < gridX - 1)
                    currentNode.neighbors.Add(generatedNodesArray[i + 1, j]); // Right neighbor
                if (j > 0)
                    currentNode.neighbors.Add(generatedNodesArray[i, j - 1]); // Bottom neighbor
                if (j < gridZ - 1)
                    currentNode.neighbors.Add(generatedNodesArray[i, j + 1]); // Top neighbor
            }
        }
    }
    private static Node GetNode(int i, int j)
    {
        return generatedNodesArray[i, j];
    }

    private static List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private static float CalculateHeuristicCost(Node node)
    {
        int moveCost = 10; // the value of the tile itself, does not depends upon path

        // Assign different heuristic costs based on the node type
        switch (node.nodeType)
        {
            case NodeType.Grass:
                return moveCost * 1;

            case NodeType.Lava:
                return moveCost * 3;

            default:
                return moveCost;

        }
    }

    public static Tile.TileType GetTileType(Tile tile) 
    { 
        return tile.tileType;
    }

    private static float CalculateDistance(Node nodeA, Node nodeB)
    {
        return Vector3.Distance(nodeA.position, nodeB.position);
    }


}

