using System;
using UnityEngine;

namespace DN
{
    public class EntitySpawn
    {
        public string EntityId { get; private set; }
        public string[] BiomeIds { get; private set; }
        public string[] IgnoredBiomeIds { get; private set; }
        public float SpawnProbability { get; private set; }
        public int MaxSpawns { get; private set; }

        public EntitySpawn(string entityId)
        {
            try
            {
                EntityId = entityId.Trim().ToLower();
                Spreadsheet.Row properties = GameManager.Instance.EntitiesSpreadsheet[EntityId];
                BiomeIds = properties.Array("biome_ids");
                IgnoredBiomeIds = properties.Array("ignored_biome_ids");
                SpawnProbability = properties.Float("spawn_probability", 1.0f);
                MaxSpawns = properties.Int("max_spawns", -1);
            }
            catch (Exception error)
            {
                string debugId = !string.IsNullOrEmpty(EntityId) ? $"\"{EntityId}\"" : "null";
                Debug.LogError($"Entity with id {debugId} spreadsheet failure: {error.Message}", WorldGenerator.Instance.gameObject);
            }
        }
    }
}
