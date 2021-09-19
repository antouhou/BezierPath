using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BezierPath
{
    internal struct CurveEntry
    {
        public double StartDistance;
        public double EndDistance;
        public CubicBezierCurve Curve;
    }
    public class BezierMovementPath
    {
        private double _estimatedLength = 0;
        private readonly List<CurveEntry> _curves = new List<CurveEntry>();
        private readonly List<Vector3> _controlPointsList = new List<Vector3>();
        private float _smoothness = 0;

        public BezierMovementPath(List<Vector3> controlPoints, float smoothness)
        {
            Rebuild(controlPoints, smoothness);
        }

        public void Clear()
        {
            _controlPointsList.Clear();
            _curves.Clear();
            _estimatedLength = 0.0;
            _smoothness = 0;
        }
        
        public BezierMovementPath() {}

        public void Rebuild(List<Vector3> controlPoints, float smoothness)
        {
            Clear();

            _smoothness = smoothness;
            _controlPointsList.AddRange(controlPoints);
            for (var i = 0; i < controlPoints.Count; i++)
            {
                _curves.Add(CreateCurveEntry(controlPoints[i], i));
            }
        }

        private CurveEntry CreateCurveEntry(Vector3 center, int index)
        {
            var previousCenter = index == 0 ? center : _controlPointsList[index - 1];
            // For the last tile exit is the center of the tile
            var nextCenter = index == _controlPointsList.Count - 1 ? center : _controlPointsList[index + 1];

            var enterPoint = Vector3.Lerp(previousCenter, center, 0.5f);
            var exitPoint = Vector3.Lerp(center, nextCenter, 0.5f);

            var enterTangent = Vector3.Lerp(enterPoint, center, _smoothness);
            var exitTangent = Vector3.Lerp(exitPoint, center, _smoothness);

            var curve = new CubicBezierCurve(
                enterPoint,
                exitPoint,
                enterTangent,
                exitTangent
            );

            var newEstimatedLength = _estimatedLength + curve.estimatedLength;
            var curveEntry = new CurveEntry
            {
                Curve = curve,
                StartDistance = _estimatedLength,
                EndDistance = newEstimatedLength,
            };

            _estimatedLength = newEstimatedLength;
            return curveEntry;
        }

        private CurveEntry GetCurveAtDistance(float distance)
        {
            return _curves.FirstOrDefault(curveIndex => distance >= curveIndex.StartDistance && distance <= curveIndex.EndDistance);
        }

        public Vector3 GetPointAt(float distance)
        {
            if (distance > _estimatedLength) return GetLastPoint();

            var curve = GetCurveAtDistance(distance);
            return curve.Curve.GetPoint(curve.Curve.NormalizeMovement(distance - curve.StartDistance));
        }

        public Vector3 GetLastPoint()
        {
            return _curves.Last().Curve.endPoint;
        }

        public double GetEstimatedLength()
        {
            return _estimatedLength;
        }
    }
}