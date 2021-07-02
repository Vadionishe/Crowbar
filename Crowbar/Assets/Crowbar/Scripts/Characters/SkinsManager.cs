using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crowbar
{
    public class SkinsManager : MonoBehaviour
    {
        public static SkinsManager skinsManager;

        [Serializable]
        public class Hat
        {
            public int id;
            public Sprite sprite;
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;
        }

        public List<Hat> hats;

        public static void SetHat(SpriteRenderer hatRenderer, int id)
        {
            Hat hat = skinsManager.hats.Find(h => h.id == id);

            if (hat != null)
            {
                hatRenderer.sprite = hat.sprite;

                hatRenderer.gameObject.transform.localPosition = hat.position;
                hatRenderer.gameObject.transform.localEulerAngles = hat.rotation;
                hatRenderer.gameObject.transform.localScale = hat.scale;
            }
        }

        private void Awake()
        {
            skinsManager = this;
        }
    }
}
