using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace AudioModule
{
    public sealed partial class AudioSystem
    {
        public static AudioSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<AudioSystem>();
                return _instance;
            }
        }
        
        private static AudioSystem _instance;

        private static readonly Dictionary<Scene, AudioSystem> _singletons = new();

        public static AudioSystem Resolve(in Component component) => Resolve(component.gameObject);

        public static AudioSystem Resolve(in GameObject gameObject) => Resolve(gameObject.scene);

        public static AudioSystem Resolve(Scene scene)
        {
            if (_singletons.TryGetValue(scene, out AudioSystem singleton) && singleton)
                return singleton;

            List<GameObject> gameObjects = ListPool<GameObject>.Get();
            scene.GetRootGameObjects(gameObjects);
            for (int i = 0, count = gameObjects.Count; i < count; i++)
            {
                GameObject go = gameObjects[i];
                singleton = go.GetComponentInChildren<AudioSystem>();
                if (!singleton)
                    continue;

                _singletons[scene] = singleton;
                return singleton;
            }

            ListPool<GameObject>.Release(gameObjects);
            throw new Exception($"Audio System of type {nameof(AudioSystem)} is not found!");
        }
    }
}