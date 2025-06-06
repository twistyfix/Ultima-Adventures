using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Targeting;
using Server.Gumps;
using Server.Mobiles;
using Server.Gumps;
using Server.Misc;
using Server.Regions;

namespace Server.Engines.Harvest
{
	public abstract class HarvestSystem
	{
		private List<HarvestDefinition> m_Definitions;

		public List<HarvestDefinition> Definitions { get { return m_Definitions; } }

		public HarvestSystem()
		{
			m_Definitions = new List<HarvestDefinition>();
		}

		public virtual bool CheckTool( Mobile from, Item tool )
		{
			bool wornOut = ( tool == null || tool.Deleted || (tool is IUsesRemaining && ((IUsesRemaining)tool).UsesRemaining <= 0) );

			if ( wornOut )
			{
				if (from is PlayerMobile && ((PlayerMobile)from).GetFlag( PlayerFlag.IsAutomated ) )
					AdventuresAutomation.StopAction((PlayerMobile)from);
				
				from.SendLocalizedMessage( 1044038 ); // You have worn out your tool!
			}

			return !wornOut;
		}

		public virtual bool CheckHarvest( Mobile from, Item tool )
		{
			return CheckTool( from, tool );
		}

		public virtual bool CheckHarvest( Mobile from, Item tool, HarvestDefinition def, object toHarvest )
		{
			return CheckTool( from, tool );
		}

		public virtual bool CheckRange( Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed )
		{
			bool inRange = ( from.Map == map && from.InRange( loc, def.MaxRange ) );

			if ( !inRange )
				def.SendMessageTo( from, timed ? def.TimedOutOfRangeMessage : def.OutOfRangeMessage );

			return inRange;
		}

		public virtual bool CheckResources( Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed )
		{
			if (from == null || map == null || tool == null || loc == Point3D.Zero)
												{
													from.SendMessage("2There was an issue, screenshot this.");
													if (map == null)
														from.SendMessage("map is null");
													if (tool == null)
														from.SendMessage("tool is null");
													if (loc == Point3D.Zero)
														from.SendMessage("loc is zero");
													
													return false;
												}

			HarvestBank bank = def.GetBank( map, loc.X, loc.Y );
			bool available = ( bank != null && bank.Current >= def.ConsumedPerHarvest );

			if ( !available && from is PlayerMobile && ((PlayerMobile)from).GetFlag( PlayerFlag.IsAutomated ) )
			{
				PlayerMobile pm = (PlayerMobile)from;
				if ( AdventuresAutomation.TaskTarget.Contains((PlayerMobile)from)) // ran out of resources on the current target area
				{
					from.SendMessage("Looking for new harvest location, that location is now empty.");

					AdventuresAutomation.TaskTarget.Remove((PlayerMobile)from); // this means next doaction itll try another spot.
					return false;
				}
			}
			else if (!available)
				def.SendMessageTo( from, timed ? def.DoubleHarvestMessage : def.NoResourcesMessage );

			return available;
		}

		public virtual void OnBadHarvestTarget( Mobile from, Item tool, object toHarvest )
		{
		}

		public virtual object GetLock( Mobile from, Item tool, HarvestDefinition def, object toHarvest )
		{
			/* Here we prevent multiple harvesting.
			 * 
			 * Some options:
			 *  - 'return tool;' : This will allow the player to harvest more than once concurrently, but only if they use multiple tools. This seems to be as OSI.
			 *  - 'return GetType();' : This will disallow multiple harvesting of the same type. That is, we couldn't mine more than once concurrently, but we could be both mining and lumberjacking.
			 *  - 'return typeof( HarvestSystem );' : This will completely restrict concurrent harvesting.
			 */

		    return typeof( HarvestSystem );
		}

		public virtual void OnConcurrentHarvest( Mobile from, Item tool, HarvestDefinition def, object toHarvest )
		{
		}

