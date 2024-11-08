using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
    private struct FractalPart
    {
        public Vector3 direction, worldPosition;
        public Quaternion rotation, worldRotation;
        public float spinAngle;
        //public Transform transform;
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
    Matrix4x4[][] matrices;

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

    ComputeBuffer[] matricesBuffers;

    static readonly int matricesId = Shader.PropertyToID("_Matrices");

    private void OnValidate()
    {
        if (parts != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void OnEnable()
    {
        parts = new FractalPart[depth][];
        matrices = new Matrix4x4[depth][];

        matricesBuffers = new ComputeBuffer[depth];
        int stride = 16 * 4;

        //int length = 1;
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new FractalPart[length];
            matrices[i] = new Matrix4x4[length];
            matricesBuffers[i] = new ComputeBuffer(length, stride);
            //length *= 5;
        }

        //float scale = 1f;
        parts[0][0] = CreatePart(0);

        // li : Level Index
        // ci : Child Index
        // fpi : Fractal Part Iterator

        for (int li = 1; li < parts.Length; li++)
        {
            //scale *= 0.5f;
            FractalPart[] levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                for (int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }

    }

    private void OnDisable()
    {
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].Release();
        }

        parts = null;
        matrices = null;
        matricesBuffers = null;
    }

    private void Update()
    {
        //Quaternion deltaRotation = Quaternion.Euler(0f, 22.5f * Time.deltaTime, 0f);
        float spinAngleDelta = 22.5f * Time.deltaTime;
        FractalPart rootPart = parts[0][0];

        //rootPart.rotation *= deltaRotation;
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = 
            rootPart.rotation * Quaternion.Euler(0f, rootPart.spinAngle, 0f);

        parts[0][0] = rootPart;
        matrices[0][0] = Matrix4x4.TRS
            (
                rootPart.worldPosition, rootPart.worldRotation, Vector3.one
            );

        float scale = 1f;
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;
            FractalPart[] parentParts = parts[li - 1];
            FractalPart[] levelParts = parts[li];
            Matrix4x4[] levelMatrices = matrices[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi++)
            {
                //Transform parentTransform = parentParts[fpi / 5].transform;
                FractalPart parent = parentParts[fpi / 5];
                FractalPart part = levelParts[fpi];
                //part.rotation *= deltaRotation;
                part.spinAngle += spinAngleDelta;
                part.worldRotation =
                    parent.worldRotation *
                    (part.rotation * Quaternion.Euler(0f, part.spinAngle, 0f));

                part.worldPosition =
                    parent.worldPosition +
                    parent.worldRotation *
                    (1.5f * scale * part.direction);

                levelParts[fpi] = part;
                levelMatrices[fpi] = Matrix4x4.TRS(
                    part.worldPosition, part.worldRotation, scale * Vector3.one
                    );
            }
        }

        var bounds = new Bounds(Vector3.zero, 3f * Vector3.one);
        for(int i = 0; i < matricesBuffers.Length; i++)
        {
            ComputeBuffer buffer = matricesBuffers[i];
            buffer.SetData(matrices[i]);
            material.SetBuffer(matricesId, buffer);
            Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, buffer.count);
        }
    }

    private FractalPart CreatePart(int childIndex) => new FractalPart()
    {
        direction = directions[childIndex],
        rotation = rotations[childIndex]
    };

    #region Lagacy
    /*
    var go = new GameObject("Fractal Part Lv : " + levelIndex + " Child : " + childIndex);
    go.transform.localScale = scale * Vector3.one;
    go.transform.SetParent(transform, false);
    go.AddComponent<MeshFilter>().mesh = mesh;
    go.AddComponent<MeshRenderer>().material = material;
    //Debug.Log($"Fractal Part Lv : {levelIndex} , Child : {childIndex}, Direction : {directions[levelIndex]}");
    */
    #endregion

}
