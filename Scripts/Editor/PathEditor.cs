using System;
using UnityEditor;
using UnityEngine;

namespace Bezier
{
    [CustomEditor(typeof(Path))]
    public class PathEditor : Editor
    {
        bool displayCurves = false;
        bool showCurveEditor = true;
        bool showSettings = true;
        bool showDots = false;
        bool showHandles = false;
        int division = 20;
        bool uniformSegments = false;
        float segmentLength = 0.5f;

        bool addCurveButton;
        bool removeCurveButton;
        bool copySegmentsButton;

        GUIStyle indented = new GUIStyle();
        GUIStyle black = new GUIStyle();

        [Serializable]
        private class Wrapper<Vector3>
        {
            public Vector3[] points;
        }

        void OnEnable ()
        {
            indented.padding = new RectOffset(15,0,0,0);
            black.normal.textColor = Color.black;
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();
            
            Path path = target as Path;
            if (path == null)
                return;
            
            showCurveEditor = EditorGUILayout.Foldout(showCurveEditor, "Curves Editor");
            if (showCurveEditor)
            {
                EditorGUILayout.BeginVertical(indented);

                for (int i = 0; i < path.curves.Count; i++)
                {
                    if (path.curves[i] == null)
                        continue;
                    
                    EditorGUILayout.LabelField("Curve " + i);

                    EditorGUILayout.BeginVertical(indented);

                    path.curves[i].type = (CurveType)EditorGUILayout.EnumPopup("Type", path.curves[i].type);

                    // Point 0
                    if (i == 0)
                        path.curves[i].p[0] = EditorGUILayout.Vector3Field("Start Position", path.curves[i].p[0]);
                    else
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        path.curves[i].p[0] = EditorGUILayout.Vector3Field("Start Position", path.curves[i].p[0]);
                        path.curves[i].p[0] = path.curves[i-1].endPosition;
                        EditorGUI.EndDisabledGroup();
                    }
                    // Point 1
                    if (path.curves[i].type == CurveType.Linear)
                        path.curves[i].p[1] = EditorGUILayout.Vector3Field("End Position", path.curves[i].p[1]);
                    else if (path.curves[i].type == CurveType.Quadratic)
                        path.curves[i].p[1] = EditorGUILayout.Vector3Field("Tangent", path.curves[i].p[1]);
                    else
                        path.curves[i].p[1] = EditorGUILayout.Vector3Field("Start Tangent", path.curves[i].p[1]);
                    // Point 2
                    if (path.curves[i].type == CurveType.Quadratic)
                        path.curves[i].p[2] = EditorGUILayout.Vector3Field("End Position", path.curves[i].p[2]);
                    // Point 3
                    if (path.curves[i].type == CurveType.Cubic)
                    {
                        path.curves[i].p[2] = EditorGUILayout.Vector3Field("End Tangent", path.curves[i].p[2]);
                        path.curves[i].p[3] = EditorGUILayout.Vector3Field("End Position", path.curves[i].p[3]);
                    }

                    EditorGUILayout.BeginHorizontal();
                    addCurveButton = GUILayout.Button("+", GUILayout.Width(30));
                    if (addCurveButton)
                    {
                        path.AddEmptyCurve(i+1);
                    }
                    removeCurveButton = GUILayout.Button("-", GUILayout.Width(30));
                    if (removeCurveButton)
                    {
                        path.RemoveCurve(i);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.BeginHorizontal();
                addCurveButton = GUILayout.Button("+", GUILayout.Width(30));
                if (addCurveButton)
                {
                    path.AddEmptyCurve();
                }
                removeCurveButton = GUILayout.Button("-", GUILayout.Width(30));
                if (removeCurveButton)
                {
                    path.RemoveCurve();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            // Settings group
            showSettings = EditorGUILayout.Foldout(showSettings, "Settings");
            if (showSettings)
            {
                EditorGUILayout.BeginVertical(indented);
                displayCurves = EditorGUILayout.Toggle("Display Curves", displayCurves);
                showHandles = EditorGUILayout.Toggle("Show Handles", showHandles);
                showDots = EditorGUILayout.Toggle("Show Dots", showDots);
                division = EditorGUILayout.IntField("Division", division);
                uniformSegments = EditorGUILayout.Toggle("Uniform Segments", uniformSegments);
                segmentLength =  EditorGUILayout.FloatField("Segment Length", segmentLength);
                copySegmentsButton = GUILayout.Button("Copy Segments to Json (Clipboard)");
                if (copySegmentsButton)
                {
                    Vector3[] segments;
                    if (uniformSegments)
                        segments = path.GetUniformSegments(segmentLength, division);
                    else
                        segments = path.GetSegments(division);
                    Wrapper<Vector3> wrapper = new Wrapper<Vector3>();
                    wrapper.points = segments;
                    GUIUtility.systemCopyBuffer = JsonUtility.ToJson(wrapper);
                }
                EditorGUILayout.EndVertical();
            }
        }

        public void OnSceneGUI ()
        {
            Path path = (target as Path);
            if (path == null)
                return;
            if (path.curves.Count == 0)
                return;
            
            if (!displayCurves)
                return;

            Vector3 offset = path.transform.position;
            Vector3[] segments;
            if (uniformSegments)
                segments = path.GetUniformSegments(segmentLength, division);
            else
                segments = path.GetSegments(division);  
            // Draw tangents
            Handles.color = Color.black;
            for (int i = 0; i < path.curves.Count; i++)
            {
                if (path.curves[i].type == CurveType.Quadratic)
                {
                    Handles.DrawLine(path.curves[i].p[0] + offset, 
                        path.curves[i].p[1] + offset);
                    Handles.DrawLine(path.curves[i].p[2] + offset,
                        path.curves[i].p[1] + offset);
                }
                else if (path.curves[i].type == CurveType.Cubic)
                {
                    Handles.DrawLine(path.curves[i].p[0] + offset, 
                        path.curves[i].p[1] + offset);
                    Handles.DrawLine(path.curves[i].p[3] + offset, 
                        path.curves[i].p[2] + offset);
                }
            }
            // Draw lines
            Handles.color = Color.white;
            for (int i = 0; i < segments.Length-1; i++)
            {
                Handles.DrawLine(segments[i], segments[i+1]);
            }
            // Draw dots
            if (showDots)
            {
                for (int i = 0; i < segments.Length-1; i++)
                {
                    Handles.DotHandleCap(0, segments[i], Quaternion.identity,
                        0.05f, EventType.Repaint);
                }
                Handles.DotHandleCap(0, segments[segments.Length-1], Quaternion.identity,
                    0.05f, EventType.Repaint);
            }
            // Draw points
            Vector3 p0, p1, p2, p3;
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < path.curves.Count; i++)
            {
                if (i == 0)
                {
                    Handles.Label(path.curves[i].p[0] + offset, "C"+i+"P0");

                    if (showHandles)
                    {
                        p0 = Handles.PositionHandle(path.curves[i].p[0] + offset, Quaternion.identity);
                        path.curves[i].p[0] = p0 - offset;
                    }
                }
                if (path.curves[i].type == CurveType.Linear)
                {
                    Handles.Label(path.curves[i].p[1] + offset, "C"+i+"P1");

                    if (showHandles)
                    {
                        p1 = Handles.PositionHandle(path.curves[i].p[1] + offset, Quaternion.identity);
                        path.curves[i].p[1] = p1 - offset;
                    }
                }
                else if (path.curves[i].type == CurveType.Quadratic)
                {
                    Handles.Label(path.curves[i].p[1] + offset, "C"+i+"P1", black);
                    Handles.Label(path.curves[i].p[2] + offset, "C"+i+"P2");

                    if (showHandles)
                    {
                        p1 = Handles.PositionHandle(path.curves[i].p[1] + offset, Quaternion.identity);
                        p2 = Handles.PositionHandle(path.curves[i].p[2] + offset, Quaternion.identity);
                        path.curves[i].p[1] = p1 - offset;
                        path.curves[i].p[2] = p2 - offset;
                    }
                }
                else if (path.curves[i].type == CurveType.Cubic)
                {
                    Handles.Label(path.curves[i].p[1] + offset, "C"+i+"P1", black);
                    Handles.Label(path.curves[i].p[2] + offset, "C"+i+"P2", black);
                    Handles.Label(path.curves[i].p[3] + offset, "C"+i+"P3");

                    if (showHandles)
                    {
                        p1 = Handles.PositionHandle(path.curves[i].p[1] + offset, Quaternion.identity);
                        p2 = Handles.PositionHandle(path.curves[i].p[2] + offset, Quaternion.identity);
                        p3 = Handles.PositionHandle(path.curves[i].p[3] + offset, Quaternion.identity);
                        path.curves[i].p[1] = p1 - offset;
                        path.curves[i].p[2] = p2 - offset;
                        path.curves[i].p[3] = p3 - offset;
                    }
                }
            }
        }
    }
}