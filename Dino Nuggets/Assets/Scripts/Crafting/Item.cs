using System;
using UnityEngine;

namespace DN
{
    public class Item
    {
        public string ItemId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int MaxStack { get; private set; }
        public int InventoryWidth { get; private set; }
        public int InventoryHeight { get; private set; }
        public int Durability { get; private set; }
        public bool Despawns { get; private set; }
        public int Attack { get; private set; }
        public float AttackSpeed { get; private set; }
        public string[] OffensiveToolIds { get; private set; }
        public string[] ProjectileIds { get; private set; }

        public Item(string itemId)
        {
            try
            {
                ItemId = itemId.Trim().ToLower();
                Spreadsheet.Row properties = GameManager.Instance.ItemsSpreadsheet[ItemId];
                Name = properties.String("name", "null");
                Description = properties.String("description", "null");
                MaxStack = properties.Int("max_stack", 99);
                InventoryWidth = properties.Int("inventory_width", 1);
                InventoryHeight = properties.Int("inventory_height", 1);
                Durability = properties.Int("durability", -1);
                Despawns = properties.Bool("despawns", true);
                Attack = properties.Int("attack", 1);
                AttackSpeed = properties.Float("attack_speed", 1.0f);
                OffensiveToolIds = properties.Array("offensive_tool_ids");
                ProjectileIds = properties.Array("projectile_ids");
            }
            catch (Exception error)
            {
                string debugId = !string.IsNullOrEmpty(ItemId) ? $"\"{ItemId}\"" : "null";
                Debug.LogError($"Item with id {debugId} spreadsheet failure: {error.Message}", WorldGenerator.Instance.gameObject);
            }
        }
    }
}
