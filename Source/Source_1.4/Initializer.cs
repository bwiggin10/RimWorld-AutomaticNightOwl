using RimWorld;
using System.Reflection;
using Verse;
using HarmonyLib;

namespace AutomaticNightOwl
{
    [StaticConstructorOnStartup]
    public static class Initializer
    {
        static Initializer()
        {
            Harmony harmony = new Harmony("AutomaticNightOwl_Ben");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
