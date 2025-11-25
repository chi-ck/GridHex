using GridHex;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GridHex
{
    [CustomEditor(typeof(TileLayer))]
    public class TileLayerInspector : Editor
    {
        TileLayer _tileLayer;
        Grid Grid => _tileLayer.Grid;
        public Layout Layout => Grid.Layout;
        private int _LayerIndex => Grid.LayerIndex;
        private int _TileRotation;
        private int _ControlID = 0;
        private Vector3 _MousePositionGUI;
        private Ray _MouseRay;

        private const int _sp1 = 120;
        private const int _sp3Controls = 150;
        private const int fatHeight = 40;
        private bool _HasRenderedHover;

        private Dictionary<int, TileData> _TileData = new Dictionary<int, TileData>();
        public class TileData
        {
            public int TileId;
            public Texture2D Thumbnail;
            public TileData(int tileID)
            {
                TileId = tileID;
            }
        }

        private void OnEnable()
        {
            _tileLayer = (TileLayer)target;

            if (!Application.isPlaying)
            {
                foreach (Transform t in _tileLayer.transform)
                {
                    if (t.gameObject.name == "HoverInstance")
                        DestroyImmediate(t.gameObject);
                }
            }

            _tileLayer.LoadedGroups = Utility.LoadTileGroups();

            if (_tileLayer.Group == null && _tileLayer.LoadedGroups.Count > 0)
                _tileLayer.Group = _tileLayer.LoadedGroups[0];

            CreateTileData();
        }

        private void OnDisable()
        {
            _tileLayer.DestroyHoverInstance();
            Tools.hidden = false;
        }

        public void OnMouseExitSceneWindow()
        {
            _tileLayer.DestroyHoverInstance();
        }

        private void OnSceneGUI()
        {
            _HasRenderedHover = false;

            _ControlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            _MousePositionGUI = Event.current.mousePosition;
            _MouseRay = HandleUtility.GUIPointToWorldRay(_MousePositionGUI);
            Event e = Event.current;

            if (e.type == EventType.MouseLeaveWindow)
                _tileLayer.DestroyHoverInstance();

            DrawHoverSurroundGrid( Layout, _tileLayer.LocalHoverHex);

            //网格选择计算
            var normal = Grid.transform.TransformDirection(Vector3Int.up).normalized;
            var planeposition = Grid.transform.TransformPoint(new Vector3(0, _LayerIndex , 0));
            Plane plane = new Plane(normal, planeposition);

            plane.Raycast(_MouseRay, out float distance);
            Vector3 worldHit = _MouseRay.GetPoint(distance);

            _tileLayer.LocalHoverHex = Layout.PixelToHexRounded(new Vector2((Grid.transform.InverseTransformPoint(worldHit)).x, (Grid.transform.InverseTransformPoint(worldHit)).z));

            if (e.type == EventType.ScrollWheel)
            {
                int scroll = (e.delta.y > 0) ? -1 : 1;
                if(e.control)
                {
                    int delta = (scroll > 0) ? 60 : -60;
                    _TileRotation += delta;
                    Event.current.Use();
                }
            }

            RenderHoverInstance(_tileLayer.LocalHoverHex, Quaternion.AngleAxis(_TileRotation, Vector3.up));

            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag || e.type == EventType.MouseUp)
            {
                GridSelection(e.type, e, _tileLayer.LocalHoverHex);
            }

            if (e.type == EventType.MouseLeaveWindow)
            {
                OnMouseExitSceneWindow();
            }

            if (!_HasRenderedHover)
                _tileLayer.DestroyHoverInstance();

            // 对六边形节点更改进行刷新
            _tileLayer.VerifyNodes();
            _tileLayer.RefreshNodes();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Layer Name:"/* , GUILayout.Width(_sp1) */);
                    string displayName = EditorGUILayout.DelayedTextField(_tileLayer.LayerName/* , GUILayout.Width(_sp1) */);
                    EditorGUIUtility.labelWidth = 0;
                    if (displayName != _tileLayer.LayerName)
                    {
                        _tileLayer.gameObject.name = "Tile Layer: " + displayName;
                        _tileLayer.LayerName = displayName;
                        CreateTileData();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Current layer height:"/* , GUILayout.Width(_sp1) */);
                    int newLayerHeight = EditorGUILayout.DelayedIntField(_LayerIndex/* , GUILayout.Width(_sp1) */);
                    if (newLayerHeight != _LayerIndex)
                    {
                        Grid.SetLayerIndex(newLayerHeight);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                var groupNames = _tileLayer.LoadedGroups.Select(g => g.name).ToList();
                var activeGroupName = _tileLayer.Group != null ? _tileLayer.Group.name : "";
                int index = Math.Max(groupNames.IndexOf(activeGroupName), 0);

                EditorGUILayout.BeginHorizontal();
                {
                    EditorStyles.popup.fixedHeight = fatHeight;
                    int newIndex = EditorGUILayout.Popup(index, groupNames.ToArray(), GUILayout.Width(_sp1 * 2), GUILayout.Height(fatHeight));
                    EditorStyles.popup.fontSize = 0;
                    EditorStyles.popup.fixedHeight = 0;
                    if (index != newIndex)
                    {
                        _tileLayer.Group = _tileLayer.LoadedGroups[newIndex];
                        CreateTileData();
                        _tileLayer.ResetActiveTileID();
                    }
                }
                EditorGUILayout.EndHorizontal();


                GUIStyle offsetLabel = new GUIStyle(GUI.skin.label);
                offsetLabel.alignment = TextAnchor.MiddleCenter;
                offsetLabel.padding = new RectOffset(0, 0, 13, 13);
                offsetLabel.richText = true;
                offsetLabel.wordWrap = true;
                offsetLabel.normal.textColor = Color.white;
                offsetLabel.alignment = TextAnchor.MiddleLeft;

                foreach (var tile in _tileLayer.Tiles)
                {
                    int tileID = tile.TileID;
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            if (_tileLayer.ActiveTileID == tileID)
                            {
                                var linestyle = new GUIStyle();
                                linestyle.normal.background = EditorGUIUtility.whiteTexture;
                                linestyle.margin = new RectOffset(0, 0, 5, 5);
                                linestyle.fixedHeight = 35;
                                GUILayout.Box("", linestyle, GUILayout.Width(2));
                            }

                            //verify thumbnail - sometimes this is missing, probably because AssetPreview.GetAssetPreview is faulty?
                            if (_TileData[tileID].Thumbnail == null && tile.Default != null)
                            {
                                var dtex = AssetPreview.GetAssetPreview(tile.Default);
                                if (dtex != null)
                                    _TileData[tileID].Thumbnail = dtex;
                            }

                            if (_TileData[tileID].Thumbnail != null)
                            {
                                GUIContent previewContent = new GUIContent(_TileData[tileID].Thumbnail);
                                if (GUILayout.Button(previewContent, GUILayout.Width(fatHeight), GUILayout.Height(fatHeight)))
                                {
                                    _tileLayer.SetActiveTileID(tileID);
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField($"  <color=yellow> {tile.Name}", offsetLabel);
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                RenderControls();
            }
            EditorGUILayout.EndVertical();
        }

        public void RenderHoverInstance(Hex internalHex, Quaternion localRotation)
        {
            var activeTile = Utility.GetTile(_tileLayer.ActiveTileID);

            if (_tileLayer.ActiveTileID == -1 || activeTile == null)
            {
                _tileLayer.DestroyHoverInstance();
                return;
            }

            if (_tileLayer.ContainsKey(internalHex))
            {
                _tileLayer.DestroyHoverInstance();
                return;
            }

            GameObject prefab = activeTile.Default;

            if (prefab != null)
            {
                var hoverInstance = _tileLayer.HoverInstance.instance;
                if (_tileLayer.HoverPrefabObject != prefab || hoverInstance == null)
                {
                    _tileLayer.DestroyHoverInstance();
                    hoverInstance = PrefabUtility.InstantiatePrefab(prefab, _tileLayer.transform) as UnityEngine.GameObject;
                    hoverInstance.name = "HoverInstance";
                    _tileLayer.HoverInstance = (_tileLayer.ActiveTileID, hoverInstance);
                    _tileLayer.HoverPrefabObject = prefab;
                }
                Vector2 hexToPixel = Grid.Layout.HexToPixel(internalHex);
                _tileLayer.HoverInstance.instance.transform.position = new Vector3(hexToPixel.x,_LayerIndex, hexToPixel.y);

                _tileLayer.HoverInstance.instance.transform.rotation = Grid.transform.rotation * localRotation;
            }

            _HasRenderedHover = true;
        }

        public void GridSelection(EventType eventType, Event e, Hex hex)
        {
            if (eventType == EventType.MouseDown || eventType == EventType.MouseDrag)
            {
                if (e.button == 0)
                {
                    if (!_tileLayer.ContainsKey(hex))
                    {
                        int rotation = _TileRotation;

                        _tileLayer.TryPlacementSingle(hex, Quaternion.AngleAxis(rotation, Vector3.up), _tileLayer.ActiveTileID);
                    }
                    e.Use();
                }
                else if (e.button == 1)
                {
                    if (_tileLayer.ContainsKey(hex))
                    {
                        _tileLayer.TryUnplacingSingle(hex);
                    }
                    e.Use();
                }
            }
        }

        private void DrawHoverSurroundGrid(Layout layout, Hex hex)
        {
            List<Vector2> corners = layout.PolygonCorners(hex);

            for (int i = 0; i < 6; i++)
            {
                Vector3 a = new Vector3(corners[i].x, _LayerIndex, corners[i].y);
                Vector3 b = new Vector3(corners[(i + 1) % 6].x, _LayerIndex, corners[(i + 1) % 6].y);
                Handles.DrawLine(a, b);
            }
        }

        void CreateTileData()
        {
            _TileData.Clear();
            foreach (var tile in _tileLayer.Tiles)
            {
                TileData data = new TileData(tile.TileID);

                _TileData.Add(tile.TileID, data);

                Texture2D tex = null;//Resources.Load("Icons/square") as Texture2D;
                if (tile.Default != null)
                {

                    var dtex = AssetPreview.GetAssetPreview(tile.Default);
                    if (dtex != null)
                        tex = dtex;
                }
                data.Thumbnail = tex;
            }
        }

        void RenderControls()
        {
            //add tile
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Place tile", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Left mouse button (click or drag)");
            EditorGUILayout.EndHorizontal();

            //remove tile
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Remove tile", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Right mouse button (click or drag)");
            EditorGUILayout.EndHorizontal();

            //rotating blocks
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rotate block (Y-Axis)", GUILayout.Width(_sp3Controls));
            EditorGUILayout.LabelField("Left Control + Mouse wheel (scroll)");
            EditorGUILayout.EndHorizontal();
        }
    }
}
