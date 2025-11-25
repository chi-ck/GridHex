using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GridHex
{
    [System.Serializable]
    public class HexNode
    {
        public TileLayer Layer;
        public Hex InternalHex;
        public Quaternion LocalRotation = Quaternion.identity;

        [SerializeField] private GameObject _instance;
        [SerializeField] private string _group; 
        [SerializeField] private string _tileName; 
        [SerializeField] private int _tileID = -1; //
        [SerializeField] private int _ruleID = -1; // -1意味着没用规则瓦片，使用默认预制体
        public int TileID { get => _tileID; }
        public int RuleID { get => _ruleID; }
        /// <summary>
        /// -1意味着使用默认预制体
        /// </summary>
        public void SetRuleID(int ruleId)
        {
            _ruleID = ruleId;
        }

        public string TileName => _tileName;

        public HexNode(TileLayer layer, string group, string tileName, int tileID, Hex hex, Quaternion localRotation, GameObject instance = null)
        {
            Layer = layer;
            UpdateTileInfo(tileID, tileName, group);

            LocalRotation = localRotation;
            InternalHex =hex;

            if (instance != null)
            {
                Instance = instance;
            }
        }

        public void UpdateTileInfo(int tileID, string tileName, string group)
        {
            _tileID = tileID;
            _tileName = tileName;
            _group = group;
        }

        public Tile GetTile()
        {
            return Utility.GetTile(_tileID, _tileName, _group);
        }

        public GameObject Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }
    }


    public class TileLayer : MonoBehaviour, ISerializationCallbackReceiver
    {
        public string LayerName;
        private Dictionary<Hex,HexNode> _hexNodes =new Dictionary<Hex, HexNode> ();

        public bool ContainsKey(Hex hex)
        {
            return _hexNodes.ContainsKey(hex);
        }
        public List<HexNode> GetAllInternalNodes()
        {
            return _hexNodes.Values.ToList();
        }

        public TileGroup Group;
        public List<Tile> Tiles => Group != null ? Group.Tiles : new List<Tile>();

        private int _activeTileID = -1;
        public int ActiveTileID => _activeTileID;
        public void SetActiveTileID(int tileID)
        {
            _activeTileID = tileID;
        }
        public void ResetActiveTileID()
        {
            _activeTileID = -1;
            if (Tiles.Count > 0)
                _activeTileID = Tiles[0].TileID;
        }

        public List<TileGroup> LoadedGroups = new List<TileGroup>();
        private List<Hex> _toVerify = new List<Hex>();
        private List<Hex> _toUpdate= new List<Hex> ();

        private (int tileID, GameObject instance) _hoverInstance;
        public (int tileID, GameObject instance) HoverInstance { get => _hoverInstance; set => _hoverInstance = value; }
        public GameObject HoverPrefabObject; //the object of the rule that won, not the instance
        public Hex LocalHoverHex;

        private Grid _Grid;
        public Grid Grid
        {
            get
            {
                if (_Grid == null)
                    _Grid = GetComponentsInParent<Grid>(includeInactive: true)[0];
                return _Grid;
            }
            set
            {
                _Grid = value;
            }
        }


        #region Serialization
        [SerializeField] private List<Hex> _NodesKeys = new List<Hex>();
        [SerializeField] private List<HexNode> _NodesValues = new List<HexNode>();
        public void OnBeforeSerialize()
        {
            _NodesKeys = _hexNodes.Keys.ToList();
            _NodesValues = _hexNodes.Values.ToList();

        }

        public void OnAfterDeserialize()
        {
            _hexNodes = new Dictionary<Hex, HexNode>();
            for (int i = 0; i < _NodesKeys.Count; i++)
            {
                _hexNodes.Add(_NodesKeys[i], _NodesValues[i]);
            }
        }
        #endregion

        void UpdateNeighbors(Hex originalHex) //slow if called repeatly while overlapping
        {
            var neighbors = this.GetNeighborsHex(originalHex);
            foreach (var neighbor in neighbors)
            {
                if (_hexNodes.ContainsKey(neighbor))
                {
                    RequireVerification(neighbor);
                }
            }
        }


        /// <summary>
        /// 向图层添加一个节点
        /// </summary>
        /// <param name="internalHex"></param>
        /// <param name="tileID"></param>
        public void TryPlacementSingle(Hex internalHex, Quaternion localRotation, int tileID)
        {
            var tile = Utility.GetTile(tileID);
            if (tile == null)
            {
                return;
            }

            AddNodeInternal(tile.TileID, tile.Name, tile.Group, internalHex, localRotation);
            UpdateNeighbors(internalHex);
        }

        /// <summary>
        /// 向图层移除一个节点
        /// </summary>
        /// <param name="internalHex"></param>
        public void TryUnplacingSingle(Hex internalHex)
        {
            RemoveNodeInternal(internalHex);
            UpdateNeighbors(internalHex);
        }

        private void AddNodeInternal(int tileId, string tileName, string group, Hex hex, Quaternion localRotation)
        {
            if (!_hexNodes.ContainsKey(hex))
            {
                _hexNodes.Add(hex, new HexNode(this,group,tileName,tileId,hex, localRotation));

                RequireUpdate(hex);
            }
        }

        private void RemoveNodeInternal(Hex hex)
        {
            if (_hexNodes.TryGetValue(hex, out HexNode node))
            {
                node.DeleteInstance();
                _hexNodes.Remove(hex);
            }
        }

        /// <summary>
        /// 刷新单个节点，相应地更新游戏对象实例。
        /// </summary>
        /// <param name="hex"></param>
        public void RequireUpdate(Hex hex)
        {
            if (_hexNodes.TryGetValue(hex, out HexNode node))
            {
                _toUpdate.Add(hex);
            }
        }
        public void RequireVerification(Hex internalHex)
        {
            if (_hexNodes.TryGetValue(internalHex, out HexNode node))
            {
                _toVerify.Add(internalHex);
            }
        }
        public void VerifyNodes()
        {
            for (int i = 0; i < _toVerify.Count; i++)
            {
                if (_hexNodes.TryGetValue(_toVerify[i], out HexNode node))
                {
                    node.VerifyInstance();
                }
            }
            _toVerify.Clear();
        }
        public void RefreshNodes()
        {
            for (int i = 0; i < _toUpdate.Count; i++)
            {
                if (_hexNodes.TryGetValue(_toUpdate[i], out HexNode node))
                {
                    node.UpdateInstance();                   
                }
            }
            _toUpdate.Clear();
        }
        public void DestroyHoverInstance()
        {
            if (_hoverInstance.instance != null)
            {
                DestroyImmediate(_hoverInstance.instance);
                _hoverInstance.tileID = -1;
                _hoverInstance.instance = null;
            }
        }

    }
}