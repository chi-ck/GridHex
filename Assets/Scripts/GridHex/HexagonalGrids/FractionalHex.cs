using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridHex
{
    public struct FractionalHex
    {
        public FractionalHex(float q, float r, float s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
            if (Math.Round(q + r + s) != 0) throw new ArgumentException("q + r + s must be 0");
        }
        public readonly float q;
        public readonly float r;
        public readonly float s;

        public Hex HexRound()
        {
            int qi = (int)(Math.Round(q));
            int ri = (int)(Math.Round(r));
            int si = (int)(Math.Round(s));
            float q_diff = Math.Abs(qi - q);
            float r_diff = Math.Abs(ri - r);
            float s_diff = Math.Abs(si - s);
            if (q_diff > r_diff && q_diff > s_diff)
            {
                qi = -ri - si;
            }
            else
                if (r_diff > s_diff)
            {
                ri = -qi - si;
            }
            else
            {
                si = -qi - ri;
            }
            return new Hex(qi, ri, si);
        }


        public FractionalHex HexLerp(FractionalHex b, float t)
        {
            return new FractionalHex(q * (1.0f - t) + b.q * t, r * (1.0f - t) + b.r * t, s * (1.0f - t) + b.s * t);
        }


        static public List<Hex> HexLinedraw(Hex a, Hex b)
        {
            int N = a.Distance(b);
            FractionalHex a_nudge = new FractionalHex(a.q + 1e-06f, a.r + 1e-06f, a.s - 2e-06f);
            FractionalHex b_nudge = new FractionalHex(b.q + 1e-06f, b.r + 1e-06f, b.s - 2e-06f);
            List<Hex> results = new List<Hex> { };
            float step = 1.0f / Math.Max(N, 1);
            for (int i = 0; i <= N; i++)
            {
                results.Add(a_nudge.HexLerp(b_nudge, step * i).HexRound());
            }
            return results;
        }

    }
}