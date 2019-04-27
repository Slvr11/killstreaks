using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using InfinityScript;
using static InfinityScript.GSCFunctions;
using static killstreaks.killstreaks;

namespace killstreaks
{
    public class teamammorefill : BaseScript
    {
        public static Action<Entity> teamAmmoRefillGiveFunc;
        public static Func<Entity, bool> teamAmmoRefillUseFunc;
        public teamammorefill()
        {
            teamAmmoRefillGiveFunc = initTeamAmmoRefill;
            teamAmmoRefillUseFunc = tryUseTeamAmmoRefill;
        }

        private static void initTeamAmmoRefill(Entity player)
        {

        }

        private static bool tryUseTeamAmmoRefill(Entity player)
        {
            bool result = giveTeamAmmoRefill(player);

            return result;
        }

        private static bool giveTeamAmmoRefill(Entity player)
        {
            if (isTeamBased)
            {
                foreach (Entity teammate in Players)
                {
                    if (teammate.SessionTeam == player.SessionTeam)
                    {
                        refillAmmo(teammate, true);
                    }
                }
            }
            else
            {
                refillAmmo(player, true);
            }

            teamPlayerCardSplash("used_team_ammo_refill", player, true);

            return true;
        }
    }
}
