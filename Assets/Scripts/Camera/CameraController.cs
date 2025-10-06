using RacingGame.Car;
using UnityEngine;
using UnityEngine.SocialPlatforms;


namespace RacingGame.CameraSystem
{
    public class CameraController : MonoBehaviour
    {
        private Camera _mainCamera;
        
        [Header("玩家"), SerializeField]
        private GameObject player;
        private CarController _controller;
        [Header("相机目标位置"),SerializeField]
        private GameObject target;
        
        [Header("相机跟随速度"),SerializeField]
        private float followSpeed;

        [Header("相机FOV")] 
        private float _defaultFOV;
        [SerializeField]private float targetFOV;
        [SerializeField,Range(0,5)]private float smoothFOV;

        private void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            _controller = player.GetComponent<CarController>();
            _mainCamera = Camera.main;
            _defaultFOV = _mainCamera.fieldOfView;
            
        }
        private void FixedUpdate()
        {
            FollowTarget();
            BoostFOV();
        }
        /// <summary>
        /// 跟随目标
        /// </summary>
        private void FollowTarget()
        {
            // 计算目标跟随速度
            float targetFollowSpeed = (_controller.kph > 50) ? 10 : _controller.kph / 2;
            
            // 平滑更新跟随速度
            followSpeed = Mathf.Lerp(followSpeed, targetFollowSpeed, Time.deltaTime);
            
            //位置更新：当前位置 -- Lerp --> 目标位置
            gameObject.transform.position = Vector3.Lerp(
                transform.position,
                target.transform.position,
                Time.deltaTime * followSpeed
                );
            //朝向玩家
            gameObject.transform.LookAt(player.transform);
        }

        /// <summary>
        /// 设置相机FOV
        /// </summary>
        private void BoostFOV()
        {
            _mainCamera.fieldOfView = Mathf.Lerp(
                _mainCamera.fieldOfView, 
                GameInputManager.Instance.Sprint ? targetFOV : _defaultFOV,
                Time.deltaTime * smoothFOV);
        }
    }
}