using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BrickPatterns
{
    /// <summary>
    /// Generates a noise map based on the given parameters.
    /// </summary>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    /// <param name="mapScale"></param>
    /// <param name="octaves"></param>
    /// <param name="persistance"></param>
    /// <param name="lacunarity"></param>
    /// <returns></returns>
    public static float[,] PerlinNoiseMap(int mapWidth, int mapHeight, int seed, float mapScale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // pseudo random number generator (prng)
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // clamp mapScale
        if (mapScale <= 0)
        {
            mapScale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i < octaves; i++)
                {
                    float mapCoordX = (x - halfWidth) / mapScale * frequency + octaveOffsets[i].x;
                    float mapCoordY = (y - halfHeight) / mapScale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(mapCoordX, mapCoordY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;

            }
        }

        // normalize noiseMap with values from 0 - 1
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    public static float[,] FallOffCenter(int mapWidth, int mapHeight, float edge, float size)
    {
        float[,] map = new float[mapWidth, mapHeight];

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                float mapCoordX = (x / (float)map.GetLength(0)) * 2 - 1;
                float mapCoordY = (y / (float)map.GetLength(1)) * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(mapCoordX), Mathf.Abs(mapCoordY));
                map[x, y] = Evaluate(value, edge, size);
            }
        }

        return map;
    }

    public static float[,] FallOffBottom(int mapWidth, int mapHeight, float edge, float size)
    {
        float[,] map = new float[mapWidth, mapHeight];

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                float value = y / (float)map.GetLength(1);
                map[x, y] = Evaluate(value, edge, size);
            }
        }

        return map;
    }

    public static float[,] FallOffTop(int mapWidth, int mapHeight, float edge, float size)
    {
        float[,] map = new float[mapWidth, mapHeight];

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                float value = 1 - y / (float)map.GetLength(1);
                map[x, y] = Evaluate(value, edge, size);
            }
        }

        return map;
    }

    public static float[,] FallOffSides(int mapWidth, int mapHeight, float edge, float size)
    {
        float[,] map = new float[mapWidth, mapHeight];

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                int center = Mathf.RoundToInt(((float)map.GetLength(0) * 0.5f));
                float value = Mathf.Lerp(1, 0, Mathf.Abs((float)center - x) / (float)center);

                map[x, y] = Evaluate(value, edge, size);
            }
        }

        return map;
    }
    
    public static float[,] FallOffVerticalSlices(int mapWidth, int mapHeight, float edge, float size)
    {
        float[,] map = new float[mapWidth, mapHeight];
        bool getEvenNumbers = Random.Range(0f, 1f) < 0.5f ? true : false;

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if ((getEvenNumbers && x % 2 == 0)
                   || (getEvenNumbers == false && x % 2 != 0))
                {
                    map[x, y] = Evaluate(1f, edge, size);
                }
            }
        }

        return map;
    }

    public static float[,] FallOffHorizontalSlices(int mapWidth, int mapHeight, float edge, float size)
    {
        float[,] map = new float[mapWidth, mapHeight];
        bool getEvenNumbers = Random.Range(0f, 1f) < 0.5f ? true : false;

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if ((getEvenNumbers && y % 2 == 0)
                   || (getEvenNumbers == false && y % 2 != 0))
                {
                    map[x, y] = Evaluate(1f, edge, size);
                }
            }
        }

        return map;
    }

    static float Evaluate(float value, float a, float b)
    {
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }

    public static float[,] Mirror(float[,] map)
    {
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = Mathf.RoundToInt(map.GetLength(0) / 2); x < map.GetLength(0); x++)
            {
                map[x, y] = map[map.GetLength(0) - 1 - x, y];
            }
        }

        return map;
    }
}

