using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
    private struct FractalPart
    {
        public Vector3 direction;
        public Quaternion rotation;
        public Transform transform;
    }

    [SerializeField, Range(1, 8)]
    private int depth = 4; // Control the maximum depth of fractal

    #region Lagacy
    /*
    private void Start()
    {
        name = "Fractal : " + depth;    // Rename Fractal Object
        if(depth <= 1)  // If Depth <= 1 stop Instantiate Fractal
        {
            return;
        }

        #region Create Child
        Fractal child_U = CreateChild(Vector3.up, Quaternion.identity);
        Fractal child_R = CreateChild(Vector3.right, Quaternion.Euler(0f, 0f, -90f));
        Fractal child_L = CreateChild(Vector3.left, Quaternion.Euler(0f, 0f, 90f));
        Fractal child_F = CreateChild(Vector3.forward, Quaternion.Euler(90f, 0f, 0f));
        Fractal child_B = CreateChild(Vector3.back, Quaternion.Euler(-90f, 0f, 0f));

        child_U.transform.SetParent(transform, false);
        child_R.transform.SetParent(transform, false);
        child_L.transform.SetParent(transform, false);
        child_F.transform.SetParent(transform, false);
        child_B.transform.SetParent(transform, false);
        #endregion
    }

    private void Update()
    {
        transform.Rotate(0f, 22.5f * Time.deltaTime, 0f);
    }

    /// <summary>
    /// Create Sphere Child on Direction and Rotation
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    private Fractal CreateChild(Vector3 direction, Quaternion rotation)
    {
        Fractal child = Instantiate(this);
        child.depth -= 1;
        child.transform.localPosition = 0.75f * direction;
        child.transform.localRotation = rotation;
        child.transform.localScale = 0.5f * Vector3.one;

        return child;
    }
    */
    #endregion

    [SerializeField] Mesh mesh;
    [SerializeField] Material material;

    private FractalPart[][] parts;

    static Vector3[] directions =
    {
        Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
    };

    static Quaternion[] rotations =
    {
        Quaternion.identity,
        Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f),
    };

    private void Awake()
    {
        parts = new FractalPart[depth][];
        //int length = 1;
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new FractalPart[length];
            //length *= 5;
        }

        float scale = 1f;
        CreatePart(0, 0, scale);

        for (int li = 0; li < parts.Length; li++)
        {
            scale *= 0.5f;
            FractalPart[] levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi *= 5)
            {
                for (int ci = 0; ci < 5; ci++)
                {
                    CreatePart(li, ci, scale);
                }
            }
        }
    }

    private void CreatePart(int levelIndex, int childIndex, float scale)
    {
        var go = new GameObject("Fractal Part Lv : " + levelIndex + " Child : " + childIndex);
        go.transform.localScale = scale * Vector3.one;
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().material = material;
    }
}
