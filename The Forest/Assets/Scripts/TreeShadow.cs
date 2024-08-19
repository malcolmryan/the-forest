/**
 * Construct shadow casters for trees
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using Unity.VisualScripting;

[RequireComponent(typeof(ShadowCaster2D))]
public class TreeShadow : MonoBehaviour
{

#region Parameters
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private int nShadowPoints = 8;

    [SerializeField] private int normalMapSize = 512;
    [SerializeField] private float normalNoise = 0.05f;
#endregion 

#region Components
    private ShadowCaster2D shadowCaster;
#endregion

#region Reflection
    // these fields aren't publically exposed, so we need reflection to access them
    private static BindingFlags accessFlagsPrivate = BindingFlags.NonPublic | BindingFlags.Instance;
    private static FieldInfo meshField = 
        typeof(ShadowCaster2D).GetField("m_Mesh", accessFlagsPrivate);
    private static FieldInfo shapePathField =
        typeof(ShadowCaster2D).GetField("m_ShapePath", accessFlagsPrivate);
    private static MethodInfo onEnableMethod =
        typeof(ShadowCaster2D).GetMethod("OnEnable", accessFlagsPrivate);
#endregion

#region Init & Destroy
    void Awake()
    {
        shadowCaster = GetComponent<ShadowCaster2D>();
        MakeShadowCasterShape();
    }

    private void MakeShadowCasterShape()
    {
        Vector3[] path = new Vector3[nShadowPoints];

        for (int i = 0; i < nShadowPoints; i++)
        {
            Quaternion q = Quaternion.Euler(0, 0, i * 360f / nShadowPoints);
            path[i] = q * (radius * Vector3.right);
        }
        shapePathField.SetValue(shadowCaster, path);
        meshField.SetValue(shadowCaster, null);
        onEnableMethod.Invoke(shadowCaster, new object[0]);
    }
#endregion Init

#region Editor
    public void BuildNormalMap()
    {
        Texture2D texture = new Texture2D(normalMapSize, normalMapSize, TextureFormat.RGB24, false);
        Color[] colours = new Color[normalMapSize * normalMapSize];

        int k = 0;
        for (int j = 0; j < normalMapSize; j++)
        {
            for (int i = 0; i < normalMapSize; i++)
            {
                float x = 2f * i / normalMapSize - 1f;
                float y = 2f * j / normalMapSize - 1f;

                Vector3 n = new Vector3(x, y, 0);
                float s = n.sqrMagnitude;
                if (s > 1)
                {
                    n = Vector3.zero;
                }
                else 
                {
                    // the positive square root points upwards
                    n.z = 0;
//                    n.z = Mathf.Sqrt(1-s);
                    
                    n.x += normalNoise * (2 * Random.value - 1);
                    n.y += normalNoise * (2 * Random.value - 1);
                    n.z += normalNoise * (2 * Random.value - 1);
                    n.Normalize();
                }

                n = (n + Vector3.one) / 2;                
                colours[k++] = new Color(n.x, n.y, n.z, 1);
            }
        }

        texture.SetPixels(colours, 0);
        texture.Apply();
        SaveTexture(texture);
    }

    private Texture2D SaveTexture(Texture2D texture) 
    {
        var path = EditorUtility.SaveFilePanel(
            "Save texture as PNG", "", "TreeNormalMap", "png");

        if (path.Length != 0)
        {
            var pngData = texture.EncodeToPNG();
            if (pngData != null)
            {
                File.WriteAllBytes(path, pngData);
            }
        }

        UnityEditor.AssetDatabase.Refresh();

        return texture;
    }
#endregion

#region Update
    void Update()
    {
    }
#endregion Update

#region FixedUpdate
    void FixedUpdate()
    {        
    }
#endregion FixedUpdate

#region Gizmos
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // Don't run in the editor
            return;
        }
    }
#endregion Gizmos
}



[CustomEditor(typeof(TreeShadow))]
public class TreeShadowEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TreeShadow tree = (TreeShadow)target;
        if(GUILayout.Button("Build Normal Map"))
        {
            tree.BuildNormalMap();
        }
    }
}