using System;
using UnityEngine;

namespace DN
{
    public class Biome
    {
        public string BiomeId { get; private set; }
        public string Name { get; private set; }
        public (string Id, float Probability)[] AdjacentBiomes { get; private set; }
        public int MinRadius { get; private set; }
        public int MaxRadius { get; private set; }
        public float MaxDepth { get; private set; }
        public float MaxHeight { get; private set; }
        public float Smoothness { get; private set; }
        public float WaterLevel { get; private set; }
        public int MinSpawns { get; private set; }
        public int MaxSpawns { get; private set; }
        public Material Material { get; private set; }

        public Biome(string biomeId)
        {
            try
            {
                BiomeId = biomeId.Trim().ToLower();
                Spreadsheet.Row properties = GameManager.Instance.BiomesSpreadsheet[BiomeId];
                Name = properties.String("name", "null");
                string[] ids = properties.Array("adjacent_biome_ids");
                string[] probabilities = properties.Array("adjacent_biome_probability");
                AdjacentBiomes = new (string, float)[ids.Length];
                for (int i = 0; i < AdjacentBiomes.Length; ++i)
                    AdjacentBiomes[i] = new(ids[i], float.Parse(probabilities[i]));
                MinRadius = properties.Int("min_radius", 1);
                MaxRadius = properties.Int("max_radius", 2);
                MaxDepth = properties.Float("max_depth", 0.0f);
                MaxHeight = properties.Float("max_height", 0.0f);
                Smoothness = properties.Float("smoothness", 0.5f);
                WaterLevel = properties.Float("water_level", 0.25f);
                MinSpawns = properties.Int("min_spawns", 5);
                MaxSpawns = properties.Int("max_spawns", 15);
                Material = GameManager.Instance.MaterialTable.Get<Material>(properties.String("material_id", BiomeId));
            }
            catch (Exception error)
            {
                string debugId = !string.IsNullOrEmpty(BiomeId) ? $"\"{BiomeId}\"" : "null";
                Debug.LogError($"Biome with id {debugId} spreadsheet failure: {error.Message}", WorldGenerator.Instance.gameObject);
            }
        }
    }
}
