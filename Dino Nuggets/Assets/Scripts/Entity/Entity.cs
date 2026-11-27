using System;
using UnityEngine;

namespace DN
{
    public enum EntityBehavior { STATIC, PASSIVE, NEUTRAL, HOSTILE, PLAYER, ITEM, PROJECTILE }
    public enum EntityHabitat { LAND, AIR, SEA }

    public class Entity : ReplicatedMonoBehaviour
    {
        [SerializeField] Collider collider;
        [SerializeField] Rigidbody rigidbody;
        Vector2Int spawnChunk;

        public Collider Collider => collider;
        public Rigidbody Rigidbody => rigidbody;

        public string EntityId { get; private set; }
        public string Name { get; private set; }
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public int Defense { get; private set; }
        public string[] VulnerableToolIds { get; private set; }
        public int DroppedExp { get; private set; }
        public int CurrentExp { get; private set; }
        public (string Id, int Amount, float Probability)[] ItemDrops { get; private set; }
        public EntityBehavior Behavior { get; private set; }
        public EntityHabitat Habitat { get; private set; }
        public float PerceptionDistance { get; private set; }
        public float Speed { get; private set; }
        public int Attack { get; private set; }
        public float AttackSpeed { get; private set; }
        public GameObject ModelPrefab { get; private set; }
        public EntitySfx Sfx { get; private set; }
        public float Scale { get; private set; }
        public string InteractableId { get; private set; }

        protected override void OnDestroy()
        {
            if (!string.IsNullOrEmpty(EntityId))
            {
                --WorldGenerator.Instance.Chunks[spawnChunk.x, spawnChunk.y].EntityCount;
                --WorldGenerator.Instance.SpawnedEntityCounts[EntityId];
            }

            base.OnDestroy();
        }

        [Rpc]
        public void InitEntityRpc(string json)
        {
            try
            {
                EntityId = Network.Deserialize<string>(json).Trim().ToLower();
                if (string.IsNullOrEmpty(EntityId))
                {
                    Debug.LogError("Null or empty entity id", gameObject);
                    return;
                }

                Spreadsheet.Row properties = GameManager.Instance.EntitiesSpreadsheet[EntityId];
                if (EntityId == "player")
                {
                    foreach (var member in Network.Instance.Lobby.Value.Members)
                    {
                        if (OwnerId == member.Id.Value)
                        {
                            Name = member.Name;
                            break;
                        }
                    }
                }
                else
                    Name = properties.String("name", "null");
                MaxHealth = properties.Int("health", 1);
                CurrentHealth = MaxHealth;
                Defense = properties.Int("defense", 0);
                VulnerableToolIds = properties.Array("vulnerable_tool_ids");
                DroppedExp = properties.Int("dropped_exp", 0);
                CurrentExp = 0;
                string[] ids = properties.Array("item_drop_ids");
                string[] amounts = properties.Array("item_drop_amounts");
                string[] probabilities = properties.Array("item_drop_probability");
                ItemDrops = new (string, int, float)[ids.Length];
                for (int i = 0; i < ItemDrops.Length; ++i)
                    ItemDrops[i] = new(ids[i], int.Parse(amounts[i]), float.Parse(probabilities[i]));
                Behavior = (EntityBehavior)Enum.Parse(typeof(EntityBehavior), properties.String("behavior", "static").ToUpper());
                Habitat = (EntityHabitat)Enum.Parse(typeof(EntityHabitat), properties.String("habitat", "land").ToUpper());
                PerceptionDistance = properties.Float("perception_distance", 0.0f);
                Speed = properties.Float("speed", 1.0f);
                Attack = properties.Int("attack", 1);
                AttackSpeed = properties.Float("attack_speed", 1.0f);
                ModelPrefab = GameManager.Instance.ModelTable.Get<GameObject>(properties.String("model_id", EntityId));
                Sfx = GameManager.Instance.SfxTable.Get(properties.String("sfx_id", EntityId));
                Scale = properties.Float("scale", 1.0f);
                InteractableId = properties.String("interactable_id");

                gameObject.name = Name;
                transform.localScale *= Scale;
                GameObject model = Instantiate(ModelPrefab);
                model.transform.parent = transform;
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;

                spawnChunk = WorldGenerator.Instance.GetChunk(transform.position);
                ++WorldGenerator.Instance.Chunks[spawnChunk.x, spawnChunk.y].EntityCount;
                ++WorldGenerator.Instance.SpawnedEntityCounts[EntityId];
            }
            catch (Exception error)
            {
                string debugId = !string.IsNullOrEmpty(EntityId) ? $"\"{EntityId}\"" : "null";
                Debug.LogError($"Entity with id {debugId} spreadsheet failure: {error.Message}", gameObject);
            }
        }
    }
}
