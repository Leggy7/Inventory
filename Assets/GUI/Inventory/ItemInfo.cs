using UnityEngine;

namespace GUI.Inventory
{
    [CreateAssetMenu(fileName = "Item info", menuName = "Inventory/itemInfo", order = 0)]
    public class ItemInfo : ScriptableObject
    {
        public Sprite sprite;
        public string itemName;
    }
}
