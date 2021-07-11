using UnityEngine;

namespace Crowbar
{
    public class DebugMenu : MonoBehaviour
    {
        public GameObject[] debugMenu;

        private void Update()
        {
            if (Input.GetKey(KeyCode.C) && Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.W))
                foreach (GameObject go in debugMenu)
                    go.SetActive(true);
        }
    }
}
