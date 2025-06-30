using System;
using GifImporter;
using UnityEngine;
using UnityEngine.UI;

namespace GifImporter
{
    [ExecuteAlways]
    public class GifPlayer : MonoBehaviour
    {
        public Gif   Gif;
        public float SpeedMultiplier = 1;

        private int   _index;
        private Gif   _setGif;
        private float _lastTime;
        private float _delay;

        private void OnEnable()
        {
            if (Gif == null) return;
            var frames = Gif.Frames;
            if (frames == null || frames.Count == 0) return;

            if (_index > frames.Count - 1)
            {
                _index = _index % frames.Count;
            }

            var frame = frames[_index];
            Apply(frame);
        }

        private void Update()
        {
            if (Gif == null) return;
            var frames = Gif.Frames;
            if (frames == null || frames.Count == 0) return;
            var  absSpeed = Mathf.Abs(SpeedMultiplier);
            bool forward  = SpeedMultiplier >= 0;
            if (absSpeed < 0.001) return;

            int index = _index;

            var endFrame = _lastTime + _delay/absSpeed;
            if (Application.isPlaying && endFrame < Time.time)
            {
                if (forward) index++;
                else index--;
            }

            if (index > frames.Count - 1)
            {
                index %= frames.Count;
            }
            
            if (index < 0)
            {
                index = frames.Count-1;
            }

            if (index != _index || _setGif != Gif)
            {
                _index = index;
                var frame = frames[_index];
                Apply(frame);
            }
        }

        private void Apply(GifFrame frame)
        {
            Image image = null;
            if (TryGetComponent<SpriteRenderer>(out var spriteRenderer) || TryGetComponent(out image))
            {
                _lastTime = Time.time;
                _delay    = (frame.DelayInMs * 0.001f);
                if (spriteRenderer != null) spriteRenderer.sprite = frame.Sprite;
                else if (image != null) image.sprite              = frame.Sprite;

                _setGif = Gif;
            }
        }
    }
}