		public virtual void OnHarvestStarted( Mobile from, Item tool, HarvestDefinition def, object toHarvest )
		{
		}

		public virtual bool BeginHarvesting( Mobile from, Item tool )
		{
			if ( !CheckHarvest( from, tool ) )
				return false;

			if (from is PlayerMobile && ((PlayerMobile)from).GetFlag( PlayerFlag.IsAutomated ))
			{
				AdventuresAutomation.DoAction((PlayerMobile)from);
				return true;
			}
			else
				from.Target = new HarvestTarget( tool, this );


			return true;
		}

        public static void SendHarvestTarget( Mobile from, object o )
        {
            if (!(o is object[]))
                return;
            object[] arglist = (object[])o;
 
            if (arglist.Length != 2)
                return;
 
            if (!(arglist[0] is Item))
                return;
 
            if (!(arglist[1] is HarvestSystem))
                return;
               
            from.Target = new HarvestTarget((Item)arglist[0], (HarvestSystem)arglist[1] );
        }

		public virtual void FinishHarvesting( Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked )
		{
			bool automated = false;
			if (from is PlayerMobile && ((PlayerMobile)from).GetFlag( PlayerFlag.IsAutomated ) )
				automated = true;

			from.EndAction( locked );

			if ( !CheckHarvest( from, tool ) )
			{
				if (automated)
					AdventuresAutomation.StopAction((PlayerMobile)from);

				return;
			}

			int tileID;
			Map map;
			Point3D loc;

			if ( !GetHarvestDetails( from, tool, toHarvest, out tileID, out map, out loc ) )
			{
				OnBadHarvestTarget( from, tool, toHarvest );
				return;
			}
			else if ( !def.Validate( tileID ) )
			{
				OnBadHarvestTarget( from, tool, toHarvest );
				return;
			}
			
			if ( !CheckRange( from, tool, def, map, loc, true ) )
				return;
			else if ( !CheckResources( from, tool, def, map, loc, true ) )
				return;
			else if ( !CheckHarvest( from, tool, def, toHarvest ) )
				return;

			if ( SpecialHarvest( from, tool, def, map, loc ) )
				return;

			HarvestBank bank = def.GetBank( map, loc.X, loc.Y );

			if ( bank == null )
				return;

			HarvestVein vein = bank.Vein;

			if ( vein != null )
				vein = MutateVein( from, tool, def, bank, toHarvest, vein );

			if ( vein == null )
				return;

			HarvestResource primary = vein.PrimaryResource;
			HarvestResource fallback = vein.FallbackResource;
			HarvestResource resource = MutateResource( from, tool, def, map, loc, vein, primary, fallback );

			double skillBase = from.Skills[def.Skill].Base;
			double skillValue = from.Skills[def.Skill].Value;

			Type type = null;

			if ( skillValue >= resource.ReqSkill && from.CheckSkill( def.Skill, resource.MinSkill, resource.MaxSkill ) )
			{
				type = GetResourceType( from, tool, def, map, loc, resource );

				if ( type != null )
					type = MutateType( type, from, tool, def, map, loc, resource );

				if ( type != null )
				{
					Item item = Construct( type, from );

					if ( item == null )
					{
						type = null;
					}
					else
					{
						if ( item.Stackable )
						{
							Region reg = Region.Find( from.Location, from.Map );

							int amount = def.ConsumedPerHarvest;
							if (from is PlayerMobile)
							{
								if ( ((PlayerMobile)from).Avatar )
									amount = (int)Math.Ceiling( amount * 1.15);
							}
							int feluccaAmount = def.ConsumedPerFeluccaHarvest;

							int racialAmount = (int)Math.Ceiling( amount * 1.1 );
							int feluccaRacialAmount = (int)Math.Ceiling( feluccaAmount * 1.1 );

							bool eligableForRacialBonus = ( def.RaceBonus && from.Race == Race.Human );
							bool inFelucca = (map == Map.Tokuno); // WIZARD

							if ( item is BlankScroll )
							{
							    amount = Utility.RandomMinMax( amount, (int)(amount+(from.Skills[SkillName.Inscribe].Value/10)) );
							    from.SendMessage( "You find some blank scrolls.");
							}


							if( Worlds.GetMyWorld( from.Map, from.Location, from.X, from.Y ) == "the Isles of Dread" && bank.Current >= feluccaAmount )
								item.Amount = feluccaAmount;
							else if ( reg.IsPartOf( "the Mines of Morinia" ) && item is BaseOre && Utility.RandomMinMax( 1, 3 ) > 1 )
								item.Amount = 2 * amount;
							else
								item.Amount = amount;

							bool FindSpecialOre = false;
								if ( ( item is AgapiteOre || item is VeriteOre || item is ValoriteOre ) && Utility.RandomMinMax( 1, 2 ) == 1 )
									FindSpecialOre = true;

							bool FindSpecialGranite = false;
								if ( ( item is AgapiteGranite || item is VeriteGranite || item is ValoriteGranite ) && Utility.RandomMinMax( 1, 2 ) == 1 )
									FindSpecialGranite = true;

							bool FindGhostLog = false;
								if ( (item is WalnutLog) || (item is RosewoodLog) || (item is PineLog) || (item is OakLog) )
									FindGhostLog = true;

							bool FindBlackLog = false;
								if ( (item is AshLog) || (item is CherryLog) || (item is GoldenOakLog) || (item is HickoryLog) || (item is MahoganyLog) )
									FindBlackLog = true;

							bool FindToughLog = false;
								if ( !(item is Log) )
									FindToughLog = true;

							if ( Worlds.IsExploringSeaAreas( from ) && item is BaseLog )
							{
								int driftWood = item.Amount;
								item.Delete();
								item = new DriftwoodLog( driftWood );
								from.SendMessage( "You chop some driftwood logs.");
							}
							else if ( Worlds.GetMyWorld( from.Map, from.Location, from.X, from.Y ) == "the Underworld" && FindSpecialOre && item is BaseOre && from.Map == Map.TerMur )
							{
								int xormiteOre = item.Amount;
								item.Delete();
								item = new XormiteOre( xormiteOre );
								from.SendMessage( "You dig up some xormite ore.");
							}
							else if ( Worlds.GetMyWorld( from.Map, from.Location, from.X, from.Y ) == "the Underworld" && FindSpecialOre && item is BaseOre )
							{
								int mithrilOre = item.Amount;
								item.Delete();
								item = new MithrilOre( mithrilOre );
								from.SendMessage( "You dig up some mithril ore.");
							}
							else if ( Worlds.GetMyWorld( from.Map, from.Location, from.X, from.Y ) == "the Serpent Island" && FindSpecialOre && item is BaseOre )
							{
								int obsidianOre = item.Amount;
								item.Delete();
								item = new ObsidianOre( obsidianOre );
								from.SendMessage( "You dig up some obsidian ore.");
							}
							else if ( Worlds.IsExploringSeaAreas( from ) && FindSpecialOre && item is BaseOre )
							{
								int nepturiteOre = item.Amount;
								item.Delete();
								item = new NepturiteOre( nepturiteOre );
								from.SendMessage( "You dig up some nepturite ore.");
							}
							else if ( Worlds.GetMyWorld( from.Map, from.Location, from.X, from.Y ) == "the Underworld" && FindSpecialGranite && item is BaseGranite && from.Map == Map.TerMur )
							{
								int xormiteGranite = item.Amount;
								item.Delete();
								item = new XormiteGranite( xormiteGranite );
								from.SendMessage( "You dig up xormite granite.");
							}
							else if ( Worlds.GetMyWorld( from.Map, from.Location, from.X, from.Y ) == "the Underworld" && FindSpecialGranite && item is BaseGranite )
							{
								int mithrilGranite = item.Amount;
								item.Delete();
								item = new MithrilGranite( mithrilGranite );
								from.SendMessage( "You dig up mithril granite.");
							}
							else if ( Worlds.GetMyWorld( from.Map, from.Location, from.X, from.Y ) == "the Serpent Island" && FindSpecialGranite && item is BaseGranite )
							{
								int obsidianGranite = item.Amount;
								item.Delete();
								item = new ObsidianGranite( obsidianGranite );
								from.SendMessage( "You dig up obsidian granite.");
							}
							else if ( Worlds.IsExploringSeaAreas( from ) && FindSpecialGranite && item is BaseGranite )
							{
								int nepturiteGranite = item.Amount;
								item.Delete();
								item = new NepturiteGranite( nepturiteGranite );
								from.SendMessage( "You dig up nepturite granite.");
							}
							else if ( reg.IsPartOf( typeof( NecromancerRegion ) ) && FindBlackLog && item is BaseLog )
							{
								int blackLog = item.Amount;
								item.Delete();
								item = new EbonyLog( blackLog );
								from.SendMessage( "You chop some ebony logs.");
							}
							else if ( reg.IsPartOf( typeof( NecromancerRegion ) ) && FindGhostLog && item is BaseLog )
							{
								int ghostLog = item.Amount;
								item.Delete();
								item = new GhostLog( ghostLog );
								from.SendMessage( "You chop some ghost logs.");
							}
							else if ( Worlds.GetMyWorld( from.Map, from.Location, from.X, from.Y ) == "the Underworld" && FindToughLog && item is BaseLog )
							{
								int toughLog = item.Amount;
								item.Delete();
								item = new PetrifiedLog( toughLog );
								from.SendMessage( "You chop some petrified logs.");
							}
							else if ( ( reg.IsPartOf( "Shipwreck Grotto" ) || reg.IsPartOf( "Barnacled Cavern" ) ) && FindToughLog && item is BaseLog )
							{
								int driftWood = item.Amount;
								item.Delete();
								item = new DriftwoodLog( driftWood );
								from.SendMessage( "You chop some driftwood logs.");
							}
							else if ( ( reg.IsPartOf( "Shipwreck Grotto" ) || reg.IsPartOf( "Barnacled Cavern" ) || reg.IsPartOf( "Savage Sea Docks" ) || reg.IsPartOf( "Serpent Sail Docks" ) || reg.IsPartOf( "Anchor Rock Docks" ) || reg.IsPartOf( "Kraken Reef Docks" ) || reg.IsPartOf( "the Forgotten Lighthouse" ) ) && FindSpecialGranite && item is BaseGranite )
							{
								int nepturiteGranite = item.Amount;
								item.Delete();
								item = new NepturiteGranite( nepturiteGranite );
								from.SendMessage( "You dig up nepturite granite.");
							}
							else if ( ( reg.IsPartOf( "Shipwreck Grotto" ) || reg.IsPartOf( "Barnacled Cavern" ) || reg.IsPartOf( "Savage Sea Docks" ) || reg.IsPartOf( "Serpent Sail Docks" ) || reg.IsPartOf( "Anchor Rock Docks" ) || reg.IsPartOf( "Kraken Reef Docks" ) || reg.IsPartOf( "the Forgotten Lighthouse" ) ) && FindSpecialOre && item is BaseOre )
							{
								int nepturiteOre = item.Amount;
								item.Delete();
								item = new NepturiteOre( nepturiteOre );
								from.SendMessage( "You dig up some nepturite ore.");
							}

							else if ( item is IronOre ){ from.SendMessage( "You dig up some ore."); }
							else if ( item is DullCopperOre ){ from.SendMessage( "You dig up some dull copper ore."); }
							else if ( item is ShadowIronOre ){ from.SendMessage( "You dig up some shadow iron ore."); }
							else if ( item is CopperOre ){ from.SendMessage( "You dig up some copper ore."); }
							else if ( item is BronzeOre ){ from.SendMessage( "You dig up some bronze ore."); }
							else if ( item is GoldOre ){ from.SendMessage( "You dig up some golden ore."); }
							else if ( item is AgapiteOre ){ from.SendMessage( "You dig up some agapite ore."); }
							else if ( item is VeriteOre ){ from.SendMessage( "You dig up some verite ore."); }
							else if ( item is ValoriteOre ){ from.SendMessage( "You dig up some valorite ore."); }
							else if ( item is DwarvenOre ){ from.SendMessage( "You dig up some dwarven ore."); }

							else if ( item is Granite ){ from.SendMessage( "You dig up granite."); }
							else if ( item is DullCopperGranite ){ from.SendMessage( "You dig up dull copper granite."); }
							else if ( item is ShadowIronGranite ){ from.SendMessage( "You dig up shadow iron granite."); }
							else if ( item is CopperGranite ){ from.SendMessage( "You dig up copper granite."); }
							else if ( item is BronzeGranite ){ from.SendMessage( "You dig up bronze granite."); }
							else if ( item is GoldGranite ){ from.SendMessage( "You dig up golden granite."); }
							else if ( item is AgapiteGranite ){ from.SendMessage( "You dig up agapite granite."); }
							else if ( item is VeriteGranite ){ from.SendMessage( "You dig up verite granite."); }
							else if ( item is ValoriteGranite ){ from.SendMessage( "You dig up valorite granite."); }
							else if ( item is DwarvenGranite ){ from.SendMessage( "You dig up dwarven granite."); }

							else if ( item is Log ){ from.SendMessage( "You chop some logs."); }
							else if ( item is AshLog ){ from.SendMessage( "You chop some ash logs."); }
							else if ( item is CherryLog ){ from.SendMessage( "You chop some cherry logs."); }
							else if ( item is EbonyLog ){ from.SendMessage( "You chop some ebony logs."); }
							else if ( item is GoldenOakLog ){ from.SendMessage( "You chop some golden oak logs."); }
							else if ( item is HickoryLog ){ from.SendMessage( "You chop some hickory logs."); }
							else if ( item is MahoganyLog ){ from.SendMessage( "You chop some mahogany logs."); }
							else if ( item is OakLog ){ from.SendMessage( "You chop some oak logs."); }
							else if ( item is PineLog ){ from.SendMessage( "You chop some pine logs."); }
							else if ( item is RosewoodLog ){ from.SendMessage( "You chop some rosewood logs."); }
							else if ( item is WalnutLog ){ from.SendMessage( "You chop some walnut logs."); }
							else if ( item is ElvenLog ){ from.SendMessage( "You chop some elven logs."); }

							if ( Worlds.GetMyWorld( from.Map, from.Location, from.X, from.Y ) == "the Savaged Empire" && from.Skills[SkillName.Mining].Value > Utility.RandomMinMax( 1, 500 ) )
							{
								Container pack = from.Backpack;
								DugUpCoal coal = new DugUpCoal( Utility.RandomMinMax( 1, 2 ) );
								from.AddToBackpack ( coal );
								from.SendMessage( "You dig up some coal.");
							}
							else if ( Worlds.GetMyWorld( from.Map, from.Location, from.X, from.Y ) == "the Island of Umber Veil" && from.Skills[SkillName.Mining].Value > Utility.RandomMinMax( 1, 500 ) )
							{
								Container pack = from.Backpack;
								DugUpZinc zinc = new DugUpZinc( Utility.RandomMinMax( 1, 2 ) );
								from.AddToBackpack ( zinc );
								from.SendMessage( "You dig up some zinc.");
							}

							if ( tool is FishingPole && Server.Engines.Harvest.Fishing.IsNearHugeShipWreck( from ) && from.Skills[SkillName.Fishing].Value >= Utility.RandomMinMax( 1, 250 ) )
							{
								Server.Engines.Harvest.Fishing.FishUpFromMajorWreck( from );
							}
							else if ( tool is FishingPole && Server.Engines.Harvest.Fishing.IsNearSpaceCrash( from ) && from.Skills[SkillName.Fishing].Value >= Utility.RandomMinMax( 1, 250 ) )
							{
								Server.Engines.Harvest.Fishing.FishUpFromSpaceship( from );
							}
							else if ( tool is FishingPole && Server.Engines.Harvest.Fishing.IsNearUnderwaterRuins( from ) && from.Skills[SkillName.Fishing].Value >= Utility.RandomMinMax( 1, 250 ) )
							{
								Server.Engines.Harvest.Fishing.FishUpFromRuins( from );
							}
						}
						else if ( item is BlueBook || item is LoreBook || item is DDRelicBook || item is MyNecromancerSpellbook || item is MySpellbook || item is MyNinjabook || item is MySamuraibook || item is MyPaladinbook || item is MySongbook || item is ArtifactManual )
						{
						    from.SendMessage( "You find a book.");
						    if ( item is DDRelicBook ){ ((DDRelicBook)item).RelicGoldValue = ((DDRelicBook)item).RelicGoldValue + Utility.RandomMinMax( 1, (int)(from.Skills[SkillName.Inscribe].Value*2) ); }
						    else if ( item is BlueBook ){ item.Name = "Book"; item.Hue = RandomThings.GetRandomColor(0); item.ItemID = RandomThings.GetRandomBookItemID(); }
						}
						else if ( item is SomeRandomNote || item is ScrollClue || item is LibraryScroll1 || item is LibraryScroll2 || item is LibraryScroll3 || item is LibraryScroll4 || item is LibraryScroll5 || item is LibraryScroll6 || item is DDRelicScrolls )
						{
						    from.SendMessage( "You find a scroll.");
						    if ( item is DDRelicScrolls ){ ((DDRelicScrolls)item).RelicGoldValue = ((DDRelicScrolls)item).RelicGoldValue + Utility.RandomMinMax( 1, (int)(from.Skills[SkillName.Inscribe].Value*2) ); }
						}

						bank.Consume( item.Amount, from );

						if ( Give( from, item, def.PlaceAtFeetIfFull ) )
						{
							SendSuccessTo( from, item, resource );
						}
						else
						{
							SendPackFullTo( from, item, def, resource );
							item.Delete();
						}

						BonusHarvestResource bonus = def.GetBonusResource();

						if ( bonus != null && bonus.Type != null && skillBase >= bonus.ReqSkill )
						{
							Item bonusItem = Construct( bonus.Type, from );

							if ( Give( from, bonusItem, true ) )	//Bonuses always allow placing at feet, even if pack is full irregrdless of def
							{
								bonus.SendSuccessTo( from );
							}
							else
							{
								item.Delete();
							}
						}

						if ( tool is IUsesRemaining )
						{
							IUsesRemaining toolWithUses = (IUsesRemaining)tool;

							toolWithUses.ShowUsesRemaining = true;

							if ( toolWithUses.UsesRemaining > 0 )
								--toolWithUses.UsesRemaining;

							if ( toolWithUses.UsesRemaining < 1 )
							{
								tool.Delete();
								def.SendMessageTo( from, def.ToolBrokeMessage );
							}
						}
					}
				}
			}

			if ( type == null )
				def.SendMessageTo( from, def.FailMessage );

			OnHarvestFinished( from, tool, def, vein, bank, resource, toHarvest );
		}

