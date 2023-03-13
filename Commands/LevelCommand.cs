using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using WeaponFusion.Items;

namespace WeaponFusion.Commands
{
	public class LevelCommand : ModCommand
	{
		private const string nsLocalization = "Mods.WeaponFusion.Command";

		public override string Command => "wfusion";
		public override string Description => Language.GetTextValue($"{nsLocalization}.OverrideDescription");
		public override string Usage => "/wfusion {set;add} [number]";
		public override CommandType Type => CommandType.Chat;

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			// Main.CurrentPlayer ?
			if (Main.ServerSideCharacter && !WeaponFusionConfig.Current.EnabledCommands) {
				caller.Reply(Language.GetTextValue($"{nsLocalization}.ReplyUnauthorize"));
				return;
			}

			int argsTotal = 2;
			if (args.Length != argsTotal)
			{
				caller.Reply(Language.GetTextValue($"{nsLocalization}.ReplyTooManyArguments", args.Length, argsTotal, Usage));
				return;
			}

			if (!int.TryParse(args[1], out int nbLevel))
			{
				caller.Reply(Language.GetTextValue($"{nsLocalization}.ReplyParseError", args[1], Usage));
				return;
			}

			switch (args[0])
			{
				case "add":
					if (caller.Player != null)
						AddLevel(caller, caller.Player, nbLevel);
					else
						AddLevel(caller, Main.LocalPlayer, nbLevel);
					break;

				case "set":
					if (caller.Player != null)
						SetLevel(caller, caller.Player, nbLevel);
					else
						SetLevel(caller, Main.LocalPlayer, nbLevel);
					break;

				default:
					caller.Reply(Language.GetTextValue($"{nsLocalization}.ReplyWrongArgument", args[0], Usage));
					break;
			}
		}

		private static void ShowSuccess(CommandCaller caller)
		{
			caller.Reply(Language.GetTextValue($"{nsLocalization}.ReplySuccess"));
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
