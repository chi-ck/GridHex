using UnityEditor;
using UnityEngine;

namespace GridHex
{
    [CustomEditor(typeof(TileGroup))]
    public class TileGroupInspector : Editor
    {
        TileGroup _group;

        private void OnEnable()
        {
            _group = (TileGroup)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            foreach (var tile in _group.Tiles.ToArray())
            {
                tile.SetGroupName(_group.name);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                tile.RenderTileGUI(out bool dirty, _group);
                EditorGUILayout.EndVertical();
                GUILayout.Space(20);
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add new tile"))
                {
                    _group.Tiles.Add(new Tile("New Tile"));
                }
                if (GUILayout.Button("Remove last tile"))
                {
                    if (_group.Tiles.Count > 0)
                    {
                        _group.Tiles.RemoveAt(_group.Tiles.Count - 1);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck() || GUI.changed)
            {
                _group.ConstructMapping();
                EditorUtility.SetDirty(_group);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}