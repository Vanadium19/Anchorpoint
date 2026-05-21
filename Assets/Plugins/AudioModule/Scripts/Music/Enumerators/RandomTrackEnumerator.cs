using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AudioModule
{
    public sealed class RandomTrackEnumerator : IEnumerator<AudioClip>, IReadOnlyList<AudioClip>
    {
        public AudioClip Current => _clips[_pointer];
        
        object IEnumerator.Current => Current;

        public int Count => _clips.Count;
        private readonly IReadOnlyList<AudioClip> _clips;
        
        private int _pointer = -1;

        public RandomTrackEnumerator(IReadOnlyList<AudioClip> clips)
        {
            _clips = clips;
            
            if (_clips.Count == 0)
            {
                throw new Exception("List can't be empty!");
            }
        }

        public AudioClip this[int index]
        {
            get { return _clips[index]; }
        }

        public bool MoveNext()
        {
            _pointer = UnityEngine.Random.Range(0, _clips.Count);
            return true;
        }

        public void Reset()
        {
            _pointer = -1;
        }

        public void Dispose()
        {
        }

        public IEnumerator<AudioClip> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}