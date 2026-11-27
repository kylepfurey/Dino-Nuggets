using System;
using UnityEngine;

namespace DN
{
    public class Recipe
    {
        public string ItemId { get; private set; }
        public string RequiredBenchId { get; private set; }
        public (string Id, int Amount)[] RequiredItems { get; private set; }
        public int YieldAmount { get; private set; }

        public Recipe(string itemId)
        {
            try
            {
                ItemId = itemId.Trim().ToLower();
                Spreadsheet.Row properties = GameManager.Instance.CraftingSpreadsheet[ItemId];
                RequiredBenchId = properties.String("required_bench_id");
                string[] ids = properties.Array("required_item_ids");
                string[] amounts = properties.Array("required_item_amounts");
                RequiredItems = new (string, int)[ids.Length];
                for (int i = 0; i < RequiredItems.Length; ++i)
                    RequiredItems[i] = new(ids[i], int.Parse(amounts[i]));
                YieldAmount = properties.Int("yield_amount", 1);
            }
            catch (Exception error)
            {
                string debugId = !string.IsNullOrEmpty(ItemId) ? $"\"{ItemId}\"" : "null";
                Debug.LogError($"Recipe with id {debugId} spreadsheet failure: {error.Message}", WorldGenerator.Instance.gameObject);
            }
        }
    }
}
