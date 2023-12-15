using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Kevin
{
    public class KevinAI : MonoBehaviour
    {
        private abstract class KevinState
        {
            protected readonly KevinAI AI;
            protected KevinTarget[] _targets;

            protected KevinState(KevinAI ai)
            {
                AI = ai;
                _targets = FindObjectsOfType<KevinTarget>();
            }

            public virtual void OnStateEnd()
            { }
            
            public virtual void Update()
            { }

            public virtual void OnStateStart()
            { }
        }

        private class WanderState : KevinState
        {
            private readonly KevinTarget[] _primeTargets = new KevinTarget[3];

            public WanderState(KevinAI ai) : base(ai)
            { }

            public override void OnStateStart()
            {
                AI._agent.speed = AI.walkSpeed;
            }

            private float _targetsTimer;
            private float _destinationTimer;

            public override void Update()
            {
                if (_targetsTimer < 0f)
                {
                    UpdateTargets();
                    _targetsTimer += 1f;
                }

                if (_destinationTimer < 0f)
                {
                    UpdateDestination();
                    _destinationTimer += Random.Range(4.5f, 10f);
                }

                if (AI._agent.remainingDistance < 0.1f)
                {
                    _destinationTimer -= Time.deltaTime;
                }

                _targetsTimer -= Time.deltaTime;
            }

            private void UpdateTargets()
            {
                // set three targets (farthest from player)
                Array.Sort(_targets, (a, b) => (int)Mathf.Sign(
                    (b.transform.position - AI._camera.transform.position).sqrMagnitude -
                    (a.transform.position - AI._camera.transform.position).sqrMagnitude)
                );

                for (var i = 0; i < 3; i++)
                    _primeTargets[i] = _targets[i];
            }

            private void UpdateDestination()
            {
                var target = Mathf.FloorToInt(Random.value * 3f);
                AI._agent.SetDestination(_primeTargets[target].transform.position);
            }
        }
        
        // parameters
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float runSpeed = 4f;

        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;

        // references
        private NavMeshAgent _agent;
        private Camera _camera;

        // internal logic
        private KevinState _state;

        private void SetState(KevinState newState)
        {
            _state?.OnStateEnd();
            _state = newState;
            newState.OnStateStart();
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _camera = Camera.main;
        }

        private void Start()
        {
            SetState(new WanderState(this));
        }

        private void Update()
        {
            _state.Update();
        }
    }
}
