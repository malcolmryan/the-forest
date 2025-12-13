// SceneCameraDatabase
// 2018-10-11 halley - provided on Unity Forums
// https://discussions.unity.com/t/my-script-to-save-each-scenes-scene-view-camera-independently/717803

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//
// This is a very dumb class, just simulates a Dictionary which we can lookup
// or save.  Since ScriptableObject/JsonUtility cannot serialize a generic
// Dictionary, we have to emulate!
//
// The keys are unique strings (such as GUIDs), the values are a special class
// which keeps a number of editor-only scene camera parameters.
//

public class SceneCameraDatabase : ScriptableObject
{
    [Serializable]
    public class SceneCamera
    {
        public Vector3 pivot;
        public Quaternion rotation;
        public bool orthographic;
        public bool isRotationLocked;
        public bool in2DMode;
        public float size;
    }

    // Ugly list-of-pairs for now.
    public SerializableDictionary<string,SceneCamera> cameras = new SerializableDictionary<string,SceneCamera>();

    public int Count { get { return cameras.Count; } }

    public bool Contains(string guid)
    {
        return cameras.ContainsKey(guid);
    }

    public SceneCamera GetSceneCamera(string guid)
    {
        if (cameras.ContainsKey(guid))
        {
            return cameras[guid];            
        }
        else
        {
            return null;
        }
    }

    public void PutSceneCamera(string guid, SceneCamera camera)
    {
        if (camera == null)
            return;

        cameras[guid] = camera;

        // We want to ensure this is saved, but
        // we do not want to trigger an Undo step.
        EditorUtility.SetDirty(this);
    }
}