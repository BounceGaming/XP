﻿using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using XPSystem.API;
using Badge = XPSystem.API.Features.Badge;

namespace XPSystem.Patches
{
    using LiteNetLib4Mirror.Open.Nat;

    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetText))]
    public class RankChangePatch
    {
        internal static bool Prefix(ServerRoles __instance, string i)
        {
            var ply = Player.Get(__instance._hub);
            var log = ply.GetLog();
            Badge badge = Main.Instance.Config.DNTBadge;
            if(!ply.DoNotTrack)
            {
                foreach (var kvp in Main.Instance.Config.LevelsBadge.OrderBy(kvp => kvp.Key))
                {
                    if (log.LVL > kvp.Key && Main.Instance.Config.LevelsBadge.OrderByDescending(kvp2 => kvp2.Key).ElementAt(0).Key != kvp.Key)
                        continue;
                    badge = kvp.Value;
                    break;
                }
            }

            string newValue;
            if (!i.ContainsIgnoreCase("\n"))
            {
                newValue = Main.Instance.Config.BadgeStructure
                    .Replace("%lvl%", log.LVL.ToString())
                    .Replace("%badge%", badge.Name)
                    .Replace("%oldbadge%", string.IsNullOrWhiteSpace(i) ? ply.Group?.BadgeText : i);
                newValue += "\n";
            }
            else
            {
                newValue = i;
            }
            

            Log.Debug(i);
            Log.Debug(newValue);
            Log.Debug($"Override color : {Main.Instance.Config.OverrideColor}");
            Log.Debug("Group color : " + ply.Group?.BadgeColor);
            Log.Debug("Badge color : " + badge.Color);
            if (NetworkServer.active)
                __instance.Network_myText = newValue;
            __instance.MyText = newValue;

            if (Main.Instance.Config.OverrideColor)
            {
                ply.RankColor = badge.Color;
                return false;
            }
            ServerRoles.NamedColor namedColor = __instance.NamedColors.FirstOrDefault(row => row.Name == __instance.MyColor);
            if (namedColor == null)
                return false;
            __instance.CurrentColor = namedColor;
            
            return false;

        }
    }
}
