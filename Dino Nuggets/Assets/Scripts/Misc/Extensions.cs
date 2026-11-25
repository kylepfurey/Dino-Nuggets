using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DN
{
    public static class Extensions
    {
        public static void ForEach(this IEnumerable collection, Action<object> action) { foreach (var value in collection) action(value); }
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action) { foreach (var value in collection) action(value); }

        public static bool IsNearby(this Vector2 vector, Vector2 other, float maxDistance = 0.1f) => (other - vector).sqrMagnitude < maxDistance * maxDistance;
        public static bool IsNearby(this Vector3 vector, Vector3 other, float maxDistance = 0.1f) => (other - vector).sqrMagnitude < maxDistance * maxDistance;

        public static T Random<T>(this T[] array) => array.Length > 0 ? array[UnityEngine.Random.Range(0, array.Length)] : default(T);

        public static void SpawnSound(this AudioClip clip)
        {
            if (clip == null)
                return;
            GameObject gameObject = new(clip.name);
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.spatialBlend = 0.0f;
            audioSource.volume = 1.0f;
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            audioSource.Play();
            UnityEngine.Object.Destroy(gameObject, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        }
        public static void SpawnSound(this AudioClip clip, Transform parent)
        {
            if (clip == null)
                return;
            GameObject gameObject = new(clip.name);
            gameObject.transform.parent = parent;
            gameObject.transform.localPosition = Vector3.zero;
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.spatialBlend = 1.0f;
            audioSource.volume = 1.0f;
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            audioSource.Play();
            UnityEngine.Object.Destroy(gameObject, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        }
        public static void SpawnSound(this AudioClip clip, Vector3 position, Transform parent = null)
        {
            if (clip == null)
                return;
            GameObject gameObject = new(clip.name);
            gameObject.transform.position = position;
            gameObject.transform.parent = parent;
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.spatialBlend = 1.0f;
            audioSource.volume = 1.0f;
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            audioSource.Play();
            UnityEngine.Object.Destroy(gameObject, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        }
    }
}
