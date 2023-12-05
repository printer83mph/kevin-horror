using System;
using UnityEngine;
using UnityEngine.AI;

namespace Kevin
{
    public class KevinAI : MonoBehaviour
    {
        private abstract class KevinState
        {
            public abstract void Update();
        }

        private class StalkState : KevinState
        {
            public override void Update()
            {
                // TODO
            }
        }
        
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float runSpeed = 4f;

        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;

        private NavMeshAgent _agent;

        private KevinState _state;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            _state = new StalkState();
        }

        // Update is called once per frame
        void Update()
        {
            _state.Update();
        }
    }
}
