using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGG.Tool.Singleton;

public class GameInputManager : Singleton<GameInputManager>
{
    private GameInputAction _gameInputAction;

    public Vector2 Move => _gameInputAction.Player.Move.ReadValue<Vector2>();
    public Vector2 Look => _gameInputAction.Player.Look.ReadValue<Vector2>();
    public float RightTrigger => _gameInputAction.Player.RightTrigger.ReadValue<float>();
    public float LeftTrigger => _gameInputAction.Player.LeftTrigger.ReadValue<float>();

    public Vector2 Steer => _gameInputAction.Player.Steer.ReadValue<Vector2>();
    public bool HandBrake => _gameInputAction.Player.HandBrake.IsPressed();
    public bool Sprint => _gameInputAction.Player.Sprint.IsPressed();
    

    protected override void Awake()
    {
        base.Awake();
        _gameInputAction ??= new GameInputAction(); //�ǿյģ��򴴽��µ�ʵ��
    }

    private void OnEnable()
    {
        _gameInputAction.Enable();
    }
    private void OnDisable()
    {
        _gameInputAction.Disable();
    }
}
