using UnityEngine;
using UnityEngine.UI;

namespace Fonts
{
    public class FontPilot : MonoBehaviour
    {
        public Font desired;
        private void Awake()
        {
            var texts = GameObject.FindObjectsOfType<Text>();
            foreach (var text in texts)
            {
                text.font = desired;
            }
        }
    }
}
