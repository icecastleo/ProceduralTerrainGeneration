using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum TextureType { 
    Grass, Dessert, Snow, Rock, TextureEnd
}

public class TerrainGenerator : MonoBehaviour {

    // must be power of 2
    public int mapWidth = 512;
    public int mapHeight = 512;
    public int terrainHeight = 20;

    public float frequency = 20f;

    public int octaves = 6;
    public float lacunarity = 2.0f;
    public float gain = 0.6f;

    public float sharpness = 0.5f;

 
    PerlinNoise heightPerlin;
    PerlinNoise moisturePerlin;
    PerlinNoise treePerlin;

    public Transform treePrefab;

    public bool autoUpdate;

    public void GenerateMap()
    {
        heightPerlin = new PerlinNoise();
        moisturePerlin = new PerlinNoise();
        treePerlin = new PerlinNoise();

        MapDisplay display = GetComponentInChildren<MapDisplay>(true);
        display.gameObject.SetActive(true);

        Terrain terrain = GetComponentInChildren<Terrain>(true);
        terrain.gameObject.SetActive(false);

        float[,] heightsMap = generateHeights();
        display.DrawNoiseMap(heightsMap);
        //display.transform.localScale = new Vector3(mapWidth, 1, mapHeight);
        display.transform.position = new Vector3(-mapWidth / 2, 0, -mapHeight / 2);
    }

    // Use this for initialization
    void Start()
    {
        heightPerlin = new PerlinNoise();
        moisturePerlin = new PerlinNoise();
        treePerlin = new PerlinNoise();

        MapDisplay display = GetComponentInChildren<MapDisplay>(true);
        display.gameObject.SetActive(false);

        Terrain terrain = GetComponentInChildren<Terrain>(true);
        terrain.gameObject.SetActive(true);

        float[,] heightsMap = generateHeights();
        float[,] moisturesMap = generateMoisture();
        terrain.terrainData = GenerateTerrain(terrain.terrainData, heightsMap, moisturesMap);
        terrain.transform.position = new Vector3(-mapWidth / 2, 0, -mapHeight / 2);

        AddPlant(terrain, heightsMap, moisturesMap);
    }
	
	// Update is called once per frame
	void Update () {
   
    }

    TerrainData GenerateTerrain(TerrainData terrainData, float[,] heights, float[,] moistures)
    {
        terrainData.heightmapResolution = mapWidth + 1;
        //terrainData.alphamapResolution = Mathf.Max(mapWidth, mapHeight);
        terrainData.size = new Vector3(mapWidth, terrainHeight, mapHeight);

        terrainData.SetHeights(0, 0, heights);

        AddSplatmap(terrainData, heights, moistures);

        return terrainData;
    }

    void AddPlant(Terrain terrain, float[,] heightMap, float[,] moistureMap)
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float height = heightMap[x, y];

                if (height < 0.5f && moistureMap[x, y] > 0.25f)
                {
                    // Add tree randomly
                    if (treePerlin.noise((float)x / mapWidth * frequency, (float)y / mapHeight * frequency) > 0.5f && Random.Range(0, 20) == 0)
                    {
                        Transform temp = Instantiate(treePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        temp.parent = terrain.transform;
                        temp.transform.localPosition = new Vector3(y, height, x);
                    }
                }
            }
        }
    }

    float[,] generateHeights()
    {
        float[,] heightMap = new float[mapWidth, mapHeight];

        float amplitude = 1;
        float freq = frequency;

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                freq = frequency;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + halfWidth) * freq;
                    float sampleY = (y + halfHeight) * freq;
                    float perlinValue = heightPerlin.noise(sampleX / (float)mapWidth, sampleY / (float)mapHeight);

                    float RidgedNoise = 1.0f - Mathf.Abs(perlinValue);
                    float BillowNoise = perlinValue * perlinValue;

                    perlinValue = Mathf.Lerp(perlinValue, BillowNoise, Mathf.Max(0f, sharpness));
                    perlinValue = Mathf.Lerp(perlinValue, RidgedNoise, Mathf.Abs(Mathf.Min(0f, sharpness)));

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= gain;
                    freq *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                heightMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                heightMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, heightMap[x, y]);

                // set height under 0.3 to be flat
                heightMap[x, y] = Mathf.InverseLerp(0.3f, 1.0f, Mathf.Max(0.3f, heightMap[x, y]));
            }
        }

        return heightMap;
    }

    public float[,] generateMoisture()
    {
        float[,] moistureMap = new float[mapWidth, mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float sampleX = (float)x / mapWidth * frequency;
                float sampleY = (float)y / mapHeight * frequency;

                float perlinValue = moisturePerlin.noise(sampleX, sampleY);

                moistureMap[x, y] = (perlinValue + 1f) / 2f;
            }
        }

        return moistureMap;
    }

    void AddSplatmap(TerrainData terrainData, float[,] heightMap, float[,] moistureMap)
    {
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float x_01 = (float)x / (float)terrainData.alphamapWidth;
                float y_01 = (float)y / (float)terrainData.alphamapHeight;

                int hx = Mathf.RoundToInt(x_01 * mapWidth);
                int hy = Mathf.RoundToInt(y_01 * mapHeight);

                //// Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                //float height = terrainData.GetHeight(hy, hx);

                //// normalized
                //height /= terrainData.size.y;

                if(hx == mapWidth)
                {
                    hx--;
                }

                if(hy == mapHeight)
                {
                    hy--;
                }

                float height = heightMap[hx, hy];

                //// Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                //Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);

                //// Calculate the steepness of the terrain
                //float steepness = terrainData.GetSteepness(y_01, x_01);

                float[] splatWeights = new float[terrainData.alphamapLayers];

                // TODO: smothness height
                if (height > 0.5)
                {
                    // smotheness snow texture when moisture > 0.3
                    float snow = Mathf.InverseLerp(0.25f, 0.5f, Mathf.Min(moistureMap[hx, hy], 0.7f));

                    splatWeights[(int)TextureType.Snow] = snow;
                    splatWeights[(int)TextureType.Rock] = 1.0f - snow;

                } else
                {
                    // smotheness grass texture when moisture > 0.25
                    float grass = Mathf.InverseLerp(0.25f, 0.5f, Mathf.Min(moistureMap[hx, hy], 0.5f));

                    splatWeights[(int)TextureType.Grass] = grass;
                    splatWeights[(int)TextureType.Dessert] = 1.0f - grass;
                }

                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = 0f;
    
                foreach(float f in splatWeights)
                {
                    z += f;
                }

                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;

                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}
