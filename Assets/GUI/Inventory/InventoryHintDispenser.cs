using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GUI.Inventory
{
    public class InventoryHintDispenser : MonoBehaviour
    {
        public PlayerInput input;
        public Sprite gamePadNorth;
        public Sprite gamePadSouth;
        public Sprite backspace;
        public Sprite enter;

        public Transform cancelTransform;
        public Transform enterTransform;

        private static readonly int FadeIn = Animator.StringToHash("FadeIn");
        private static readonly int FadeOut = Animator.StringToHash("FadeOut");

        private void Start()
        {
            FadeCancel(true);
        }

        public void UpdateHints(bool pickingState, bool overlapped = false)
        {
            if (!pickingState)
            {
                if (overlapped)
                {
                    UpdateGrabHint("Grab");
                    FadeEnter(true);
                }

                if (!overlapped)
                {
                    FadeEnter(false);
                }
                UpdateCancelHint("Reroll");
            }
            if (!pickingState && overlapped)
            {
                UpdateGrabHint("Grab");
                UpdateCancelHint("Reroll");
            }

            if (pickingState && !overlapped)
            {
                UpdateGrabHint("Drop");
                UpdateCancelHint("Trash");
            }

            if (pickingState && overlapped)
            {
                UpdateGrabHint("Swap");
                UpdateCancelHint("Trash");
            }
        }

        private void UpdateCancelHint(string labelText)
        {
            cancelTransform.Find("Image").GetComponent<Image>().sprite = PickDeleteIcon();
            cancelTransform.Find("Text").GetComponent<Text>().text = labelText;
        }

        private void UpdateGrabHint(string labelText)
        {
            enterTransform.Find("Image").GetComponent<Image>().sprite = PickGrabIcon();
            enterTransform.Find("Text").GetComponent<Text>().text = labelText;
        }

        private Sprite PickGrabIcon()
        {
            var controlScheme = input.currentControlScheme;

            Sprite icon = null;
            if (controlScheme.Contains("Keyboard"))
            {
                icon = enter;
            }

            if (controlScheme.Contains("Gamepad") || controlScheme.Contains("Joystick"))
            {
                icon = gamePadSouth;
            }

            return icon;
        }

        private Sprite PickDeleteIcon()
        {
            var controlScheme = input.currentControlScheme;

            Sprite icon = null;
            if (controlScheme.Contains("Keyboard"))
            {
                icon = backspace;
            }

            if (controlScheme.Contains("Gamepad") || controlScheme.Contains("Joystick"))
            {
                icon = gamePadNorth;
            }

            return icon;
        }

        private void FadeCancel(bool fadeIn)
        {
            var animator = cancelTransform.GetComponent<Animator>();
            
            if (fadeIn && animator.GetBool(FadeIn)) return;
            
            animator.SetBool(FadeIn, fadeIn);
            animator.SetBool(FadeOut, !fadeIn);
        }

        private void FadeEnter(bool fadeIn)
        {
            var animator = enterTransform.GetComponent<Animator>();
            animator.SetBool(FadeIn, fadeIn);
            animator.SetBool(FadeOut, !fadeIn);
        }
    }
}
