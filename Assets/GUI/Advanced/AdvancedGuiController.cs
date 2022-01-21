using System.Collections;
using GUI.Resolution;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GUI.Advanced
{
    public class AdvancedGuiController : MonoBehaviour
    {
        public PlayerInput input;

        public Sprite menu;
        public Sprite view;
        public Sprite escape;
        public Sprite tab;
        
        public AudioClip advancedSound;
        public AudioClip quitSound;

        public Image quitImage;
        public Image advancedImage;
        public Text advancedText;
        
        public GameObject resolutionGui;

        private AudioSource _audio;

        private const string ShowMoreText = "Show more";
        private const string ShowLessText = "Show less";
        
        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
        }

        private void Start()
        {
            resolutionGui.SetActive(false);
            UpdateButtonsLabels();
        }

        public void Quit()
        {
            StartCoroutine(TararaAndQuit());
        }

        public void Advanced()
        {
            advancedText.text = resolutionGui.activeInHierarchy ? ShowMoreText : ShowLessText;
            resolutionGui.SetActive(!resolutionGui.activeInHierarchy);
            _audio.clip = advancedSound;
            _audio.Play();
        }

        public void UpdateButtonsLabels()
        {
            quitImage.sprite = PickQuitIcon();
            advancedImage.sprite = PickSelectIcon();
            
            if(resolutionGui.activeInHierarchy)
                resolutionGui.GetComponent<ResolutionGuiController>().UpdateResIcons();
        }
        
        private Sprite PickQuitIcon()
        {
            var controlScheme = input.currentControlScheme;

            Sprite icon = null;
            if (controlScheme.Contains("Keyboard"))
            {
                icon = escape;
            }

            if (controlScheme.Contains("Gamepad") || controlScheme.Contains("Joystick"))
            {
                icon = menu;
            }

            return icon;
        }
        
        private Sprite PickSelectIcon()
        {
            var controlScheme = input.currentControlScheme;

            Sprite icon = null;
            if (controlScheme.Contains("Keyboard"))
            {
                icon = tab;
            }

            if (controlScheme.Contains("Gamepad") || controlScheme.Contains("Joystick"))
            {
                icon = view;
            }

            return icon;
        }

        private IEnumerator TararaAndQuit()
        {
            _audio.clip = quitSound;
            _audio.Play();
            
            while (_audio.isPlaying)
            {
                yield return null;
            }
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
