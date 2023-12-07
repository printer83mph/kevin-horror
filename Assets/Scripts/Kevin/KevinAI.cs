using UnityEngine;
using UnityEngine.AI;

namespace Kevin
{
    public class KevinAI : MonoBehaviour
    {
        private abstract class KevinState
        {
            protected KevinAI AI;

            protected KevinState(KevinAI ai)
            {
                AI = ai;
            }
            public abstract void Update();
        }

        private class StalkState : KevinState
        {
            private readonly KevinTarget[] _targets = FindObjectsOfType<KevinTarget>();
            private int _targetIndex = 0;

            private float _waitTime;

            public StalkState(KevinAI ai) : base(ai)
            {}

            public override void Update()
            {
                _waitTime -= Time.deltaTime;
                if (_waitTime < 0f)
                {
                    _waitTime = 10f;
                    _targetIndex = (_targetIndex + 1) % _targets.Length;
                    AI._agent.SetDestination(_targets[_targetIndex].transform.position);
                }

                AI._agent.speed = AI._agent.remainingDistance < 6f ? AI.walkSpeed : AI.runSpeed;
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
            _state = new StalkState(this);
        }

        // Update is called once per frame
        void Update()
        {
            _state.Update();
        }
    }
}
