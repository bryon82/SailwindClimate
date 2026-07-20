using System.Linq;
using UnityEngine;

namespace Climate
{
    public class ShipItemBarometer : ShipItem
    {
        private Transform _needle;

        private float _minAngle;
        private float _maxAngle;
        private float _smoothingK;

        private float _pressure;
        private float _smoothedAngle;

        public override void OnLoad()
        {
            _needle = gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Needle").FirstOrDefault();
            _minAngle = -118f;
            _maxAngle = 240f;
            _smoothingK = -6f;
        }

        public override void ExtraLateUpdate()
        {
            if (_needle == null)
                return;

            _pressure = PressureService.GetNormalizedPressure();
            var targetAngle = Mathf.Lerp(_minAngle, _maxAngle, _pressure);
            _smoothedAngle = Mathf.Lerp(_smoothedAngle, targetAngle, 1f - Mathf.Exp(_smoothingK * Time.deltaTime));
            _needle.localRotation = Quaternion.Euler(_smoothedAngle, -90f, 90f);
        }
    }
}
