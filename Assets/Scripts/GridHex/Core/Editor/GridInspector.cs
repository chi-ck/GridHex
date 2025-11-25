using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GridHex
{
    [CustomEditor(typeof(Grid), true)]
    public class GridInspector : Editor
    {
        Grid _grid;
        private string _gridHierarchyName
        {
            get
            {
                return "Grid";
            }
        }
        public virtual void OnEnable()
        {
            _grid = (Grid)target;

            var layers = _grid.GetComponentsInChildren<TileLayer>();
            _grid.TileLayers = layers.ToList();
        }

        public override void OnInspectorGUI()
        {
            var size = EditorGUILayout.Vector2Field("Size", _grid.Size);
            if (size != _grid.Size)
            {
                _grid.Size = size;
                foreach (var tilelayer in _grid.TileLayers)
                {
                    var nodes = tilelayer.GetAllInternalNodes();
                    foreach (var node in nodes)
                        node.UpdateInstanceTransform();
                }
            }


            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Add new tile layer"))
            {
                var child = new GameObject();
                child.transform.SetParent(_grid.transform);
                child.AddComponent<TileLayer>();
            }
            EditorGUILayout.EndVertical();

            if (_grid.gameObject.name != _gridHierarchyName)
                _grid.gameObject.name = _gridHierarchyName;
        }
    }
}