using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basics
{
    public class GPUGraph : MonoBehaviour
    {
        [SerializeField] private ComputeShader computeShader;

        [SerializeField] private Material material;
        [SerializeField] private Mesh mesh;

        const int maxResolution = 200;

        [Range(10, maxResolution)]
        [SerializeField] private int resolution = 20;

        [SerializeField]
        private FunctionLibrary.EFunctionName function;

        public enum ETransitionMode { Cycle, Random }
        [SerializeField]
        private ETransitionMode _transitionMode;

        [SerializeField, Min(0f)]
        private float _functionDuration = 1f, _transitionDuration = 1f;

        private float _duration;
        private bool transitioning;
        private FunctionLibrary.EFunctionName _transitionFunction;

        private ComputeBuffer positionsBuffer;


        static readonly int
            positionsId = Shader.PropertyToID("_Positions"),
            resolutionId = Shader.PropertyToID("_Resolution"),
            stepId = Shader.PropertyToID("_Step"),
            timeId = Shader.PropertyToID("_Time"),
            transitionProgressId = Shader.PropertyToID("_TransitionProgress");

        private void UpdateFunctionOnGPU()
        {
            float step = 2f / resolution;
            computeShader.SetInt(resolutionId, resolution);
            computeShader.SetFloat(stepId, step);
            computeShader.SetFloat(timeId, Time.time);

            if (transitioning)
            {
                computeShader.SetFloat(
                    transitionProgressId,
                    Mathf.SmoothStep(0f, 1f, _duration / _transitionDuration)
                );
            }
            var kernelIndex =
                (int)function + (int)(transitioning ? _transitionFunction : function) * FunctionLibrary.FunctionCount;
            //var kernelIndex = (int)function;
            computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);

            int groups = Mathf.CeilToInt(resolution / 8f);
            Debug.Log(kernelIndex);
            computeShader.Dispatch(kernelIndex, groups, groups, 1);

            material.SetBuffer(positionsId, positionsBuffer);
            material.SetFloat(stepId, step);

            var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
            Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, maxResolution * maxResolution);
        }

        private void OnEnable()
        {
            positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
        }

        private void OnDisable()
        {
            positionsBuffer.Release();
            positionsBuffer = null;
        }

        private void Update()
        {
            _duration += Time.deltaTime;
            if (transitioning)
            {
                if (_duration >= _transitionDuration)
                {
                    _duration -= _transitionDuration;
                    transitioning = false;
                }
            }
            else if (_duration >= _functionDuration)
            {
                _duration -= _functionDuration;
                transitioning = true;
                _transitionFunction = function;
                //_function = FunctionLibrary.GetNextFunctionName(_function);
                PickNextFunction();
            }

            UpdateFunctionOnGPU();
        }

        private void PickNextFunction()
        {
            function = _transitionMode == ETransitionMode.Cycle ?
                FunctionLibrary.GetNextFunctionName(function) :
                FunctionLibrary.GetRandomFunctionNameOtherThan(function);
        }
    }
}

