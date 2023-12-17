using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class PlayFieldGeneration : MonoBehaviour
{
    private float tileLength = 1f;
    
    public GameObject[] tiles;
    [SerializeField] private int gridX;
    [SerializeField] private int gridZ;
    [SerializeField] private float tileOffset = 0.1f;

    [SerializeField] private GameObject startTilePrefab;
    [SerializeField] private GameObject bossTilePrefab;

    public Tile grassTile;
    public Tile lavaTile;


    public Tile[,] GeneratedTilesArray;

    public Vector3 gridOrigin = Vector3.zero;

    bool startTileExists = false;
    bool bossTileExists = false;

    void Start()
    {
        tileOffset += tileLength;

        UnityEngine.Debug.Log("tiles prefab are:");
        UnityEngine.Debug.Log("0, grass" + tiles[0]);
        UnityEngine.Debug.Log("1, lava" + tiles[1]);


        CreateModelOfPlayfield();
        //ShowGeneratedTilesArray();
        //GenerateGrid();
        CreatePathInModel();
        ClearMemory();
    }

    void GenerateGrid()
    {
        GeneratedTilesArray = new Tile[gridX, gridZ];

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridZ; j++)
            {
                if (!startTileExists)
                {
                    GameObject startTile = Instantiate(startTilePrefab, new Vector3(i * tileOffset, 0, j * tileOffset) + gridOrigin, Quaternion.identity);
                    startTileExists = true;
                    Tile startTileAsTile = new Tile { tilePrefab = startTile };
                    startTileAsTile.costMultiplier = 1;
                    GeneratedTilesArray.SetValue(startTileAsTile, i, j);
                }
                else if (!bossTileExists && i == gridX - 1 && j == gridZ - 1)
                {
                    GameObject bossTile = Instantiate(bossTilePrefab, new Vector3(i * tileOffset, 0, j * tileOffset) + gridOrigin, Quaternion.identity);
                    bossTileExists = true;
                    Tile bossTileAsTile = new Tile { tilePrefab = bossTile };
                    bossTileAsTile.costMultiplier = 1;
                    GeneratedTilesArray.SetValue(bossTileAsTile, i, j);
                }
                else
                {
                    Vector3 spawnPosition = new Vector3(i * tileOffset, 0, j * tileOffset) + gridOrigin;
                    ChosingTile(spawnPosition, Quaternion.identity, i, j);
                }
            }
        }

    }

    private void CreateModelOfPlayfield()
    {
        int[,] modelArray = new int[gridX, gridZ];

        GeneratedTilesArray = new Tile[gridX, gridZ];

        modelArray[0, 0] = Tile.TileType.Grass.GetHashCode();
        modelArray[gridX - 1, gridZ - 1] = Tile.TileType.Grass.GetHashCode();

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridZ; j++)
            {
                if (modelArray[i, j] != (int)Tile.TileType.Grass)
                {
                    int randomInt = UnityEngine.Random.Range((int)Tile.TileType.Grass, (int)Tile.TileType.Lava + 1);
                    Tile.TileType randomTile = (Tile.TileType)randomInt;

                    switch (randomTile)
                    {
                        case Tile.TileType.Grass:
                            {
                                modelArray[i, j] = Tile.TileType.Grass.GetHashCode();
                                Tile tile = ScriptableObject.CreateInstance<Tile>();
                                tile.tileName = "Grass";
                                tile.costMultiplier = 1;
                                tile.isGrass = true;
                                tile.tilePrefab = tiles[0];
                                tile.tileType = randomTile;
                                tile.position = new Vector3(i * tileOffset, 0, j * tileOffset) + gridOrigin;
                                //Debug.Log("Tile prefab: " + tile.tilePrefab);
                                GeneratedTilesArray.SetValue(tile, i, j);
                                break;
                            }
                        case Tile.TileType.Lava:
                            {
                                modelArray[i, j] = Tile.TileType.Lava.GetHashCode();
                                Tile tile = ScriptableObject.CreateInstance<Tile>();
                                tile.tileName = "Lava";
                                tile.isGrass = false;
                                tile.costMultiplier = 10;
                                tile.tileType = randomTile;
                                tile.position = new Vector3(i * tileOffset, 0, j * tileOffset) + gridOrigin;
                                tile.tilePrefab = tiles[1];
                                //Debug.Log("Tile prefab: " + tile.tilePrefab);
                                GeneratedTilesArray.SetValue(tile, i, j);
                                break;
                            }
                        default:
                            break;
                    }
                }
                else if (!startTileExists && i == 0 && j == 0)
                {
                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                    tile.tileName = "Start";
                    tile.costMultiplier = 1;
                    tile.isGrass = true;
                    tile.tileType = Tile.TileType.Grass;
                    tile.position = new Vector3(i * tileOffset, 0, j * tileOffset) + gridOrigin;
                    GeneratedTilesArray.SetValue(tile, i, j);
                    startTileExists = true;
                }
                else if (!bossTileExists && i == gridX - 1 && j == gridZ - 1)
                {
                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                    tile.tileName = "Boss (End)";
                    tile.costMultiplier = 1;
                    tile.isGrass = true;
                    tile.tileType = Tile.TileType.Grass;
                    tile.position = new Vector3(i * tileOffset, 0, j * tileOffset) + gridOrigin;
                    GeneratedTilesArray.SetValue(tile, i, j);
                    bossTileExists = true;
                }



            }
        }
    }



    private void ShowGeneratedTilesArray()
    {
        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridZ; j++)
            {
                UnityEngine.Debug.Log(GeneratedTilesArray[i, j].tileName + " " + i + " " + j);
            }
        }
    }

    private void ShowListV3(List<Vector3> list)
    {
        int i = 0;

        UnityEngine.Debug.Log("Path is: ");

        foreach (var item in list)
        {
            UnityEngine.Debug.Log("element "+ i + ": (" + list[i].x + "; " + list[i].y + "; " + list[i].z + ";) ");
            i++;
        }

        UnityEngine.Debug.Log("Path ended.");
    }

    void CreatePathInModel()
    {
        Vector3 startTilePosition = new Vector3(0 * tileOffset , 0, 0 * tileOffset) + gridOrigin;
        Vector3 bossTilePosition = new Vector3((gridX - 1) * tileOffset, 0, (gridZ - 1) * tileOffset) + gridOrigin; 
        if(startTilePosition != GeneratedTilesArray[0,0].position || bossTilePosition != GeneratedTilesArray[gridX-1, gridZ-1].position)
        {
            UnityEngine.Debug.Log("CreatePathInModel() ERROR: vector positions didn't match");
            return;
        }

        //coordinates of elemenst in GeneratedTilesArray
        int startCoordinatesI = 0;
        int startCoordinatesJ = 0;
        int targetCoordinatesI = gridX-1;
        int targetCoordinatesj = gridZ-1;

        List<Vector3> path = PathFinder.FindPath(startCoordinatesI, startCoordinatesJ, targetCoordinatesI, targetCoordinatesj, GeneratedTilesArray);
        if (path != null)
        {
            ShowListV3(path);
            ChangeLavaToGrass(path);
        }

        /*if (path != null && path.Count > 0)
        {
            Debug.Log("Here is the path: ");

            for (int i = 0; i < path.Count; i++)
            {
                Debug.Log("X: " + path[i].x + " Z: " + path[i].z);

                if (path[i].costMultiplier == 10)
                {
                    path[i].costMultiplier = 1;
                }
            }

            int pathCounter = 0;

            for (int x = 0; x < gridX; x++)
            {
                for (int z = 0; z < gridZ; z++)
                {
                    // If the current tile is in the path
                    if (pathCounter < path.Count && x == path[pathCounter].x && z == path[pathCounter].z)
                    {
                        // If the current tile is a lava tile
                        if (GeneratedTilesArray[x, z].name == "LavaTile")
                        {
                            GeneratedTilesArray[x, z] = grassTile;
                            GeneratedTilesArray[x, z].name = "GrassTile";
                            GeneratedTilesArray[x, z].costMultiplier = path[pathCounter].costMultiplier;
                            GeneratedTilesArray[x, z].isWalkable = path[pathCounter].isWalkable;
                        }
                        pathCounter++;
                    }
                    // If the current tile is not in the path and it's a grass tile, keep it as is
                    else if (GeneratedTilesArray[x, z].name == "GrassTile")
                    {
                        GeneratedTilesArray[x, z].costMultiplier = 1;
                        GeneratedTilesArray[x, z].isWalkable = true;
                    }
                    // If the current tile is not in the path and it's a lava tile, keep it as is
                    else if (GeneratedTilesArray[x, z].name == "LavaTile")
                    {
                        GeneratedTilesArray[x, z].costMultiplier = 10;
                        GeneratedTilesArray[x, z].isWalkable = false;
                    }
                }
            }

            Debug.Log("End of path.");
        }*/

        else
        {
            UnityEngine.Debug.Log("CreatePathInModel() ERROR: Path is null or empty");
        }

        
       /* Debug.Log("New Array: ");
        ShowGeneratedTilesArray();*/

        SpawnTiles();
    }

    private void ChangeLavaToGrass(List<Vector3> path)
    {
        int counter = 0;

        foreach (Vector3 position in path)
        {
            int x = (int)(position.x / tileOffset); //hardcoded moment 10*1.1(tileOffset) = 11 => int 11 for 10th element (won`t work for grid > 10); / tileOffset fixes it
            int z = (int)(position.z / tileOffset);
            

            if (GeneratedTilesArray[x, z].isGrass == false)
            {
                GeneratedTilesArray[x, z].isGrass = true;
                GeneratedTilesArray[x, z].tileName = "Grass";
                GeneratedTilesArray[x, z].costMultiplier = 1;
                GeneratedTilesArray[x, z].tilePrefab = tiles[0];
                counter++;
            }
        }
        UnityEngine.Debug.Log("Transformed " + counter + " tiles.");

    }

    void DebugLogArray()
    {
        for (int x = 0; x < GeneratedTilesArray.GetLength(0); x++)
        {
            for (int z = 0; z < GeneratedTilesArray.GetLength(1); z++)
            {
                UnityEngine.Debug.Log("x: " + x + " z: " + z + " " + GeneratedTilesArray[x, z].name);
            }
        }
    }

    private void SpawnTiles()
    {
        startTileExists = false;
        bossTileExists = false;

        for (int i = 0; i < GeneratedTilesArray.GetLength(0); i++)
        {
            for (int j = 0; j < GeneratedTilesArray.GetLength(1); j++)
            {
                if (!startTileExists)
                {
                    GameObject startTile = Instantiate(startTilePrefab, new Vector3(i * tileOffset, 0, j * tileOffset) + gridOrigin, Quaternion.identity);
                    startTileExists = true;
                    Tile startTileAsTile = new Tile { tilePrefab = startTile };
                    startTileAsTile.costMultiplier = 1;
                    GeneratedTilesArray.SetValue(startTileAsTile, i, j);
                }
                else if (!bossTileExists && i == gridX - 1 && j == gridZ - 1)
                {
                    GameObject bossTile = Instantiate(bossTilePrefab, new Vector3(i * tileOffset, 0, j * tileOffset) + gridOrigin, Quaternion.identity);
                    bossTileExists = true;
                    Tile bossTileAsTile = new Tile { tilePrefab = bossTile };
                    bossTileAsTile.costMultiplier = 1;
                    GeneratedTilesArray.SetValue(bossTileAsTile, i, j);
                }
                else
                {
                    if (GeneratedTilesArray[i, j].tilePrefab == null)
                    {
                        UnityEngine.Debug.Log("Tile prefab not found for Lava tile. Assigning default lava prefab.");
                        GeneratedTilesArray[i, j].tilePrefab = tiles[0];
                    }
                    Vector3 spawnPosition = new Vector3(i * tileOffset, 0, j * tileOffset) + gridOrigin;
                    GameObject randomTile = Instantiate(GeneratedTilesArray[i, j].tilePrefab, spawnPosition, Quaternion.identity);
                }
            }
        }
    }

    void ChosingTile(Vector3 spawnPosition, Quaternion spawnRotation, int i, int j)
    {
        int tileIndex = UnityEngine.Random.Range(0, tiles.Length);
        GameObject randomTile = Instantiate(tiles[tileIndex], spawnPosition, spawnRotation);

        Tile tile = new Tile() { tilePrefab = randomTile };
        if (tileIndex == 0)
        {
            tile = grassTile;
            tile.tileName = "Grass";
            tile.costMultiplier = 1;

        }
        else
        {
            tile = lavaTile;
            tile.tileName = "Lava";
            tile.costMultiplier = 10;

        }
        GeneratedTilesArray.SetValue(tile, i, j);
    }

    void ClearMemory()
    {
        GeneratedTilesArray = null;
        tiles = null;
        GC.Collect();
    }

}