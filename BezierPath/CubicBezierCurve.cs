using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BezierPath
{
    [Serializable]
    public class CubicBezierCurve
    {
        // Legendre-Gauss abscissae with n=24 (x_i values, defined at i=n as the roots of the nth order Legendre polynomial Pn(x))
        private static double[] tvalues =
        {
            -0.0640568928626056260850430826247450385909,
            0.0640568928626056260850430826247450385909,
            -0.1911188674736163091586398207570696318404,
            0.1911188674736163091586398207570696318404,
            -0.3150426796961633743867932913198102407864,
            0.3150426796961633743867932913198102407864,
            -0.4337935076260451384870842319133497124524,
            0.4337935076260451384870842319133497124524,
            -0.5454214713888395356583756172183723700107,
            0.5454214713888395356583756172183723700107,
            -0.6480936519369755692524957869107476266696,
            0.6480936519369755692524957869107476266696,
            -0.7401241915785543642438281030999784255232,
            0.7401241915785543642438281030999784255232,
            -0.8200019859739029219539498726697452080761,
            0.8200019859739029219539498726697452080761,
            -0.8864155270044010342131543419821967550873,
            0.8864155270044010342131543419821967550873,
            -0.9382745520027327585236490017087214496548,
            0.9382745520027327585236490017087214496548,
            -0.9747285559713094981983919930081690617411,
            0.9747285559713094981983919930081690617411,
            -0.9951872199970213601799974097007368118745,
            0.9951872199970213601799974097007368118745
        };

        // Legendre-Gauss weights with n=24 (w_i values, defined by a function linked to in the Bezier primer article)
        private static double[] cvalues =
        {
            0.1279381953467521569740561652246953718517,
            0.1279381953467521569740561652246953718517,
            0.1258374563468282961213753825111836887264,
            0.1258374563468282961213753825111836887264,
            0.121670472927803391204463153476262425607,
            0.121670472927803391204463153476262425607,
            0.1155056680537256013533444839067835598622,
            0.1155056680537256013533444839067835598622,
            0.1074442701159656347825773424466062227946,
            0.1074442701159656347825773424466062227946,
            0.0976186521041138882698806644642471544279,
            0.0976186521041138882698806644642471544279,
            0.086190161531953275917185202983742667185,
            0.086190161531953275917185202983742667185,
            0.0733464814110803057340336152531165181193,
            0.0733464814110803057340336152531165181193,
            0.0592985849154367807463677585001085845412,
            0.0592985849154367807463677585001085845412,
            0.0442774388174198061686027482113382288593,
            0.0442774388174198061686027482113382288593,
            0.0285313886289336631813078159518782864491,
            0.0285313886289336631813078159518782864491,
            0.0123412297999871995468056670700372915759,
            0.0123412297999871995468056670700372915759
        };

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

            estimatedLength = EstimateLengthGauss();
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

        public override string ToString()
        {
            return $"{startPoint}, {startTangent}, {endTangent}, {endPoint}";
        }

        private double EstimateLengthGauss()
        {
            const double z = 0.5;
            double sum = 0;
            var len = tvalues.Length;
            int i;
            double t;
            for (i = 0; i < len; i++)
            {
                t = z * tvalues[i] + z;
                sum += cvalues[i] * Arc(t);
            }

            return z * sum;
        }

        private double Arc(double t)
        {
            var d = Derivative((float) t);
            var l = d.x * d.x + d.y * d.y + d.z * d.z;
            return Math.Sqrt(l);
        }

        private Vector3 Derivative(double t)
        {
            var mt = 1 - t;
            var dpoints = Derive(new[] {startPoint, startTangent, endTangent, endPoint})[0];

            var a = mt * mt;
            var b = mt * t * 2;
            var c = t * t;

            var x = a * dpoints[0].x + b * dpoints[1].x + c * dpoints[2].x;
            var y = a * dpoints[0].y + b * dpoints[1].y + c * dpoints[2].y;
            var z = a * dpoints[0].z + b * dpoints[1].z + c * dpoints[2].z;

            var ret = new Vector3((float) x, (float) y, (float) z);
            return ret;
        }

        private List<List<Vector3>> Derive(IEnumerable<Vector3> points)
        {
            var dpoints = new List<List<Vector3>>();
            var pointsList = points.ToList();
            var pointsCount = pointsList.Count;

            for (var pointIndex = pointsCount - 1; pointsCount > 1; pointsCount--, pointIndex--)
            {
                var list = new List<Vector3>();
                for (var j = 0; j < pointIndex; j++)
                    list.Add(new Vector3(
                        pointIndex * (pointsList[j + 1].x - pointsList[j].x),
                        pointIndex * (pointsList[j + 1].y - pointsList[j].y),
                        pointIndex * (pointsList[j + 1].z - pointsList[j].z)
                    ));

                dpoints.Add(list);
                pointsList = list;
            }

            return dpoints;
        }
    }
}