using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor {

	public override void OnInspectorGUI() {

        FieldOfView fov = (FieldOfView)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("viewRadius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("viewAngle"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("hasPeripheralVision"));
        if (fov.hasPeripheralVision) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("viewRadiusPeripheralVision"));
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("edgeResolveIterations"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("edgeDstThreshold"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("delayBetweenFOVUpdates"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("targetMask"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("obstacleMask"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("visualizeFieldOfView"));
        if (fov.visualizeFieldOfView) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("meshResolution"));
            if (fov.hasPeripheralVision) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("meshResolutionPeripheralVision"));
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("viewMeshFilter"));
        }


        serializedObject.ApplyModifiedProperties();
    }
}
