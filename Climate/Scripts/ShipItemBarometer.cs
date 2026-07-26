using System.Linq;
using UnityEngine;

namespace Climate
{
    public class ShipItemBarometer : ShipItem
    {        
        private readonly float _minAngle = -118f;
        private readonly float _maxAngle = 240f;
        private readonly float _smoothingK = -6f;
        private readonly float _sampleInterval = 1f;

        private Transform _needle;
        private float _sampleTimer;
        private float _smoothedAngle;        
        private float _pressure;

        public override void OnLoad()
        {
            _needle = gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Needle").FirstOrDefault();

            SamplePressure();
        }

        public override void ExtraLateUpdate()
        {
            _sampleTimer += Time.deltaTime;
            if (_sampleTimer >= _sampleInterval)
            {
                _sampleTimer = 0f;
                SamplePressure();
            }

            UpdateNeedle();
        }

        private void SamplePressure()
        {
            var coords = FloatingOriginManager.instance.GetGlobeCoords(transform);
            _pressure = PressureService.GetNormalizedPressure(coords, GameState.day);
        }

        private void UpdateNeedle()
        {
            if (_needle == null)
                return;

            var targetAngle = Mathf.Lerp(_minAngle, _maxAngle, _pressure);
            _smoothedAngle = Mathf.Lerp(_smoothedAngle, targetAngle, 1f - Mathf.Exp(_smoothingK * Time.deltaTime));
            _needle.localRotation = Quaternion.Euler(_smoothedAngle, -90f, 90f);
        }
    }
}
