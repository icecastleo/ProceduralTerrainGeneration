using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainGenerator tg = (TerrainGenerator)target;

        if (DrawDefaultInspector())
        {
            if (tg.autoUpdate)
            {
                tg.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            tg.GenerateMap();
        }
    }
}