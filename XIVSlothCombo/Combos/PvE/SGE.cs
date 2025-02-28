using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using System.Collections.Generic;
using XIVSlothCombo.Core;
using XIVSlothCombo.CustomComboNS;
using XIVSlothCombo.CustomComboNS.Functions;
using XIVSlothCombo.Services;

namespace XIVSlothCombo.Combos.PvE
{
    internal static class SGE
    {
        internal const byte JobID = 40;

        // Actions
        internal const uint
            // Heals and Shields
            Diagnosis = 24284,
            Prognosis = 24286,
            Physis = 24288,
            Druochole = 24296,
            Kerachole = 24298,
            Ixochole = 24299,
            Pepsis = 24301,
            Physis2 = 24302,
            Taurochole = 24303,
            Haima = 24305,
            Panhaima = 24311,
            Holos = 24310,
            EukrasianDiagnosis = 24291,
            EukrasianPrognosis = 24292,
            Egeiro = 24287,

            // DPS
            Dosis = 24283,
            Dosis2 = 24306,
            Dosis3 = 24312,
            EukrasianDosis = 24293,
            EukrasianDosis2 = 24308,
            EukrasianDosis3 = 24314,
            Phlegma = 24289,
            Phlegma2 = 24307,
            Phlegma3 = 24313,
            Dyskrasia = 24297,
            Dyskrasia2 = 24315,
            Toxikon = 24304,
            Pneuma = 24318,

            // Buffs
            Soteria = 24294,
            Zoe = 24300,
            Krasis = 24317,

            // Other
            Kardia = 24285,
            Eukrasia = 24290,
            Rhizomata = 24309;

        // Action Groups
        internal static readonly List<uint>
            AddersgallList = new() { Taurochole, Druochole, Ixochole, Kerachole },
            PhlegmaList = new() { Phlegma, Phlegma2, Phlegma3 };

        // Action Buffs
        internal static class Buffs
        {
            internal const ushort
                Kardia = 2604,
                Kardion = 2605,
                Eukrasia = 2606,
                EukrasianDiagnosis = 2607,
                EukrasianPrognosis = 2609;
        }

        internal static class Debuffs
        {
            internal const ushort
                EukrasianDosis = 2614,
                EukrasianDosis2 = 2615,
                EukrasianDosis3 = 2616;
        }

        // Debuff Pairs of Actions and Debuff
        internal static readonly Dictionary<uint, ushort>
            DosisList = new()
            {
                { Dosis,  Debuffs.EukrasianDosis  },
                { Dosis2, Debuffs.EukrasianDosis2 },
                { Dosis3, Debuffs.EukrasianDosis3 }
            };

        // Sage Gauge & Extensions
        private static SGEGauge Gauge => CustomComboFunctions.GetJobGauge<SGEGauge>();
        private static bool HasAddersgall(this SGEGauge gauge) => gauge.Addersgall > 0;
        private static bool HasAddersting(this SGEGauge gauge) => gauge.Addersting > 0;

        internal static class Config
        {
            #region DPS
            internal static bool SGE_ST_Dosis_AltMode => CustomComboFunctions.GetIntOptionAsBool(nameof(SGE_ST_Dosis_AltMode));
            internal static bool SGE_ST_Dosis_Toxikon => CustomComboFunctions.GetIntOptionAsBool(nameof(SGE_ST_Dosis_Toxikon));
            internal static int SGE_ST_Dosis_EDosisHPPer => CustomComboFunctions.GetOptionValue(nameof(SGE_ST_Dosis_EDosisHPPer));
            internal static bool SGE_ST_Dosis_EDosis_Adv => PluginConfiguration.GetCustomBoolValue(nameof(SGE_ST_Dosis_EDosis_Adv));
            internal static float SGE_ST_Dosis_EDosisThreshold => CustomComboFunctions.GetOptionFloat(nameof(SGE_ST_Dosis_EDosisThreshold));
            internal static int SGE_ST_Dosis_Lucid => CustomComboFunctions.GetOptionValue(nameof(SGE_ST_Dosis_Lucid));
            #endregion

