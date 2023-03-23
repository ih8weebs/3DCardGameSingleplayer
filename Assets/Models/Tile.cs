using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

[CreateAssetMenu(fileName = "New Tile", menuName = "Tile", order = 0)]
[DataContract]
public class Tile : ScriptableObject
{

    public string tileName { get; set; }
    
    public GameObject tilePrefab { get; set; }
    
    public int costMultiplier { get; set; }

    public bool isWalkable { get; set; }


}
