using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Inventory
{
    public class FloatingItemController : MonoBehaviour
    {
        public float transitionSpeed;
        public InventoryGuiController inventory;
        public Image image;
        private RectTransform _transform;
        private ItemInfo _item;

        private void Awake()
        {
            _transform = GetComponent<RectTransform>();
            image.gameObject.SetActive(false);
        }

        public void Setup(ItemInfo itemInfo, Vector3 startingPosition)
        {
            _item = itemInfo;
            image.sprite = _item.sprite;
            image.gameObject.SetActive(true);
            _transform.position = startingPosition;
        }

        public async UniTask Move(Vector3 destination)
        {
            await Moving(destination);
        }

        private async UniTask Moving(Vector3 destination)
        {
            var p = _transform.position;
            var difference = destination - p;
            var amountToMove = difference.magnitude;
            var direction = difference.normalized;
            var amountMoved = 0.0f;
            
            while (amountMoved < amountToMove)
            {
                var frameAmount = transitionSpeed * Time.deltaTime;
                amountMoved += frameAmount;
                if (amountMoved > amountToMove) frameAmount -= amountMoved - amountToMove;
                _transform.position += frameAmount * direction;
                await UniTask.NextFrame();
            }

            inventory.ReceiveItem(_item, true);
            image.gameObject.SetActive(false);
        }
    }
}
