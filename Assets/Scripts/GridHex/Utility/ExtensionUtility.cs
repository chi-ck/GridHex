using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GridHex
{
    public static class ExtensionUtility 
    {
        public static void DeleteInstance(this HexNode node)
        {
            if(node.Instance!=null)
            {
                node.Instance.SetActive(true);

                GameObject.DestroyImmediate(node.Instance);
            }
        }
        /// <summary>
        /// 类似于更新实例，但如果可能的话，会尽量保留当前的皮肤/游戏对象。只有在邻居发生变化时，才会更换实例。
        /// </summary>
        /// <param name="node"></param>
        public static void VerifyInstance(this HexNode node)
        {
            if (node.Instance == null)
            {
                node.UpdateInstance();
            }
            else
            {
                GameObject prefab = null;
                var tile = node.GetTile();
                int[] addedRotation = new int[] { -1, 0 };

                if (tile.HasRules)
                {
                    bool[] neighbors = node.Layer.GetNeighborsBoolSelfSpace(node.InternalHex, node.LocalRotation);
                    var rule = tile.GetRule(neighbors, out addedRotation);

                    if (rule != null)
                    {
                        if (rule.RuleID != node.RuleID)
                        {
                            node.SetRuleID(rule.RuleID);
                            prefab = rule.Object;
                        }
                    }
                    else
                    {
                        if (node.RuleID != -1)
                        {
                            prefab = tile.Default;
                            node.SetRuleID(-1);
                        }
                    }
                }
                else
                {
                    if (node.RuleID != -1)
                    {
                        prefab = tile.Default;
                        node.SetRuleID(-1);
                    }
                }

                if (addedRotation[0] == 1)
                {
                    node.LocalRotation *= Quaternion.AngleAxis(addedRotation[1], Vector3.up);
                }

                if (prefab != null)
                {
                    if (node.Instance == null || (node.Instance != null && PrefabUtility.GetCorrespondingObjectFromSource(node.Instance) != prefab))
                    {
                        node.CreateInstance(prefab);
                    }
                }

                if (node.Instance != null)
                {
                    node.UpdateInstanceTransform();
                }


#if UNITY_EDITOR
                if (node.Instance != null)
                    EditorUtility.SetDirty(node.Instance);
                if (node.Layer != null)
                    EditorUtility.SetDirty(node.Layer);
#endif
            }
        }


        public static void UpdateInstance(this HexNode node)
        {
            GameObject prefab = null;
            int[] addedRotation = new int[] { -1, 0 };

            var tile = node.GetTile();

            if (tile.HasRules)
            {
                bool[] neighbors = node.Layer.GetNeighborsBoolSelfSpace(node.InternalHex, node.LocalRotation);

                var rule = tile.GetRule(neighbors, out addedRotation);

                if (rule != null)
                {
                    node.SetRuleID(rule.RuleID);
                    prefab = rule.Object;
                }
                else
                {
                    prefab = tile.Default;
                    node.SetRuleID(-1);
                }
            }
            else
            {
                prefab = tile.Default;
                node.SetRuleID(-1);
            }

            if (addedRotation[0]==1)
            {
                node.LocalRotation *= Quaternion.AngleAxis(addedRotation[1], Vector3.up);
            }

            if (prefab != null)
            {
                if (node.Instance == null || (node.Instance != null && PrefabUtility.GetCorrespondingObjectFromSource(node.Instance) != prefab))
                {
                    node.CreateInstance(prefab);
                }
            }

            if (node.Instance != null)
            {
                node.UpdateInstanceTransform();
            }

#if UNITY_EDITOR
            if (node.Instance != null)
                EditorUtility.SetDirty(node.Instance);
            if (node.Layer != null)
                EditorUtility.SetDirty(node.Layer);
#endif
        }

        public static void CreateInstance(this HexNode node, GameObject prefab)
        {
            var newInstance = PrefabUtility.InstantiatePrefab(prefab,node.Layer.transform) as UnityEngine.GameObject;

            if (node.Instance != null)
            {
                GameObject.DestroyImmediate(node.Instance);
            }

            node.Instance = newInstance;
        }

        public static void UpdateInstanceTransform(this HexNode node)
        {
            if (node.Instance != null)
            {
                Vector2 hexToPixel = node.Layer.Grid.Layout.HexToPixel(node.InternalHex);
                node.Instance.transform.position = new Vector3(hexToPixel.x, node.Layer.Grid.LayerIndex, hexToPixel.y);
                node.Instance.transform.rotation = node.Layer.Grid.transform.rotation * node.LocalRotation;
            }
        }

        public static List<Hex> GetNeighborsHex(this TileLayer layer, Hex internalHex)
        {
            var myNeighbors = new List<Hex>();

            Hex iteration;
            for (int i=0;i<6;i++)
            {
                iteration= internalHex.Neighbor(i);
                if (layer.ContainsKey(iteration))
                {
                    myNeighbors.Add(iteration);
                }
            }

            return myNeighbors;
        }

        public static bool[] GetNeighborsBoolSelfSpace(this TileLayer layer, Hex internalHex,Quaternion localRotation)
        {
            bool[] neighbors= new bool[6];
            Hex iteration;

            for (int i = 0; i < 6; i++)
            {
                // 使用布局系统将六边形方向（q, r, s）转换为像素坐标（即平面坐标）
                Vector2 direction = layer.Grid.Layout.HexToPixel(Hex.Direction(i));
                // 使用四元数旋转方向向量，旋转后得到新的方向
                Vector3 rotatedDirection = localRotation * new Vector3(direction.x, 0, direction.y);
                // 将旋转后的像素方向转换回六边形方向
                var hexDirection = layer.Grid.Layout.PixelToHexRounded(new Vector2(rotatedDirection.x,rotatedDirection.z));
                // 相加获取方向上的六边形坐标
                iteration= internalHex.Add(hexDirection);

                if(layer.ContainsKey(iteration) )
                {
                    neighbors[i] = true;
                }
            }
            return neighbors;
        }
    }
}