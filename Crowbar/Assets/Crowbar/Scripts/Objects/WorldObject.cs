﻿using Mirror;
using UnityEngine;

namespace Crowbar
{
    /// <summary>
    /// Main object for network
    /// </summary>
    [RequireComponent(typeof(SyncPosition))]
    public class WorldObject : NetworkBehaviour
    {
        [Header("World object properties")]
        [Tooltip("Can object to be child")]
        public bool canParenting = true;
    }
}
