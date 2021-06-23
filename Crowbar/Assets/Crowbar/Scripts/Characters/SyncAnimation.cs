using UnityEngine;
using Mirror;

namespace Crowbar
{
    public class SyncAnimation : NetworkBehaviour
    {
        public bool isServerObject;

        public Animator animator;
    }
}
