using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


namespace GridHex
{
    public enum Relation
    {
        None = 0,
        Arrow = 1,
        Edge = 2
    }

    [System.Serializable]
    public class Rule
    {
        public GameObject Object;

        public Relation[] Relations = new Relation[6];//0 nothing, 1 arrow 2 edge
        public bool AllowRotation;

        [SerializeField] private int _ruleID = -1;
        public Rule()
        {
            _ruleID = System.Guid.NewGuid().GetHashCode();
        }
        public int RuleID
        {
            get
            {
                if (_ruleID == -1)
                    _ruleID = System.Guid.NewGuid().GetHashCode();
                return _ruleID;
            }
        }
    }


    [System.Serializable]
    public class Tile
    {
        [SerializeField] private string _group; //name of the TileGroup this belongs to
        [SerializeField] private int _tileID = -1;

        public GameObject Default;

        [SerializeField]
        [FormerlySerializedAs("DisplayName")]
        private string _name;

        public string Name => _name;

        public List<Rule> Rules = new List<Rule>();
        public bool HasRules => Rules.Count > 0;
        public int TileID
        {
            get
            {
                if (_tileID == -1)
                    _tileID = System.Guid.NewGuid().GetHashCode();
                return _tileID;
            }
        }

        public void SetGroupName(string group)
        {
            _group = group;
        }

        public string Group => _group;

        public Tile(string displayName)
        {
            _name = displayName;
        }
        private const float _width = 80;

        public Rule GetRule(bool[] neighbors, out int[] addedRotation)
        {
            {
                addedRotation = new int[] { -1, 0 }; // 默认情况下，返回一个默认的旋转（-1 表示未进行旋转，0 表示旋转角度为 0）

                if (!HasRules)
                    return null;

                foreach (var rule in Rules)
                {
                    if (DoesRulePass(rule, neighbors, out int[] succesfullRotation))
                    {
                        addedRotation = succesfullRotation;
                        return rule;
                    }
                }

                return null;
            }
        }

        private bool DoesRulePass(Rule rule, bool[] neighbors, out int[] rotation)
        {
            rotation = new int[] { 0, 0 }; // 默认旋转，0 表示无旋转，第二个元素用于保存旋转角度

            if (rule == null) // 如果规则为空，则返回 false
                return false;

            // 检查规则中的边缘匹配和箭头匹配
            bool passEdges = DoEdgesMatch(rule.Relations, neighbors);
            bool passArrows = DoArrowsMatch(rule.Relations, neighbors);

            // 如果边缘和箭头都匹配，则返回 true
            if (passEdges && passArrows)
            {
                return true;
            }

            // 如果边缘和箭头匹配失败，则尝试对规则进行旋转
            var checkedRotation = rule.Relations;
            rotation[0] = -1;
            rotation[1] = 0;

            if (rule.AllowRotation)
            {
                for (int i = 0; i < 5; i++)
                {
                    rotation[1] += 60;
                    var rotated = RotateRelationsClockwise(checkedRotation);
                    passEdges = DoEdgesMatch(rotated, neighbors);
                    passArrows = DoArrowsMatch(rotated, neighbors);
                    if (passEdges && passArrows)
                    {
                        rotation[0] = 1;
                        return true;
                    }
                    checkedRotation = rotated;
                }
            }
            return false;
        }
        private bool DoEdgesMatch(Relation[] relations, bool[] neighbors)
        {
            for (int i = 0; i < relations.Length; i++)
            {
                if (relations[i] == Relation.Edge && neighbors[i]) 
                    return false;
            }
            return true;
        }
        private bool DoArrowsMatch(Relation[] relations, bool[] neighbors)
        {
            for (int i = 0; i < relations.Length; i++)
            {
                if (relations[i] == Relation.Arrow && !neighbors[i])
                    return false;
            }
            return true;
        }

        private Relation[] RotateRelationsClockwise(Relation[] relations)
        {
            Relation[] rotated = new Relation[6];

            rotated[5] = relations[4];
            rotated[0] = relations[5];
            rotated[1] = relations[0];
            rotated[2] = relations[1];
            rotated[3] = relations[2];
            rotated[4] = relations[3];

            return rotated;
        }

