using System.Linq;
using UnityEngine;

namespace Climate
{
    public class ShipItemThermometer : ShipItem
    {
        private Transform _needle;

        private float _minAngle;
        private float _maxAngle;
        private float _smoothingK;
        private float _sampleInterval;

        private float _temperature;
        private float _smoothedAngle;
        private float _sampleTimer;

        public override void OnLoad()
        {
            _needle = gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Needle").FirstOrDefault();
            _minAngle = -45f;
            _maxAngle = 225f;
            _smoothingK = -2f;
            _sampleInterval = 1f;

            SampleTemp();
        }

        public override void ExtraLateUpdate()
        {
            _sampleTimer += Time.deltaTime;
            if (_sampleTimer >= _sampleInterval)
            {
                _sampleTimer = 0f;
                SampleTemp();
            }

            UpdateNeedle();
        }

        private void SampleTemp()
        {
            var coords = FloatingOriginManager.instance.GetGlobeCoords(transform);
            _temperature = TemperatureService.GetNormalizedTemperature(coords, Sun.sun.localTime, GameState.day);
        }

        private void UpdateNeedle()
        {
            if (_needle == null)
                return;

            var targetAngle = Mathf.Lerp(_minAngle, _maxAngle, _temperature);
            _smoothedAngle = Mathf.Lerp(_smoothedAngle, targetAngle, 1f - Mathf.Exp(_smoothingK * Time.deltaTime));
            _needle.localRotation = Quaternion.Euler(_smoothedAngle, -90f, 90f);
        }
    }
}
