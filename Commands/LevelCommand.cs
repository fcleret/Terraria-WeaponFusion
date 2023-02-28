using Terraria;
using Terraria.ModLoader;
using WeaponFusion.Items;

namespace WeaponFusion.Commands
{
    public class LevelCommand : ModCommand
    {
        public override string Command => "wf";

        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length != 3)
            {
                ShowUsage(caller);
                return;
            }

            switch (args[1])
            {
                case "add":
                    if (caller.Player != null)
                        AddLevel(caller, caller.Player, int.Parse(args[2]));
                    else
                        AddLevel(caller, Main.LocalPlayer, int.Parse(args[2]));
                    break;

                case "set":
                    if (caller.Player != null)
                        SetLevel(caller, caller.Player, int.Parse(args[2]));
                    else
                        SetLevel(caller, Main.LocalPlayer, int.Parse(args[2]));
                    break;

                default:
                    ShowUsage(caller);
                    break;
            }
        }

        private static void ShowUsage(CommandCaller caller)
        {
            caller.Reply("Usage: /wf level {set;add} [value]");
        }

        private static void ShowSuccess(CommandCaller caller)
        {
            caller.Reply("New level has been applied.");
        }

        private static void AddLevel(CommandCaller caller, Player player, int value)
        {
            Item item = player.inventory[player.selectedItem];
            ModGlobalItem.SetLevel(item, ModGlobalItem.GetLevel(item) + value);
            ShowSuccess(caller);
        }

        private static void SetLevel(CommandCaller caller, Player player, int value)
        {
            ModGlobalItem.SetLevel(player.inventory[player.selectedItem], value);
            ShowSuccess(caller);
        }
    }
}
