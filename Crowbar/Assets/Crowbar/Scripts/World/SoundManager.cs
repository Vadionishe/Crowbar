﻿using UnityEngine;

namespace Crowbar
{
    public class SoundManager : MonoBehaviour
    {
        public AudioClip overWaterSound;
        public AudioClip underWaterSound;

        public AudioSource audioSource;

        private void Update()
        {
            audioSource.volume = Settings.volume / 2f;

            if (GameUI.localStats != null)
            {
                bool inWater = GameUI.localStats.oxygenChecker.CheckCollision();
                Place place = GameUI.localStats.GetComponentInParent<Place>();

                if (inWater)
                {
                    if (audioSource.clip != underWaterSound)
                    {
                        audioSource.clip = underWaterSound;
                        audioSource.Play();
                    }
                }
                else
                {
                    if (place != null && place.placeSound != null)
                    {
                        if (audioSource.clip != place.placeSound)
                        {
                            audioSource.clip = place.placeSound;
                            audioSource.Play();
                        }
                    }
                    else
                    {
                        if (audioSource.clip != overWaterSound)
                        {
                            audioSource.clip = overWaterSound;
                            audioSource.Play();
                        }
                    }                   
                }
            }
        }
    }
}