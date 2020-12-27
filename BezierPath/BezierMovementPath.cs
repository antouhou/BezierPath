using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BezierPath
{
    public class BezierMovementPath
    {
        public double Length;
        public BezierPathSegment[] PathSegments;
        public List<Vector3> Points = new List<Vector3>();
        public double SegmentSize;

        public BezierMovementPath(IReadOnlyList<Vector3> points, float smoothness, float segmentSize)
        {
            PathSegments = points.Select((center, index) =>
            {
                // For the first tile on the path enter point is the center of the tile
                var previousCenter = index == 0 ? center : points[index - 1];
                // For the last tile exit is the center of the tile
                var nextCenter = index == points.Count - 1 ? center : points[index + 1];

                var enterPoint = Vector3.Lerp(previousCenter, center, 0.5f);
                var exitPoint = Vector3.Lerp(center, nextCenter, 0.5f);

                var enterTangent = Vector3.Lerp(enterPoint, center, smoothness);
                var exitTangent = Vector3.Lerp(exitPoint, center, smoothness);

                var curve = new CubicBezierCurve(
                    enterPoint,
                    exitPoint,
                    enterTangent,
                    exitTangent
                );

                return new BezierPathSegment(curve, segmentSize);
            }).ToArray();

            for (var i = 0; i < PathSegments.Length; i++)
            {
                var segment = PathSegments[i];
                // For all the segments except the first, the last point of the previous segment
                // is approximately equal to starting point of the next
                Points.AddRange(i == 0 ? segment.Points : segment.Points.Skip(1));

                SegmentSize = SegmentSize == 0 ? segment.SegmentSize : (SegmentSize + segment.SegmentSize) / 2;
            }

            Length = Points.Count * SegmentSize;
        }

        public Vector3 Move(float distance)
        {
            if (distance > Length) return Points.Last();

            var previousClosestPointIndex = (int) (distance / SegmentSize);
            var nextClosestPointIndex = (int) (distance / SegmentSize) + 1;

            var previousClosestPoint = Points[previousClosestPointIndex];
            var nextClosestPoint = Points[nextClosestPointIndex];
            var overshot = distance - (distance - SegmentSize * previousClosestPointIndex);

            return Vector3.MoveTowards(previousClosestPoint, nextClosestPoint, (float) overshot);
        }
    }
}