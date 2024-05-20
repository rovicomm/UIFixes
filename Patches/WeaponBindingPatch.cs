﻿using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.InputSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace UIFixes
{
    public class WeaponBindingPatch : ModulePatch
    {
        private static Type ControlSettingsClass;
        private static MethodInfo GetKeyNameMethod;

        protected override MethodBase GetTargetMethod()
        {
            ControlSettingsClass = PatchConstants.EftTypes.Single(x => x.GetMethod("GetBoundItemNames") != null); // GClass960
            GetKeyNameMethod = AccessTools.Method(ControlSettingsClass, "GetKeyName");
            return AccessTools.Method(ControlSettingsClass, "GetBoundItemNames");
        }

        [PatchPostfix]
        public static void Postfix(object __instance, EBoundItem boundItem, ref string __result)
        {
            switch(boundItem)
            {
                case EBoundItem.Item1:
                    __result = GetKeyNameMethod.Invoke(__instance, [EGameKey.SecondaryWeapon]) as string;
                    break;
                case EBoundItem.Item2:
                    __result = GetKeyNameMethod.Invoke(__instance, [EGameKey.PrimaryWeaponFirst]) as string;
                    break;
                case EBoundItem.Item3:
                    __result = GetKeyNameMethod.Invoke(__instance, [EGameKey.PrimaryWeaponSecond]) as string;
                    break;
            }
        }
    }
}
