using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GridHex
{
    [CreateAssetMenu(menuName = "GridHex/TileGroup")]
    public class TileGroup : ScriptableObject, ISerializationCallbackReceiver
    {
        //serialized data
        public List<Tile> Tiles = new List<Tile>();
        private Dictionary<int, Tile> _map = new Dictionary<int, Tile>();
        private Dictionary<string, Tile> _nameMap = new Dictionary<string, Tile>();

        public Tile GetTile(int id)
        {
            if (_map.TryGetValue(id, out Tile tile))
            {
                return tile;
            }
            return null;
        }

        public Tile GetTile(string name)
        {
            if (_nameMap.TryGetValue(name, out Tile tile))
            {
                return tile;
            }

            //backwards compatibility
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i].Name == name)
                    return Tiles[i];
            }
            return null;
        }

        public void ConstructMapping()
        {
            _map = new Dictionary<int, Tile>();
            _nameMap = new Dictionary<string, Tile>();
            for (int i = 0; i < Tiles.Count; i++)
            {
                _map.Add(Tiles[i].TileID, Tiles[i]);

                if (!_nameMap.ContainsKey(Tiles[i].Name))
                    _nameMap.Add(Tiles[i].Name, Tiles[i]);
                else
                    Debug.LogWarning($"Autotiles3D:  You have multiple tiles with the same name (duplicated name: {Tiles[i].Name}). This is no longer permitted since Autotiles 1.3.\n Make sure to change your tiles to unique names");
            }
        }

        public void OnAfterDeserialize()
        {
            ConstructMapping();
        }

        public void OnBeforeSerialize()
        {
        }
    }
}
