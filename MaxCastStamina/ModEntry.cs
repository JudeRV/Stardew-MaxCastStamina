using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace MaxCastStamina
{
    public class ModEntry : Mod
    {
        float oldCastPower;
        float newCastPower;
        float oldStamina;
        float newStamina;
        bool hasMaxCasted;
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.player.CurrentTool is FishingRod rod)
            {
                newCastPower = rod.castingPower;
                newStamina = Game1.player.stamina;
                if (newCastPower > 0.99f)
                {
                    if (newCastPower == oldCastPower)
                    {
                        hasMaxCasted = true;
                    }
                }
                if (hasMaxCasted)
                {
                    if (newStamina < oldStamina)
                    {
                        Game1.player.stamina = oldStamina;
                        rod.castingPower = 0;
                        hasMaxCasted = false;
                    }
                }
                oldCastPower = newCastPower;
                oldStamina = newStamina;
            }
        }
    }
}
