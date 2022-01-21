using System;
using System.Collections;
using System.Collections.Generic;
using Resolutions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GUI.Resolution
{
    public class ResolutionGuiController : MonoBehaviour
    {
        public GameObject resolutionTypePrefab;
        public Sprite rButton;
        public Sprite lButton;
        public Sprite rightBracket;
        public Sprite leftBracket;
        
        public Color highlighted;
        public Color normal;
        public Transform resolutionLabelContainer;
        public Text bottomLabel;
        public Image prevIcon;
        public Image nextIcon;
        public PlayerInput input;
        
        private Dictionary<ResolutionType, Text> textMap = new Dictionary<ResolutionType, Text>();
        private const string StaticLabel = "Current input contorl is";

        private void Awake()
        {
            foreach (var res in ResolutionMapper.Map)
            {
                var label = Instantiate(resolutionTypePrefab, resolutionLabelContainer);
                var textLabel = label.GetComponent<Text>();
                textLabel.text = $"{res.Key.ToString()} [{(int)res.Value.x}, {(int)res.Value.y}]";
                textMap.Add(res.Key, textLabel);
            }
            
            UpdateResIcons();
        }

        public void UpdateLabel()
        {
            StartCoroutine(WaitAndUpdate());
        }

        private IEnumerator WaitAndUpdate()
        {
            // wait a couple of frame to let the system update the resolution
            yield return null;
            yield return null;
            
            foreach (var item in textMap)
            {
                item.Value.color = item.Key == ResolutionMapper.Current ? highlighted : normal;
            }
        }

        public void UpdateResIcons()
        {
            prevIcon.sprite = PickPreviousResolutionIcon();
            nextIcon.sprite = PickNextResolutionIcon();
            bottomLabel.text = $"{StaticLabel} [{input.currentControlScheme}]";
        }
        
        private Sprite PickPreviousResolutionIcon()
        {
            var controlScheme = input.currentControlScheme;

            Sprite icon = null;
            if (controlScheme.Contains("Keyboard"))
            {
                icon = leftBracket;
            }

            if (controlScheme.Contains("Gamepad") || controlScheme.Contains("Joystick"))
            {
                icon = lButton;
            }

            return icon;
        }
        
        private Sprite PickNextResolutionIcon()
        {
            var controlScheme = input.currentControlScheme;

            Sprite icon = null;
            if (controlScheme.Contains("Keyboard"))
            {
                icon = rightBracket;
            }

            if (controlScheme.Contains("Gamepad") || controlScheme.Contains("Joystick"))
            {
                icon = rButton;
            }

            return icon;
        }

        private void OnEnable()
        {
            UpdateLabel();
            UpdateResIcons();
        }
    }
}
