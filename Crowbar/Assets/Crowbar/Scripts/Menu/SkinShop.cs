using Crowbar.Server;
using System.Collections.Generic;
using UnityEngine;

namespace Crowbar
{
    public class SkinShop : MonoBehaviour
    {
        public static SkinShop skinShop;

        public List<SkinHat> hats;

        private void Start()
        {
            if (Account.IsAuthentication)
                LoadHats(Account.idHats, Account.idCurrentHat);
        }

        public void LoadHats(List<int> idHats, int idCurrentHat)
        {
            AllReset();

            foreach (int id in idHats)
            {
                SkinHat hat = hats.Find(h => h.id == id);

                if (hat != null)
                    hat.SetBuy();
            }

            SkinHat currentHat = hats.Find(h => h.id == idCurrentHat);

            if (currentHat != null)
                currentHat.Select();
        }

        public void AllDeselect()
        {
            hats.ForEach(h => h.Deselect());
        }

        public void AllReset()
        {
            hats.ForEach(h => h.NoBuy());
        }

        private void Awake()
        {
            skinShop = this;
        }
    }
}
