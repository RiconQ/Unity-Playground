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

        [Range(10, 200)]
        [SerializeField] private int _resolution = 20;

        [SerializeField]
        private FunctionLibrary.EFunctionName _function;

        public enum ETransitionMode { Cycle, Random }
        [SerializeField]
        private ETransitionMode _transitionMode;

        [SerializeField, Min(0f)]
        private float _functionDuration = 1f, _transitionDuration = 1f;

        private float _duration;
        private bool transitioning;
        private FunctionLibrary.EFunctionName _transitionFunction;

        private ComputeBuffer _positionsBuffer;


        static readonly int
            positionsId = Shader.PropertyToID("_Positions"),
            resolutionId = Shader.PropertyToID("_Resolution"),
            stepId = Shader.PropertyToID("_Step"),
            timeId = Shader.PropertyToID("_Time");

        private void UpdateFunctionOnGPU()
        {
            float step = 2f / _resolution;
            computeShader.SetInt(resolutionId, _resolution);
            computeShader.SetFloat(stepId, step);
            computeShader.SetFloat(timeId, Time.time);
            computeShader.SetBuffer(0, positionsId, _positionsBuffer);

            int groups = Mathf.CeilToInt(_resolution / 8f);
            computeShader.Dispatch(0, groups, groups, 1);

            var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / _resolution));
            Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, _positionsBuffer.count);
        }

        private void Enable()
        {
            _positionsBuffer = new ComputeBuffer(_resolution * _resolution, 3 * 4);
        }

        private void OnDisable()
        {
            _positionsBuffer.Release();
            _positionsBuffer = null;
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
                _transitionFunction = _function;
                //_function = FunctionLibrary.GetNextFunctionName(_function);
                PickNextFunction();
            }

            UpdateFunctionOnGPU();
        }

        private void PickNextFunction()
        {
            _function = _transitionMode == ETransitionMode.Cycle ?
                FunctionLibrary.GetNextFunctionName(_function) :
                FunctionLibrary.GetRandomFunctionNameOtherThan(_function);
        }
    }
}

