using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AudioModule
{
    public sealed class SequenceTrackEnumerator : IEnumerator<AudioClip>, IReadOnlyList<AudioClip>
    {
        public AudioClip Current => _clips[_pointer];

        object IEnumerator.Current => Current;
        public int Count => _clips.Count;

        private readonly IReadOnlyList<AudioClip> _clips;
        private readonly bool _loop;
        private readonly bool _reverse;
        private int _pointer = -1;

        public SequenceTrackEnumerator(IReadOnlyList<AudioClip> clips, bool loop, bool reverse)
        {
            _clips = clips;
            _loop = loop;
            _reverse = reverse;

            if (_clips.Count == 0)
            {
                throw new Exception("List can't be empty!");
            }

            if (reverse)
            {
                _pointer = _clips.Count;
            }
        }

        public AudioClip this[int index]
        {
            get { return _clips[index]; }
        }

        public bool MoveNext()
        {
            if (_reverse)
            {
                _pointer--;

                if (_pointer >= 0)
                {
                    return true;
                }

                if (_loop)
                {
                    _pointer = _clips.Count - 1;
                    return true;
                }

                return false;
            }
            else
            {
                _pointer++;

                if (_loop)
                {
                    _pointer %= _clips.Count;
                    return true;
                }

                return _pointer < _clips.Count;
            }
        }

        public void Reset()
        {
            if (_reverse)
            {
                _pointer = _clips.Count;
            }
            else
            {
                _pointer = -1;
            }
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