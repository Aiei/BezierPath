using System.Collections.Generic;
using UnityEngine;

namespace Bezier
{
    public class Path : MonoBehaviour
    {
        public List<Curve> curves = new List<Curve>();

        public void AddCurve (int index = -1)
        {
            if (index == -1)
                curves.Add(new Curve());
            else
                curves.Insert(index, new Curve());
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

            return segments.ToArray();
        }

        public Vector3[] GetUniformSegments (float length, int accuracy)
        {
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
    }
}