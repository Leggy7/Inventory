using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GUI.Advanced;
using GUI.Resolution;
using Resolutions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GUI.Inventory
{
    public class InventoryGuiController : MonoBehaviour
    {
        public GameObject inventoryCellPrefab;
        public int cols = 6;
        public int rows = 3;
        public int spawningItems = 5;
        public ResolutionType resolution;
        public ItemNameLabelController nameLabel;
        public ResolutionGuiController resolutionGui;
        public AdvancedGuiController advancedGui;
        public GameObject selection;
        public FloatingItemController floatingItem;


        private ItemInfo[] _itemInfos;
        private InventoryCellController[,] _cells;
        private InventoryCellController _selected;
        private InventoryCellController _swappingCell;
        private InventoryState State { get; set; }
        private InventoryCellController _picked;
        private Vector2 _currentResolution;
        private InventoryAudioPlayer _audioPlayer;
        private InventoryHintDispenser _hintDispenser;

        private bool _isQuitting;
        private bool _landingItem;
        private static readonly int Flip = Animator.StringToHash("Flip");


        private void Awake()
        {
            Cursor.visible = false;

            selection.GetComponent<RectTransform>().sizeDelta = GetComponent<GridLayoutGroup>().cellSize;
            
            _itemInfos = Resources.LoadAll<ItemInfo>("ItemInfos");
            _audioPlayer = GetComponent<InventoryAudioPlayer>();
            _hintDispenser = transform.Find("Hints").GetComponent<InventoryHintDispenser>();
            
            InitializeInventory();
            State = InventoryState.Idle;
            
            ResolutionMapper.SetResolution(resolution);
            resolutionGui.UpdateLabel();
        }

        private void Start()
        {
            UpdateHints();
            UpdateNameLabel(_selected.Name);
        }

        /// <summary>
        /// This method is called at the animation end.
        /// We need to drop down the item in the selected cell or the item in the swapped position.
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="swapped"></param>
        public void ReceiveItem(ItemInfo itemInfo, bool swapped = false)
        {
            _landingItem = false;
            if(swapped) _swappingCell.DropDown(itemInfo);
            else _selected.DropDown(itemInfo);
            
            UpdateHints();
            UpdateNameLabel(_selected.Name);
        }

        private void SelectNext(Vector2 direction)
        {
            var selectionController = selection.GetComponent<CellSelectionController>();
            
            var currentCoords = PickSlot(_selected);
            currentCoords += direction;

            if (currentCoords.x < 0 ||
                currentCoords.y < 0 ||
                currentCoords.x >= _cells.GetLength(0) ||
                currentCoords.y >= _cells.GetLength(1))
                return;
            
            selectionController.Move(
                _cells[(int)currentCoords.x, (int)currentCoords.y].
                    GetComponent<RectTransform>().position);
            
            _selected = _cells[(int)currentCoords.x, (int)currentCoords.y];
            UpdateHints();
            UpdateNameLabel(_selected.Name);
        }

        private void InitializeInventory()
        {
            _cells = new InventoryCellController[rows, cols];
        
            var gridLayout = GetComponent<GridLayoutGroup>();
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = cols;
        
            for(var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    var cell = Instantiate(inventoryCellPrefab, transform);
                    _cells[row, col] = cell.GetComponent<InventoryCellController>();
                }
            }
            
            PopulateRandomSlots(spawningItems);
            
            _selected = _cells[0, 0];
            StartCoroutine(WaitAfterInitializeInventory());
        }

        private void PopulateRandomSlots(int howMany)
        {
            howMany = Mathf.Min(howMany, cols * rows);
            for (var n = 0; n < howMany; n++)
            {
                var randomSlot = PickRandomSlot();
                randomSlot.LoadContentInfo(_itemInfos[Random.Range(0, _itemInfos.Length)]);
            }
        }

        /// <summary>
        /// This is due because it needs to be called after inventory grid population.
        /// Thus, we need to wait a couple of frames.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitAfterInitializeInventory()
        {
            yield return null;
            yield return null;
            selection.SetActive(true);
            selection.GetComponent<CellSelectionController>().
                SetAtPosition(_cells[0, 0].GetComponent<RectTransform>().position);
            floatingItem.transform.SetAsLastSibling();
            selection.transform.SetAsLastSibling();
        }

        private InventoryCellController PickRandomSlot(bool empty = true)
        {
            var cellList = new List<InventoryCellController>();
            for (var row = 0; row < _cells.GetLength(0); row++)
            {
                for (var col = 0; col < _cells.GetLength(1); col++)
                {
                    if (empty && _cells[row, col].Empty || !empty)
                    {
                        cellList.Add(_cells[row, col]);
                    }
                }
            }
            if(cellList.Count <= 0) throw new Exception("no space left in inventory.");

            return cellList[Random.Range(0, cellList.Count)];
        }

        private Vector2 PickSlot(Component target)
        {
            for (var row = 0; row < _cells.GetLength(0); row++)
            {
                for (var col = 0; col < _cells.GetLength(1); col++)
                {
                    if (_cells[row, col] == target)
                    {
                        return new Vector2(row, col);
                    }
                }
            }
            throw new Exception($"Cannot find {target.gameObject.name} in inventory.");
        }

        private void PickUpItem()
        {
            var itemInfo = ScriptableObject.CreateInstance<ItemInfo>();
            itemInfo.sprite = _selected.contentIcon.sprite;
            itemInfo.itemName = _selected.Name;
            _selected.DropDown(null);
            _picked = _selected;
            _picked.SetAnchor();
            State = InventoryState.Picked;
            _audioPlayer.PlayPick();
            selection.GetComponent<CellSelectionController>().GetItem(itemInfo);
            UpdateHints();
        }

        private void LandItem()
        {
            _landingItem = true;
            if(!_picked.Equals(_selected))
                _picked.DropDown(null);
            
            selection.GetComponent<CellSelectionController>().ReleaseItem();
                
            _picked = null;
            State = InventoryState.Idle;
            _audioPlayer.PlayPick();
        }

        private void SwapItems()
        {
            _landingItem = true;
            var info = CreateItemInfo(_selected);
            _swappingCell = _picked;
            floatingItem.Setup(info, _selected.GetComponent<RectTransform>().position);
            floatingItem.Move(_picked.GetComponent<RectTransform>().position);
            _picked.Select(false);
            selection.GetComponent<CellSelectionController>().ReleaseItem();
            _picked = null;
            State = InventoryState.Idle;
            _audioPlayer.PlayPick();
        }

        private void DeleteItem()
        {
            _picked.DropDown(null);
            selection.GetComponent<CellSelectionController>().Trash();
            _picked = null;
            State = InventoryState.Idle;
            _audioPlayer.PlayTrash();
            UpdateHints();
            UpdateNameLabel(_selected.Name);
        }

        private IEnumerator ReloadInventory()
        {
            var items = GameObject.FindGameObjectsWithTag("InventorySlot")
                .Where(x => !x.GetComponent<InventoryCellController>().Empty).Select(x => x.GetComponent<InventoryCellController>());

            var inventoryCellControllers = items as InventoryCellController[] ?? items.ToArray();
            Debug.Log($"I found {items.Count()} cells to flip");
            Debug.Log($"Corresponding to {inventoryCellControllers.Count()} controllers.");
            foreach (var item in inventoryCellControllers)
            {
                item.BeginFlip();
            }

            while (true)
            {
                var animationEnded = true;
                foreach (var item in inventoryCellControllers.Select(i => i.GetComponent<Animator>()))
                {
                    if (item.GetBool(Flip)) animationEnded = false;
                }

                if (animationEnded) break;
                yield return null;
            }
            
            _audioPlayer.PlayReroll();
            PopulateRandomSlots(spawningItems);
            
            UpdateHints();
            UpdateNameLabel(_selected.Name);
        }

        private void UpdateHints()
        {
            if (!_hintDispenser) return;
            _hintDispenser.UpdateHints(
                State == InventoryState.Picked, 
                !_selected.Empty);
        }

        private void UpdateNameLabel(string text)
        {
            if(_selected.Empty && nameLabel.Visible) nameLabel.FadeOut();
            else if(!(_selected.Empty || nameLabel.Visible)) nameLabel.FadeIn();
            nameLabel.UpdateName(text);
        }

        private void ToggleDarkness()
        {
            var items = GameObject.FindGameObjectsWithTag("InventorySlot")
                .Where(x => !x.GetComponent<InventoryCellController>().Empty);

            foreach (var item in items)
            {
                item.GetComponent<InventoryCellController>().ToggleDarkness(_picked == null);
            }
        }

        private ItemInfo CreateItemInfo(InventoryCellController cell)
        {
            var info = ScriptableObject.CreateInstance<ItemInfo>();
            info.sprite = cell.contentIcon.sprite;
            info.itemName = cell.Name;
            return info;
        }

        /// <summary>
        /// Input system's event functions
        /// </summary>
        #region Events

        private void OnNavigate(InputValue value)
        {
            if (_landingItem) return;
            
            var received = (Vector2) value.Get();
            var direction = new Vector2(-received.y, received.x);
            SelectNext(direction);
        }

        private void OnConfirm()
        {
            if (_selected.Empty && State == InventoryState.Idle || _landingItem) return;

            if(State == InventoryState.Idle)
            {
                PickUpItem();
            }
            else if(State == InventoryState.Picked && _selected.Empty)
            {
                LandItem();
            }
            else if (State == InventoryState.Picked && !_selected.Empty)
            {
                SwapItems();
            }

            ToggleDarkness();
        }

        private void OnCancel()
        {
            if (State == InventoryState.Idle) StartCoroutine(ReloadInventory());
            if (State != InventoryState.Picked) return;
            DeleteItem();
            ToggleDarkness();
        }

        private void OnNextResolution()
        {
            ResolutionMapper.NextResolution();
            if(resolutionGui.gameObject.activeInHierarchy)
                resolutionGui.UpdateLabel();
        }

        private void OnPreviousResolution()
        {
            ResolutionMapper.PreviousResolution();
            if(resolutionGui.gameObject.activeInHierarchy)
                resolutionGui.UpdateLabel();
        }

        private void OnQuit()
        {
            if (_isQuitting) return;
            _isQuitting = true;
            advancedGui.Quit();
        }

        private void OnAdvanced()
        {
            advancedGui.Advanced();
        }

        private void OnControlsChanged()
        {
            UpdateHints();
            advancedGui.UpdateButtonsLabels();
        }

        #endregion
    }
}
