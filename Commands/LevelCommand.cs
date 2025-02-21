using System;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using WeaponFusion.Items;

namespace WeaponFusion.Commands
{
	public class LevelCommand : ModCommand
	{
		#region Fields

		private const string NsLocalization = "Mods.WeaponFusion.Command";

        public override CommandType Type => CommandType.Chat;
        public override string Command => "wfusion";
		public override string Description => Language.GetTextValue($"{NsLocalization}.OverrideDescription");
		public override string Usage => "/wfusion {set;add} [number]";

		#endregion

		#region Methods

		public override void Action(CommandCaller caller, string input, string[] args)
        {
			try
            {
                if (!WeaponFusionConfig.Current.EnabledCommands
                    && Netplay.Clients.FirstOrDefault(e => e?.Id == Main.CurrentPlayer.whoAmI)?.Socket.GetRemoteAddress().IsLocalHost() is true)
                {
                    caller.Reply(Language.GetTextValue($"{NsLocalization}.ReplyUnauthorize"));
                    return;
                }
            }
			catch (Exception)
			{
				caller.Reply("Error");
			}

			int argsTotal = 2;
			if (args.Length != argsTotal)
			{
				caller.Reply(Language.GetTextValue($"{NsLocalization}.ReplyTooManyArguments", args.Length, argsTotal, Usage));
				return;
			}

			if (!int.TryParse(args[1], out int nbLevel))
			{
				caller.Reply(Language.GetTextValue($"{NsLocalization}.ReplyParseError", args[1], Usage));
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
					caller.Reply(Language.GetTextValue($"{NsLocalization}.ReplyWrongArgument", args[0], Usage));
					break;
			}
		}

		private static void ShowSuccess(CommandCaller caller)
		{
			caller.Reply(Language.GetTextValue($"{NsLocalization}.ReplySuccess"));
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

		#endregion
	}
}
