using UnityEngine;
using UnityEngine.AI;

namespace Kevin
{
    public class KevinAnimator : MonoBehaviour
    {
        private Animator _animator;
        private NavMeshAgent _agent;
        private KevinAI _ai;
        
        private static readonly int Speed = Animator.StringToHash("Speed");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _ai = GetComponent<KevinAI>();
        }

        // Updates the animator's "speed" float based on its velocity and walk/run speed
        void Update()
        {
            var animatorSpeed = _agent.velocity.magnitude;
            
            if (animatorSpeed < _ai.WalkSpeed)
                animatorSpeed /= _ai.WalkSpeed * 2f;
            else
                animatorSpeed = Mathf.Lerp(0.5f, 1f, 
                    Mathf.InverseLerp(_ai.WalkSpeed, _ai.RunSpeed, animatorSpeed));
            
            _animator.SetFloat(Speed, animatorSpeed);
        }
    }
}
