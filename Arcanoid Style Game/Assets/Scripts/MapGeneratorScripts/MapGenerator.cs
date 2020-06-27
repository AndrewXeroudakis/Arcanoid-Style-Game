using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class MapGenerator : MonoBehaviour
{
    // select unbreakable pattern
    enum UnbreakablePattern { NONE = 0, RANDOM, MAZE };
    public int unbreakablePattern = 1;
    float[,] unbreakableMap;
    int wallWeight = 1;
    public int emptyWeight = 2;
    public bool mirrorUnbreakable = true;
    public struct Placement
    {
        public const int solid = 0;
        public const int hollow = 1;
    }
    private float wall = 2f;
    private float explosive = 1f;
    private float empty = 0f;
    private float pathColor = 0.5f;
    int[] placement = new int[] { 2, 0 }; // wall, empty
    int[] placementExplosive = new int[] { 1, 0 }; // explosive, empty
    private float unbreakableBrickValue = -1f;

    enum ExplosivePattern { NONE = 0, RANDOM };
    public int explosivePattern = 1;
    float[,] explosiveMap;
    int explosiveWeight = 1;
    public int emptyExplosiveWeight = 2;
    public bool mirrorExplosive = true;
    private float explosiveBrickValue = -2f;

    private Vector2Int[] directions = new Vector2Int[] {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

    int[] placementWeights;
    int placementWeightTotal;

    // select fall off pattern
    enum FallOffPattern { NONE = 0, CENTER, BOTTOM, TOP, SIDES, VERTICAL_SLICES, HORIZONTAL_SLICES };
    public int fallOffPattern = 2;
    public float edge = 0.5f; // edge 0 - 20, the larger the edge the harder the limit of bricks
    public float size = 0.01f; // size 0 - 20, the larger the size the more bricks

    public bool generateBricks = false;

    // set brick space vars
    private Vector2 startingPositionMin = new Vector2(0.5f, 6.75f);
    private Vector2 startingPositionMax = new Vector2(4.5f, 12.75f);
    public Vector2 startingPosition = new Vector2(0.5f, 8f); // x = startingPositionMin.x, y = startingPositionMax.y - startingPositionMin.y

    private Vector2Int brickSpaceDimensionsMin = new Vector2Int(3, 4);
    private Vector2Int brickSpaceDimensionsMax = new Vector2Int(9, 13);
    public Vector2Int brickSpaceDimensions = new Vector2Int(9, 13);

    public Vector2 brickOffset = new Vector2(1f, 0.5f);
    public GameObject brick;
    public GameObject unbreakableBrick;
    public GameObject explosiveBrick;

    // noise
    public int mapWidth = 9;
    public int mapHeight = 16;
    public float mapScale = 5f;
    public int octaves = 3;
    [Range(0, 1)]
    public float persistance = 1;
    public float lacunarity = 1;
    public int seed = 0;
    public Vector2 offset = new Vector2 ( 0, 0 );

    // hits
    //private float hitsPercentage1 = 0.6f;
    //private float hitsPercentage2 = 0.9f;
    private float brickFactor = 0;
    //private float hitsPercentage3 = 0.9f;

    public bool autoUpdate;

    public void Generate()
    {
        mapWidth = brickSpaceDimensions.x;
        mapHeight = brickSpaceDimensions.y;

        // generate unbreakable brick map
        unbreakableMap = GenerateUnbreakableMap();

        // generate explosive brick map
        explosiveMap = GenerateExplosiveMap();

        // generate noise map
        float[,] noiseMap = GeneratePerlinNoiseMap();

        // generate fall off map
        float[,] fallOffMap = GenerateFallOffMap();

        // mix noise map with fall off map
        noiseMap = MixMaps(noiseMap, fallOffMap);

        // mirror map
        noiseMap = BrickPatterns.Mirror(noiseMap);

        // mix unbreakable map with explosive map;
        MixMapsExplosive(unbreakableMap, explosiveMap);

        // mix noise map with unbreakable map;
        MixMapsUnbreakable(noiseMap, unbreakableMap);

        // remove random horizontal lines
        RemoveRandomHorizontalLines(noiseMap);

        // remove rogue explosive bricks
        RemoveRogueExplosives(noiseMap);

        // add bricks if empty
        noiseMap = CheckAndFixEmptyMap(noiseMap);

        // display map
        //MapDisplay display = FindObjectOfType<MapDisplay>();
        //display.DrawNoiseMap(noiseMap);

        // generate bricks
        if (generateBricks)
        {
            generateBricks = false;

            DestroyPreviousStuff();

            // spawn bricks
            float[,] brickSpace = noiseMap;

            for (int y = 0; y < brickSpace.GetLength(1); y++)
            {
                for (int x = 0; x < brickSpace.GetLength(0); x++)
                {
                    float sample = brickSpace[x, y];
                    //Debug.Log(sample);
                    if (sample == unbreakableBrickValue)
                    {
                        Instantiate(unbreakableBrick, startingPosition + new Vector2(x * brickOffset.x, y * brickOffset.y), transform.rotation);
                    }
                    else if (sample == explosiveBrickValue)
                    {
                        Instantiate(explosiveBrick, startingPosition + new Vector2(x * brickOffset.x, y * brickOffset.y), transform.rotation);
                    }
                    else if (sample > 0)
                    {
                        GameObject newBrick = (GameObject)GameObject.Instantiate(brick, startingPosition + new Vector2(x * brickOffset.x, y * brickOffset.y), transform.rotation);

                        float brHits = sample * brickFactor * 9;
                        newBrick.GetComponent<Brick>().hits = Mathf.Clamp(Mathf.RoundToInt(brHits), 1, 3);
                        //Debug.Log(brHits);
                        /*if (sample > 0 && sample <= hitsPercentage1)
                        {
                            newBrick.GetComponent<Brick>().hits = 1;
                        }
                        else if (sample > hitsPercentage1 && sample <= hitsPercentage2)
                        {
                            newBrick.GetComponent<Brick>().hits = 2;
                        }
                        else if (sample > hitsPercentage2)
                        {
                            newBrick.GetComponent<Brick>().hits = 3;
                        }*/
                    }
                }
            }
        }
    }

    public float[,] GenerateUnbreakableMap()
    {
        // create fallOffMap 2D array
        unbreakableMap = new float[mapWidth, mapHeight];

        // add pattern to the array
        switch (unbreakablePattern)
        {
            case (int)UnbreakablePattern.NONE:
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        for (int x = 0; x < mapWidth; x++)
                        {
                            unbreakableMap[x, y] = 0;
                        }
                    }
                }
                break;
            case (int)UnbreakablePattern.RANDOM:
                {
                    InitializePlacementWeights(wallWeight, emptyWeight);

                    do
                    {
                        for (int y = 0; y < mapHeight; y++)
                        {
                            for (int x = 0; x < mapWidth; x++)
                            {
                                if (y == 0 || y == mapHeight - 1)
                                {
                                    unbreakableMap[x, y] = empty;
                                }
                                else
                                {
                                    //int[] numbers = { Placement.wall, Placement.empty, Placement.empty, Placement.empty };
                                    unbreakableMap[x, y] = placement[RandomWeighted(placementWeights, placementWeightTotal)]; //numbers[UnityEngine.Random.Range(0, numbers.Length)]; //
                                    //Debug.Log(unbreakableMap[x, y]);
                                }
                            }
                        }

                        // mirror unbreakable map
                        if (mirrorUnbreakable) { unbreakableMap = BrickPatterns.Mirror(unbreakableMap); } // && emptyWeight > 6

                    } while (ConfirmPath(unbreakableMap) == false);


                }
                break;
            case (int)UnbreakablePattern.MAZE:
                {
                    //unbreakableMap = BrickPatterns.UnbreakableMaze(mapWidth, mapHeight, emptyWeight);
                }
                break;
        }

        return unbreakableMap;
    }

    public float[,] GenerateExplosiveMap()
    {
        explosiveMap = new float[mapWidth, mapHeight];

        // add pattern to the array
        switch (explosivePattern)
        {
            case (int)ExplosivePattern.NONE:
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        explosiveMap[x, y] = 0;
                    }
                }
            }
            break;
            case (int)ExplosivePattern.RANDOM:
            {
                InitializePlacementWeights(explosiveWeight, emptyExplosiveWeight);

                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        if (unbreakableMap[x, y] == empty)
                        {
                            // set point
                            explosiveMap[x, y] = placementExplosive[RandomWeighted(placementWeights, placementWeightTotal)];
                        }
                    }
                }

                // mirror unbreakable map
                if (mirrorExplosive) { explosiveMap = BrickPatterns.Mirror(explosiveMap); }
                
            }
            break;
        }

        return explosiveMap;
    }

    public float[,] GeneratePerlinNoiseMap()
    {
        // create and initialize brickSpace 2D array to zeros
        float[,] noiseMap = new float[mapWidth, mapHeight];

        noiseMap = BrickPatterns.PerlinNoiseMap(mapWidth, mapHeight, seed, mapScale, octaves, persistance, lacunarity, offset);

        return noiseMap;
    }

    public float[,] GenerateFallOffMap()
    {
        // create fallOffMap 2D array
        float[,] fallOffMap = new float[mapWidth, mapHeight];

        // add pattern to the array
        switch (fallOffPattern)
        {
            case (int)FallOffPattern.NONE:
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        for (int x = 0; x < mapWidth; x++)
                        {
                            fallOffMap[x, y] = 0;
                        }
                    }
                }
                break;
            case (int)FallOffPattern.CENTER:
                {
                    fallOffMap = BrickPatterns.FallOffCenter(mapWidth, mapHeight, edge, size);
                }
                break;
            case (int)FallOffPattern.BOTTOM:
                {
                    fallOffMap = BrickPatterns.FallOffBottom(mapWidth, mapHeight, edge, size);
                }
                break;
            case (int)FallOffPattern.TOP:
                {
                    fallOffMap = BrickPatterns.FallOffTop(mapWidth, mapHeight, edge, size);
                }
                break;
            case (int)FallOffPattern.SIDES:
                {
                    fallOffMap = BrickPatterns.FallOffSides(mapWidth, mapHeight, edge, size);
                }
                break;
            case (int)FallOffPattern.VERTICAL_SLICES:
                {
                    fallOffMap = BrickPatterns.FallOffVerticalSlices(mapWidth, mapHeight, edge, size);
                }
                break;
            case (int)FallOffPattern.HORIZONTAL_SLICES:
                {
                    fallOffMap = BrickPatterns.FallOffHorizontalSlices(mapWidth, mapHeight, edge, size);
                }
                break;
        }

        return fallOffMap;
    }

    public float[,] MixMaps(float[,] noiseMap, float[,] map)
    {
        for (int y = 0; y < noiseMap.GetLength(1); y++)
        {
            for (int x = 0; x < noiseMap.GetLength(0); x++)
            {
                noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - map[x, y]);
            }
        }

        return noiseMap;
    }

    public void MixMapsUnbreakable(float[,] noiseMap, float[,] map)
    {
        for (int y = 0; y < noiseMap.GetLength(1); y++)
        {
            for (int x = 0; x < noiseMap.GetLength(0); x++)
            {
                if (map[x, y] == wall)
                {
                    noiseMap[x, y] = unbreakableBrickValue;
                    //Debug.Log("unbreakable" + unbreakableBrickValue);
                }
                else if (map[x, y] == explosive)
                {
                    noiseMap[x, y] = explosiveBrickValue;
                    //Debug.Log("unbreakable" + unbreakableBrickValue);
                }
                //noiseMap[x, y] = noiseMap[x, y] - map[x, y];
            }
        }

        //return noiseMap;
    }

    public void MixMapsExplosive(float[,] unbreakableMap, float[,] explosiveMap)
    {
        for (int y = 0; y < unbreakableMap.GetLength(1); y++)
        {
            for (int x = 0; x < unbreakableMap.GetLength(0); x++)
            {
                if (explosiveMap[x, y] == explosive)
                {
                    unbreakableMap[x, y] = explosive;
                }
            }
        }
    }
    
    public void InitializePlacementWeights(int solidWeight, int hollowWeight)
    {
        placementWeights = new int[2]; // 2 is the total number of placements
        placementWeights[Placement.solid] = solidWeight;
        placementWeights[Placement.hollow] = hollowWeight;
        placementWeightTotal = 0;
        foreach (int w in placementWeights)
        {
            placementWeightTotal += w;
        }
    }

    // RandomWeighted : Method that returns a random weighted integer
    int RandomWeighted(int[] weights, int weightTotal)
    {
        int result = 0, total = 0;
        int randVal = UnityEngine.Random.Range(0, weightTotal);
        for (result = 0; result < weights.Length; result++)
        {
            total += weights[result];
            if (total > randVal) break;
        }
        return result;
    }

    public bool ConfirmPath(float[,] originalMap)
    {
        bool hasPath = false;

        // make a copy 2D array to modify
        float[,] restoredMap = originalMap.Clone() as float[,];

        List<Vector2Int> changingNodes = new List<Vector2Int>();
        changingNodes.Add(new Vector2Int(0, 0));
        List<Vector2Int> vectorsToRemove = new List<Vector2Int>();
        List<Vector2Int> vectorsToAdd = new List<Vector2Int>();
        int failSafeCounter = 0;

        // begin loop to change values
        do
        {
            failSafeCounter++;

            foreach (Vector2Int vector in changingNodes)
            {
                // set the value of this vector
                if (restoredMap[vector.x, vector.y] != pathColor) { restoredMap[vector.x, vector.y] = pathColor; }
                vectorsToRemove.Add(vector);

                // check the 4 directions around this vector
                for (int i = 0; i < directions.Length; i++)
                {
                    // get node in direction
                    Vector2Int node = vector + directions[i];

                    // check if the node is inside the playSpaceSize and is empty and add it to the vectorsToAdd list
                    if (node.x >= 0 && node.y >= 0 && node.x < restoredMap.GetLength(0) && node.y < restoredMap.GetLength(1))
                    {
                        if (restoredMap[node.x, node.y] == empty)
                        {
                            vectorsToAdd.Add(node);
                            restoredMap[node.x, node.y] = pathColor;
                        }
                    }
                }
            }

            foreach (Vector2Int vector in vectorsToAdd)
            {
                changingNodes.Add(vector);
            }

            foreach (Vector2Int vector in vectorsToRemove)
            {
                changingNodes.Remove(vector);
            }

        } while ((changingNodes.Count > 0) || (failSafeCounter < restoredMap.GetLength(0) * restoredMap.GetLength(1)));

        // check if path exists
        if (restoredMap[0, 0] == pathColor && restoredMap[restoredMap.GetLength(0) - 1, restoredMap.GetLength(1) - 1] == pathColor)
        {
            hasPath = true;

            // finally replace empty nodes in original map with walls
            for (int y = 0; y < restoredMap.GetLength(1); y++)
            {
                for (int x = 0; x < restoredMap.GetLength(0); x++)
                {
                    if (restoredMap[x, y] == empty)
                    {
                        unbreakableMap[x, y] = wall;
                    }
                }
            }
        }

        return hasPath;
    }

    void RemoveRogueExplosives(float[,] map)
    {
        // list of explosives to remove
        List<Vector2Int> explosivesToRemove = new List<Vector2Int>();

        // find rogue explosives
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if (map[x, y] == explosiveBrickValue)
                {
                    Vector2Int point = new Vector2Int(x, y);
                    bool rogue = true;

                    // check the 4 directions around this vector
                    for (int i = 0; i < directions.Length; i++)
                    {
                        // get node in direction
                        Vector2Int node = point + directions[i];

                        // check if the node is inside the playSpaceSize and is empty
                        if (node.x >= 0 && node.y >= 0 && node.x < map.GetLength(0) && node.y < map.GetLength(1))
                        {
                            if (map[node.x, node.y] != empty)
                            {
                                rogue = false;
                                i = directions.Length;
                            }
                        }
                    }

                    // check if rogue
                    if (rogue) { explosivesToRemove.Add(point); }
                }
            }
        }

        // remove explosives
        foreach (Vector2Int explosiveBrick in explosivesToRemove)
        {
            map[explosiveBrick.x, explosiveBrick.y] = empty;
        }
    }

    void RemoveRandomHorizontalLines(float[,] map)
    {
        int lines = Mathf.FloorToInt((map.GetLength(1) * 0.2f) - brickFactor);
        List<int> linesToRemove = new List<int>();

        //Debug.Log("lines = " + lines);

        if (lines > 0)
        {
            System.Random rn = new System.Random();

            do
            {
                
                linesToRemove.Add(rn.Next(0, map.GetLength(1)));
                
                lines -= 1;
            } while (lines > 0) ;
        }

        // remove lines
        if (linesToRemove.Count > 0)
        {
            for (int i = 0; i < linesToRemove.Count; i++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    // check if brick
                    if (map[x, linesToRemove[i]] > 0 ) { map[x, linesToRemove[i]] = 0; }
                }
                //Debug.Log("linesToRemove = " + linesToRemove[i]);
            }
        }
    }

    private float[,] CheckAndFixEmptyMap(float[,] map)
    {
        // Check if empty
        bool emptyMap = true;
        int brickCounter = 0;

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if (map[x,y] > 0)
                {
                    brickCounter += 1;
                    if (brickCounter >= 4)
                    {
                        emptyMap = false;
                        x = map.GetLength(0);
                        y = map.GetLength(1);
                    }
                }
            }
        }

        // fix map
        if (emptyMap)
        {
            // create new map
            float[,] newMap = new float[9, 13];
            for (int y = 0; y < newMap.GetLength(1); y++)
            {
                for (int x = 0; x < newMap.GetLength(0); x++)
                {
                    newMap[x, y] = 0;
                }
            }

            // correct startingPosition
            startingPosition = new Vector2(startingPositionMin.x, startingPositionMin.y);

            // randomly select shape
            Vector2Int[] shapeArray = squareShape;
            startingShapes = UnityEngine.Random.Range(0, (int)Enum.GetValues(typeof(StartingShapes)).Cast<StartingShapes>().Max() + 1);
            switch (startingShapes)
            {
                case (int)StartingShapes.SQUARE: { shapeArray = squareShape;} break;
                case (int)StartingShapes.TRIANGLE: { shapeArray = triangleShape; } break;
                case (int)StartingShapes.RECTANGLE: { shapeArray = rectangleShape; } break;
                case (int)StartingShapes.RHOMBUS: { shapeArray = rhombusShape; } break;
                case (int)StartingShapes.CROSS: { shapeArray = crossShape; } break;
                case (int)StartingShapes.HEART: { shapeArray = heartShape; } break;
                case (int)StartingShapes.SMILE: { shapeArray = smileShape; } break;
            }
            /*Vector2Int[] shapeArray = iShape;
            shapes = UnityEngine.Random.Range(0, (int)Enum.GetValues(typeof(Shapes)).Cast<Shapes>().Max());
            switch (shapes)
            {
                case (int)Shapes.I: { shapeArray = iShape;} break;
                case (int)Shapes.HEART: { shapeArray = heartShape; } break;
                case (int)Shapes.B: { shapeArray = bShape; } break;
                case (int)Shapes.R: { shapeArray = rShape; } break;
                case (int)Shapes.C: { shapeArray = cShape; } break;
                case (int)Shapes.K: { shapeArray = kShape; } break;
                case (int)Shapes.S: { shapeArray = sShape; } break;
                case (int)Shapes.SMILE: { shapeArray = smileShape; } break;
            }*/

            // set positions
            for (int i = 0; i < shapeArray.Length; i++)
            {
                newMap[shapeArray[i].x, shapeArray[i].y] = 0.6f;
            }

            // replace old map
            map = newMap;
            Debug.Log("Empty Map Fixed");
        }

        return map;
    }

    public void SetMapGeneratorParameters(int level, int month, int difficulty, float intensity) // level (1 - infinity), difficulty (1 - infinity), intensity (0 - 1)
    {
        // rounding some values
        //intensity = 0f;
        float intensityRounded = (float)System.Math.Round(intensity, 2);

        //------- Brick Space Dimentions -------\\
        int bSDVariationYRange = 2 + month; // 3 - 5
        int bSDVariationY = Mathf.Clamp(Mathf.RoundToInt((float)difficulty * month * intensityRounded), 0, brickSpaceDimensionsMax.y) - UnityEngine.Random.Range(0, bSDVariationYRange);

        brickSpaceDimensions.x = brickSpaceDimensionsMax.x;
        brickSpaceDimensions.y = Mathf.Clamp(brickSpaceDimensionsMin.y + bSDVariationY, brickSpaceDimensionsMin.y, brickSpaceDimensionsMax.y);
        //Debug.Log("brickSpaceDimensions.y = " + brickSpaceDimensions.y);

        //------- Starting Position -------\\
        int sPVariationYRange = 3;
        int sPVariationY = UnityEngine.Random.Range(-sPVariationYRange + month, month - 1); // month 1: (-2, 0) month 2: (-1, 1) month 3: (0, 2)
        int bricksTotalY = brickSpaceDimensionsMax.y - (brickSpaceDimensionsMin.y + brickSpaceDimensions.y);
        int intensityY = Mathf.Clamp(Mathf.RoundToInt(intensityRounded * bricksTotalY) + sPVariationY, 0, bricksTotalY);

        //startingPosition.x = 0f;
        startingPosition.y = Mathf.Clamp(startingPositionMax.y + (-1 * intensityY * brickOffset.y) + (-1 * brickSpaceDimensions.y * brickOffset.y), startingPositionMin.y, startingPositionMax.y);

        //------- fall off pattern -------\\
        switch (month)
        {
            case 1:
            {
                /*if (level < 3) //10
                    {
                    int[] patterns = new int[] { (int)FallOffPattern.CENTER, (int)FallOffPattern.BOTTOM, (int)FallOffPattern.BOTTOM, (int)FallOffPattern.SIDES };
                    fallOffPattern = patterns[UnityEngine.Random.Range(0, patterns.Length)];
                }
                else*/
                {
                    int[] patterns = new int[] { (int)FallOffPattern.CENTER, (int)FallOffPattern.BOTTOM };
                    fallOffPattern = patterns[UnityEngine.Random.Range(0, patterns.Length)];
                }

                mirrorUnbreakable = true;
            }
            break;
            case 2:
            {
                /*if (level < 3)
                {
                    int[] patterns = new int[] { (int)FallOffPattern.BOTTOM, (int)FallOffPattern.CENTER, (int)FallOffPattern.SIDES, (int)FallOffPattern.HORIZONTAL_SLICES, (int)FallOffPattern.VERTICAL_SLICES };
                        fallOffPattern = patterns[UnityEngine.Random.Range(0, patterns.Length)];
                }
                else*/
                {
                    int[] patterns = new int[] { (int)FallOffPattern.CENTER, (int)FallOffPattern.SIDES, (int)FallOffPattern.HORIZONTAL_SLICES, (int)FallOffPattern.VERTICAL_SLICES };
                    fallOffPattern = patterns[UnityEngine.Random.Range(0, patterns.Length)];
                }

                mirrorUnbreakable = false;
            }
            break;
            case 3:
            {
                /*if (level < 3)
                {
                    int[] patterns = new int[] { (int)FallOffPattern.CENTER, (int)FallOffPattern.TOP, (int)FallOffPattern.BOTTOM, (int)FallOffPattern.SIDES };
                        fallOffPattern = patterns[UnityEngine.Random.Range(0, patterns.Length)];
                }
                else*/
                {
                    int[] patterns = new int[] { (int)FallOffPattern.CENTER, (int)FallOffPattern.TOP };
                    fallOffPattern = patterns[UnityEngine.Random.Range(0, patterns.Length)];
                }

                System.Random randomBool = new System.Random();
                mirrorUnbreakable = randomBool.NextDouble() > 0.5;
            }
            break;
        }
        //fallOffPattern = (int)FallOffPattern.BOTTOM;
        
        edge = Mathf.Clamp(1 - intensityRounded, 0f, 0.5f);
        //edge = 0.5f;
        //Debug.Log(intensityY);

        //size = Mathf.Clamp(UnityEngine.Random.Range(0.01f, 5f) * difficulty / 10 * intensity, 0.01f, 20f);
        //size = 1f; // 0.1f;
        size = Mathf.Clamp(0.25f + intensityRounded, 0f, 1f);
        
        //lacunarity = 4;
        if (month == 1)
        {
            int[] lacunarityArray = new int[] { 1, 10 };
            lacunarity = lacunarityArray[UnityEngine.Random.Range(0, lacunarityArray.Length)];
        }
        else
        {
            lacunarity = difficulty * intensityRounded;
        }

        //set seed
        System.Random r = new System.Random();
        seed = r.Next(0, 20000);

        //------- unbreakable brick pattern -------\\
        unbreakablePattern = 1;

        if (mirrorUnbreakable) { emptyWeight = Mathf.Clamp(Mathf.RoundToInt(30 - 30 * intensityRounded), 9, 30); }
        else { emptyWeight = Mathf.Clamp(Mathf.RoundToInt(30 - 30 * intensityRounded), 15, 30); }
        //emptyWeight = 30;

        //------- explosive brick pattern -------\\
        explosivePattern = 1;

        //System.Random randBool = new System.Random();
        mirrorExplosive = false; //randBool.NextDouble() > 0.5;

        if (mirrorExplosive) { emptyExplosiveWeight = Mathf.Clamp(Mathf.RoundToInt(50 - 50 * intensityRounded), 30, 50); }
        else { emptyExplosiveWeight = Mathf.Clamp(Mathf.RoundToInt(50 - 50 * intensityRounded), 40, 50); }

        //------- number of hits -------\\
        brickFactor = intensityRounded;

    }

    void DestroyPreviousStuff()
    {
        // destroy previous bricks
        foreach (GameObject brick in GameObject.FindGameObjectsWithTag("Breakable"))
        {
            DestroyImmediate(brick);
        }

        // destroy previous unbreakable bricks
        foreach (GameObject brick in GameObject.FindGameObjectsWithTag("Unbreakable"))
        {
            DestroyImmediate(brick);
        }

        // destroy previous powers
        foreach (GameObject power in GameObject.FindGameObjectsWithTag("Power"))
        {
            DestroyImmediate(power);
        }

        // destroy previous bolts
        foreach (GameObject bolt in GameObject.FindGameObjectsWithTag("Bolt"))
        {
            DestroyImmediate(bolt);
        }

        // destroy previous enemies
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            DestroyImmediate(enemy);
        }
    }

    // clamp some editor values
    void OnValidate()
    {
        if (fallOffPattern < 0)
        {
            fallOffPattern = 0;
        }
        else if (fallOffPattern > Enum.GetNames(typeof(FallOffPattern)).Length - 1)
        {
            fallOffPattern = Enum.GetNames(typeof(FallOffPattern)).Length - 1;
        }

        if (mapWidth < 1)
        {
            mapWidth = 1;
        }

        if (mapHeight < 1)
        {
            mapHeight = 1;
        }

        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        if (octaves < 0)
        {
            octaves = 0;
        }
    }
    enum StartingShapes { SQUARE = 0, TRIANGLE, RECTANGLE, RHOMBUS, CROSS, HEART, SMILE};
    private int startingShapes = 0;

    // Square Shape
    private Vector2Int[] squareShape = new Vector2Int[] {
        new Vector2Int(3, 6),
        new Vector2Int(4, 6),
        new Vector2Int(5, 6),
        new Vector2Int(3, 7),
        new Vector2Int(5, 7),
        new Vector2Int(3, 8),
        new Vector2Int(5, 8),
        new Vector2Int(3, 9),
        new Vector2Int(5, 9),
        new Vector2Int(3, 10),
        new Vector2Int(5, 10),
        new Vector2Int(3, 11),
        new Vector2Int(4, 11),
        new Vector2Int(5, 11)
    };

    // Triangle Shape
    private Vector2Int[] triangleShape = new Vector2Int[] {
        new Vector2Int(2, 6),
        new Vector2Int(3, 6),
        new Vector2Int(4, 6),
        new Vector2Int(5, 6),
        new Vector2Int(6, 6),
        new Vector2Int(2, 7),
        new Vector2Int(3, 7),
        new Vector2Int(4, 9),
        new Vector2Int(5, 7),
        new Vector2Int(6, 7),
        new Vector2Int(3, 8),
        new Vector2Int(5, 8),
        new Vector2Int(3, 9),
        new Vector2Int(5, 9),
        new Vector2Int(4, 10),
        new Vector2Int(4, 11)
    };

    // Rectangle Shape
    private Vector2Int[] rectangleShape = new Vector2Int[] {
        new Vector2Int(2, 7),
        new Vector2Int(3, 7),
        new Vector2Int(4, 7),
        new Vector2Int(5, 7),
        new Vector2Int(6, 7),
        new Vector2Int(2, 8),
        new Vector2Int(6, 8),
        new Vector2Int(2, 9),
        new Vector2Int(6, 9),
        new Vector2Int(2, 10),
        new Vector2Int(6, 10),
        new Vector2Int(2, 11),
        new Vector2Int(3, 11),
        new Vector2Int(4, 11),
        new Vector2Int(5, 11),
        new Vector2Int(6, 11)
    };

    // Rhombus Shape
    private Vector2Int[] rhombusShape = new Vector2Int[] {
        new Vector2Int(4, 2),
        new Vector2Int(4, 3),
        new Vector2Int(3, 4),
        new Vector2Int(5, 4),
        new Vector2Int(3, 5),
        new Vector2Int(5, 5),
        new Vector2Int(2, 6),
        new Vector2Int(6, 6),
        new Vector2Int(2, 7),
        new Vector2Int(6, 7),
        new Vector2Int(3, 8),
        new Vector2Int(5, 8),
        new Vector2Int(3, 9),
        new Vector2Int(5, 9),
        new Vector2Int(4, 10),
        new Vector2Int(4, 11)
    };

    // Cross Shape
    private Vector2Int[] crossShape = new Vector2Int[] {
        new Vector2Int(4, 6),
        new Vector2Int(4, 7),
        new Vector2Int(3, 8),
        //new Vector2Int(4, 8),
        new Vector2Int(5, 8),
        new Vector2Int(3, 9),
        //new Vector2Int(4, 9),
        new Vector2Int(5, 9),
        new Vector2Int(4, 10),
        new Vector2Int(4, 11)
    };

    enum Shapes { I = 0, HEART, B, R, C, K, S, SMILE };
    private int shapes = 0;

    // "I" Shape
    private Vector2Int[] iShape = new Vector2Int[] {
        new Vector2Int(3, 0),
        new Vector2Int(4, 0),
        new Vector2Int(5, 0),
        new Vector2Int(4, 1),
        new Vector2Int(4, 2),
        new Vector2Int(4, 3),
        new Vector2Int(4, 4),
        new Vector2Int(4, 5),
        new Vector2Int(4, 6),
        new Vector2Int(4, 7),
        new Vector2Int(4, 8),
        new Vector2Int(4, 9),
        new Vector2Int(4, 10),
        new Vector2Int(3, 11),
        new Vector2Int(4, 11),
        new Vector2Int(5, 11)
    };

    // Heart Shape
    private Vector2Int[] heartShape = new Vector2Int[] {
        new Vector2Int(4, 0),
        new Vector2Int(4, 1),
        new Vector2Int(3, 2),
        new Vector2Int(3, 3),
        new Vector2Int(2, 4),
        new Vector2Int(2, 5),
        new Vector2Int(1, 6),
        new Vector2Int(1, 7),
        new Vector2Int(1, 8),
        new Vector2Int(1, 9),
        new Vector2Int(2, 10),
        new Vector2Int(2, 11),
        new Vector2Int(3, 10),
        new Vector2Int(3, 11),
        new Vector2Int(4, 8),
        new Vector2Int(4, 9),
        new Vector2Int(5, 10),
        new Vector2Int(5, 11),
        new Vector2Int(6, 10),
        new Vector2Int(6, 11),
        new Vector2Int(7, 6),
        new Vector2Int(7, 7),
        new Vector2Int(7, 8),
        new Vector2Int(7, 9),
        new Vector2Int(6, 4),
        new Vector2Int(6, 5),
        new Vector2Int(5, 2),
        new Vector2Int(5, 3)
    };

    // "B" Shape
    private Vector2Int[] bShape = new Vector2Int[] {
        new Vector2Int(2, 0),
        new Vector2Int(3, 0),
        new Vector2Int(4, 0),
        new Vector2Int(5, 0),
        new Vector2Int(2, 1),
        new Vector2Int(6, 1),
        new Vector2Int(2, 2),
        new Vector2Int(6, 2),
        new Vector2Int(2, 3),
        new Vector2Int(6, 3),
        new Vector2Int(2, 4),
        new Vector2Int(6, 4),
        new Vector2Int(2, 5),
        new Vector2Int(6, 5),
        new Vector2Int(2, 6),
        new Vector2Int(3, 6),
        new Vector2Int(4, 6),
        new Vector2Int(5, 6),
        new Vector2Int(2, 7),
        new Vector2Int(6, 7),
        new Vector2Int(2, 8),
        new Vector2Int(6, 8),
        new Vector2Int(2, 9),
        new Vector2Int(6, 9),
        new Vector2Int(2, 10),
        new Vector2Int(6, 10),
        new Vector2Int(2, 11),
        new Vector2Int(3, 11),
        new Vector2Int(4, 11),
        new Vector2Int(5, 11)
    };

    // "R" Shape
    private Vector2Int[] rShape = new Vector2Int[] {
        new Vector2Int(2, 0),
        new Vector2Int(6, 0),
        new Vector2Int(2, 1),
        new Vector2Int(6, 1),
        new Vector2Int(2, 2),
        new Vector2Int(6, 2),
        new Vector2Int(2, 3),
        new Vector2Int(6, 3),
        new Vector2Int(2, 4),
        new Vector2Int(6, 4),
        new Vector2Int(2, 5),
        new Vector2Int(6, 5),
        new Vector2Int(2, 6),
        new Vector2Int(3, 6),
        new Vector2Int(4, 6),
        new Vector2Int(5, 6),
        new Vector2Int(2, 7),
        new Vector2Int(6, 7),
        new Vector2Int(2, 8),
        new Vector2Int(6, 8),
        new Vector2Int(2, 9),
        new Vector2Int(6, 9),
        new Vector2Int(2, 10),
        new Vector2Int(6, 10),
        new Vector2Int(2, 11),
        new Vector2Int(3, 11),
        new Vector2Int(4, 11),
        new Vector2Int(5, 11)
    };

    // "C" Shape
    private Vector2Int[] cShape = new Vector2Int[] {
        new Vector2Int(3, 0),
        new Vector2Int(4, 0),
        new Vector2Int(5, 0),
        new Vector2Int(2, 1),
        new Vector2Int(6, 1),
        new Vector2Int(2, 2),
        //new Vector2Int(6, 2),
        new Vector2Int(2, 3),
        new Vector2Int(2, 4),
        new Vector2Int(2, 5),
        new Vector2Int(2, 6),
        new Vector2Int(2, 7),
        new Vector2Int(2, 8),
        new Vector2Int(2, 9),
        //new Vector2Int(6, 9),
        new Vector2Int(2, 10),
        new Vector2Int(6, 10),
        new Vector2Int(3, 11),
        new Vector2Int(4, 11),
        new Vector2Int(5, 11)
    };

    // "K" Shape
    private Vector2Int[] kShape = new Vector2Int[] {
        new Vector2Int(2, 0),
        new Vector2Int(6, 0),
        new Vector2Int(2, 1),
        new Vector2Int(6, 1),
        new Vector2Int(2, 2),
        new Vector2Int(6, 2),
        new Vector2Int(2, 3),
        new Vector2Int(6, 3),
        new Vector2Int(2, 4),
        new Vector2Int(6, 4),
        new Vector2Int(2, 5),
        new Vector2Int(6, 5),
        new Vector2Int(2, 6),
        new Vector2Int(3, 6),
        new Vector2Int(4, 6),
        new Vector2Int(5, 6),
        new Vector2Int(2, 7),
        new Vector2Int(6, 7),
        new Vector2Int(2, 8),
        new Vector2Int(6, 8),
        new Vector2Int(2, 9),
        new Vector2Int(6, 9),
        new Vector2Int(2, 10),
        new Vector2Int(6, 10),
        new Vector2Int(2, 11),
        new Vector2Int(6, 11)
    };

    // "S" Shape
    private Vector2Int[] sShape = new Vector2Int[] {
        new Vector2Int(3, 0),
        new Vector2Int(4, 0),
        new Vector2Int(5, 0),
        new Vector2Int(2, 1),
        new Vector2Int(6, 1),
        new Vector2Int(6, 2),
        new Vector2Int(6, 3),
        new Vector2Int(6, 4),
        new Vector2Int(6, 5),
        new Vector2Int(3, 6),
        new Vector2Int(4, 6),
        new Vector2Int(5, 6),
        new Vector2Int(2, 7),
        new Vector2Int(2, 8),
        new Vector2Int(2, 9),
        new Vector2Int(2, 10),
        new Vector2Int(6, 10),
        new Vector2Int(3, 11),
        new Vector2Int(4, 11),
        new Vector2Int(5, 11)
    };

    // Smile Shape
    private Vector2Int[] smileShape = new Vector2Int[] {
        new Vector2Int(3, 0),
        new Vector2Int(4, 0),
        new Vector2Int(5, 0),
        new Vector2Int(2, 1),
        new Vector2Int(6, 1),
        new Vector2Int(1, 2),
        new Vector2Int(7, 2),
        new Vector2Int(0, 3),
        new Vector2Int(8, 3),
        new Vector2Int(0, 4),
        new Vector2Int(3, 4),
        new Vector2Int(4, 4),
        new Vector2Int(5, 4),
        new Vector2Int(8, 4),
        new Vector2Int(0, 5),
        new Vector2Int(2, 5),
        new Vector2Int(6, 5),
        new Vector2Int(8, 5),
        new Vector2Int(0, 6),
        new Vector2Int(8, 6),
        new Vector2Int(0, 7),
        new Vector2Int(8, 7),
        new Vector2Int(0, 8),
        new Vector2Int(3, 8),
        new Vector2Int(5, 8),
        new Vector2Int(8, 8),
        new Vector2Int(0, 9),
        new Vector2Int(8, 9),
        new Vector2Int(1, 10),
        new Vector2Int(7, 10),
        new Vector2Int(2, 11),
        new Vector2Int(6, 11),
        new Vector2Int(3, 12),
        new Vector2Int(4, 12),
        new Vector2Int(5, 12),
    };
}
