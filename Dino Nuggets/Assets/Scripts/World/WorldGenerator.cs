using System;
using System.Collections.Generic;
using UnityEngine;

namespace DN
{
    public class WorldGenerator : ReplicatedMonoBehaviour
    {
        public static WorldGenerator Instance { get; private set; }

        public struct Chunk
        {
            public string BiomeId;
            public int EntityCount;
            public int MaxEntities;
        }

        [SerializeField] float canvasWidth;
        [SerializeField] int partitions;
        [SerializeField] string initialBiome;
        Dictionary<string, Biome> biomeCache = new();
        Dictionary<string, EntitySpawn> entitySpawnCache = new();
        public readonly Dictionary<string, int> SpawnedEntityCounts = new();

        public float CanvasWidth => canvasWidth;
        public int Partitions => partitions;
        public Chunk[,] Chunks { get; private set; }

        protected override void Awake()
        {
            Instance = this;
            transform.position = Vector3.zero;

            base.Awake();
        }

        public void Generate()
        {
            Chunks = new Chunk[partitions, partitions];
            GenerateBiome(initialBiome, new Vector2Int(UnityEngine.Random.Range(0, partitions), UnityEngine.Random.Range(0, partitions)));
        }
        void GenerateBiome(string initialBiomeId, Vector2Int initialChunk)
        {
            Stack<(string biomeId, Vector2Int chunk)> remainingChunks = new();
            remainingChunks.Push((initialBiomeId, initialChunk));

            while (remainingChunks.Count > 0)
            {
                var _ = remainingChunks.Pop();
                string biomeId = _.biomeId;
                Vector2Int chunk = _.chunk;
                if (chunk.x < 0 || chunk.y < 0 || chunk.x >= partitions || chunk.y >= partitions || !string.IsNullOrEmpty(Chunks[chunk.x, chunk.y].BiomeId))
                    continue;

                Biome biome = GetBiome(biomeId);

                int radius = UnityEngine.Random.Range(biome.MinRadius, biome.MaxRadius + 1);
                int minX = Math.Max(0, chunk.x - radius);
                int maxX = Math.Min(partitions - 1, chunk.x + radius);
                int minY = Math.Max(0, chunk.y - radius);
                int maxY = Math.Min(partitions - 1, chunk.y + radius);
                int radiusSquared = radius * radius;
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        int dx = x - chunk.x;
                        int dy = y - chunk.y;
                        if (dx * dx + dy * dy > radiusSquared)
                            continue;

                        if (x < 0 || y < 0 || x >= partitions || y >= partitions || !string.IsNullOrEmpty(Chunks[x, y].BiomeId))
                            continue;

                        Chunks[x, y].BiomeId = biomeId;
                        Chunks[x, y].EntityCount = 0;
                        Chunks[x, y].MaxEntities = UnityEngine.Random.Range(biome.MinSpawns, biome.MaxSpawns + 1);
                    }
                }

                List<Vector2Int> nextChunks = new();
                ++radius;
                minX = Math.Max(0, chunk.x - radius);
                maxX = Math.Min(partitions - 1, chunk.x + radius);
                minY = Math.Max(0, chunk.y - radius);
                maxY = Math.Min(partitions - 1, chunk.y + radius);
                radiusSquared = radius * radius;
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        int dx = x - chunk.x;
                        int dy = y - chunk.y;
                        if (dx * dx + dy * dy > radiusSquared)
                            continue;

                        nextChunks.Add(new Vector2Int(x, y));
                    }
                }

                nextChunks.Shuffle();
                foreach (var nextChunk in nextChunks)
                {
                    string nextBiomeId = null;
                    while (nextBiomeId == null)
                    {
                        int index = UnityEngine.Random.Range(0, biome.AdjacentBiomes.Length);
                        if (UnityEngine.Random.value <= biome.AdjacentBiomes[index].Probability)
                            nextBiomeId = biome.AdjacentBiomes[index].Id;
                    }
                    remainingChunks.Push((nextBiomeId, nextChunk));
                }
            }
        }

        public Vector2Int GetChunk(Vector3 positon)
        {
            float chunkSize = canvasWidth / partitions;

            int x = Mathf.FloorToInt((positon.x - transform.position.x) / chunkSize);
            int y = Mathf.FloorToInt((positon.z - transform.position.z) / chunkSize);

            x = Mathf.Clamp(x, 0, partitions - 1);
            y = Mathf.Clamp(y, 0, partitions - 1);

            return new Vector2Int(x, y);
        }
        public Biome GetBiome(string biomeId)
        {
            if (biomeCache.TryGetValue(biomeId, out var value))
                return value;
            Biome biome = new(biomeId);
            biomeCache.Add(biomeId, biome);
            return biome;
        }
        public EntitySpawn GetEntitySpawn(string entityId)
        {
            if (entitySpawnCache.TryGetValue(entityId, out var value))
                return value;
            EntitySpawn spawn = new(entityId);
            entitySpawnCache.Add(entityId, spawn);
            return spawn;
        }
    }
}
