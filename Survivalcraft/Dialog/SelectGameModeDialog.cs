using Engine;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Game
{
	public class SelectGameModeDialog : ListSelectionDialog
	{
		public SelectGameModeDialog(string title, bool allowAdventure, Action<GameMode> selectionHandler)
			: base(title, GetAllowedGameModes(allowAdventure), 140f, delegate (object item)
			{
				GameMode gameMode = (GameMode)item;
				XElement node = ContentManager.Get<XElement>("Widgets/SelectGameModeItem");
				ContainerWidget obj = (ContainerWidget)LoadWidget(null, node, null);
				obj.Children.Find<LabelWidget>("SelectGameModeItem.Name").Text = LanguageControl.Get("GameMode", gameMode.ToString());
				obj.Children.Find<LabelWidget>("SelectGameModeItem.Description").Text = StringsManager.GetString("GameMode." + gameMode.ToString() + ".Description");
				return obj;
			}, delegate (object item)
			{
				selectionHandler((GameMode)item);
			})
		{
			base.ContentSize = new Vector2(750f, 420f);
		}

        public static IEnumerable<GameMode> GetAllowedGameModes(bool allowAdventure)
		{
			if (!allowAdventure)
			{
				return new GameMode[5]
				{
					GameMode.Creative,
					GameMode.Survival,
					GameMode.Challenging,
					GameMode.Harmless,
					GameMode.Cruel
				};
			}
			return new GameMode[6]
			{
				GameMode.Creative,
				GameMode.Survival,
				GameMode.Challenging,
				GameMode.Harmless,
				GameMode.Cruel,
				GameMode.Adventure
			};
		}
	}
}
