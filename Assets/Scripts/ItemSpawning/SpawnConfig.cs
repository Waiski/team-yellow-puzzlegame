// =============================
// SpawnTable.cs
// ScriptableObject describing spawn weights for each item prefab.
// Create via Assets > Create > MatchStyle > Spawn Table
// =============================
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "YellowPuzzleGames/SpawnConfig", fileName = "Level-XXX")]
public class SpawnConfig : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public GameObject Prefab;
        [Min(1)] public int Count;
    }

    public List<Entry> Items = new List<Entry>();
}