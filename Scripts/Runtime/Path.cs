using System.Collections.Generic;
using UnityEngine;

namespace Bezier
{
    public class Path : MonoBehaviour
    {
        public List<Curve> curves = new List<Curve>();

        public Vector3 startPosition
        {
            get
            {
                return OffsetVector(curves[0].startPosition, transform.position,
                    transform.rotation);
            }
        }

        public Vector3 endPosition
        {
            get
            {
                return OffsetVector(curves[curves.Count-1].endPosition,
                    transform.position, transform.rotation);
            }
        }

        public void AddEmptyCurve (int index = -1)
        {
            if (index == -1)
                curves.Add(new Curve());
            else
                curves.Insert(index, new Curve());
        }

        public void AddCurve (CurveType type, Vector3[] points, int index = -1)
        {
            if (index == -1)
                curves.Add(new Curve(type, points));
            else
                curves.Insert(index, new Curve(type, points));
        }

        public void RemoveCurve (int index = -1)
        {
            if (index == -1)
                curves.RemoveAt(curves.Count-1);
            else
                curves.RemoveAt(index);
        }

        public Vector3[] GetSegments (int division)
        {
            if (curves.Count == 0)
                return null;

            List<Vector3> segments = new List<Vector3>();

            segments.Add(curves[0].startPosition);
            for (int i = 0; i < curves.Count; i++)
            {
                segments.AddRange(curves[i].GetSegments(division));
            }

            Vector3[] v = segments.ToArray();
            v = OffsetVectors(v, transform.position, transform.rotation);

            return v;
        }

        public Vector3[] GetUniformSegments (float length, int accuracy)
        {
            if (curves.Count == 0)
                return null;

            Vector3[] segments = GetSegments(accuracy);
            List<Vector3> uSegments = new List<Vector3>();
            // Include first point
            uSegments.Add(segments[0]);

            float distance;
            for (int i = 0; i < segments.Length; i++)
            {
                distance = Vector3.Distance(segments[i], uSegments[uSegments.Count-1]);
                if (distance > length)
                    uSegments.Add(segments[i]);
            }
            // Include last point
            if (uSegments[uSegments.Count-1] != segments[segments.Length-1])
                uSegments.Add(segments[segments.Length-1]);
            return uSegments.ToArray();
        }

        Vector3[] OffsetVectors (Vector3[] vectors, Vector3 offset, Quaternion rotation)
        {
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i] = rotation * vectors[i];
                vectors[i] += offset; 
            }
            return vectors;
        }

        Vector3 OffsetVector (Vector3 vector, Vector3 offset, Quaternion rotation)
        {
            vector = rotation * vector;
            vector += offset;
            return vector;
        }
    }
}