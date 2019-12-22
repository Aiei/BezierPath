using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier
{
    [Serializable]
    public class Curve
    {
        public CurveType type;
        public Vector3[] p;

        public Curve ()
        {
            type = CurveType.Cubic;
            p = new Vector3[4];
        }

        public Curve (CurveType type, Vector3[] p)
        {
            this.type = type;
            this.p = p;
        }

        public Vector3 startPosition
        {
            get
            {
                return p[0];
            }
            set
            {
                p[0] = value;
            }
        }

        public Vector3 endPosition
        {
            get
            {
                if (type == CurveType.Linear)
                    return p[1];
                else if (type == CurveType.Quadratic)
                    return p[2];
                else
                    return p[3];
            }
            set
            {
                if (type == CurveType.Linear)
                    p[1] = value;
                else if (type == CurveType.Quadratic)
                    p[2] = value;
                else
                    p[3] = value;
            }
        }

        public Vector3 GetSegment (float t)
        {
            t = Mathf.Clamp01(t);
            float _t = 1 - t;

            // All equations according to https://en.wikipedia.org/wiki/B%C3%A9zier_curve
            switch (this.type)
            {
                case CurveType.Linear:
                    return p[0]+t*(p[1]-p[0]);
                case CurveType.Quadratic:
                    return _t*(_t*p[0]+t*p[1])+t*(_t*p[1]+t*p[2]);
                default: // Cubic
                    return (_t*_t*_t*p[0])+(3*_t*_t*t*p[1])+(3*_t*t*t*p[2])+(t*t*t*p[3]);
            }
        }

        public Vector3[] GetSegments (int division)
        {
            Vector3[] segments = new Vector3[division];

            float t;
            for (int i = 0; i < division; i++)
            {
                t = (float)(i+1)/division;
                segments[i] = GetSegment(t);
            }

            return segments;
        }

        public Vector3[] GetUniformSegments (float length, int accuracy)
        {
            List<Vector3> segments = new List<Vector3>();

            // Start segments with start point
            segments.Add(p[0]);

            float t;
            Vector3 s;
            float distance;
            for (int i = 0; i < accuracy; i++)
            {
                t = (float)(i+1)/accuracy;
                s = GetSegment(t);
                distance = Vector3.Distance(s, segments[segments.Count-1]);
                if (distance >= length)
                    segments.Add(s);
            }
            // Close segments with end point
            if (segments[segments.Count-1] != endPosition)
                segments.Add(endPosition);

            return segments.ToArray();
        }
    }
}