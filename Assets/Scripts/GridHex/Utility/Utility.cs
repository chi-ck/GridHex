using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridHex
{
    public static class Utility 
    {

        private static Dictionary<int, Tile> _cache = new Dictionary<int, Tile>();
        private static Dictionary<string, Tile> _tagCache = new Dictionary<string, Tile>();
        private static Dictionary<string, TileGroup> _groupCache = new Dictionary<string, TileGroup>();

        private static TileGroup[] _gCache = null;
        public static TileGroup[] Groups
        {
            get
            {
                if (_gCache == null)
                    _gCache = Resources.LoadAll<TileGroup>("");
                return _gCache;
            }
        }
        public static Tile GetTile(int tileID)
        {
            Tile tile;

            //try id cache
            if (_cache.TryGetValue(tileID, out tile))
            {
                if (tile != null)
                    return tile;
                _cache.Remove(tileID);
            }

            foreach (var g in Groups)
            {
                tile = g.GetTile(tileID);
                if (tile != null)
                {
                    _cache.Add(tileID, tile);
                    return tile;
                }               ;
            }
            return null;
        }

        public static Tile GetTile(int tileID, string name, string group)
        {
            Tile tile;

            //try id cache
            if (_cache.TryGetValue(tileID, out tile))
            {
                if (tile != null)
                    return tile;
                _cache.Remove(tileID);
            }

            string tag = group + name;
            //try tag cache
            if (_tagCache.TryGetValue(tag, out tile))
            {
                if (tile != null)
                    return tile;
                _tagCache.Remove(tag);
            }

            //get group
            TileGroup tileGroup = GetGroup(group);

            if (tileGroup != null)
            {
                //try id first
                tile = tileGroup.GetTile(tileID);
                if (tile != null)
                {
                    _cache.Add(tileID, tile);
                    return tile;
                }

                //try tag next
                tile = tileGroup.GetTile(name);
                if (tile != null)
                {
                    _tagCache.Add(tag, tile);
                    return tile;
                }
            }

            //try looking just via ID one more time
            if (tileID != -1)
            {
                return GetTile(tileID);
            }

            return null;
        }
        public static TileGroup GetGroup(string name)
        {
            TileGroup group = null;

            if (string.IsNullOrEmpty(name))
                return null;

            if (_groupCache.TryGetValue(name, out group))
            {
                if (group != null)
                    return group;
                _groupCache.Remove(name);
            }

            foreach (var g in Groups)
            {
                if (g == null)
                    continue;
                if (g.name == name)
                {
                    _groupCache.Add(g.name, g);
                    return g;
                }
            }
            return null;
        }

        public static List<TileGroup> LoadTileGroups()
        {
            return Resources.LoadAll<TileGroup>("").ToList();
        }
    }
}
