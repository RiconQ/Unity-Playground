using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class HashVisualization : MonoBehaviour
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct HashJob : IJobFor
    {
        [WriteOnly]
        public NativeArray<uint> hashes;

        public int resolution;
        public float invResolution;

        public int seed;

        public SmallXXHash hash;

        public void Execute(int i)
        {
            int v = (int)floor(invResolution * i + 0.00001f);
            int u = i - resolution * v - resolution / 2;
            v -= resolution / 2;

            hashes[i] = hash.Eat(u).Eat(v);
        }
    }

    static int
        hashesId = Shader.PropertyToID("_Hashes"),
        configId = Shader.PropertyToID("_Config");

    [SerializeField] private Mesh instanceMesh;
    [SerializeField] private Material material;
    [SerializeField, Range(1, 512)] private int resolution = 16;
    [SerializeField] private int seed = 0;

    [SerializeField, Range(-2f, 2f)] private float verticalOffest = 1f;

    private NativeArray<uint> hashes;
    private ComputeBuffer hashesBuffer;
    private MaterialPropertyBlock propertyBlock;

    private void OnEnable()
    {
        int length = resolution * resolution;
        hashes = new NativeArray<uint>(length, Allocator.Persistent);
        hashesBuffer = new ComputeBuffer(length, 4);

        new HashJob
        {
            hashes = hashes,
            resolution = resolution,
            invResolution = 1f / resolution,
            seed = seed,
            hash = SmallXXHash.Seed(seed)
        }.ScheduleParallel(hashes.Length, resolution, default).Complete();

        hashesBuffer.SetData(hashes);

        propertyBlock ??= new MaterialPropertyBlock();
        propertyBlock.SetBuffer(hashesId, hashesBuffer);
        propertyBlock.SetVector(configId, new Vector4(resolution, 1f / resolution, verticalOffest / resolution));
    }

    private void OnDisable()
    {
        hashes.Dispose();
        hashesBuffer.Release();
        hashesBuffer = null;
    }

    private void OnValidate()
    {
        if (hashesBuffer != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedProcedural
            (
            instanceMesh, 0, material, new Bounds(Vector3.zero, Vector3.one),
            hashes.Length, propertyBlock
            );
    }
}
