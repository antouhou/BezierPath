using System;
using UnityEngine;

namespace BezierPath
{
    [Serializable]
    public struct CubicBezierCurve
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
        public Vector3 startTangent;
        public Vector3 endTangent;
        public double estimatedLength;
        public bool isStraightLine;

        public CubicBezierCurve(Vector3 startPoint, Vector3 endPoint, Vector3 startTangent, Vector3 endTangent)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.startTangent = startTangent;
            this.endTangent = endTangent;

            estimatedLength = EstimateLength(startPoint, startTangent, endTangent, endPoint);
            isStraightLine = Math.Abs(Vector3.Distance(startPoint, endPoint) - estimatedLength) < 0.00001;
        }

        public double NormalizeMovement(double movementInUnits)
        {
            return movementInUnits / estimatedLength;
        }

        public Vector3 GetPoint(double t)
        {
            if (isStraightLine) return Lerp(startPoint, endPoint, t);

            // Layer 1
            var q = Lerp(startPoint, startTangent, t);
            var r = Lerp(startTangent, endTangent, t);
            var s = Lerp(endTangent, endPoint, t);

            // Layer 2
            var p = Lerp(q, r, t);
            var T = Lerp(r, s, t);

            // Final interpolated position
            var u = Lerp(p, T, t);

            return u;
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, double t)
        {
            return new Vector3((float) (a.x + (b.x - a.x) * t), (float) (a.y + (b.y - a.y) * t),
                (float) (a.z + (b.z - a.z) * t));
        }

        private static double EstimateLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var chord = Vector3.Distance(p3, p0);
            var contNet = Vector3.Distance(p0, p1) + Vector3.Distance(p2, p1) + Vector3.Distance(p3, p2);

            var appArcLength = (contNet + chord) / 2;
            return appArcLength;
        }
    }
}