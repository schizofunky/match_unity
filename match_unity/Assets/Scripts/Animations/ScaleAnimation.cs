using UnityEngine;
using System.Collections;
using Assets.Scripts;

namespace Assets.Scripts.Animations {
    class ScaleAnimation : ITileAnimation{

        private int _time;
        private GameObject _tile;
        private bool _yoyo;
        private bool _completed = false;
        private float _targetScale;
        private int _iterations = 0;
        private float _startScale;
        private float _originalScale;
        private float _updateValue;

        public ScaleAnimation(GameObject tile, float targetScale, int time, bool yoyo) {
            _targetScale = targetScale;
            _yoyo = yoyo;
            _tile = tile;
            _time = time;
            _startScale = _tile.transform.localScale.x;
            _originalScale = _startScale;
            _updateValue = CalculateDistancePerTick();
        }

        public void UpdateAnimation() {
            if (!_completed) {
                _iterations++;
                float uniformScale = _tile.transform.localScale.x + _updateValue;
                _tile.transform.localScale = new Vector3(uniformScale, uniformScale, 1);                        
                if (_iterations >= _time) {
                    if (_yoyo) {
                        ReverseAnimation();
                    } else {
                        _completed = true;
                    }
                }
            }
        }

        public bool IsCompleted() {
            return _completed;
        }

        public void ReverseAnimation() {
            _completed = false;
            _iterations = 0;
            _targetScale = _startScale;
            _startScale = _tile.transform.localScale.x;
            _updateValue = CalculateDistancePerTick();
        }

        public void Reset() {
            _tile.transform.localScale = new Vector3(_originalScale, _originalScale, 1);
        }

        private float CalculateDistancePerTick() {
            return (_targetScale - _startScale) / _time;
        }
    }
}
