using System;
using UnityEngine;

namespace RacingGame.Car
{
    public class CarController : MonoBehaviour
    {
        /// <summary>
        /// 驱动类型
        /// </summary>
        internal enum DriveType
        {
            FrontWheelDrive,
            RearWheelDrive,
            AllWheelDrive
        }

        [Header("驱动类型"), SerializeField] private DriveType _driveType;
        
        [Header("Objects")]
        private Rigidbody _rigidbody;
        private GameObject _centerOfMass;   //车子的质心
        private GameObject _wheelColliders;//轮子collider父级
        private GameObject _wheelMeshes;//轮子Mesh父级
        [SerializeField]private WheelCollider[] wheelColliders = new WheelCollider[4];   //轮子collider
        [SerializeField]private GameObject[] wheelMeshes = new GameObject[4];            //轮子Mesh
        
        [Header("Debug Infos")]
        public float[] slips = new float[4];    //Debug: 滑动
        
        [Header("车型相关")]
        public float turnRadius = 6f;           //轮子半径
        public float wheelBase = 2.55f;         //前后轴距
        public float wheelTrack = 1.5f;         //左右轮距
        
        public float motorTorque = 1500;                                 //扭矩(车轮转动力)
        public float brakePower = 90000;                                //手刹制动力
        // public float steeringMax = 30;   //最大转向角

        public float downForceValue = 50f;      //下压力
        public float kph;   //  速度 km/h

        private void Start()
        {
            GetObjects();
        }

        private void FixedUpdate()
        {
            AddDownForce();
            
            AnimateWheels();

            MoveVehicle();
            SteerVehicle();

            GetFriction();
        }

        /// <summary>
        /// 车子移动
        /// </summary>
        private void MoveVehicle()
        {
            //如果手柄有输入就优先传入手柄值
            //  手柄、键盘的前进后退
            var accelerateInput = (Mathf.Abs(GameInputManager.Instance.RightTrigger) > 0.01f || Mathf.Abs(GameInputManager.Instance.LeftTrigger) > 0.01f)
                ? ((GameInputManager.Instance.RightTrigger) - (GameInputManager.Instance.LeftTrigger))
                : GameInputManager.Instance.Move.y;
            //  手刹
            var handBrake =  GameInputManager.Instance.HandBrake;
            //  目标转矩
            var targetMotorTorque = (accelerateInput != 0) ? motorTorque : 0;

            switch (_driveType)
            {
                case DriveType.AllWheelDrive:
                    //更新每个轮子的转矩
                    foreach (var wheelCollider in wheelColliders)
                    {
                        wheelCollider.motorTorque = accelerateInput * targetMotorTorque / 4;
                    }
                    break;
                case DriveType.FrontWheelDrive:
                    //更新前轮的转矩
                    for(int i = 0; i < wheelColliders.Length - 2; i++)
                    {
                        wheelColliders[i].motorTorque = accelerateInput * targetMotorTorque / 2;
                    }
                    break;
                case DriveType.RearWheelDrive:
                    //更新后轮的转矩
                    for(int i = 2; i < wheelColliders.Length; i++)
                    {
                        wheelColliders[i].motorTorque = accelerateInput * targetMotorTorque / 2;
                    }
                    break;
            }
            
            //  km/h = m/s * 3.6
            kph = _rigidbody.linearVelocity.magnitude * 3.6f;
            
            //手刹 - 只对后轮施加制动力
            var handBrakeTorque = handBrake ? brakePower : 0f;
            wheelColliders[2].brakeTorque = wheelColliders[3].brakeTorque = handBrakeTorque;

        }

        /// <summary>
        /// 车子转向
        /// </summary>
        private void SteerVehicle()
        {
            // var targetSteeringMax = (steerInput != 0) ? steeringMax : 0;    //目标最大转向角
            // //只更新前轮的转向角度
            // for (int i = 0; i < wheelMeshes.Length - 2; i++)
            // {
            //     wheelColliders[i].steerAngle = steerInput * targetSteeringMax;
            // }
            var steerInput = GameInputManager.Instance.Steer.x;        //转向输入

            if (steerInput < 0)
            {   //左转
                wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (wheelTrack / 2))) * steerInput;    //左轮转向角
                wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (wheelTrack / 2))) * steerInput;    //右轮转向角
            }
            else if (steerInput > 0)
            {   //右转
                wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (wheelTrack / 2))) * steerInput;    //左轮转向角
                wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (wheelTrack / 2))) * steerInput;    //右轮转向角
            }
            else
            {   //无转向
                wheelColliders[0].steerAngle = 0;   //左轮转向角
                wheelColliders[1].steerAngle = 0;   //右轮转向角
            }
        }

        /// <summary>
        /// 轮子转动动画——改变Mesh的属性
        /// </summary>
        private void AnimateWheels()
        {
            var wheelPosition = Vector3.zero;           //位置
            var wheelRotation = Quaternion.identity; //朝向
            
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                //获取wheelCollider的位置朝向
                wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);
                //改变wheelmesh的位置朝向
                wheelMeshes[i].transform.position = wheelPosition;
                wheelMeshes[i].transform.rotation = wheelRotation;
            }
        }

        /// <summary>
        /// 给车子施加下压力
        /// </summary>
        private void AddDownForce()
        {
            var currentSpeed = _rigidbody.linearVelocity.magnitude;
            _rigidbody.AddForce(Vector3.down * (downForceValue * currentSpeed));
        }

        /// <summary>
        /// 获取摩擦力
        /// </summary>
        /// 读取每个车轮与地面的接触信息，提取车轮的纵向打滑量 forwardSlip
        /// forwardSlip : 反映车轮转速与地面线速度的偏差（纵向打滑比）。数值越大，越打滑.
        private void GetFriction()
        {
            for(int i = 0; i < wheelColliders.Length; i++)
            {
                wheelColliders[i].GetGroundHit(out var hit);
                slips[i] = hit.forwardSlip;
            }
        }
        
        private void GetObjects()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _centerOfMass = GameObject.Find("CenterOfMass");
            _rigidbody.centerOfMass = _centerOfMass.transform.localPosition;     //把自定义的质心设置为rigidbody的质心
            
            _wheelColliders = GameObject.Find("WheelColliders");
            _wheelMeshes = GameObject.Find("WheelMeshes");
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                wheelColliders[i] = _wheelColliders.transform.Find($"{i}").gameObject.GetComponent<WheelCollider>();
                wheelMeshes[i] = _wheelMeshes.transform.Find($"{i}").gameObject;
            }
        }
        
    }
}