            #region Healing
            internal static int SGE_ST_Heal_Zoe => CustomComboFunctions.GetOptionValue(nameof(SGE_ST_Heal_Zoe));
            internal static int SGE_ST_Heal_Haima => CustomComboFunctions.GetOptionValue(nameof(SGE_ST_Heal_Haima));
            internal static int SGE_ST_Heal_Krasis => CustomComboFunctions.GetOptionValue(nameof(SGE_ST_Heal_Krasis));
            internal static int SGE_ST_Heal_Pepsis => CustomComboFunctions.GetOptionValue(nameof(SGE_ST_Heal_Pepsis));
            internal static int SGE_ST_Heal_Soteria => CustomComboFunctions.GetOptionValue(nameof(SGE_ST_Heal_Soteria));
            internal static int SGE_ST_Heal_Diagnosis => CustomComboFunctions.GetOptionValue(nameof(SGE_ST_Heal_Diagnosis));
            internal static int SGE_ST_Heal_Druochole => CustomComboFunctions.GetOptionValue(nameof(SGE_ST_Heal_Druochole));
            internal static int SGE_ST_Heal_Taurochole => CustomComboFunctions.GetOptionValue(nameof(SGE_ST_Heal_Taurochole));
            #endregion

            internal static int SGE_AoE_Phlegma_Lucid => CustomComboFunctions.GetOptionValue(nameof(SGE_AoE_Phlegma_Lucid));
            internal static int SGE_Eukrasia_Mode => CustomComboFunctions.GetOptionValue(nameof(SGE_Eukrasia_Mode));
        }

        /*
         * SGE_Kardia
         * Soteria becomes Kardia when Kardia's Buff is not active or Soteria is on cooldown.
         */
        internal class SGE_Kardia : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_Kardia;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
                => actionID is Soteria && (!HasEffect(Buffs.Kardia) || IsOnCooldown(Soteria)) ? Kardia : actionID;
        }

