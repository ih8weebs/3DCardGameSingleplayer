using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Animations;

public class PathFinder
{
    private PathNode[,] nodesArray;
    private readonly int maxX;
    private readonly int maxZ;
    private const int STRAIGHT_MOVE_COST = 10;
    private const int DIAGONAL_MOVE_COST = 14;


    public PathFinder(Tile[,] generatedTiles, int maxX, int maxZ)
    {
        this.maxX = maxX;
        this.maxZ = maxZ;
        nodesArray = new PathNode[maxX, maxZ];

        for (int x = 0; x < maxX; x++)
        {
            for (int z = 0; z < maxZ; z++)
            {
                nodesArray[x, z] = new PathNode(x, z, generatedTiles[x, z].costMultiplier, isWalkable: generatedTiles[x, z].isWalkable);
            }
        }
        CalculateNodesF();
        /*Debug.Log("IN PathFinder");
        for (int x = 0; x < maxX; x++)
        {
            for (int z = 0; z < maxZ; z++)
            {
                Debug.Log("x = " + x + ", z = " + z + ", fCost = " + nodesArray[x, z].fCost + ", gCost = " + nodesArray[x, z].gCost + ", hCost = " + nodesArray[x, z].hCost);
            }
        }*/

    }

    public List<PathNode> FindPathOld(int startX, int startZ, int targetX, int targetZ)
    {
        PathNode startNode = nodesArray[startX, startZ];
        PathNode endNode = nodesArray[targetX, targetZ];

        List<PathNode> openList = new List<PathNode> { startNode };
        List<PathNode> ClosedList = new List<PathNode>();



        for (int x = 0; x < targetX + 1; x++)
        {
            for (int z = 0; z < targetZ + 1; z++)
            {
                PathNode pathnode = nodesArray[x, z];
                pathnode.gCost = int.MaxValue;
                pathnode.CalculateFCost();
                pathnode.cameFromeNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            ClosedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (ClosedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.isWalkable)
                {
                    ClosedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromeNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        return null;
    }


    private PathNode GetLowestFNode(List<PathNode> list)
    {
        if(list == null || list.Count == 0)
        {
            return null;
        }

        PathNode curNode = list[0];

        for(int i = 1; i < list.Count; i++)
        {
            if(curNode.fCost > list[i].fCost)
            {
                curNode = list[i];
            }
        }

        return curNode;
    }

    public List<PathNode> FindPath(int startX, int startZ, int targetX, int targetZ)
    {
        CalculateNodesF();

        PathNode startNode = nodesArray[startX, startZ];
        PathNode endNode = nodesArray[targetX, targetZ];

        List<PathNode> openList = new List<PathNode> { startNode };
        List<PathNode> closedList = new List<PathNode>();


        List<PathNode> path = new List<PathNode>();


        while (true)
        {
            PathNode curNode = GetLowestFCostNode(openList);
            openList.Remove(curNode);
            closedList.Add(curNode);

            //curNode.x == endNode.x & curNode.z == endNode.z
            if (curNode == endNode)
            {
                return path;
            }

            List<PathNode> neighboursList = GetNeighbourList(curNode);

            for (int i = 0; i < neighboursList.Count; i++)
            {
                if (!closedList.Contains(neighboursList[i]) || !openList.Contains(neighboursList[i]))
                {
                    openList.Add(neighboursList[i]);
                }
            }

        }

        return null;
    }

    public List<PathNode> FindPathDima(int startX, int startZ, int targetX, int targetZ)
    {
        CalculateNodesF();

        PathNode startNode = nodesArray[startX, startZ];
        PathNode endNode = nodesArray[targetX, targetZ];

        List<PathNode> openList = new List<PathNode> { startNode };
        List<PathNode> closedList = new List<PathNode>();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            List<PathNode> neighboursList = GetNeighbourList(currentNode);

            foreach (PathNode neighbourNode in neighboursList)
            {
                if (!neighbourNode.isWalkable || closedList.Contains(neighbourNode))
                {
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);

                if (!openList.Contains(neighbourNode))
                {
                    openList.Add(neighbourNode);
                }
                else if (tentativeGCost >= neighbourNode.gCost)
                {
                    continue;
                }

                neighbourNode.gCost = tentativeGCost;
                neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                neighbourNode.fCost = neighbourNode.gCost + neighbourNode.hCost;
                neighbourNode.cameFromeNode = currentNode;
            }
        }

        return null;
    }


    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        if (currentNode.x - 1 >= 0)
        {
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.z));
            if (currentNode.z - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.z - 1));
            }
            if (currentNode.z + 1 < maxZ)
            {
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.z + 1));
            }
        }
        if (currentNode.x + 1 < maxX)
        {
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.z));
            if (currentNode.z - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.z - 1));
            }
            if (currentNode.z + 1 < maxZ)
            {
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.z + 1));
            }
        }
        if (currentNode.z - 1 >= 0)
        {
            neighbourList.Add(GetNode(currentNode.x, currentNode.z - 1));
        }
        if (currentNode.z + 1 < maxZ)
        {
            neighbourList.Add(GetNode(currentNode.x, currentNode.z + 1));
        }

        Debug.Log("Neighbour List for node ({0}, {1}):" + " " + currentNode.x + " " + currentNode.z);
        foreach (PathNode neighbor in neighbourList)
        {
            Debug.Log(" - ({0}, {1}) with f-cost {2}" + " " + neighbor.x + " " + neighbor.z + " " + neighbor.fCost);
        }


        return neighbourList;
    }

    private PathNode GetNode(int x, int z)
    {
        return nodesArray[x, z];
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromeNode != null)
        {
            path.Add(currentNode.cameFromeNode);
            currentNode = currentNode.cameFromeNode;
        }
        path.Reverse();
        return path;

    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }

        Debug.Log("Lowest f-cost node: ({0}, {1}) with f-cost {2}" + " " + lowestFCostNode.x + " " + lowestFCostNode.z + " " + lowestFCostNode.fCost);


        return lowestFCostNode;

    }
    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int zDistance = Mathf.Abs(a.z - b.z);
        int remaining = Mathf.Abs(xDistance - zDistance);

        return DIAGONAL_MOVE_COST * Mathf.Min(xDistance, zDistance) + STRAIGHT_MOVE_COST * remaining;
    }
    void CalculateNodesF()
    {
        for (int x = 0; x < maxX; x++)
        {
            for (int z = 0; z < maxZ; z++)
            {
                CalculateG(nodesArray[x, z], nodesArray[0, 0]);
                CalculateH(nodesArray[x, z], nodesArray[maxX - 1, maxZ - 1]);
                nodesArray[x, z].CalculateFCost();
            }
        }
    }

    void CalculateG(PathNode node, PathNode startNode)
    {
        if (node.x == nodesArray[0, 0].x & node.z == nodesArray[0, 0].z)
        {
            node.gCost = 0;
            node.hCost = 0;
            return;
        }

        if (node.x == nodesArray[maxX - 1, maxZ - 1].x & node.z == nodesArray[maxX - 1, maxZ - 1].z)
        {
            node.gCost = 0;
            node.hCost = 0;
            return;
        }
        int tempX = node.x;
        int tempZ = node.z;
        while (tempX != startNode.x || tempZ != startNode.z)
        {
            if (tempX > startNode.x)
            {
                if (tempZ > startNode.z)
                {
                    tempX -= 1;
                    tempZ -= 1;
                    node.gCost += DIAGONAL_MOVE_COST * nodesArray[tempX, tempZ].costMultiplier;
                }
                else
                {
                    tempX -= 1;
                    node.gCost += STRAIGHT_MOVE_COST * nodesArray[tempX, tempZ].costMultiplier;
                }
            }
            else if (tempZ > startNode.z)
            {
                tempZ -= 1;
                node.gCost += STRAIGHT_MOVE_COST * nodesArray[tempX, tempZ].costMultiplier;
            }
        }
    }

    void CalculateH(PathNode node, PathNode finishNode)
    {
        if (node.x == nodesArray[0, 0].x & node.z == nodesArray[0, 0].z)
        {
            node.gCost = 0;
            node.hCost = 0;
            return;
        }

        if (node.x == nodesArray[maxX - 1, maxZ - 1].x & node.z == nodesArray[maxX - 1, maxZ - 1].z)
        {
            node.gCost = 0;
            node.hCost = 0;
            return;
        }
        int tempX = node.x;
        int tempZ = node.z;
        while (tempX != finishNode.x || tempZ != finishNode.z)
        {
            if (tempX < finishNode.x)
            {
                if (tempZ < finishNode.z)
                {
                    tempX += 1;
                    tempZ += 1;
                    node.hCost += DIAGONAL_MOVE_COST * nodesArray[tempX, tempZ].costMultiplier;
                }
                else
                {
                    tempX += 1;
                    node.hCost += STRAIGHT_MOVE_COST * nodesArray[tempX, tempZ].costMultiplier;
                }
            }
            else if (tempZ < finishNode.z)
            {
                tempZ += 1;
                node.hCost += STRAIGHT_MOVE_COST * nodesArray[tempX, tempZ].costMultiplier;
            }
        }
    }

}
