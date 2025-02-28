using Dalamud.Game.ClientState.JobGauge.Types;
using XIVSlothCombo.Core;
using XIVSlothCombo.CustomComboNS;

namespace XIVSlothCombo.Combos.PvE
{
    internal static class GNB
    {
        public const byte JobID = 37;

        public static int MaxCartridges(byte level)
        {
            return level >= Levels.CartridgeCharge3 ? 3 : 2;
        }

        public const uint
            KeenEdge = 16137,
            NoMercy = 16138,
            BrutalShell = 16139,
            DemonSlice = 16141,
            SolidBarrel = 16145,
            GnashingFang = 16146,
            SavageClaw = 16147,
            DemonSlaughter = 16149,
            WickedTalon = 16150,
            SonicBreak = 16153,
            Continuation = 16155,
            JugularRip = 16156,
            AbdomenTear = 16157,
            EyeGouge = 16158,
            BowShock = 16159,
            HeartOfLight = 16160,
            BurstStrike = 16162,
            FatedCircle = 16163,
            Aurora = 16151,
            DoubleDown = 25760,
            DangerZone = 16144,
            BlastingZone = 16165,
            Bloodfest = 16164,
            Hypervelocity = 25759,
            RoughDivide = 16154,
            LightningShot = 16143;

        public static class Buffs
        {
            public const ushort
                NoMercy = 1831,
                Aurora = 1835,
                ReadyToRip = 1842,
                ReadyToTear = 1843,
                ReadyToGouge = 1844,
                ReadyToBlast = 2686;
        }

        public static class Debuffs
        {
            public const ushort
                BowShock = 1838,
                SonicBreak = 1837;
        }

        public static class Levels
        {
            public const byte
                NoMercy = 2,
                BrutalShell = 4,
                LightningShot = 15,
                DangerZone = 18,
                SolidBarrel = 26,
                BurstStrike = 30,
                DemonSlaughter = 40,
                Aurora = 45,
                SonicBreak = 54,
                RoughDivide = 56,
                GnashingFang = 60,
                BowShock = 62,
                Continuation = 70,
                FatedCircle = 72,
                Bloodfest = 76,
                BlastingZone = 80,
                EnhancedContinuation = 86,
                CartridgeCharge3 = 88,
                DoubleDown = 90;
        }
        public static class Config
        {
            public const string
                GNB_RoughDivide_HeldCharges = "GnbKeepRoughDivideCharges";
        }


        internal class GNB_ST_MainCombo : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_MainCombo;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                var gauge = GetJobGauge<GNBGauge>();
                if (actionID == SolidBarrel)
                {
                    var roughDivideChargesRemaining = PluginConfiguration.GetCustomIntValue(Config.GNB_RoughDivide_HeldCharges);
                    var quarterWeave = GetCooldownRemainingTime(actionID) < 1 && GetCooldownRemainingTime(actionID) > 0.6;

                    if (IsEnabled(CustomComboPreset.GNB_RangedUptime) && !InMeleeRange() && LevelChecked(LightningShot) && HasBattleTarget())
                        return LightningShot;

                    if (comboTime > 0)
                    {
                        if (quarterWeave && IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup) && IsEnabled(CustomComboPreset.GNB_ST_NoMercy))
                        {
                            if (LevelChecked(NoMercy) && IsOffCooldown(NoMercy))
                            {
                                if (LevelChecked(BurstStrike) &&
                                   ((gauge.Ammo is 1 && IsOffCooldown(Bloodfest) && CombatEngageDuration().TotalSeconds < 30) || //Opener Conditions
                                   (CombatEngageDuration().Minutes == 2 && GetCooldownRemainingTime(DoubleDown) < 4) || //2 min delay
                                   (CombatEngageDuration().Minutes != 2 && gauge.Ammo == MaxCartridges(level) && GetCooldownRemainingTime(GnashingFang) < 4))) //Regular NMGF
                                    return NoMercy;
                                if (!LevelChecked(BurstStrike)) //no cartridges unlocked
                                    return NoMercy;
                            }
                        }

                        //oGCDs
                        if (CanWeave(actionID))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                            {
                                if (IsEnabled(CustomComboPreset.GNB_ST_Bloodfest) && HasEffect(Buffs.NoMercy) && gauge.Ammo == 0 && IsOffCooldown(Bloodfest) && LevelChecked(Bloodfest) && IsOnCooldown(GnashingFang))
                                    return Bloodfest;

                                //Blasting Zone outside of NM
                                if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && LevelChecked(DangerZone) && IsOffCooldown(DangerZone) && !HasEffect(Buffs.NoMercy))
                                {
                                    if ((IsOnCooldown(GnashingFang) && gauge.AmmoComboStep != 1 && GetCooldownRemainingTime(NoMercy) > 17) || //Post Gnashing Fang
                                        !LevelChecked(GnashingFang)) //Pre Gnashing Fang
                                        return OriginalHook(DangerZone);
                                }

                                //Ensures Early Weave if available
                                if (HasEffect(Buffs.NoMercy) && IsOnCooldown(DoubleDown) && IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && IsOffCooldown(DangerZone))
                                    return OriginalHook(DangerZone);

                                //Continuation
                                if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(Continuation) &&
                                    (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)))
                                    return OriginalHook(Continuation);

