using UnityEngine;
using UnityEngine.UI;

namespace GUI.Inventory
{
    public class ItemNameLabelController : MonoBehaviour
    {
        public Text labelText; 
        public bool Visible { get; private set; }
        private Animator _animator;
        private static readonly int LabelFadeIn = Animator.StringToHash("FadeIn");
        private static readonly int LabelFadeOut = Animator.StringToHash("FadeOut");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            Visible = true;
        }

        public void FadeIn()
        {
            _animator.SetBool(LabelFadeIn, true);
            _animator.SetBool(LabelFadeOut, false);
            Visible = true;
        }

        public void FadeOut()
        {
            _animator.SetBool(LabelFadeIn, false);
            _animator.SetBool(LabelFadeOut, true);
            Visible = false;
        }

        public void UpdateName(string labelName)
        {
            labelText.text = labelName;
        }
    }
}
