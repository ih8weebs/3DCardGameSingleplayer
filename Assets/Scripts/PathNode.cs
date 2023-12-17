using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PathNode
{
    public int x;
    public int z;
    public int gCost;
    public int hCost;
    public int fCost;
    public PathNode cameFromeNode;
    public bool isWalkable;

    public PathNode(int x, int z, int costMultiplier, bool isWalkable)
    {
        this.x = x;
        this.z = z;
        this.isWalkable = isWalkable;
    }
    
    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}
