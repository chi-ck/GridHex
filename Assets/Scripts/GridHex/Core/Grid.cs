using System.Collections.Generic;
using UnityEngine;

namespace GridHex
{
    public class Grid : MonoBehaviour
    {
        public Vector2 Size;

        public List<TileLayer> TileLayers = new List<TileLayer>();


        public Layout Layout
        {
            get
            {
                return new Layout(Layout.pointy, Size, new Vector2(0,0));
            }
        }

        [SerializeField] private int _LayerIndex;
        public int LayerIndex { get { return _LayerIndex; } protected set { _LayerIndex = value; } }

        public void SetLayerIndex(int index)
        {
            _LayerIndex = index;
        }
    }
}