using System.Collections.Generic;
using UnityEngine;
using Crowbar.Server;

namespace Crowbar
{
    public class ManagerAchivments : MonoBehaviour
    {
        public List<Achivment> achivments;

        private void Start()
        {
            if (Account.IsAuthentication)
                SetActiveAchivments(Account.idAchivments);
        }

        public void SetActiveAchivments(List<int> idAchivments)
        {
            foreach (int id in idAchivments)
            {
                Achivment achivment = achivments.Find(a => a.id == id);

                if (achivment != null)
                    achivment.SetActive();
            }
        }
    }
}
