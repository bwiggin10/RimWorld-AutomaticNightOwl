using UnityEngine;
using Verse;

namespace AutomaticNightOwl
{
    public class AutomaticNightOwl_ModSettings : ModSettings
    {
        public bool Additions = false;

        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard ls = new Listing_Standard
            {
                ColumnWidth = 500f
            };
            ls.Begin(canvas);
            string text_cn = Translator.Translate("Additions");
            ls.CheckboxLabeled(text_cn, ref Additions, Translator.Translate("AdditionsSetting"));
            ls.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Additions, "Additions", false, false);
        }
    }
}