		public virtual void OnHarvestFinished( Mobile from, Item tool, HarvestDefinition def, HarvestVein vein, HarvestBank bank, HarvestResource resource, object harvested )
		{
		}

		public virtual bool SpecialHarvest( Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc )
		{
			return false;
		}

		public virtual Item Construct( Type type, Mobile from )
		{
			try{ return Activator.CreateInstance( type ) as Item; }
			catch{ return null; }
		}

		public virtual HarvestVein MutateVein( Mobile from, Item tool, HarvestDefinition def, HarvestBank bank, object toHarvest, HarvestVein vein )
		{
			return vein;
		}

		public virtual void SendSuccessTo( Mobile from, Item item, HarvestResource resource )
		{
			resource.SendSuccessTo( from );
		}

		public virtual void SendPackFullTo( Mobile from, Item item, HarvestDefinition def, HarvestResource resource )
		{
			def.SendMessageTo( from, def.PackFullMessage );
		}

		public virtual bool Give( Mobile m, Item item, bool placeAtFeet )
		{
			if ( m.PlaceInBackpack( item ) )
				return true;

			if ( !placeAtFeet )
				return false;

			Map map = m.Map;

			if ( map == null )
				return false;

			List<Item> atFeet = new List<Item>();

			foreach ( Item obj in m.GetItemsInRange( 0 ) )
				atFeet.Add( obj );

			for ( int i = 0; i < atFeet.Count; ++i )
			{
				Item check = atFeet[i];

				if ( check.StackWith( m, item, false ) )
					return true;
			}

			item.MoveToWorld( m.Location, map );
			return true;
		}

