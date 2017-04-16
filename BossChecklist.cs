﻿using System;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.UI;
using Terraria.DataStructures;
using BossChecklist.UI;
using Microsoft.Xna.Framework;
using Terraria.UI.Chat;
using System.Linq;
using Terraria.GameContent.UI.Chat;

namespace BossChecklist
{
	public class BossChecklist : Mod
	{
		static internal BossChecklist instance;
		internal static ModHotKey ToggleChecklistHotKey;
		internal static UserInterface bossChecklistInterface;
		internal BossChecklistUI bossChecklistUI;
		private double pressedToggleChecklistHotKeyTime;

		// Mods that have been added manually
		internal bool vanillaLoaded = true;
		internal bool thoriumLoaded;

		// Mods with bosses that could use suppory, but need fixes in the tmod files.
		//internal bool sacredToolsLoaded;
		//internal bool crystiliumLoaded;

		// Mods that have been added natively, no longer need code here.
		//internal bool tremorLoaded;
		//internal bool bluemagicLoaded;
		//internal bool joostLoaded;
		//internal bool calamityLoaded;
		//internal bool pumpkingLoaded;

		public BossChecklist()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
			};
		}

		public override void Load()
		{
			instance = this;
			ToggleChecklistHotKey = RegisterHotKey("Toggle Boss Checklist", "P");
			if (!Main.dedServ)
			{
				bossChecklistUI = new BossChecklistUI();
				bossChecklistUI.Activate();
				bossChecklistInterface = new UserInterface();
				bossChecklistInterface.SetState(bossChecklistUI);
			}
		}

		int lastSeenScreenWidth;
		int lastSeenScreenHeight;
		public override void ModifyInterfaceLayers(List<MethodSequenceListItem> layers)
		{
			int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (MouseTextIndex != -1)
			{
				layers.Insert(MouseTextIndex, new MethodSequenceListItem(
					"BossChecklist: Boss Checklist",
					delegate
					{
						if (BossChecklistUI.visible)
						{
							if (lastSeenScreenWidth != Main.screenWidth || lastSeenScreenHeight != Main.screenHeight)
							{
								bossChecklistInterface.Recalculate();
								lastSeenScreenWidth = Main.screenWidth;
								lastSeenScreenHeight = Main.screenHeight;
							}

							bossChecklistInterface.Update(Main._drawInterfaceGameTime);
							bossChecklistUI.Draw(Main.spriteBatch);

							if (BossChecklistUI.hoverText != "")
							{
								float x = Main.fontMouseText.MeasureString(BossChecklistUI.hoverText).X;
								Vector2 vector = new Vector2((float)Main.mouseX, (float)Main.mouseY) + new Vector2(16f, 16f);
								if (vector.Y > (float)(Main.screenHeight - 30))
								{
									vector.Y = (float)(Main.screenHeight - 30);
								}
								if (vector.X > (float)(Main.screenWidth - x - 30))
								{
									vector.X = (float)(Main.screenWidth - x - 30);
								}
								//Utils.DrawBorderStringFourWay(Main.spriteBatch, Main.fontMouseText, BossChecklistUI.hoverText,
								//	vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
								//	Utils.draw

								//ItemTagHandler.GenerateTag(item)
								int hoveredSnippet = -1;
								TextSnippet[] array = ChatManager.ParseMessage(BossChecklistUI.hoverText, Color.White);
								ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, array,
									vector, 0f, Vector2.Zero, Vector2.One, out hoveredSnippet/*, -1f, 2f*/);

								if (hoveredSnippet > -1)
								{
									array[hoveredSnippet].OnHover();
									//if (Main.mouseLeft && Main.mouseLeftRelease)
									//{
									//	array[hoveredSnippet].OnClick();
									//}
								}
							}
						}
						return true;
					},
					null)
				);
			}
		}

		// An alternative approach to the weak reference approach is to do the following in YOUR mod in PostSetupContent
		//Mod bossChecklist = ModLoader.GetMod("BossChecklist");
		//if (bossChecklist != null)
		//{
		//	bossChecklist.Call("AddBoss", "My BossesName", 2.3f, (Func<bool>)(() => MyMod.MyModWorld.downedMyBoss));
		//}
		public override void PostSetupContent()
		{
			try
			{
				thoriumLoaded = ModLoader.GetMod("ThoriumMod") != null;
				//bluemagicLoaded = ModLoader.GetMod("Bluemagic") != null;
				//calamityLoaded = ModLoader.GetMod("CalamityMod") != null;
				//joostLoaded = ModLoader.GetMod("JoostMod") != null;
				//crystiliumLoaded = ModLoader.GetMod("CrystiliumMod") != null;
				//sacredToolsLoaded = ModLoader.GetMod("SacredTools") != null;
				//tremorLoaded = ModLoader.GetMod("Tremor") != null;
				//pumpkingLoaded = ModLoader.GetMod("Pumpking") != null;
			}
			catch (Exception e)
			{
				ErrorLogger.Log("BossChecklist PostSetupContent Error: " + e.StackTrace + e.Message);
			}
		}

		// Messages:
		// string:"AddBoss" - string:Bossname - float:bossvalue - Func<bool>:BossDowned
		public override object Call(params object[] args)
		{
			try
			{
				string message = args[0] as string;
				if (message == "AddBoss")
				{
					string bossname = args[1] as string;
					float bossValue = Convert.ToSingle(args[2]);
					Func<bool> bossDowned = args[3] as Func<bool>;
					if (!Main.dedServ)
						bossChecklistUI.AddBoss(bossname, bossValue, bossDowned);
					return "Success";
				}
				else if (message == "AddBossWithInfo")
				{
					string bossname = args[1] as string;
					float bossValue = Convert.ToSingle(args[2]);
					Func<bool> bossDowned = args[3] as Func<bool>;
					string bossInfo = args[4] as string;
					if (!Main.dedServ)
						bossChecklistUI.AddBoss(bossname, bossValue, bossDowned, bossInfo);
					return "Success";
				}
				else
				{
					ErrorLogger.Log("BossChecklist Call Error: Unknown Message: " + message);
				}
			}
			catch (Exception e)
			{
				ErrorLogger.Log("BossChecklist Call Error: " + e.StackTrace + e.Message);
			}
			return "Failure";
		}
	}
}

