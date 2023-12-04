using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Linq;


namespace AutomaticNightOwl
{
    public class AutomaticNightOwl_Mod : Mod
    {
        public static AutomaticNightOwl_ModSettings settings;

        public AutomaticNightOwl_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<AutomaticNightOwl_ModSettings>();
        }
        public override string SettingsCategory()
        {
            return "Automatic Night Owl";
        }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            settings.DoWindowContents(canvas);
        }

    }

    [StaticConstructorOnStartup]
    public static class AutomaticNightOwl
    {
        private static void AutoNightOwl(Pawn pawn)
        {

            // Base
            if (pawn?.story?.traits?.HasTrait(TraitDefOf.NightOwl) == true &&
                    pawn.timetable != null &&
                    !WorldComp.PawnsWithNightOwl.Contains(pawn))
            {
                pawn.timetable.times = new List<TimeAssignmentDef>(GenDate.HoursPerDay);
                for (int i = 0; i < GenDate.HoursPerDay; i++)
                {
                    TimeAssignmentDef setNightOwlHours = i >= 11 && i <= 18 ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything;
                    pawn.timetable.times.Add(setNightOwlHours);
                }
                WorldComp.PawnsWithNightOwl.Add(pawn);
            }

            if (AutomaticNightOwl_Mod.settings.Additions == true)
            {
                // Additions - Dirtmoles
                if (ModsConfig.BiotechActive)
                {
                    if (pawn?.genes?.Xenotype == XenotypeDefOf.Dirtmole &&
                            pawn.timetable != null &&
                            !WorldComp.PawnsWithNightOwl.Contains(pawn))
                    {
                        pawn.timetable.times = new List<TimeAssignmentDef>(GenDate.HoursPerDay);
                        for (int i = 0; i < GenDate.HoursPerDay; i++)
                        {
                            TimeAssignmentDef setNightOwlHours = i >= 11 && i <= 18 ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything;
                            pawn.timetable.times.Add(setNightOwlHours);
                        }
                        WorldComp.PawnsWithNightOwl.Add(pawn);
                    }
                }

                // Additions - Vampires
                if (pawn.health.hediffSet.HasHediff(HediffDefOf.Vampirism))
                {
                    if (pawn.timetable != null &&
                            !WorldComp.PawnsWithNightOwl.Contains(pawn))
                    {
                        pawn.timetable.times = new List<TimeAssignmentDef>(GenDate.HoursPerDay);
                        for (int i = 0; i < GenDate.HoursPerDay; i++)
                        {
                            TimeAssignmentDef setNightOwlHours = i >= 11 && i <= 18 ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything;
                            pawn.timetable.times.Add(setNightOwlHours);
                        }
                        WorldComp.PawnsWithNightOwl.Add(pawn);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
        public static class Patch_Thing_SpawnSetup
        {
            // Patching for initial pawns
            public static void Postfix(Thing __instance)
            {
                if (__instance is Pawn p && p.Faction?.IsPlayer == true && p.def?.race?.Humanlike == true)
                {
                    AutoNightOwl(p);
                }
            }
        }

        [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), nameof(InteractionWorker_RecruitAttempt.DoRecruit), new Type[] { typeof(Pawn), typeof(Pawn), typeof(string), typeof(string), typeof(bool), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal })]
        public static class Patch_InteractionWorker_RecruitAttempt
        {
            // Patching for recruited prisoners
            public static void Postfix(Pawn recruiter, Pawn recruitee)
            {
                if (recruitee is Pawn p && p.Faction?.IsPlayer == true && p.def?.race?.Humanlike == true)
                {
                    AutoNightOwl(p);
                }
            }
        }

        [HarmonyPatch(typeof(InteractionWorker_EnslaveAttempt), nameof(InteractionWorker_EnslaveAttempt.Interacted))]
        public static class Patch_InteractionWorker_EnslaveAttempt
        {
            // Patching for enslaved prisoners
            public static void Postfix(Pawn initiator, Pawn recipient)
            {
                if (recipient is Pawn p && p.GuestStatus == GuestStatus.Slave)
                {
                    AutoNightOwl(p);
                }
            }
        }

        [DefOf]
        // Base
        public static class TraitDefOf
        {
            public static TraitDef NightOwl;
        }
        // Additions
        public static class MemeDefOf
        {
            public static MemeDef Tunneler;
        }

        public static class XenotypeDefOf
        {
            public static XenotypeDef Dirtmole;
        }

        public static class HediffDefOf
        {
            public static HediffDef Vampirism;
        }

    }




    class WorldComp : WorldComponent
    {
        // Using a HashSet for quick lookup
        public static HashSet<Pawn> PawnsWithNightOwl = new HashSet<Pawn>();
        // I've found it easier to have a null list for use when exposing data
        // This shouldn't be needed but mods will remove Pawns from the game completely (RuntimeGC for instance)
        // and HashSet will fail if more than one null value is added.
        private List<Pawn> usedForExposingData = null;

        public WorldComp(World w) : base(w)
        {
            // Make sure the static HashSet is cleared whenever a game is created or loaded.
            PawnsWithNightOwl.Clear();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                // When saving, populate the list
                usedForExposingData = new List<Pawn>(PawnsWithNightOwl);
            }

            Scribe_Collections.Look(ref usedForExposingData, "pawnsWithNightOwl", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // When loading, clear the HashSet then populate it with the loaded data
                PawnsWithNightOwl.Clear();
                foreach (var v in usedForExposingData)
                {
                    // Remove any null records
                    if (v != null)
                    {
                        PawnsWithNightOwl.Add(v);
                    }
                }
            }

            if (Scribe.mode == LoadSaveMode.Saving ||
                Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // Add hints to the garbage collector that this memory can be collected
                usedForExposingData?.Clear();
                usedForExposingData = null;
            }
        }
    }
}