		public virtual Type MutateType( Type type, Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource )
		{
			return from.Region.GetResource( type );
		}

		public virtual Type GetResourceType( Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource )
		{
			if ( resource.Types.Length > 0 )
				return resource.Types[Utility.Random( resource.Types.Length )];

			return null;
		}

		public virtual HarvestResource MutateResource( Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestVein vein, HarvestResource primary, HarvestResource fallback )
		{
			bool racialBonus = (def.RaceBonus && from.Race == Race.Elf );

			if( vein.ChanceToFallback > (Utility.RandomDouble() + (racialBonus ? .20 : 0)) )
				return fallback;

			double skillValue = from.Skills[def.Skill].Value;

			if ( fallback != null && (skillValue < primary.ReqSkill || skillValue < primary.MinSkill) )
				return fallback;

			return primary;
		}

		public virtual bool OnHarvesting( Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked, bool last )
		{
			bool automated = false;
			if (from is PlayerMobile && ((PlayerMobile)from).GetFlag( PlayerFlag.IsAutomated ) )
				automated = true;

			if ( !CheckHarvest( from, tool ) )
			{
				if (automated)
					AdventuresAutomation.StopAction((PlayerMobile)from);

				from.EndAction( locked );
				return false;
			}

			int tileID;
			Map map;
			Point3D loc;

			if ( !GetHarvestDetails( from, tool, toHarvest, out tileID, out map, out loc ) )
			{
				from.EndAction( locked );
				OnBadHarvestTarget( from, tool, toHarvest );

				if (automated)
					AdventuresAutomation.StopAction((PlayerMobile)from);

				return false;
			}
			else if ( !def.Validate( tileID ) )
			{
				from.EndAction( locked );
				OnBadHarvestTarget( from, tool, toHarvest );

				if (automated)
					AdventuresAutomation.StopAction((PlayerMobile)from);

				return false;
			}
			else if ( !CheckRange( from, tool, def, map, loc, true ) )
			{
				from.EndAction( locked );

				if (automated)
					AdventuresAutomation.StopAction((PlayerMobile)from);

				return false;
			}
			else if ( !CheckResources( from, tool, def, map, loc, true ) )
			{
				from.EndAction( locked );

				return false;
			}
			else if ( !CheckHarvest( from, tool, def, toHarvest ) )
			{
				from.EndAction( locked );

				if (automated)
					AdventuresAutomation.StopAction((PlayerMobile)from);

				return false;
			}

			DoHarvestingEffect( from, tool, def, map, loc );

			new HarvestSoundTimer( from, tool, this, def, toHarvest, locked, last ).Start();

			return !last;
		}

