using UnityEngine;

namespace GridHex
{
    public struct Orientation
    {
        public Orientation(float f0, float f1, float f2, float f3, float b0, float b1, float b2, float b3, float start_angle)
        {
            this.f0 = f0;
            this.f1 = f1;
            this.f2 = f2;
            this.f3 = f3;
            this.b0 = b0;
            this.b1 = b1;
            this.b2 = b2;
            this.b3 = b3;
            this.start_angle = start_angle;
        }
        public readonly float f0;
        public readonly float f1;
        public readonly float f2;
        public readonly float f3;
        public readonly float b0;
        public readonly float b1;
        public readonly float b2;
        public readonly float b3;
        public readonly float start_angle;
    }
}
