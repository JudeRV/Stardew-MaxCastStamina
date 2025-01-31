using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace MaxCastStamina
{
    public class ModEntry : Mod
    {
        ModConfig? config;
        int staminaToRestore = 0;

        float oldCastPower;
        float newCastPower;

        float oldStamina;
        float newStamina;

        public override void Entry(IModHelper helper)
        {       
            config = helper.ReadConfig<ModConfig>();
            if (config is null)
            {
                Monitor.Log("Couldn't load config -- setting StaminaToRestore to default", LogLevel.Warn);
            }
            else
            {
                staminaToRestore = config.StaminaToRestore;
            }
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                return;
            }

            configMenu.Register(
                mod: ModManifest,
                reset: () => config = new ModConfig(),
                save: () => Helper.WriteConfig(config)
                );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Amount of stamina to restore",
                tooltip: () => "The amount of stamina that should be restored upon a max cast. When set to 0 (default), this will restore the vanilla amount of stamina to stay at the same amount. Change this if you have other mods that modify stamina for this event, or for fun :D",
                getValue: () => staminaToRestore,
                setValue: value => config.StaminaToRestore = staminaToRestore = value
                );
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {   
            // Only check if the player is holding a fishing rod
            if (Game1.player.CurrentTool is FishingRod rod)
            {
                newCastPower = rod.castingPower;
                newStamina = Game1.player.stamina;
                // Check for max cast
                if (newCastPower > 0.99f)
                {
                    // Make sure the player didn't just go past the max cast without stopping on it
                    if (newCastPower == oldCastPower)
                    {
                        if (newStamina < oldStamina)
                        {
                            // Handle edge case where casting would've made the player exhausted
                            if (newStamina < 0)
                            {
                                Game1.player.exhausted.Value = false;
                                Game1.addHUDMessage(HUDMessage.ForCornerTextbox("But luckily your fishing skills saved you!"));
                            }
                            // Reset casting power for next time and give player their stamina back
                            rod.castingPower = 0;
                            // Restore default amount if not specified in config
                            float restoreAmount = staminaToRestore == 0 ? oldStamina - newStamina : staminaToRestore;
                            if (newStamina + restoreAmount > Game1.player.maxStamina.Value)
                            {
                                restoreAmount = Game1.player.maxStamina.Value - newStamina;
                            }

                            Game1.player.stamina += restoreAmount;
                            Monitor.Log($"Restoring {Math.Round(restoreAmount)} stamina for a max cast. Nice shot!", LogLevel.Trace);
                        }
                    }
                }
                // Update stamina and casting power values for next tick
                oldCastPower = newCastPower;
                oldStamina = newStamina;
            }
        }
    }
    public sealed class ModConfig
    {
        public int StaminaToRestore { get; set; } = 0;
    }
}