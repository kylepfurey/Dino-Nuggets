using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DN
{
    [CreateAssetMenu(fileName = "Table", menuName = "Scriptable Objects/Table")]
    public class Table : ScriptableObject
    {
        [SerializeField] Dictionary<string, Object> table;
        public T Get<T>(string id) where T : Object
        {
            if (!table.TryGetValue(id.Trim().ToLower(), out var result))
                return (T)table["default"];
            return (T)result;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Dictionary<string, Object> table = new();
            foreach (var pair in this.table)
                table[pair.Key.Trim().ToLower()] = pair.Value;
            this.table = table;
            if (!this.table.ContainsKey("default"))
                this.table["default"] = null;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
