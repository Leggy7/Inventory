using UnityEngine;

namespace GUI.Inventory
{
    public class InventoryAudioPlayer : MonoBehaviour
    {
        public AudioClip selectAudio;
        public AudioClip pickAudio;
        public AudioClip rerollAudio;
        public AudioClip trashAudio;

        private AudioSource _source;
        
        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        public void PlaySelect()
        {
            _source.clip = selectAudio;
            _source.Play();
        }

        public void PlayPick()
        {
            _source.clip = pickAudio;
            _source.Play();
        }

        public void PlayReroll()
        {
            _source.clip = rerollAudio;
            _source.Play();
        }

        public void PlayTrash()
        {
            _source.clip = trashAudio;
            _source.Play();
        }
    }
}
