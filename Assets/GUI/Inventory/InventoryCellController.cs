using UnityEngine;
using UnityEngine.UI;

namespace GUI.Inventory
{
    public class InventoryCellController : MonoBehaviour
    {
        public Image frameImage;
        public Sprite normalSprite;
        public Sprite selectedSprite;
        public Image contentIcon;
        public Image veil;
        public Color available;
        public Color interaction;
        public bool Empty { get; private set; } = true;
        public string Name { get; private set; }
        private bool _picked;
        private Animator _animator;
        private static readonly int Endark = Animator.StringToHash("Endark");
        private static readonly int Enlight = Animator.StringToHash("Enlight");
        private static readonly int Flip = Animator.StringToHash("Flip");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _animator.SetBool(Flip, false);
        }

        public void LoadContentInfo(ItemInfo info)
        {
            contentIcon.sprite = info.sprite;
            contentIcon.enabled = true;
            Name = info.itemName;
            Empty = false;
        }

        public void SetAnchor()
        {
            veil.gameObject.SetActive(true);
            veil.color = interaction;
        }

        public void Select(bool on = true, bool picking = false)
        {
            frameImage.sprite = on ? selectedSprite : normalSprite;
            veil.gameObject.SetActive(on && picking);
            
            if (picking && Empty)
            {
                veil.color = available;
            }
            if (_picked || picking && !Empty)
            {
                veil.color = interaction;
            }
        }

        public void DropDown(ItemInfo info)
        {
            var cancelled = info == null;
            
            Empty = cancelled;
            _picked = false;
            contentIcon.enabled = !cancelled;
            contentIcon.sprite = cancelled ? null: info.sprite;
            Name = cancelled ? string.Empty : info.itemName;
            veil.gameObject.SetActive(false);
        }

        public void BeginFlip()
        {
            _animator.SetBool(Flip, true);
        }

        /// <summary>
        /// Called during flip animation
        /// </summary>
        public void EmptySlot()
        {
            DropDown(null);
        }

        /// <summary>
        /// Called at the end of the flip animation
        /// </summary>
        public void EndFlip()
        {
            _animator.SetBool(Flip, false);
            _animator.Rebind();
            _animator.Update(0f);
            Empty = true;
        }

        public void ToggleDarkness(bool isLight)
        {
            _animator.SetBool(Enlight, isLight);
            _animator.SetBool(Endark, !isLight);
        }
    }
}
