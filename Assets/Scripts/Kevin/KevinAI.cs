using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Kevin
{
    public class KevinAI : MonoBehaviour
    {
        private abstract class KevinState
        {
            protected readonly KevinAI AI;

            protected KevinState(KevinAI ai)
            { AI = ai; }

            public virtual void OnStateEnd()
            { }
            
            public virtual void Update()
            { }

            public virtual void OnStateStart()
            { }
        }

        private class WanderState : KevinState
        {
            private readonly KevinTarget[] _targets = new KevinTarget[3];

            public WanderState(KevinAI ai) : base(ai)
            { }

            public override void OnStateStart()
            {
                AI._agent.speed = AI.walkSpeed;
                AI.StartCoroutine(TargetsLoop(AI));
                AI.StartCoroutine(WanderLoop(AI));
            }

            private IEnumerator TargetsLoop(KevinAI ai)
            {
                while (true)
                {
                    Debug.Log("Hello");
                    Debug.Log(ai);
                    // set three targets (farthest from player)
                    Array.Sort(ai._targets, (a, b) => (int)Mathf.Sign(
                        (b.transform.position - ai._camera.transform.position).sqrMagnitude -
                        (a.transform.position - ai._camera.transform.position).sqrMagnitude)
                    );

                    for (var i = 0; i < 3; i++)
                        _targets[i] = ai._targets[i];

                    yield return new WaitForSeconds(1);
                }
            }

            private IEnumerator WanderLoop(KevinAI ai)
            {
                while (true)
                {
                    var target = Mathf.FloorToInt(Random.value * 3f);
                    ai._agent.SetDestination(_targets[target].transform.position);
                    yield return new WaitForSeconds(Random.Range(4.5f, 20f));
                }
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
        private KevinTarget[] _targets;

        // internal logic
        private KevinState _state;

        private void SetState(KevinState newState)
        {
            _state?.OnStateEnd();
            StopAllCoroutines();
            _state = newState;
            newState.OnStateStart();
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _camera = Camera.current;
            _targets = FindObjectsOfType<KevinTarget>();
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