		public virtual void DoHarvestingSound( Mobile from, Item tool, HarvestDefinition def, object toHarvest )
		{
			if ( def.EffectSounds.Length > 0 )
				from.PlaySound( Utility.RandomList( def.EffectSounds ) );
		}

		public virtual void DoHarvestingEffect( Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc )
		{
			from.Direction = from.GetDirectionTo( loc );

			if ( !from.Mounted )
				from.Animate( Utility.RandomList( def.EffectActions ), 5, 1, true, false, 0 );
		}

		public virtual HarvestDefinition GetDefinition( int tileID )
		{
			HarvestDefinition def = null;

			for ( int i = 0; def == null && i < m_Definitions.Count; ++i )
			{
				HarvestDefinition check = m_Definitions[i];

				if ( check.Validate( tileID ) )
					def = check;
			}

			return def;
		}

		public virtual void StartHarvesting( Mobile from, Item tool, object toHarvest )
		{
			if ( !CheckHarvest( from, tool ) )
				return;

			int tileID;
			Map map;
			Point3D loc;

			bool automated = false;
			if (from is PlayerMobile && ((PlayerMobile)from).GetFlag( PlayerFlag.IsAutomated ) )
				automated = true;

			if ( !GetHarvestDetails( from, tool, toHarvest, out tileID, out map, out loc ) )
			{
				if (automated)
					AdventuresAutomation.StopAction((PlayerMobile)from);

				OnBadHarvestTarget( from, tool, toHarvest );

				return;
			}

			HarvestDefinition def = GetDefinition( tileID );

			if ( def == null )
			{
				OnBadHarvestTarget( from, tool, toHarvest );

				if (automated)
					AdventuresAutomation.StopAction((PlayerMobile)from);

				return;
			}

			if ( !CheckRange( from, tool, def, map, loc, false ) )
			{				
				if (automated)
					AdventuresAutomation.StopAction((PlayerMobile)from);

				return;
			}
			else if ( !CheckResources( from, tool, def, map, loc, false ) )
			{				
				return;
			}
			else if ( !CheckHarvest( from, tool, def, toHarvest ) )
			{				
				if (automated)
					AdventuresAutomation.StopAction((PlayerMobile)from);

				return;
			}

			object toLock = GetLock( from, tool, def, toHarvest );

			if ( !from.BeginAction( toLock ) )
			{
				OnConcurrentHarvest( from, tool, def, toHarvest );
				
				if (automated)
					AdventuresAutomation.StopAction((PlayerMobile)from);

				return;
			
			}

			new HarvestTimer( from, tool, this, def, toHarvest, toLock ).Start();
			OnHarvestStarted( from, tool, def, toHarvest );
		}

