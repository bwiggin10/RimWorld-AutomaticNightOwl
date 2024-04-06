using UnityEngine;
using Verse;

namespace AutomaticNightOwl
{
    public class AutomaticNightOwl_ModSettings : ModSettings
    {
        public bool AddOn_Ideology;
        public bool AddOn_Biotech;

        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard ls = new Listing_Standard
            {
                ColumnWidth = 500f
            };
            ls.Begin(canvas);
            string settingsHeader = Translator.Translate("SettingsHeader");
            string addonIdeology = Translator.Translate("AddOn_Ideology");
            string addonBiotech = Translator.Translate("AddOn_Biotech");

            ls.Label(settingsHeader, -1);
            ls.CheckboxLabeled(addonIdeology, ref AddOn_Ideology);
            ls.CheckboxLabeled(addonBiotech, ref AddOn_Biotech);
            ls.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref AddOn_Ideology, "AddOn_Ideology", true, false);
            Scribe_Values.Look(ref AddOn_Biotech, "AddOn_Biotech", true, false);
        }
    }
}
