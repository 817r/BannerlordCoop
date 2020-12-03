using HarmonyLib;
using JetBrains.Annotations;
using Mono.Reflection;
using Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace Coop.Mod.Patch
{
    sealed class CoopCampaign
    {
        public static CoopCampaign Instance { get; } = new CoopCampaign();

        public Campaign CurrentCampaign
        {
            get
            {
                return Campaign.Current;
            }
        }

        public bool IsCampaignActive
        {
            get
            {
                return CurrentCampaign != null;
            }
        }
}
}
