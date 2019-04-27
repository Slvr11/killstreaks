using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using static killstreaks.killstreaks;

namespace killstreaks
{
    public class a10 : BaseScript
    {
        public a10()
        {
            GSCFunctions.PreCacheModel("vehicle_a10_warthog");
            GSCFunctions.PreCacheMpAnim("MP_A10_strafing_run");

            GSCFunctions.PreCacheShader("compass_objpoint_a10_friendly");
            GSCFunctions.PreCacheShader("compass_objpoint_a10_enemy");
            GSCFunctions.PreCacheMiniMapIcon("compass_objpoint_a10_friendly");
            GSCFunctions.PreCacheMiniMapIcon("compass_objpoint_a10_enemy");

            Dictionary<string, int> a10_fx = new Dictionary<string, int>();
            a10_fx.Add("bullet_rain", GSCFunctions.LoadFX("misc/warthog_volley_runner"));
            a10_fx.Add("bullet_impacts", GSCFunctions.LoadFX("impacts/warthog_volley_runner"));
            a10_fx.Add("bullet_dust", GSCFunctions.LoadFX("dust/wing_drop_dust"));
            a10_fx.Add("afterburner", GSCFunctions.LoadFX("fire/jet_afterburner"));
            a10_fx.Add("contrail", GSCFunctions.LoadFX("smoke/jet_contrail"));
            a10_fx.Add("wingtip_light_green", GSCFunctions.LoadFX("misc/aircraft_light_wingtip_green"));
            a10_fx.Add("wingtip_light_red", GSCFunctions.LoadFX("misc/aircraft_light_wingtip_red"));
            level.SetField("a10_fx", new Parameter(a10_fx));

            level.SetField("a10MaxHealth", 350);
        }

        public static void tryUseA10Strike(Entity player)
        {

        }
    }
}
