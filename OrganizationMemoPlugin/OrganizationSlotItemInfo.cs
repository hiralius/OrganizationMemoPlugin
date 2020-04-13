﻿using Grabacr07.KanColleWrapper.Models;
using System.Linq;
using System.Windows.Controls;

namespace OrganizationMemoPlugin
{
    public class OrganizationSlotItemInfo
    {
        public SlotItemInfo SlotItemInfo { get; set; }
        public int Slot { get; set; }

        public ToolTip hint => new ToolTip { Content = SlotItemInfo.Name };

        public bool SlotIsAircraft => new[] { SlotItemType.水上偵察機,
                                            SlotItemType.水上爆撃機,
                                            SlotItemType.艦上偵察機,
                                            SlotItemType.艦上戦闘機,
                                            SlotItemType.艦上攻撃機,
                                            SlotItemType.艦上爆撃機}.Contains(SlotItemInfo.Type);
    }
}
