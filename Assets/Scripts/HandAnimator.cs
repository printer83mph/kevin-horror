using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimator : MonoBehaviour
{
    [SerializeField] private InputActionReference triggerInputAction;
    [SerializeField] private InputActionReference gripInputAction;

    [SerializeField] private float interpSpeed = 8f;

    private Animator _animator;
    
    private static readonly int Trigger = Animator.StringToHash("Trigger");
    private static readonly int Grip = Animator.StringToHash("Grip");

    private float _triggerInterp;
    private float _gripInterp;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _triggerInterp = Mathf.MoveTowards(_triggerInterp, triggerInputAction.action.ReadValue<float>(),
            Time.deltaTime * interpSpeed);
        _gripInterp = Mathf.MoveTowards(_gripInterp, gripInputAction.action.ReadValue<float>(),
            Time.deltaTime * interpSpeed);
        
        _animator.SetFloat(Trigger, _triggerInterp);
        _animator.SetFloat(Grip, _gripInterp);
    }
}
