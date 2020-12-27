using System.Collections.Generic;
using UnityEngine;

namespace BezierPath
{
    public class BezierPathSegment
    {
        public CubicBezierCurve Curve;
        public List<Vector3> Points;
        public double SegmentSize;

        public BezierPathSegment(CubicBezierCurve curve, float segmentSize)
        {
            Curve = curve;
            Points = BuildPoints(curve, segmentSize);
        }

        private List<Vector3> BuildPoints(CubicBezierCurve curve, float segmentSize)
        {
            var list = new List<Vector3>();

            var segmentsCount = (int) (curve.estimatedLength / segmentSize);
            SegmentSize = curve.estimatedLength / segmentsCount;
            SegmentSize = segmentSize;

            for (var i = 0; i < segmentsCount; i++) list.Add(curve.GetPoint(curve.NormalizeMovement(SegmentSize * i)));

            return list;
        }
    }
}