        /*
         * SGE_Rhizo
         * Replaces all Addersgal using Abilities (Taurochole/Druochole/Ixochole/Kerachole) with Rhizomata if out of Addersgall stacks
         * (Scholar speak: Replaces all Aetherflow abilities with Aetherflow when out)
         */
        internal class SGE_Rhizo : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_Rhizo;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
                => AddersgallList.Contains(actionID) && ActionReady(Rhizomata) && !Gauge.HasAddersgall() ? Rhizomata : actionID;
        }

        /*
         * Druo/Tauro
         * Druochole Upgrade to Taurochole (like a trait upgrade)
         * Replaces Druocole with Taurochole when Taurochole is available
         * (As of 6.0) Taurochole (single target massive insta heal w/ cooldown), Druochole (Single target insta heal)
         */
        internal class SGE_DruoTauro : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_DruoTauro;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
                => actionID is Druochole && ActionReady(Taurochole) ? Taurochole : actionID;
        }

        /*
         * SGE_ZoePneuma (Zoe to Pneuma Combo)
         * Places Zoe on top of Pneuma when both are available.
         */
        internal class SGE_ZoePneuma : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_ZoePneuma;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
                => actionID is Pneuma && ActionReady(Pneuma) && IsOffCooldown(Zoe) ? Zoe : actionID;
        }

        /*
         * SGE_AoE_Phlegma (Phlegma AoE Feature)
         * Replaces Zero Charges/Stacks of Phlegma with various options
         * Lucid Dreaming, Toxikon, or Dyskrasia
         */
        internal class SGE_AoE_Phlegma : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_AoE_Phlegma;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (PhlegmaList.Contains(actionID))
                {
                    bool NoPhlegmaToxikon = IsEnabled(CustomComboPreset.SGE_AoE_Phlegma_NoPhlegmaToxikon);
                    bool OutOfRangeToxikon = IsEnabled(CustomComboPreset.SGE_AoE_Phlegma_OutOfRangeToxikon);
                    bool NoPhlegmaDyskrasia = IsEnabled(CustomComboPreset.SGE_AoE_Phlegma_NoPhlegmaDyskrasia);
                    bool NoTargetDyskrasia = IsEnabled(CustomComboPreset.SGE_AoE_Phlegma_NoTargetDyskrasia);
                    uint phlegma = OriginalHook(Phlegma); //Level appropriate Phlegma

                    // Lucid Dreaming
                    if (IsEnabled(CustomComboPreset.SGE_AoE_Phlegma_Lucid) &&
                        ActionReady(All.LucidDreaming) && CanSpellWeave(Dosis) &&
                        LocalPlayer.CurrentMp <= Config.SGE_AoE_Phlegma_Lucid)
                        return All.LucidDreaming;

                    //Toxikon
                    if (LevelChecked(Toxikon) && HasBattleTarget() && Gauge.HasAddersting())
                    {
                        if ((NoPhlegmaToxikon && !HasCharges(phlegma)) ||
                            (OutOfRangeToxikon && !InActionRange(phlegma)))
                            return OriginalHook(Toxikon);
                    }

                    //Dyskrasia
                    if (LevelChecked(Dyskrasia))
                    {
                        if ((NoPhlegmaDyskrasia && !HasCharges(phlegma)) ||
                            (NoTargetDyskrasia && CurrentTarget is null))
                            return OriginalHook(Dyskrasia);
                    }
                }

                return actionID;
            }
        }

        /*
         * SGE_ST_Dosis (Single Target Dosis Combo)
         * Currently Replaces Dosis with Eukrasia when the debuff on the target is < 3 seconds or not existing
         * Kardia reminder, Lucid Dreaming, & Toxikon optional
         */
        internal class SGE_ST_Dosis : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_ST_Dosis;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                bool AlternateMode = Config.SGE_ST_Dosis_AltMode;
                if ((!AlternateMode && DosisList.ContainsKey(actionID)) ||
                    (AlternateMode && actionID is Dosis2))
                {
                    // Kardia Reminder
                    if (IsEnabled(CustomComboPreset.SGE_ST_Dosis_Kardia) && LevelChecked(Kardia) &&
                        FindEffect(Buffs.Kardia) is null)
                        return Kardia;

                    // Lucid Dreaming
                    if (IsEnabled(CustomComboPreset.SGE_ST_Dosis_Lucid) &&
                        ActionReady(All.LucidDreaming) && CanSpellWeave(actionID) &&
                        LocalPlayer.CurrentMp <= Config.SGE_ST_Dosis_Lucid)
                        return All.LucidDreaming;

                    if (HasBattleTarget() && (!HasEffect(Buffs.Eukrasia)))
                    // Buff check Above. Without it, Toxikon and any future option will interfere in the Eukrasia->Eukrasia Dosis combo
                    {
                        // Eukrasian Dosis.
                        // If we're too low level to use Eukrasia, we can stop here.
                        if (IsEnabled(CustomComboPreset.SGE_ST_Dosis_EDosis) && LevelChecked(Eukrasia) && InCombat())
                        {
                            // Grab current Dosis via OriginalHook, grab it's fellow debuff ID from Dictionary, then check for the debuff
                            // Using TryGetValue due to edge case where the actionID would be read as Eukrasian Dosis instead of Dosis
                            // EDosis will show for half a second if the buff is removed manually or some other act of God
                            if (DosisList.TryGetValue(OriginalHook(actionID), out ushort dotDebuffID))
                            {
                                Status? dotDebuff = FindTargetEffect(dotDebuffID);
                                float refreshtimer = Config.SGE_ST_Dosis_EDosis_Adv ? Config.SGE_ST_Dosis_EDosisThreshold : 3;

                                if ((dotDebuff is null || dotDebuff.RemainingTime <= refreshtimer) &&
                                    GetTargetHPPercent() > Config.SGE_ST_Dosis_EDosisHPPer)
                                    return Eukrasia;
                            }
                        }

                        // Phlegma
                        if (IsEnabled(CustomComboPreset.SGE_ST_Dosis_Phlegma) && InCombat())
                        {
                            uint phlegma = OriginalHook(Phlegma);
                            if (InActionRange(phlegma) && ActionReady(phlegma)) return phlegma;
                        }

                        // Toxikon
                        bool alwaysShowToxikon = Config.SGE_ST_Dosis_Toxikon;    // False for moving only, True for Show All Times
                        if (IsEnabled(CustomComboPreset.SGE_ST_Dosis_Toxikon) && InCombat() &&
                            LevelChecked(Toxikon) &&
                            ((!alwaysShowToxikon && IsMoving) || alwaysShowToxikon) &&
                            Gauge.HasAddersting())
                            return OriginalHook(Toxikon);
                    }
                }
                return actionID;
            }
        }

        /*
         * SGE_Raise (Swiftcast Raise)
         * Swiftcast becomes Egeiro when on cooldown
         */
        internal class SGE_Raise : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_Raise;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
                    => actionID is All.Swiftcast && IsOnCooldown(All.Swiftcast) ? Egeiro : actionID;
        }

        /* 
         * SGE_Eukrasia (Eukrasia combo)
         * Normally after Eukrasia is used and updates the abilities, it becomes disabled
         * This will "combo" the action to user selected action
         */
        internal class SGE_Eukrasia : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_Eukrasia;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is Eukrasia && HasEffect(Buffs.Eukrasia))
                {
                    switch (Config.SGE_Eukrasia_Mode)
                    {
                        case 0: return OriginalHook(Dosis);
                        case 1: return OriginalHook(Diagnosis);
                        case 2: return OriginalHook(Prognosis);
                        default: break;
                    }
                }

                return actionID;
            }
        }

        /* 
         * SGE_ST_Heal (Diagnosis Single Target Heal)
         * Replaces Diagnosis with various Single Target healing options, 
         * Pseudo priority set by various custom user percentages
         */
        internal class SGE_ST_Heal : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_ST_Heal;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is Diagnosis)
                {
                    if (HasEffect(Buffs.Eukrasia))
                        return EukrasianDiagnosis;

                    // Set Target. Soft -> Hard -> Self priority, matching normal in-game behavior
                    GameObject? healTarget = null;
                    GameObject? softTarget = Service.TargetManager.SoftTarget;
                    if (HasFriendlyTarget(softTarget)) healTarget = softTarget;
                    if (healTarget is null && HasFriendlyTarget(CurrentTarget)) healTarget = CurrentTarget;
                    if (healTarget is null) healTarget = LocalPlayer;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Druochole) && ActionReady(Druochole) &&
                        Gauge.HasAddersgall() &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Druochole)
                        return Druochole;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Taurochole) && ActionReady(Taurochole) &&
                        Gauge.HasAddersgall() &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Taurochole)
                        return Taurochole;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Rhizomata) && ActionReady(Rhizomata) &&
                        !Gauge.HasAddersgall())
                        return Rhizomata;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Kardia) && LevelChecked(Kardia) &&
                        FindEffect(Buffs.Kardia) is null &&
                        FindEffect(Buffs.Kardion, healTarget, LocalPlayer?.ObjectId) is null)
                        return Kardia;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Soteria) && ActionReady(Soteria) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Soteria)
                        return Soteria;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Zoe) && ActionReady(Zoe) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Zoe)
                        return Zoe;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Krasis) && ActionReady(Krasis) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Krasis)
                        return Krasis;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Pepsis) && ActionReady(Pepsis) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Pepsis &&
                        FindEffect(Buffs.EukrasianDiagnosis, healTarget, LocalPlayer?.ObjectId) is not null)
                        return Pepsis;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Haima) && ActionReady(Haima) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Haima)
                        return Haima;

                    if (IsEnabled(CustomComboPreset.SGE_ST_Heal_Diagnosis) && LevelChecked(Eukrasia) &&
                        GetTargetHPPercent(healTarget) <= Config.SGE_ST_Heal_Diagnosis &&
                        (IsEnabled(CustomComboPreset.SGE_ST_Heal_Diagnosis_IgnoreShield) ||
                         FindEffect(Buffs.EukrasianDiagnosis, healTarget, LocalPlayer?.ObjectId) is null))
                        return Eukrasia;
                }

                return actionID;
            }
        }

        /* 
         * SGE_AoE_Heal (Prognosis AoE Heal)
         * Replaces Prognosis with various AoE healing options, 
         * Pseudo priority set by various custom user percentages
         */
        internal class SGE_AoE_Heal : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SGE_AoE_Heal;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is Prognosis)
                {
                    if (HasEffect(Buffs.Eukrasia))
                        return EukrasianPrognosis;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Rhizomata) && ActionReady(Rhizomata) &&
                        !Gauge.HasAddersgall())
                        return Rhizomata;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Kerachole) && ActionReady(Kerachole) &&
                        Gauge.HasAddersgall())
                        return Kerachole;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Ixochole) && ActionReady(Ixochole) &&
                        Gauge.HasAddersgall())
                        return Ixochole;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Physis))
                    {
                        uint physis = OriginalHook(Physis);
                        if (ActionReady(physis)) return physis;
                    }

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Holos) && ActionReady(Holos))
                        return Holos;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Panhaima) && ActionReady(Panhaima))
                        return Panhaima;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_Pepsis) && ActionReady(Pepsis) &&
                        FindEffect(Buffs.EukrasianPrognosis) is not null)
                        return Pepsis;

                    if (IsEnabled(CustomComboPreset.SGE_AoE_Heal_EPrognosis) && LevelChecked(Eukrasia) &&
                        (IsEnabled(CustomComboPreset.SGE_AoE_Heal_EPrognosis_IgnoreShield) ||
                         FindEffect(Buffs.EukrasianPrognosis) is null))
                        return Eukrasia;
                }

                return actionID;
            }
        }
    }
}