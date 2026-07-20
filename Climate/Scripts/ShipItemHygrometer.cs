using System.Linq;
using UnityEngine;

namespace Climate
{
    public class ShipItemHygrometer : ShipItem
    {
        private Transform _needle;

        private float _minAngle;
        private float _maxAngle;
        private float _smoothingK;
        private float _sampleInterval;

        private float _humidity;
        private float _smoothedAngle;
        private float _sampleTimer;

        public override void OnLoad()
        {
            _needle = gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Needle").FirstOrDefault();
            _minAngle = -45f;
            _maxAngle = 225f;
            _smoothingK = -2f;
            _sampleInterval = 1f;

            SampleHumidity();
        }

        public override void ExtraLateUpdate()
        {
            _sampleTimer += Time.deltaTime;
            if (_sampleTimer >= _sampleInterval)
            {
                _sampleTimer = 0f;
                SampleHumidity();
            }

            UpdateNeedle();
        }

        private void SampleHumidity()
        {
            var coords = FloatingOriginManager.instance.GetGlobeCoords(transform);
            _humidity = HumidityService.GetRelativeHumidity(coords, Sun.sun.localTime, GameState.day);
        }

        private void UpdateNeedle()
        {
            if (_needle == null)
                return;

            var targetAngle = Mathf.Lerp(_minAngle, _maxAngle, _humidity);
            _smoothedAngle = Mathf.Lerp(_smoothedAngle, targetAngle, 1f - Mathf.Exp(_smoothingK * Time.deltaTime));
            _needle.localRotation = Quaternion.Euler(_smoothedAngle, -90f, 90f);
        }
    }
}
