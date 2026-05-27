using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class StoreSellHandler : MonoBehaviour
    {
        private IUserService userService;
        private List<InventorySlotUI> activeSlots = new List<InventorySlotUI>();

        private void Start()
        {
            userService = DIContainer.Resolve<IUserService>();
        }

        public void EnableSelling(List<InventorySlotUI> slots)
        {
            DisableSelling();
            activeSlots = slots;

            foreach (var slot in activeSlots)
            {
                if (slot != null)
                {
                    slot.onRightClick += HandleRightClick;
                }
            }
        }

        public void DisableSelling()
        {
            foreach (var slot in activeSlots)
            {
                if (slot != null)
                {
                    slot.onRightClick -= HandleRightClick;
                }
            }
            activeSlots.Clear();
        }

        private void HandleRightClick(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return;
            userService.SellFish(instanceId);
        }

        private void OnDestroy()
        {
            DisableSelling();
        }
    }
}