using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom class to change default import setting for fbx files
/// </summary>
public class CustomFbxImporter : AssetPostprocessor
{
    private void OnPreprocessModel()
    {

        ModelImporter Importer = assetImporter as ModelImporter;
        Importer.importMaterials = false;
    }
}