		public virtual bool GetHarvestDetails( Mobile from, Item tool, object toHarvest, out int tileID, out Map map, out Point3D loc )
		{
			if ( toHarvest is Static && !((Static)toHarvest).Movable )
			{
				Static obj = (Static)toHarvest;

				tileID = (obj.ItemID & 0x3FFF) | 0x4000;
				map = obj.Map;
				loc = obj.GetWorldLocation();
			}
			else if ( toHarvest is StaticTarget )
			{
				StaticTarget obj = (StaticTarget)toHarvest;

				tileID = (obj.ItemID & 0x3FFF) | 0x4000;
				map = from.Map;
				loc = obj.Location;
			}
			else if ( toHarvest is LandTarget )
			{
				LandTarget obj = (LandTarget)toHarvest;

				tileID = obj.TileID;
				map = from.Map;
				loc = obj.Location;
			}
			else
			{
				tileID = 0;
				map = null;
				loc = Point3D.Zero;
				return false;
			}

			return ( map != null && map != Map.Internal );
		}
	}
}

namespace Server
{
	public interface IChopable
	{
		void OnChop( Mobile from );
	}

	[AttributeUsage( AttributeTargets.Class )]
	public class FurnitureAttribute : Attribute
	{
		public static bool Check( Item item )
		{
			return ( item != null && item.GetType().IsDefined( typeof( FurnitureAttribute ), false ) );
		}

		public FurnitureAttribute()
		{
		}
	}
}
