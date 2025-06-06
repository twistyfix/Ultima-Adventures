using System;
using Server.Items;

namespace Server.Engines.Craft
{
	public class DefCartography : CraftSystem
	{
		public override SkillName MainSkill
		{
			get	{ return SkillName.Cartography; }
		}

		public override int GumpTitleNumber
		{
			get { return 1044008; } // <CENTER>CARTOGRAPHY MENU</CENTER>
		}

		public override double GetChanceAtMin( CraftItem item )
		{
			return 0.0; // 0%
		}

		private static CraftSystem m_CraftSystem;

		public static CraftSystem CraftSystem
		{
			get
			{
				if ( m_CraftSystem == null )
					m_CraftSystem = new DefCartography();

				return m_CraftSystem;
			}
		}

		private DefCartography() : base( 1, 1, 1.25 )// base( 1, 1, 3.0 )
		{
		}

		public override int CanCraft( Mobile from, BaseTool tool, Type itemType )
		{
			if( tool == null || tool.Deleted || tool.UsesRemaining < 0 )
				return 1044038; // You have worn out your tool!
			else if ( !BaseTool.CheckAccessible( tool, from ) )
				return 1044263; // The tool must be on your person to use.

			return 0;
		}

		public override void PlayCraftEffect( Mobile from )
		{
			from.PlaySound( 0x249 );
		}

		public override int PlayEndingEffect( Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item )
		{
			if ( toolBroken )
				from.SendLocalizedMessage( 1044038 ); // You have worn out your tool

			if ( failed )
			{
				if ( lostMaterial )
					return 1044043; // You failed to create the item, and some of your materials are lost.
				else
					return 1044157; // You failed to create the item, but no materials were lost.
			}
			else
			{
				if ( quality == 0 )
					return 502785; // You were barely able to make this item.  It's quality is below average.
				else if ( makersMark && quality == 2 )
					return 1044156; // You create an exceptional quality item and affix your maker's mark.
				else if ( quality == 2 )
					return 1044155; // You create an exceptional quality item.
				else				
					return 1044154; // You create the item.
			}
		}

		public override void InitCraftList()
		{
			// Blank Scrolls
			int index;
			index = AddCraft( typeof( BlankScroll ), 1044294, 1044377, 40.0, 70.0, typeof( BarkFragment ), 1073477, 1, 1073478 );
			
			// Batch begin
			index = AddCraft( typeof( BlankScroll ), 1044294, "batch of blank scrolls", 40.0, 70.0, typeof( BarkFragment ), 1073477, 1, 1073478 );
			SetUseAllRes( index, true );
			
			// WIZARD CHANGED IT BECAUSE THERE ARE NO TOWNS...REALLY
			AddCraft( typeof( BlankMap ), 1044448, "blank map", 00.0, 40.0, typeof( BlankScroll ), 1044377, 1, 1044378 );
			AddCraft( typeof( LocalMap ), 1044448, "small map", 30.0, 70.0, typeof( BlankMap ), 1044449, 1, 1044450 );
			AddCraft( typeof(  CityMap ), 1044448, "large map", 60.0, 90.0, typeof( BlankMap ), 1044449, 1, 1044450 );
			AddCraft( typeof( SeaChart ), 1044448, "sea chart", 65.0, 95.0, typeof( BlankMap ), 1044449, 1, 1044450 );
			AddCraft( typeof( WorldMap ), 1044448, "huge map", 80.0, 99.5, typeof( BlankMap ), 1044449, 1, 1044450 );
			AddCraft( typeof( MapWorld ), 1044448, "world map", 89.5, 110.5, typeof( BlankMap ), 1044449, 1, 1044450 );

		}
	}
}