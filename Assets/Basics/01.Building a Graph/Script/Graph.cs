using System;
using UnityEngine;

namespace BuildingAGraph
{
    public class Graph : MonoBehaviour
    {
        [SerializeField] private Transform _pointPrefab;
        [Range(10, 100)]
        [SerializeField] private int _resolution = 20;

        [SerializeField]
        private FunctionLibrary.EFunctionName _function;

        public enum ETransitionMode { Cycle, Random }
        [SerializeField]
        private ETransitionMode _transitionMode;

        [SerializeField, Min(0f)]
        private float _functionDuration = 1f, _transitionDuration = 1f;

        private Transform[] _points;

        private float _duration;
        private bool transitioning;
        private FunctionLibrary.EFunctionName _transitionFunction;

        private void Awake()
        {
            float step = 2f / _resolution;
            // var position = Vector3.zero;
            var scale = Vector3.one * step;
            _points = new Transform[_resolution * _resolution];

            for (int i = 0; i < _points.Length; i++)
            {
                /*  if (x == _resolution)
                  {
                      x = 0;
                      z += 1;
                      z += 1;
                */


                Transform point = Instantiate(_pointPrefab);
                _points[i] = point;
                /*
                position.x = (x + 0.5f) * step - 1f;
                position.z = (z + 0.5f) * step - 1f;
                */
                // position.y = position.x * position.x * position.x;

                //point.localPosition = position;

                point.localScale = scale;
                point.SetParent(transform, false);
            }
        }

        private void Update()
        {
            _duration += Time.deltaTime;
            if(transitioning)
            {
                if(_duration >= _transitionDuration)
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

            if (transitioning)
            {
                UpdateFunctionTransition();
            }
            else
            {

                UpdateFunction();
            }
        }

        private void PickNextFunction()
        {
            _function = _transitionMode == ETransitionMode.Cycle ?
                FunctionLibrary.GetNextFunctionName(_function) :
                FunctionLibrary.GetRandomFunctionNameOtherThan(_function);
        }

        private void UpdateFunction()
        {
            FunctionLibrary.Function f = FunctionLibrary.GetFunction(_function);
            float time = Time.time;
            float step = 2f / _resolution;
            float v = 0.5f * step - 1f;
            for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++)
            {
                if (x == _resolution)
                {
                    x = 0;
                    z += 1;
                    v = (z + 0.5f) * step - 1f;
                }

                float u = (x + 0.5f) * step - 1f;
                //float v = (z + 0.5f) * step - 1f;
                _points[i].localPosition = f(u, v, time);
            }
        }

        private void UpdateFunctionTransition()
        {
            FunctionLibrary.Function
                from = FunctionLibrary.GetFunction(_transitionFunction),
                to = FunctionLibrary.GetFunction(_function);

            float progress = _duration / _transitionDuration;
            float time = Time.time;
            float step = 2f / _resolution;
            float v = 0.5f * step - 1f;
            for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++)
            {
                if (x == _resolution)
                {
                    x = 0;
                    z += 1;
                    v = (z + 0.5f) * step - 1f;
                }

                float u = (x + 0.5f) * step - 1f;
                //float v = (z + 0.5f) * step - 1f;
                _points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
            }
        }
    }
}

