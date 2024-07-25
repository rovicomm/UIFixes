﻿using Comfort.Common;
using EFT.InputSystem;
using HarmonyLib;
using JsonType;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UIFixes;

public static class AimToggleHoldPatches
{
    public static void Enable()
    {
        new AddTwoKeyStatesPatch().Enable();
        new AddOneKeyStatesPatch().Enable();
        new UpdateInputPatch().Enable();

        Settings.ToggleOrHoldAim.SettingChanged += OnSettingChanged;
        Settings.ToggleOrHoldSprint.SettingChanged += OnSettingChanged;
        Settings.ToggleOrHoldTactical.SettingChanged += OnSettingChanged;
        Settings.ToggleOrHoldHeadlight.SettingChanged += OnSettingChanged;
        Settings.ToggleOrHoldGoggles.SettingChanged += OnSettingChanged;
    }

    public class AddTwoKeyStatesPatch : ModulePatch
    {
        private static FieldInfo StateMachineArray;

        protected override MethodBase GetTargetMethod()
        {
            StateMachineArray = AccessTools.Field(typeof(KeyCombination), "keyCombinationState_1");
            return AccessTools.GetDeclaredConstructors(typeof(ToggleKeyCombination)).Single();
        }

        [PatchPostfix]
        public static void Postfix(ToggleKeyCombination __instance, EGameKey gameKey, ECommand disableCommand, KeyCombination.KeyCombinationState[] ___keyCombinationState_1)
        {
            bool useToggleHold = gameKey switch
            {
                EGameKey.Aim => Settings.ToggleOrHoldAim.Value,
                EGameKey.Sprint => Settings.ToggleOrHoldSprint.Value,
                _ => false
            };

            if (!useToggleHold)
            {
                return;
            }

            List<KeyCombination.KeyCombinationState> states = new(___keyCombinationState_1)
            {
                new ToggleHoldIdleState(__instance),
                new ToggleHoldClickOrHoldState(__instance),
                new ToggleHoldHoldState(__instance, disableCommand)
            };

            StateMachineArray.SetValue(__instance, states.ToArray());
        }
    }

    public class AddOneKeyStatesPatch : ModulePatch
    {
        private static FieldInfo StateMachineArray;

        protected override MethodBase GetTargetMethod()
        {
            StateMachineArray = AccessTools.Field(typeof(KeyCombination), "keyCombinationState_1");
            return AccessTools.GetDeclaredConstructors(typeof(KeyCombination)).Single();
        }

        [PatchPostfix]
        public static void Postfix(ToggleKeyCombination __instance, EGameKey gameKey, ECommand command, KeyCombination.KeyCombinationState[] ___keyCombinationState_1)
        {
            bool useToggleHold = gameKey switch
            {
                EGameKey.Tactical => Settings.ToggleOrHoldTactical.Value,
                EGameKey.ToggleGoggles => Settings.ToggleOrHoldGoggles.Value,
                EGameKey.ToggleHeadLight => Settings.ToggleOrHoldHeadlight.Value,
                _ => false
            };

            if (!useToggleHold)
            {
                return;
            }

            List<KeyCombination.KeyCombinationState> states = new(___keyCombinationState_1)
            {
                new ToggleHoldIdleState(__instance),
                new ToggleHoldClickOrHoldState(__instance),
                new ToggleHoldHoldState(__instance, command)
            };

            StateMachineArray.SetValue(__instance, states.ToArray());
        }
    }

    public class UpdateInputPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(KeyCombination), nameof(KeyCombination.UpdateInput));
        }

        [PatchPostfix]
        public static void Postfix(KeyCombination __instance)
        {
            bool useToggleHold = __instance.GameKey switch
            {
                EGameKey.Aim => Settings.ToggleOrHoldAim.Value,
                EGameKey.Tactical => Settings.ToggleOrHoldTactical.Value,
                EGameKey.ToggleGoggles => Settings.ToggleOrHoldGoggles.Value,
                EGameKey.ToggleHeadLight => Settings.ToggleOrHoldHeadlight.Value,
                EGameKey.Sprint => Settings.ToggleOrHoldSprint.Value,
                _ => false
            };

            if (useToggleHold)
            {
                __instance.method_0((KeyCombination.EKeyState)ToggleHoldState.Idle);
            }
        }
    }

    private static void OnSettingChanged(object sender, EventArgs args)
    {
        // Will "save" control settings, running GClass1911.UpdateInput, which will set (or unset) toggle/hold behavior
        Singleton<SharedGameSettingsClass>.Instance.Control.Controller.method_3();
    }
}
