// SceneCameraSaver
// 2018-10-11 halley - provided on Unity Forums 
// https://discussions.unity.com/t/my-script-to-save-each-scenes-scene-view-camera-independently/717803
// 2025-11-16 update by Malcolm Ryan

using System;
using System.IO;
using UnityEngine;
using UnityEditor;

//
// This is a static class intended to extend the Unity Editor.
//
// It is loaded before any scenes are loaded, and it tries to load
// a ScriptableObject database of saved scene camera position records.
//
// Whenever a scene is closed or saved (even if not changed), the current
// editor scene view camera's position, orientation, zoom and other
// parameters are saved into the database.
//
// Whenever a scene is loaded (non-additively), if that scene has been
// recorded in this database, the editor scene view camera is restored
// to its prior position.
//

[InitializeOnLoad]
public static class SceneCameraSaver
{
    static SceneCameraDatabase _database = null;
  
    // This path needs to start with "Assets/" so each project gets its
    // own asset to store camera settings. 

    static string path = "Assets/Editor/SceneCameraDatabase.asset";
  
    // Initialize the callbacks and create/load the database.

    static SceneCameraSaver()
    {
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += _SceneOpenedCallback;
        UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += _SceneClosedCallback;
        UnityEditor.SceneManagement.EditorSceneManager.sceneClosed += _SceneClosedCallback;
    }

    static void _SceneOpenedCallback(
        UnityEngine.SceneManagement.Scene scene,
        UnityEditor.SceneManagement.OpenSceneMode mode)
    {
        _database = LoadSceneCameraDatabase();

        // Don't do anything if scene loaded in any additive mode.
        if (mode != UnityEditor.SceneManagement.OpenSceneMode.Single)
            return;

        // Restore the camera if we can.
        string guid = AssetDatabase.AssetPathToGUID(scene.path);
        SceneCameraDatabase.SceneCamera camera = _database.GetSceneCamera(guid);
        if (camera != null)
        {
            // Set the current camera to the data we just loaded.
            // Setting the in2DMode must come first.
            SceneView.lastActiveSceneView.in2DMode = camera.in2DMode;
            SceneView.lastActiveSceneView.pivot = camera.pivot;
            SceneView.lastActiveSceneView.rotation = camera.rotation;
            SceneView.lastActiveSceneView.orthographic = camera.orthographic;
            SceneView.lastActiveSceneView.size = camera.size;
            SceneView.lastActiveSceneView.isRotationLocked = camera.isRotationLocked;
        }
    }

    static void _SceneClosedCallback(
        UnityEngine.SceneManagement.Scene scene)
    {
        _database = LoadSceneCameraDatabase();

        // Save the current camera settings to the database.
        string guid = AssetDatabase.AssetPathToGUID(scene.path);
        SceneCameraDatabase.SceneCamera camera = new SceneCameraDatabase.SceneCamera();
        camera.pivot = SceneView.lastActiveSceneView.pivot;
        camera.in2DMode = SceneView.lastActiveSceneView.in2DMode;
        if (!camera.in2DMode)
        {
            camera.rotation = SceneView.lastActiveSceneView.rotation;        
        }
        camera.isRotationLocked = SceneView.lastActiveSceneView.isRotationLocked;
        camera.orthographic = SceneView.lastActiveSceneView.orthographic;
        camera.size = SceneView.lastActiveSceneView.size;
        _database.PutSceneCamera(guid, camera);

        // Commit the database.
        SaveSceneCameraDatabase(_database);
    }

    static SceneCameraDatabase LoadSceneCameraDatabase()
    {
        // Create or load the database.
        SceneCameraDatabase asset = AssetDatabase.LoadAssetAtPath<SceneCameraDatabase>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<SceneCameraDatabase>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }
        return asset;
    }

    static void SaveSceneCameraDatabase(SceneCameraDatabase asset)
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


}