                                //60s weaves
                                if (HasEffect(Buffs.NoMercy))
                                {
                                    //Post DD
                                    if (IsOnCooldown(DoubleDown))
                                    {
                                        if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && IsOffCooldown(DangerZone))
                                            return OriginalHook(DangerZone);
                                        if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && IsOffCooldown(BowShock))
                                            return BowShock;
                                    }

                                    //Pre DD
                                    if (IsOnCooldown(SonicBreak) && !LevelChecked(DoubleDown))
                                    {
                                        if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && LevelChecked(BowShock) && IsOffCooldown(BowShock))
                                            return BowShock;
                                        if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && LevelChecked(DangerZone) && IsOffCooldown(DangerZone))
                                            return OriginalHook(DangerZone);
                                    }
                                }
                            }

                            //Rough Divide Feature
                            if (CanWeave(actionID) && LevelChecked(RoughDivide) && IsEnabled(CustomComboPreset.GNB_ST_RoughDivide) && GetRemainingCharges(RoughDivide) > roughDivideChargesRemaining && !IsMoving && !HasEffect(Buffs.ReadyToBlast))
                            {
                                if (IsNotEnabled(CustomComboPreset.GNB_ST_MeleeRoughDivide) ||
                                    (IsEnabled(CustomComboPreset.GNB_ST_MeleeRoughDivide) && GetTargetDistance() <= 1 && HasEffect(Buffs.NoMercy) && IsOnCooldown(OriginalHook(DangerZone)) && IsOnCooldown(BowShock)))
                                    return RoughDivide;
                            }
                        }

                        // 60s window features
                        if ((GetCooldownRemainingTime(NoMercy) > 57 || HasEffect(Buffs.NoMercy)) && IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                        {
                            if (LevelChecked(DoubleDown))
                            {
                                if (IsEnabled(CustomComboPreset.GNB_ST_DoubleDown) && IsOffCooldown(DoubleDown) && gauge.Ammo >= 2 && !HasEffect(Buffs.ReadyToRip) && gauge.AmmoComboStep >= 1)
                                    return DoubleDown;
                                if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && IsOffCooldown(SonicBreak) && IsOnCooldown(DoubleDown))
                                    return SonicBreak;
                            }

                            if (!LevelChecked(DoubleDown))
                            {
                                if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && LevelChecked(SonicBreak) && IsOffCooldown(SonicBreak) && !HasEffect(Buffs.ReadyToRip) && IsOnCooldown(GnashingFang))
                                    return SonicBreak;
                                //sub level 54 functionality
                                if (IsEnabled(CustomComboPreset.GNB_ST_BlastingZone) && LevelChecked(DangerZone) && !LevelChecked(SonicBreak) && IsOffCooldown(DangerZone))
                                    return OriginalHook(DangerZone);
                            }
                        }

                        //Gnashing Fang stuff
                        if (IsEnabled(CustomComboPreset.GNB_ST_Gnashing) && LevelChecked(GnashingFang))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_Starter) && IsOffCooldown(GnashingFang) && gauge.AmmoComboStep == 0 &&
                                ((gauge.Ammo == MaxCartridges(level) && (GetCooldownRemainingTime(NoMercy) > 50 || HasEffect(Buffs.NoMercy)) && CombatEngageDuration().Minutes != 2) || //Regular 60 second GF/NM timing
                                (gauge.Ammo == MaxCartridges(level) && (GetCooldownRemainingTime(NoMercy) > 50 || HasEffect(Buffs.NoMercy)) && CombatEngageDuration().Minutes == 2 && GetCooldownRemainingTime(DoubleDown) <= 1) || //2 min delay
                                (gauge.Ammo == 1 && HasEffect(Buffs.NoMercy) && GetCooldownRemainingTime(DoubleDown) > 50) || //NMDDGF windows/Scuffed windows
                                (gauge.Ammo > 0 && GetCooldownRemainingTime(NoMercy) > 17 && GetCooldownRemainingTime(NoMercy) < 35) || //Regular 30 second window                                                                        
                                (gauge.Ammo == 1 && GetCooldownRemainingTime(NoMercy) > 50 && ((IsOffCooldown(Bloodfest) && LevelChecked(Bloodfest)) || !LevelChecked(Bloodfest))))) //Opener Conditions
                                return GnashingFang;
                            if (gauge.AmmoComboStep is 1 or 2)
                                return OriginalHook(GnashingFang);
                        }

                        if (IsEnabled(CustomComboPreset.GNB_NoMercy_BurstStrike) && IsEnabled(CustomComboPreset.GNB_ST_MainCombo_CooldownsGroup))
                        {
                            if (HasEffect(Buffs.NoMercy) && gauge.AmmoComboStep == 0 && LevelChecked(BurstStrike))
                            {
                                if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                    return Hypervelocity;
                                if (gauge.Ammo != 0 && GetCooldownRemainingTime(GnashingFang) > 4)
                                    return BurstStrike;
                            }

                            //final check if Burst Strike is used right before No Mercy ends
                            if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                        }

                        // Regular 1-2-3 combo with overcap feature
                        if (lastComboMove == KeenEdge && LevelChecked(BrutalShell))
                            return BrutalShell;
                        if (lastComboMove == BrutalShell && LevelChecked(SolidBarrel))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_AmmoOvercap))
                            {
                                if (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))
                                    return Hypervelocity;
                                if (LevelChecked(BurstStrike) && (gauge.Ammo == MaxCartridges(level)))
                                    return BurstStrike;
                            }

                            return SolidBarrel;
                        }
                    }

                    return KeenEdge;
                }

                return actionID;
            }
        }

        internal class GNB_ST_GnashingFangContinuation : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_GnashingFangContinuation;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID == GnashingFang)
                {
                    var gauge = GetJobGauge<GNBGauge>();

                    if (IsOffCooldown(NoMercy) && CanDelayedWeave(actionID) && IsOffCooldown(GnashingFang) && IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_NoMercy))
                        return NoMercy;


                    if (CanWeave(actionID))
                    {
                        if (IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_Cooldowns))
                        {
                            //Blasting Zone outside of NM
                            if (level >= Levels.DangerZone && IsOffCooldown(DangerZone))
                            {
                                if (!HasEffect(Buffs.NoMercy) &&
                                    ((IsOnCooldown(GnashingFang) && gauge.AmmoComboStep != 1 && GetCooldownRemainingTime(GnashingFang) > 20) || //Post Gnashing Fang
                                    (level < Levels.GnashingFang) && IsOnCooldown(NoMercy))) //Pre Gnashing Fang
                                    return OriginalHook(DangerZone);
                            }

                            //60s weaves
                            if (HasEffect(Buffs.NoMercy))
                            {
                                //Post DD
                                if (IsOnCooldown(DoubleDown))
                                {
                                    if (IsOffCooldown(DangerZone))
                                        return OriginalHook(DangerZone);
                                    if (IsOffCooldown(BowShock))
                                        return BowShock;
                                }

                                //Pre DD
                                if (IsOnCooldown(SonicBreak) && level < Levels.DoubleDown)
                                {
                                    if (level >= Levels.BowShock && IsOffCooldown(BowShock))
                                        return BowShock;
                                    if (level >= Levels.DangerZone && IsOffCooldown(DangerZone))
                                        return OriginalHook(DangerZone);
                                }
                            }
                        }

                        if ((HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge)) && level >= Levels.Continuation)
                            return OriginalHook(Continuation);
                    }

                    if (level >= Levels.DoubleDown)
                    {
                        if (IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_DoubleDown) && IsOffCooldown(DoubleDown) && gauge.Ammo >= 2 && !HasEffect(Buffs.ReadyToRip) && gauge.AmmoComboStep >= 1)
                            return DoubleDown;
                        if (IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_Cooldowns) && IsOffCooldown(SonicBreak) && IsOnCooldown(DoubleDown))
                            return SonicBreak;
                    }

                    if (level < Levels.DoubleDown && IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_Cooldowns))
                    {
                        if (level >= Levels.SonicBreak && IsOffCooldown(SonicBreak) && !HasEffect(Buffs.ReadyToRip) && IsOnCooldown(GnashingFang))
                            return SonicBreak;

                        //sub level 54 functionality
                        if (level is >= Levels.DangerZone and < Levels.SonicBreak && IsOffCooldown(DangerZone))
                            return OriginalHook(DangerZone);
                    }

                    if ((gauge.AmmoComboStep == 0 && IsOffCooldown(GnashingFang)) || gauge.AmmoComboStep is 1 or 2)
                        return OriginalHook(GnashingFang);

                    if (IsEnabled(CustomComboPreset.GNB_ST_GnashingFang_Cooldowns))
                    {
                        if ((HasEffect(Buffs.NoMercy) || HasEffect(All.Buffs.Medicated)) && gauge.AmmoComboStep == 0 && level >= Levels.BurstStrike)
                        {
                            if (level >= Levels.EnhancedContinuation && HasEffect(Buffs.ReadyToBlast))
                                return Hypervelocity;
                            if (gauge.Ammo != 0 && IsOnCooldown(GnashingFang))
                                return BurstStrike;
                        }

                        //final check if Burst Strike is used right before No Mercy ends
                        if (level >= Levels.EnhancedContinuation && HasEffect(Buffs.ReadyToBlast))
                            return Hypervelocity;
                    }
                }

                return actionID;
            }
        }

        internal class GNB_ST_BurstStrikeContinuation : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_BurstStrikeContinuation;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID == BurstStrike && level >= Levels.EnhancedContinuation && HasEffect(Buffs.ReadyToBlast))
                    return Hypervelocity;
                return actionID;
            }
        }

        internal class GNB_AoE_MainCombo : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_AoE_MainCombo;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                var gauge = GetJobGauge<GNBGauge>();
                if (actionID == DemonSlaughter)
                {
                    if (InCombat())
                    {
                        if (CanWeave(actionID))
                        {
                            if (IsEnabled(CustomComboPreset.GNB_AoE_NoMercy) && IsOffCooldown(NoMercy) && level >= Levels.NoMercy)
                                return NoMercy;
                            if (IsEnabled(CustomComboPreset.GNB_AoE_BowShock) && level >= Levels.BowShock && IsOffCooldown(BowShock))
                                return BowShock;
                            if (IsEnabled(CustomComboPreset.GNB_AOE_DangerZone) && IsOffCooldown(DangerZone) && level >= Levels.DangerZone)
                                return OriginalHook(DangerZone);
                            if (IsEnabled(CustomComboPreset.GNB_AoE_Bloodfest) && gauge.Ammo == 0 && IsOffCooldown(Bloodfest) && level >= Levels.Bloodfest)
                                return Bloodfest;
                        }

                        if (IsEnabled(CustomComboPreset.GNB_AOE_SonicBreak) && IsOffCooldown(SonicBreak) && level >= Levels.SonicBreak)
                            return SonicBreak;
                        if (IsEnabled(CustomComboPreset.GNB_AoE_DoubleDown) && gauge.Ammo >= 2 && IsOffCooldown(DoubleDown) && level >= Levels.DoubleDown)
                            return DoubleDown;
                        if (IsEnabled(CustomComboPreset.GNB_AoE_Bloodfest) && gauge.Ammo != 0 && GetCooldownRemainingTime(Bloodfest) < 6 && level >= Levels.FatedCircle)
                            return FatedCircle;

                    }

                    if (comboTime > 0 && lastComboMove == DemonSlice && level >= Levels.DemonSlaughter)
                    {
                        return (IsEnabled(CustomComboPreset.GNB_AmmoOvercap) && level >= Levels.FatedCircle && gauge.Ammo == MaxCartridges(level)) ? FatedCircle : DemonSlaughter;
                    }

                    return DemonSlice;
                }

                return actionID;
            }
        }

        internal class GNB_ST_Bloodfest_Overcap : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_Bloodfest_Overcap;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                var gauge = GetJobGauge<GNBGauge>().Ammo;
                return (actionID == BurstStrike && gauge == 0 && level >= Levels.Bloodfest && !HasEffect(Buffs.ReadyToBlast)) ? Bloodfest : actionID;
            }
        }

        internal class GNB_BurstStrike_DoubleDown : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_BurstStrike_DoubleDown;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                var gauge = GetJobGauge<GNBGauge>().Ammo;
                return (actionID is BurstStrike && HasEffect(Buffs.NoMercy) && GetCooldownRemainingTime(DoubleDown) < 2 && gauge >= 2 && LevelChecked(DoubleDown)) ? DoubleDown : actionID;
            }
        }

        internal class GNB_NoMercy_Cooldowns : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_NoMercy_Cooldowns;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID == NoMercy)
                {
                    var gauge = GetJobGauge<GNBGauge>().Ammo;
                    if (IsOnCooldown(NoMercy) && InCombat())
                    {
                        if (IsEnabled(CustomComboPreset.GNB_NoMercy_Cooldowns_DD) && GetCooldownRemainingTime(NoMercy) < 60 && IsOffCooldown(DoubleDown) && gauge >= 2 && LevelChecked(DoubleDown))
                            return DoubleDown;
                        if (IsEnabled(CustomComboPreset.GNB_NoMercy_Cooldowns_SonicBreakBowShock))
                        {
                            if (IsOffCooldown(SonicBreak))
                                return SonicBreak;
                            if (IsOffCooldown(BowShock))
                                return BowShock;
                        }
                    }
                }

                return actionID;
            }
        }

        internal class GNB_AuroraProtection : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_AuroraProtection;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                return (actionID is Aurora && HasEffect(Buffs.Aurora)) ? WAR.NascentFlash: actionID;
            }
        }
    }
}