using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace GridHex
{
    public struct Layout
    {
        public Layout(Orientation orientation, Vector2 size, Vector2 origin)
        {
            this.orientation = orientation;
            this.size = size;
            this.origin = origin;
        }
        public readonly Orientation orientation;
        public readonly Vector2 size;
        public readonly Vector2 origin;
        static public Orientation pointy = new Orientation(Mathf.Sqrt(3.0f), Mathf.Sqrt(3.0f) / 2.0f, 0.0f, 3.0f / 2.0f, Mathf.Sqrt(3.0f) / 3.0f, -1.0f / 3.0f, 0.0f, 2.0f / 3.0f, 0.5f);
        static public Orientation flat = new Orientation(3.0f / 2.0f, 0.0f, Mathf.Sqrt(3.0f) / 2.0f, Mathf.Sqrt(3.0f), 2.0f/ 3.0f, 0.0f, -1.0f / 3.0f, Mathf.Sqrt(3.0f) / 3.0f, 0.0f);

        public Vector2 HexToPixel(Hex h)
        {
            Orientation M = orientation;
            float x = (M.f0 * h.q + M.f1 * h.r) * size.x;
            float y = (M.f2 * h.q + M.f3 * h.r) * size.y;
            return new Vector2(x + origin.x, y + origin.y);
        }


        public FractionalHex PixelToHexFractional(Vector2 p)
        {
            Orientation M = orientation;
            Vector2 pt = new Vector2((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
            float q = M.b0 * pt.x + M.b1 * pt.y;
            float r = M.b2 * pt.x + M.b3 * pt.y;
            return new FractionalHex(q, r, -q - r);
        }


        public Hex PixelToHexRounded(Vector2 p)
        {
            return PixelToHexFractional(p).HexRound();
        }


        public Vector2 HexCornerOffset(int corner)
        {
            Orientation M = orientation;
            float angle = 2.0f * Mathf.PI * (M.start_angle - corner) / 6.0f;
            return new Vector2(size.x * Mathf.Cos(angle), size.y * Mathf.Sin(angle));
        }


        public List<Vector2> PolygonCorners(Hex h)
        {
            List<Vector2> corners = new List<Vector2> { };
            Vector2 center = HexToPixel(h);
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = HexCornerOffset(i);
                corners.Add(new Vector2(center.x + offset.x, center.y + offset.y));
            }
            return corners;
        }

    }

}
