using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Inventory
{
    public class CellSelectionController : MonoBehaviour
    {
        private bool IsMoving { get; set; }
        public Image overlayItem;
        public InventoryGuiController inventoryGrid;
        public float cornerOffset;
        public Color normalColor;
        public Color pickedColor;

        private readonly List<Vector3> _directionStack = new();
        private AudioSource _audio;
        private ItemInfo _itemInfo;
        private Animator _animator;
        private Image _frameImage;
        private static readonly int PlaceInCorner = Animator.StringToHash("PlaceInCorner");
        private static readonly int Release = Animator.StringToHash("Release");
        private static readonly int Erase = Animator.StringToHash("Erase");
        
        private const float TransitionDuration = 0.15f;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _animator = GetComponent<Animator>();
            _frameImage = GetComponent<Image>();
            overlayItem.sprite = null;
            overlayItem.gameObject.SetActive(false);

            var cellSize = transform.parent.GetComponent<GridLayoutGroup>().cellSize;
            var rt = overlayItem.rectTransform;
            rt.offsetMin = new Vector2(-cellSize.x * cornerOffset, cellSize.y * cornerOffset);
            rt.offsetMax = new Vector2(-cellSize.x * cornerOffset, cellSize.y * cornerOffset);
        }

        public void SetAtPosition(Vector3 position)
        {
            GetComponent<RectTransform>().position = position;
        }
        public void Move(Vector3 where)
        {
            _directionStack.Add(where);
            if (IsMoving) return;
            
            TransposeSelection().Forget();
        }

        public void GetItem(ItemInfo info)
        {
            _itemInfo = info;
            overlayItem.gameObject.SetActive(true);
            overlayItem.sprite = info.sprite;
            _animator.SetBool(PlaceInCorner, true);
            _animator.SetBool(Release, false);
            _frameImage.color = pickedColor;
        }

        public void Trash()
        {
            _animator.SetTrigger(Erase);
        }

        public void ReleaseItem()
        {
            _animator.SetBool(PlaceInCorner, false);
            _animator.SetBool(Release, true);
            _frameImage.color = normalColor;
        }

        /// <summary>
        /// Called by the EraseItem animation
        /// </summary>
        public void DeleteItem()
        {
            overlayItem.sprite = null;
            overlayItem.gameObject.SetActive(false);
            _frameImage.color = normalColor;
        }

        /// <summary>
        /// Called by the PlaceInCorner animation.
        /// If onward is just ignored.
        /// </summary>
        public void BackToCenter()
        {
            if (_animator.GetBool(PlaceInCorner)) return;
            
            inventoryGrid.ReceiveItem(_itemInfo);
            _itemInfo = null;
            _frameImage.color = normalColor;
            overlayItem.sprite = null;
            overlayItem.gameObject.SetActive(false);
        }

        private async UniTask TransposeSelection()
        {
            IsMoving = true;
            var rectTransform = GetComponent<RectTransform>();
        
            while(_directionStack.Count > 0)
            {
                var endPosition = _directionStack[0];
                _directionStack.Remove(endPosition);
                
                var duration = TransitionDuration;
                var p = rectTransform.position;
                var direction = (endPosition - p).normalized;
                var amountToMove = (endPosition - p).magnitude;
                var quantum = amountToMove / duration;
                var amountMoved = 0.0f;
                while (duration > 0)
                {
                    var frameDelta = quantum * Time.deltaTime;
                    amountMoved += frameDelta;
                    if (amountMoved > amountToMove) frameDelta -= amountMoved - amountToMove;
                    rectTransform.position += direction * frameDelta;
                    duration -= Time.deltaTime;
                    await UniTask.NextFrame();
                }

                rectTransform.position = endPosition;
                _audio.Play();
            }
            
            IsMoving = false;
        }
    }
}
