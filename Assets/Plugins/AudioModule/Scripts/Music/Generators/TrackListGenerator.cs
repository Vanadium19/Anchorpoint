using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioModule
{
    public sealed class TrackListGenerator
    {
        private readonly List<AudioClip> _cache = new();

        public IReadOnlyList<AudioClip> Generate(IReadOnlyList<AudioClip> tracks, int count)
        {
            if (count > tracks.Count)
            {
                throw new ArgumentException("Count cannot be greater than size of the original list.");
            }

            _cache.Clear();
            _cache.AddRange(tracks);

            List<AudioClip> result = new List<AudioClip>(count);

            while (count > 0)
            {
                int index = Random.Range(0, _cache.Count);

                AudioClip clip = _cache[index];
                result.Add(clip);
                
                _cache.RemoveAt(index);
                count--;
            }
            
            return result;
        }
    }
}