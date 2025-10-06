using System;
using RacingGame.Car;
using UnityEngine;

namespace RacingGame.GameManager
{
    public class SpeedometerManager : MonoBehaviour
    {
        private GameObject _needle;
        [SerializeField] private CarController _carController;
        private float startPosition = 210f, endPosition = -34f;
        private float vehicleSpeed;
        
        private void Awake()
        {
            _needle = GameObject.Find("Needle");
        }
        private void FixedUpdate()
        {
            vehicleSpeed = _carController.kph;
            SpeedoUpdate();
        }
        
        /// <summary>
        /// 速度计指针更新
        /// </summary>
        private void SpeedoUpdate()
        {
            var perPosition = (startPosition - endPosition) / 100;
            _needle.transform.eulerAngles = new Vector3(0, 0, startPosition - perPosition * vehicleSpeed);
        }
    }
    
}


