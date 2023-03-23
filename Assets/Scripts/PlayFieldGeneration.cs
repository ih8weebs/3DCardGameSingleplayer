using System.Collections;
using System.Collections.Generic;
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

    enum TileType : int
    {
        Grass = 1,
        Lava
    }

    void Start()
    {
        tileOffset += tileLength;

        Debug.Log("tiles prefab are:");
        Debug.Log("0, grass" + tiles[0]);
        Debug.Log("1, lava" + tiles[1]);


        CreateModelOfPlayfield();
        ShowGeneratedTilesArray();
        //GenerateGrid();
        CreatePathInModel();
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

    private void OldCreateModelOfPlayfield()
    {
        int[,] modelArray = new int[gridX, gridZ];

        GeneratedTilesArray = new Tile[gridX, gridZ];

        modelArray[0, 0] = TileType.Grass.GetHashCode();
        modelArray[gridX - 1, gridZ - 1] = TileType.Grass.GetHashCode();

        // 0 = grass
        // 1 = lava

        Tile tile = new Tile();

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridZ; j++)
            {
                if (modelArray[i, j] != (int)TileType.Grass)
                {
                    int randomInt = Random.Range((int)TileType.Grass, tiles.Length + (int)TileType.Grass);
                    TileType randomTile = (TileType)randomInt;

                    switch (randomTile)
                    {
                        case TileType.Grass:
                            {
                                modelArray[randomInt, j] = TileType.Grass.GetHashCode();
                                tile = grassTile;
                                tile.tileName = "Grass";
                                tile.costMultiplier = 1;
                                tile.tilePrefab = tiles[0];

                                break;
                            }
                        case TileType.Lava:
                            {
                                modelArray[randomInt, j] = TileType.Lava.GetHashCode();
                                tile = lavaTile;
                                tile.tileName = "Lava";
                                tile.costMultiplier = 10;
                                tile.tilePrefab = tiles[1];
                                break;
                            }
                        default:
                            break;
                    }
                }
                else if (!startTileExists && i == 0 && j == 0)
                {
                    tile = grassTile;
                    tile.tileName = "Start";
                    tile.costMultiplier = 1;
                    startTileExists = true;
                }
                else if (!bossTileExists && i == gridX - 1 && j == gridZ - 1)
                {
                    tile = grassTile;
                    tile.tileName = "Boss (End)";
                    tile.costMultiplier = 1;
                    bossTileExists = true;
                }

                GeneratedTilesArray.SetValue(tile, i, j);

            }
        }



    }

    private void CreateModelOfPlayfield()
    {
        int[,] modelArray = new int[gridX, gridZ];

        GeneratedTilesArray = new Tile[gridX, gridZ];

        modelArray[0, 0] = TileType.Grass.GetHashCode();
        modelArray[gridX - 1, gridZ - 1] = TileType.Grass.GetHashCode();

        // 0 = grass
        // 1 = lava

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridZ; j++)
            {
                if (modelArray[i, j] != (int)TileType.Grass)
                {
                    int randomInt = Random.Range((int)TileType.Grass, (int)TileType.Lava + 1);
                    TileType randomTile = (TileType)randomInt;

                    switch (randomTile)
                    {
                        case TileType.Grass:
                            {
                                modelArray[i, j] = TileType.Grass.GetHashCode();
                                Tile tile = ScriptableObject.CreateInstance<Tile>();
                                tile.tileName = "Grass";
                                tile.costMultiplier = 1;
                                tile.tilePrefab = tiles[0];
                                Debug.Log("Tile prefab: " + tile.tilePrefab);
                                GeneratedTilesArray.SetValue(tile, i, j);
                                break;
                            }
                        case TileType.Lava:
                            {
                                modelArray[i, j] = TileType.Lava.GetHashCode();
                                Tile tile = ScriptableObject.CreateInstance<Tile>();
                                tile.tileName = "Lava";
                                tile.costMultiplier = 10;
                                tile.tilePrefab = tiles[1];
                                Debug.Log("Tile prefab: " + tile.tilePrefab);
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
                    GeneratedTilesArray.SetValue(tile, i, j);
                    startTileExists = true;
                }
                else if (!bossTileExists && i == gridX - 1 && j == gridZ - 1)
                {
                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                    tile.tileName = "Boss (End)";
                    tile.costMultiplier = 1;
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
                Debug.Log(GeneratedTilesArray[i, j].tileName + " " + i + " " + j);
            }
        }
    }

    void OldCreatePathInModel()
    {
        PathFinder pathFinder = new PathFinder(GeneratedTilesArray, gridX, gridZ);

        List<PathNode> path;

        path = pathFinder.FindPath(0, 0, 9, 9);

        if (path != null)
        {
            Debug.Log("Here is the path: ");
            Debug.Log("Here is the path: ");
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
            int pathX; 
            int pathZ;

            for (int x = 0; x < gridX; x++)
            {
                for (int z = 0; z < gridZ; z++)
                {
                    if (pathCounter < path.Count)
                    {
                        
                        pathX = path[pathCounter].x; 
                        pathZ = path[pathCounter].z;

                        if (x == pathX && z == pathZ)
                        {
                            GeneratedTilesArray[x, z] = grassTile;
                            GeneratedTilesArray[x, z].name = "ChangedTile";
                            GeneratedTilesArray[x, z].costMultiplier = path[pathCounter].costMultiplier;
                            GeneratedTilesArray[x, z].isWalkable = path[pathCounter].isWalkable;
                            if (pathCounter < path.Count) { pathCounter++; }
                        }
                    }
                }
            }

            Debug.Log("end of path: ");
            Debug.Log("end of path: ");
            Debug.Log("end of path: ");
        }
        else
        {
            Debug.Log("Path is null!!!");
        }

        SpawnTiles();

    }

    void CreatePathInModel()
    {
        PathFinder pathFinder = new PathFinder(GeneratedTilesArray, gridX, gridZ);

        List<PathNode> path;

        path = pathFinder.FindPath(0, 0, 9, 9);

        if (path != null && path.Count > 0)
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
        }
        else
        {
            Debug.Log("Path is null or empty!!!");
        }

        Debug.Log("New Array: ");
        ShowGeneratedTilesArray();

        SpawnTiles();
    }



    void DebugLogArray()
    {
        for (int x = 0; x < GeneratedTilesArray.GetLength(0); x++)
        {
            for (int z = 0; z < GeneratedTilesArray.GetLength(1); z++)
            {
                Debug.Log("x: " + x + " z: " + z + " " + GeneratedTilesArray[x, z].name);
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
                        Debug.Log("Tile prefab not found for Lava tile. Assigning default lava prefab.");
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
        int tileIndex = Random.Range(0, tiles.Length);
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

}