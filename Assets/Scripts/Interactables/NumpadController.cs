using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

namespace Interactables
{
    public class NumpadController : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private XRSimpleInteractable[] buttons;
        [SerializeField] private int[] correctCode;

        public UnityEvent onCorrectCode = new();

        private readonly int[] _numbers = new int[4];
        private int _currentNumberIndex = 0;

        private bool ValidateCode()
        {
            // check if code valid
            for (var i = 0; i < 4; i++)
                if (_numbers[i] != correctCode[i]) return false;

            return true;
        }

        private void PressButton(int number)
        {
            // if we're restarting
            if (_currentNumberIndex >= 4)
                _currentNumberIndex = 0;
            
            _numbers[_currentNumberIndex] = number;
            _currentNumberIndex++;

            // set TextMeshPro text
            var text = new StringBuilder();
            for (var i = 0; i < _currentNumberIndex; i++)
            {
                text.Append(_numbers[i]);
            }
            textMesh.SetText(text.ToString());

            // continue along if we're not done with the code
            if (_currentNumberIndex < 4) return;
            
            if (ValidateCode())
            {
                Debug.Log("Correct code input");
                onCorrectCode.Invoke();
            }
            else
            {
                Debug.Log("Wrong code input");
                // wrong code idk do something
            }
        }

        private void Awake()
        {
            // number is from 1-9
            var iter = 1;
            foreach (var interactable in buttons)
            {
                var number = iter;
                interactable.selectEntered.AddListener(_ =>
                {
                    PressButton(number);
                    buttons[number - 1].transform.GetChild(0).localPosition.Set(0, 0.04f, 0);
                });
                interactable.hoverEntered.AddListener(_ =>
                {
                    buttons[number - 1].transform.GetChild(0).localPosition.Set(0, 0.02f, 0);
                });
                interactable.hoverExited.AddListener(_ =>
                {
                    buttons[number - 1].transform.GetChild(0).localPosition.Set(0, 0, 0);
                });
                iter = number + 1;
            }
        }
    }
}