        public void RenderTileGUI(out bool dirty, UnityEngine.Object context)
        {

            dirty = false;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    var newName = EditorGUILayout.DelayedTextField("Name", Name);
                    if (newName != Name)
                    {
                        _name = newName;
                    }

                    var defaultGO = EditorGUILayout.ObjectField(Default, typeof(GameObject), allowSceneObjects: false) as GameObject;
                    if (defaultGO != Default)
                    {
                        Default = defaultGO;
                    }
                }
                EditorGUILayout.EndVertical();

                if (Default != null)
                {
                    var texture = AssetPreview.GetAssetPreview(Default);
                    GUIContent content = new GUIContent(texture);
                    EditorGUILayout.LabelField(content, GUILayout.Height(100/*EditorGUIUtility.singleLineHeight * 2*/));
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Rules");

            foreach (var rule in Rules.ToArray())
            {
                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);


                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Rule", GUILayout.Width(_width));
                    GUILayout.FlexibleSpace();

                    var ruleObject = EditorGUILayout.ObjectField(rule.Object, typeof(GameObject), allowSceneObjects: false, GUILayout.Width(_width * 2)) as GameObject;
                    if (rule.Object != ruleObject)
                    {
                        rule.Object = ruleObject;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Free Axis Rotation");
                EditorGUIUtility.labelWidth = 13;
                EditorGUILayout.BeginHorizontal();
                {
                    var ruleRot1 = EditorGUILayout.Toggle("Y", rule.AllowRotation, GUILayout.Width(30));
                    if (ruleRot1 != rule.AllowRotation)
                    {
                        rule.AllowRotation = ruleRot1;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical();
                    {
                        GUIContent[] content = new GUIContent[6];

                        for (int j = 0; j < content.Length; j++)
                        {
                            content[j] = GetContent(rule.Relations[j], 0);
                        }

                        int selection = -1;

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(28);
                            if (GUILayout.Button(content[4], GUILayout.Width(50), GUILayout.Height(50))) selection = 4;
                            if (GUILayout.Button(content[5], GUILayout.Width(50), GUILayout.Height(50))) selection = 5;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(content[3], GUILayout.Width(50), GUILayout.Height(50))) selection = 3;
                            if (rule.Object != null)
                            {
                                GUILayout.Button(new GUIContent(AssetPreview.GetAssetPreview(rule.Object)), GUILayout.Width(50), GUILayout.Height(50));
                            }
                            else
                            {
                                GUILayout.Button(new GUIContent(), GUILayout.Width(50), GUILayout.Height(50));
                            }
                            if (GUILayout.Button(content[0], GUILayout.Width(50), GUILayout.Height(50))) selection = 0;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(28);
                            if (GUILayout.Button(content[2], GUILayout.Width(50), GUILayout.Height(50))) selection = 2;
                            if (GUILayout.Button(content[1], GUILayout.Width(50), GUILayout.Height(50))) selection = 1;
                        }
                        EditorGUILayout.EndHorizontal();


                        if (selection > -1)
                        {
                            var tempSelection = EnumUtility.Next(rule.Relations[selection]);
                            if (rule.Relations[selection] != tempSelection)
                            {
                                rule.Relations[selection] = tempSelection;
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+"))
                {
                    Rules.Add(new Rule());
                }
                if (GUILayout.Button("-"))
                {
                    if (Rules.Count > 0)
                    {
                        Rules.RemoveAt(Rules.Count - 1);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

        }

        private GUIContent GetContent(Relation relation, int direction)
        {
            switch (relation)
            {
                case Relation.None:
                    return new GUIContent(Resources.Load("Icons/square") as Texture);
                case Relation.Arrow:
                    return new GUIContent(Resources.Load("Icons/check") as Texture);
                case Relation.Edge:
                    return new GUIContent(Resources.Load("Icons/cross") as Texture);
            }
            return new GUIContent();
        }


    }
}
