using System;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Game
{
	[Obsolete(error: true, message: "Game.ModLoader 已弃用，请使用 Game.ModInterfaces.IModLoader")]
	public abstract class ModLoader
	{
	}
}
