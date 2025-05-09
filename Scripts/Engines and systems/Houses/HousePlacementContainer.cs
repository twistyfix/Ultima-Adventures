using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Multis;
using Server.Mobiles;
using Server.Targeting;
using Server.Regions;
using Server.Engines.CannedEvil;

namespace Server.Items
{
	public class HousePlacementContainer : Item, IChopable
	{

		public int HaveWood;
		[CommandProperty( AccessLevel.GameMaster )]
		private int g_HaveWood { get{ return HaveWood; } set{ HaveWood = value; } }

		public int HaveNails;
		[CommandProperty( AccessLevel.GameMaster )]
		private int g_HaveNails { get{ return HaveNails; } set{ HaveNails = value; } }

		public int HaveStone;
		[CommandProperty( AccessLevel.GameMaster )]
		private int g_HaveStone { get{ return HaveStone; } set{ HaveStone = value; } }

		public Mobile Owner;
		[CommandProperty( AccessLevel.GameMaster )]
		private Mobile g_Owner { get{ return Owner; } set{ Owner = value; } }

		public bool claimed;
		[CommandProperty( AccessLevel.GameMaster )]
		private bool g_claimed { get{ return claimed; } set{ claimed = value; } }

		private string g_housesize { get{ return HouseSize(); } set{ } }

		private List<AddonComponent> g_ac;



		[Constructable]
		public HousePlacementContainer(AddonComponent ac) : base( 0xE43 )
		{
			Weight = 1.0;
			Name = "worksite storage";
			g_HaveNails = 0;
			g_HaveWood = 0;
			g_HaveStone = 0;
			g_Owner = null;
			g_claimed = false;
			g_ac = new List<AddonComponent>();
			g_ac.Add(ac);
		}

		private bool TryGetMultiplier(BaseWoodBoard board, out double multiplier)
		{
			switch(board.Resource)
            {
                case CraftResource.RegularWood: multiplier = 1; break;
                case CraftResource.AshTree: multiplier = 1.2; break;
                case CraftResource.CherryTree: multiplier = 1.3; break;
                case CraftResource.EbonyTree: multiplier = 1.4; break;
                case CraftResource.GoldenOakTree: multiplier = 1.5; break;
                case CraftResource.HickoryTree: multiplier = 1.6; break;
                case CraftResource.MahoganyTree: multiplier = 1.7; break;
                case CraftResource.OakTree: multiplier = 1.8; break;
                case CraftResource.PineTree: multiplier = 1.9; break;
                case CraftResource.GhostTree: multiplier = 3; break;
                case CraftResource.RosewoodTree: multiplier = 2; break;
                case CraftResource.WalnutTree: multiplier = 2.10; break;
                case CraftResource.PetrifiedTree: multiplier = 3.10; break;
                case CraftResource.DriftwoodTree: multiplier = 3.20; break;
                case CraftResource.ElvenTree: multiplier = 4; break;
                default: multiplier = 0; break; // Unknown wood type
			}

			return multiplier != 0;
		}

		private bool TryGetMultiplier(BaseGranite granite, out double multiplier)
        {
            switch (granite.Resource)
            {
                case CraftResource.Iron: multiplier = 1; break;
                case CraftResource.DullCopper: multiplier = 1.25; break;
                case CraftResource.ShadowIron: multiplier = 1.5; break;
                case CraftResource.Copper: multiplier = 1.75; break;
                case CraftResource.Bronze: multiplier = 2; break;
                case CraftResource.Gold: multiplier = 2.25; break;
                case CraftResource.Agapite: multiplier = 2.50; break;
                case CraftResource.Verite: multiplier = 2.75; break;
                case CraftResource.Valorite: multiplier = 3; break;
                case CraftResource.Nepturite: multiplier = 3.10; break;
                case CraftResource.Obsidian: multiplier = 3.10; break;
                case CraftResource.Steel: multiplier = 3.25; break;
                case CraftResource.Brass: multiplier = 3.5; break;
                case CraftResource.Mithril: multiplier = 3.75; break;
                case CraftResource.Xormite: multiplier = 3.75; break;
                case CraftResource.Dwarven: multiplier = 7.50; break;
                default: multiplier = 0; break; // Unknown wood type
            }

            return multiplier != 0;
        }


		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if (g_Owner == null && (dropped is BaseWoodBoard || dropped is Nails || dropped is BaseGranite))
			{
				g_claimed = true;
				g_Owner = from;
			}

			if (dropped is BaseWoodBoard)
			{
                double multiplier;
                if (!TryGetMultiplier((BaseWoodBoard)dropped, out multiplier)) return false;

                g_HaveWood += (int)(multiplier * dropped.Amount);
				this.InvalidateProperties();
				dropped.Delete();
				return true;
			}
			else if ( dropped is Nails)
			{
				g_HaveNails += (int)((double)dropped.Amount);
				this.InvalidateProperties();
				dropped.Delete();
				return true;
			}
			else if ( dropped is BaseGranite)
            {
                double multiplier;
                if (!TryGetMultiplier((BaseGranite)dropped, out multiplier)) return false;

                g_HaveStone += (int)(multiplier * dropped.Amount);
				this.InvalidateProperties();
				dropped.Delete();
				return true;
			}

			
			return false;
		}

		public virtual void OnChop( Mobile from )
		{
			if (from != g_Owner)
			{
				from.SendMessage("This worksite doesn't belong to you.");
				return;
			}
			else
			{
				//todo drop resources added to it?
				foreach ( AddonComponent c in g_ac )
				{
					c.Delete();
				}
			}

		}


		public void PackUp()
		{
			foreach ( AddonComponent c in g_ac )
			{
				c.Delete();
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if (g_Owner == null && from is PlayerMobile)
			{
				g_claimed = true;
				g_Owner = from;
			}

			int budget = g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100);

			if ( g_HaveWood > 500 && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > 2500 )
				from.SendGump( new HousePlacementCategoryGump( from, budget ) );

		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			
			base.AddNameProperties( list );	

			if (g_Owner == null)
				list.Add( "This site is unclaimed" ); 
			else 
				list.Add("This site is owned by " + g_Owner.Name);

			list.Add( "Drag boards or nails or stone in this container to build your house." ); 
			list.Add( "The more boards or nails or stone you place, the bigger the house will be." ); 
			list.Add( "There are " + g_HaveWood + " boards in the container, " + g_HaveNails + " nails and " + g_HaveStone + " stone.");

			if ( g_HaveWood > 500 && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > 2500 )
				list.Add( "You have enough material for a lot of size " + g_housesize + " or smaller." ); 
			else
				list.Add( "You haven't deposited enough material for a house yet." ); 

		}

		private string HouseSize()
		{
			int baseprice = 768;

			if ( g_HaveWood > (baseprice *10) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > 966666  )
				return "fortress";
			else if ( g_HaveWood > (baseprice *10) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > 766666  )
				return "castle";
			else if ( g_HaveWood > (((18*18*(baseprice*2))/50)) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((18*18*(baseprice*2))/20)  )
				return "18x18";
			else if ( g_HaveWood > (((17*17*(baseprice*2))/50)) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((17*17*(baseprice*2))/20)  )
				return "17x17";
			else if ( g_HaveWood > (((16*16*(baseprice*2))/50)) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((16*16*(baseprice*2))/20)  )
				return "16x16";
			else if ( g_HaveWood > ((15*15*(baseprice*2))/50) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((15*15*(baseprice*2))/20)  )
				return "15x15";
			else if ( g_HaveWood > ((14*14*(baseprice*2))/50) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((14*14*(baseprice*2))/20)  )
				return "14x14";
			else if ( g_HaveWood > ((13*13*(baseprice*2))/50) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((13*13*(baseprice*2))/20)  )
				return "13x13";
			else if ( g_HaveWood > ((12*12*(baseprice*2))/50) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((12*12*(baseprice*2))/20)  )
				return "12x12";
			else if ( g_HaveWood > ((11*11*baseprice)/50) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((11*11*baseprice)/20)  )
				return "11x11";
			else if ( g_HaveWood > ((10*10*baseprice)/50) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((10*10*baseprice)/20)  )
				return "10x10";
			else if ( g_HaveWood > ((9*9*baseprice)/50) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((9*9*baseprice)/20)  )
				return "9x9";
			else if ( g_HaveWood > ((8*8*baseprice)/50) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((8*8*baseprice)/20)  )
				return "8x8";
			else if ( g_HaveWood > ((7*7*baseprice)/50) && (g_HaveWood + (g_HaveNails*5) + (g_HaveStone *100)) > ((7*7*baseprice)/20)  )
				return "7x7";

			return "0";

		}

		public HousePlacementContainer( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{

			if (g_ac == null)
				g_ac = new List<AddonComponent>();

			base.Serialize( writer );

			writer.Write( (int) 1 ); // version
			writer.Write( (int) g_HaveNails );
			writer.Write( (int) g_HaveWood );
			writer.WriteItemList<AddonComponent>( g_ac );
			writer.Write( (bool) g_claimed );

			if (g_claimed)
				writer.Write( (Mobile) g_Owner );

		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if (version >= 1)
			{
				g_HaveNails = reader.ReadInt();
				g_HaveWood = reader.ReadInt();
				g_ac = reader.ReadStrongItemList<AddonComponent>();
				g_claimed = reader.ReadBool();

				if (g_claimed)
					g_Owner = reader.ReadMobile();
			}

			if ( Weight == 0.0 )
				Weight = 3.0;
		}
	}

	public class HousePlacementCategoryGump : Gump
	{
		private Mobile m_From;

		private const int LabelColor = 0x7FFF;
		private const int LabelColorDisabled = 0x4210;

		private int m_budget;

		public HousePlacementCategoryGump( Mobile from, int budget ) : base( 50, 50 )
		{
			m_budget = budget;
			m_From = from;

			from.CloseGump( typeof( HousePlacementCategoryGump ) );
			from.CloseGump( typeof( HousePlacementListGump ) );

			AddPage( 0 );

			AddBackground( 0, 0, 270, 145, 5054 );

			AddImageTiled( 10, 10, 250, 125, 2624 );
			AddAlphaRegion( 10, 10, 250, 125 );

			AddHtmlLocalized( 10, 10, 250, 20, 1060239, LabelColor, false, false ); // <CENTER>CONSTRUCTION CONTRACT</CENTER>

			AddButton( 10, 110, 4017, 4019, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 45, 110, 150, 20, 3000363, LabelColor, false, false ); // Close

			AddButton( 10, 40, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 45, 40, 200, 20, 1060390, LabelColor, false, false ); // Classic Houses

			AddButton( 10, 60, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 45, 60, 200, 20, 1060391, LabelColor, false, false ); // 2-Story Customizable Houses

			AddButton( 10, 80, 4005, 4007, 3, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 45, 80, 200, 20, 1060392, LabelColor, false, false ); // 3-Story Customizable Houses
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			if ( !m_From.CheckAlive() )
				return;

			switch ( info.ButtonID )
			{
				case 1: // Classic Houses
				{
					m_From.SendGump( new HousePlacementListGump( m_From, HousePlacementEntry.ClassicHouses, m_budget ) );
					break;
				}
				case 2: // 2-Story Customizable Houses
				{
					m_From.SendGump( new HousePlacementListGump( m_From, HousePlacementEntry.TwoStoryFoundations, m_budget ) );
					break;
				}
				case 3: // 3-Story Customizable Houses
				{
					m_From.SendGump( new HousePlacementListGump( m_From, HousePlacementEntry.ThreeStoryFoundations, m_budget ) );
					break;
				}
			}
		}
	}

	public class HousePlacementListGump : Gump
	{
		private Mobile m_From;
		private HousePlacementEntry[] m_Entries;

		private const int LabelColor = 0x7FFF;
		private const int LabelHue = 0x480;

		private int m_budget;

		public HousePlacementListGump( Mobile from, HousePlacementEntry[] entries, int budget ) : base( 50, 50 )
		{
			m_From = from;
			m_Entries = entries;
			m_budget = budget;

			from.CloseGump( typeof( HousePlacementCategoryGump ) );
			from.CloseGump( typeof( HousePlacementListGump ) );

			AddPage( 0 );

			AddBackground( 0, 0, 520, 420, 5054 );

			AddImageTiled( 10, 10, 500, 20, 2624 );
			AddAlphaRegion( 10, 10, 500, 20 );

			AddHtmlLocalized( 10, 10, 500, 20, 1060239, LabelColor, false, false ); // <CENTER>CONSTRUCTION CONTRACT</CENTER>

			AddImageTiled( 10, 40, 500, 20, 2624 );
			AddAlphaRegion( 10, 40, 500, 20 );

			AddHtmlLocalized( 50, 40, 225, 20, 1060235, LabelColor, false, false ); // House Description
			AddHtmlLocalized( 275, 40, 75, 20, 1060236, LabelColor, false, false ); // Storage
			AddHtmlLocalized( 350, 40, 75, 20, 1060237, LabelColor, false, false ); // Lockdowns
			AddHtmlLocalized( 425, 40, 75, 20, 1060034, LabelColor, false, false ); // Cost

			AddImageTiled( 10, 70, 500, 280, 2624 );
			AddAlphaRegion( 10, 70, 500, 280 );

			AddImageTiled( 10, 360, 500, 20, 2624 );
			AddAlphaRegion( 10, 360, 500, 20 );

			AddHtmlLocalized( 10, 360, 250, 20, 1060645, LabelColor, false, false ); // Bank Balance:
			AddLabel( 250, 360, LabelHue, Banker.GetBalance( from ).ToString() );

			AddImageTiled( 10, 390, 500, 20, 2624 );
			AddAlphaRegion( 10, 390, 500, 20 );

			AddButton( 10, 390, 4017, 4019, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 50, 390, 100, 20, 3000363, LabelColor, false, false ); // Close

			for ( int i = 0; i < entries.Length; ++i )
			{
				int page = 1 + (i / 14);
				int index = i % 14;

				if ( index == 0 )
				{
					if ( page > 1 )
					{
						AddButton( 450, 390, 4005, 4007, 0, GumpButtonType.Page, page );
						AddHtmlLocalized( 400, 390, 100, 20, 3000406, LabelColor, false, false ); // Next
					}

					AddPage( page );

					if ( page > 1 )
					{
						AddButton( 200, 390, 4014, 4016, 0, GumpButtonType.Page, page - 1 );
						AddHtmlLocalized( 250, 390, 100, 20, 3000405, LabelColor, false, false ); // Previous
					}
				}

				HousePlacementEntry entry = entries[i];

				if ((entry.Cost/20) < m_budget)
				{
					int y = 70 + (index * 20);

					AddButton( 10, y, 4005, 4007, 1 + i, GumpButtonType.Reply, 0 );
					AddHtmlLocalized( 50, y, 225, 20, entry.Description, LabelColor, false, false );
					AddLabel( 275, y, LabelHue, entry.Storage.ToString() );
					AddLabel( 350, y, LabelHue, entry.Lockdowns.ToString() );
					AddLabel( 425, y, LabelHue, entry.Cost.ToString() );
				}
			}
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			if ( !m_From.CheckAlive() )
				return;

			int index = info.ButtonID - 1;

			if ( index >= 0 && index < m_Entries.Length )
			{
				HousePlacementEntry entry = m_Entries[index];
				if (entry.Cost / 20 > m_budget)
					m_From.SendMessage(0, "You don't have enough materials for this size lot.");
				else if (entry.Cost > Banker.GetBalance(m_From))
					m_From.SendMessage(0, "You don't have enough gold in the bank to pay for this size lot.");
				//if ( m_From.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse( m_From ) )
				//	m_From.SendLocalizedMessage( 501271 ); // You already own a house, you may not place another!
				else
					m_From.Target = new NewHousePlacementTarget( m_Entries, m_Entries[index], m_budget );
			}
			else
			{
				m_From.SendGump( new HousePlacementCategoryGump( m_From, m_budget ) );
			}
		}
	}

	public class NewHousePlacementTarget : MultiTarget
	{
		private HousePlacementEntry m_Entry;
		private HousePlacementEntry[] m_Entries;

		private bool m_Placed;
		private int m_budget;

		public NewHousePlacementTarget( HousePlacementEntry[] entries, HousePlacementEntry entry, int budget ) : base( entry.MultiID, entry.Offset )
		{
			Range = 14;

			m_Entries = entries;
			m_Entry = entry;
			m_budget = budget;

		}

		protected override void OnTarget( Mobile from, object o )
		{
			if ( !from.CheckAlive() )
				return;

			IPoint3D ip = o as IPoint3D;

			if ( ip != null )
			{
				if ( ip is Item )
					ip = ((Item)ip).GetWorldTop();

				Point3D p = new Point3D( ip );

				Region reg = Region.Find( new Point3D( p ), from.Map );

				if ( ( from.AccessLevel >= AccessLevel.GameMaster || reg.AllowHousing( from, p ) ) && !(reg.IsPartOf( typeof( ChampionSpawnRegion ) ) || reg is ChampionSpawnRegion) )
					m_Placed = m_Entry.OnPlacement( from, p );
				else
					from.SendLocalizedMessage( 501265 ); // Housing can not be created in this area.
			}
		}

		protected override void OnTargetFinish( Mobile from )
		{
			if ( !from.CheckAlive() )
				return;

			if ( !m_Placed )
				from.SendGump( new HousePlacementListGump( from, m_Entries, m_budget ) );
		}
	}

	public class HousePlacementEntry
	{
		private Type m_Type;
		private int m_Description;
		private int m_Storage;
		private int m_Lockdowns;
		private int m_NewStorage;
		private int m_NewLockdowns;
		private int m_Vendors;
		private int m_Cost;
		private int m_MultiID;
		private Point3D m_Offset;

		public Type Type{ get{ return m_Type; } }

		public int Description{ get{ return m_Description; } }
		public int Storage{ get{ return BaseHouse.NewVendorSystem ? m_NewStorage : m_Storage; } }
		public int Lockdowns{ get{ return BaseHouse.NewVendorSystem ? m_NewLockdowns : m_Lockdowns; } }
		public int Vendors{ get{ return m_Vendors; } }
		public int Cost{ get{ return m_Cost; } }

		public int MultiID{ get{ return m_MultiID; } }
		public Point3D Offset{ get{ return m_Offset; } }

		public HousePlacementEntry( Type type, int description, int storage, int lockdowns, int newStorage, int newLockdowns, int vendors, int cost, int xOffset, int yOffset, int zOffset, int multiID )
		{
			m_Type = type;
			m_Description = description;
			m_Storage = storage;
			m_Lockdowns = lockdowns;
			m_NewStorage = newStorage;
			m_NewLockdowns = newLockdowns;
			m_Vendors = vendors;
			m_Cost = cost;

			m_Offset = new Point3D( xOffset, yOffset, zOffset );

			m_MultiID = multiID;
		}

		public BaseHouse ConstructHouse( Mobile from )
		{
			try
			{
				object[] args;

				if ( m_Type == typeof( HouseFoundation ) )
					args = new object[4]{ from, m_MultiID, m_Storage, m_Lockdowns };
				else if ( m_Type == typeof( SmallOldHouse ) || m_Type == typeof( SmallShop ) || m_Type == typeof( TwoStoryHouse ) )
					args = new object[2]{ from, m_MultiID };
				else
					args = new object[1]{ from };

				return Activator.CreateInstance( m_Type, args ) as BaseHouse;
			}
			catch
			{
			}

			return null;
		}

		public void PlacementWarning_Callback( Mobile from, bool okay, object state )
		{
			if ( !from.CheckAlive()  )
				return;

			PreviewHouse prevHouse = (PreviewHouse)state;

			if ( !okay )
			{
				prevHouse.Delete();
				return;
			}

			if ( prevHouse.Deleted )
			{
				/* Too much time has passed and the test house you created has been deleted.
				 * Please try again!
				 */
				from.SendGump( new NoticeGump( 1060637, 30720, 1060647, 32512, 320, 180, null, null ) );

				return;
			}

			Point3D center = prevHouse.Location;
			Map map = prevHouse.Map;

			prevHouse.Delete();

			ArrayList toMove;
			//Point3D center = new Point3D( p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z );
			HousePlacementResult res = HousePlacement.Check( from, m_MultiID, center, out toMove );

			switch ( res )
			{
				case HousePlacementResult.Valid:
				{
					//if ( from.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse( from ) )
					//{
					//	from.SendLocalizedMessage( 501271 ); // You already own a house, you may not place another!
					//}
					//else
					//{
						BaseHouse house = ConstructHouse( from );

						if ( house == null )
							return;

						house.Price = m_Cost;

						if ( !( house is HouseFoundation ) )
						{
							//FINAL removed for new system
							//Item contract = from.Backpack.FindItemByType( typeof( HousePlacementContainer ) );
							//if ( contract.Hue == 1 ){ contract.Hue = 0x497; }
							//house.Hue = contract.Hue;
						}

						if ( from.AccessLevel >= AccessLevel.GameMaster )
						{
							from.SendMessage( "{0} gold would have been withdrawn from your bank if you were not a GM.", m_Cost.ToString() );
						}
						else
						{
							from.Say(m_Cost); //debug
							
							if ( Banker.Withdraw( from, m_Cost ) )
							{
								from.SendLocalizedMessage( 1060398, m_Cost.ToString() ); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
							}
							else
							{
								house.RemoveKeys( from );
								house.Delete();
								from.SendLocalizedMessage( 1060646 ); // You do not have the funds available in your bank box to purchase this house.  Try placing a smaller house, or adding gold or checks to your bank box.
								return;
							}
						}

						house.MoveToWorld( center, from.Map );

						for ( int i = 0; i < toMove.Count; ++i )
						{
							object o = toMove[ i ];

							if ( o is Mobile )
								( (Mobile) o ).Location = house.BanLocation;
							else if ( o is Item )
								( (Item) o ).Location = house.BanLocation;
						}

					foreach ( Item i in from.GetItemsInRange( 15 ) )
					{
						if ( i is HousePlacementContainer && ((HousePlacementContainer)i).Owner == from )
						{
							HousePlacementContainer ct = (HousePlacementContainer)i;
							ct.PackUp();
							break;
						}
					}

					//}

					break;
				}
				case HousePlacementResult.BadItem:
				case HousePlacementResult.BadLand:
				case HousePlacementResult.BadStatic:
				case HousePlacementResult.BadRegionHidden:
				case HousePlacementResult.NoSurface:
				{
					from.SendLocalizedMessage( 1043287 ); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
					break;
				}
				case HousePlacementResult.BadRegion:
				{
					from.SendLocalizedMessage( 501265 ); // Housing cannot be created in this area.
					break;
				}
				case HousePlacementResult.BadRegionTemp:
				{
					from.SendLocalizedMessage( 501270 ); // Lord British has decreed a 'no build' period, thus you cannot build this house at this time.
					break;
				}
				case HousePlacementResult.InvalidCastleKeep:
				{
					from.SendLocalizedMessage( 1061122 ); // Castles and keeps cannot be created here.
					break;
				}
			}
		}

		public bool OnPlacement( Mobile from, Point3D p )
		{
			if ( !from.CheckAlive()  )
				return false;

			ArrayList toMove;
			Point3D center = new Point3D( p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z );
			HousePlacementResult res = HousePlacement.Check( from, m_MultiID, center, out toMove );


			switch ( res )
			{
				case HousePlacementResult.Valid:
				{

					//if ( from.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse( from ) )
					//{
					//	from.SendLocalizedMessage( 501271 ); // You already own a house, you may not place another!
					//}
					//else
					//{
						from.SendLocalizedMessage( 1011576 ); // This is a valid location.

						PreviewHouse prev = new PreviewHouse( m_MultiID );

						if ( !( prev is HouseFoundation ) )
						{
							//Item contract = from.Backpack.FindItemByType( typeof( HousePlacementContainer ) );
							//if ( contract.Hue == 1 ){ contract.Hue = 0x497; }
							//prev.Hue = contract.Hue;
						}

						MultiComponentList mcl = prev.Components;

						Point3D banLoc = new Point3D( center.X + mcl.Min.X, center.Y + mcl.Max.Y + 1, center.Z );

						for ( int i = 0; i < mcl.List.Length; ++i )
						{
							MultiTileEntry entry = mcl.List[i];

							int itemID = entry.m_ItemID;

							if ( itemID >= 0xBA3 && itemID <= 0xC0E )
							{
								banLoc = new Point3D( center.X + entry.m_OffsetX, center.Y + entry.m_OffsetY, center.Z );
								break;
							}
						}

						for ( int i = 0; i < toMove.Count; ++i )
						{
							object o = toMove[i];

							if ( o is Mobile )
								((Mobile)o).Location = banLoc;
							else if ( o is Item )
								((Item)o).Location = banLoc;
						}

						prev.MoveToWorld( center, from.Map );

						/* You are about to place a new house.
						 * Placing this house will condemn any and all of your other houses that you may have.
						 * All of your houses on all shards will be affected.
						 * 
						 * In addition, you will not be able to place another house or have one transferred to you for one (1) real-life week.
						 * 
						 * Once you accept these terms, these effects cannot be reversed.
						 * Re-deeding or transferring your new house will not uncondemn your other house(s) nor will the one week timer be removed.
						 * 
						 * If you are absolutely certain you wish to proceed, click the button next to OKAY below.
						 * If you do not wish to trade for this house, click CANCEL.
						 */
					
						
					
						from.SendGump( new WarningGump( 1060635, 30720, 1049583, 32512, 420, 280, new WarningGumpCallback( PlacementWarning_Callback ), prev ) );

						return true;
					//}

					break;
				}
				case HousePlacementResult.BadItem:
				case HousePlacementResult.BadLand:
				case HousePlacementResult.BadStatic:
				case HousePlacementResult.BadRegionHidden:
				case HousePlacementResult.NoSurface:
				{
					from.SendLocalizedMessage( 1043287 ); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
					break;
				}
				case HousePlacementResult.BadRegion:
				{
					from.SendLocalizedMessage( 501265 ); // Housing cannot be created in this area.
					break;
				}
				case HousePlacementResult.BadRegionTemp:
				{
					from.SendLocalizedMessage( 501270 ); //Lord British has decreed a 'no build' period, thus you cannot build this house at this time.
					break;
				}
				case HousePlacementResult.InvalidCastleKeep:
				{
					from.SendLocalizedMessage( 1061122 ); // Castles and keeps cannot be created here.
					break;
				}
			}

			return false;
		}

		private static Hashtable m_Table;

		static HousePlacementEntry()
		{
			m_Table = new Hashtable();

			FillTable( m_ClassicHouses );
			FillTable( m_TwoStoryFoundations );
			FillTable( m_ThreeStoryFoundations );
		}

		public static HousePlacementEntry Find( BaseHouse house )
		{
			object obj = m_Table[house.GetType()];

			if ( obj is HousePlacementEntry )
			{
				return ((HousePlacementEntry)obj);
			}
			else if ( obj is ArrayList )
			{
				ArrayList list = (ArrayList)obj;

				for ( int i = 0; i < list.Count; ++i )
				{
					HousePlacementEntry e = (HousePlacementEntry)list[i];

					if ( e.m_MultiID == house.ItemID )
						return e;
				}
			}
			else if ( obj is Hashtable )
			{
				Hashtable table = (Hashtable)obj;

				obj = table[house.ItemID];

				if ( obj is HousePlacementEntry )
					return (HousePlacementEntry)obj;
			}

			return null;
		}

		private static void FillTable( HousePlacementEntry[] entries )
		{
			for ( int i = 0; i < entries.Length; ++i )
			{
				HousePlacementEntry e = entries[i];

				object obj = m_Table[e.m_Type];

				if ( obj == null )
				{
					m_Table[e.m_Type] = e;
				}
				else if ( obj is HousePlacementEntry )
				{
					ArrayList list = new ArrayList();

					list.Add( obj );
					list.Add( e );

					m_Table[e.m_Type] = list;
				}
				else if ( obj is ArrayList )
				{
					ArrayList list = (ArrayList)obj;

					if ( list.Count == 8 )
					{
						Hashtable table = new Hashtable();

						for ( int j = 0; j < list.Count; ++j )
							table[((HousePlacementEntry)list[j]).m_MultiID] = list[j];

						table[e.m_MultiID] = e;

						m_Table[e.m_Type] = table;
					}
					else
					{
						list.Add( e );
					}
				}
				else if ( obj is Hashtable )
				{
					((Hashtable)obj)[e.m_MultiID] = e;
				}
			}
		}

		private static HousePlacementEntry[] m_ClassicHouses = new HousePlacementEntry[]
			{	// WIZARD ADDED SOME
				// HousePlacementEntry( Type type, int description, int storage, int lockdowns, int newStorage, int newLockdowns, int vendors, int cost, int xOffset, int yOffset, int zOffset, int multiID )

				new HousePlacementEntry( typeof( BlueTent ), 							1041217, 	351, 	81, 	351, 	81, 	1, 	768*27, 		0, 	4, 	0, 	0x70),
				new HousePlacementEntry( typeof( GreenTent ), 							1041218, 	351, 	81, 	351, 	81, 	1, 	768*27, 		0, 	4, 	0, 	0x72),
				new HousePlacementEntry( typeof( NewSmallStoneHomeEast ), 				1030845, 	382, 	112, 	382, 	112, 	2, 	768*51, 		4,	-2,	0, 	0x5C),
				new HousePlacementEntry( typeof( NewSmallStoneHouseEast ), 				1030848, 	382, 	112, 	382, 	112, 	2, 	768*51, 		4,	-2,	0, 	0x5F),
				new HousePlacementEntry( typeof( SmallOldHouse ), 						1011303, 	382, 	112, 	382, 	112, 	2, 	768*51, 		0, 	4, 	0, 	0x64),
				new HousePlacementEntry( typeof( SmallOldHouse ), 						1011304, 	382, 	112, 	382, 	112, 	2, 	768*51, 		0, 	4, 	0, 	0x66),
				new HousePlacementEntry( typeof( SmallOldHouse ), 						1011305, 	382, 	112, 	382, 	112, 	2, 	768*51, 		0, 	4, 	0, 	0x68),
				new HousePlacementEntry( typeof( SmallOldHouse ), 						1011306, 	382, 	112, 	382, 	112, 	2, 	768*51, 		0, 	4, 	0, 	0x6A),
				new HousePlacementEntry( typeof( SmallOldHouse ), 						1011307, 	382, 	112, 	382, 	112, 	2, 	768*51, 		0, 	4, 	0, 	0x6C),
				new HousePlacementEntry( typeof( SmallOldHouse ), 						1011308, 	382, 	112, 	382, 	112, 	2, 	768*51, 		0, 	4, 	0, 	0x6E),
				new HousePlacementEntry( typeof( NewSmallStoneStoreFront ), 			1030844, 	410, 	140, 	410, 	140, 	3, 	768*73, 		0, 	4, 	0, 	0x5B),
				new HousePlacementEntry( typeof( NewSmallWoodenShackPorch ), 			1030849, 	410, 	140, 	410, 	140, 	3, 	768*73, 		-3, 4, 	0, 	0x60),
				new HousePlacementEntry( typeof( SmallShop ), 							1011321, 	444, 	174, 	444, 	174, 	4, 	768*92, 		-1, 4, 	0, 	0xA0),
				new HousePlacementEntry( typeof( SmallShop ), 							1011322, 	444, 	174, 	444, 	174, 	4, 	768*92, 		0, 	4, 	0, 	0xA2),
				new HousePlacementEntry( typeof( NewPlainStoneHouse ), 					1030851, 	456, 	186, 	456, 	186, 	5, 	768*102, 		-5,	6,	0, 	0x62),
				new HousePlacementEntry( typeof( NewSmallLogCabinWithDeck ), 			1030860, 	449, 	179, 	449, 	179, 	4, 	768*102, 		1,	4,	0,	0x88),
				new HousePlacementEntry( typeof( NewPlainPlasterHouse ), 				1030850, 	463, 	193, 	463, 	193, 	5, 	768*111, 		-5,	4,	0, 	0x61),
				new HousePlacementEntry( typeof( NewSmallSandstoneWorkshop ), 			1030857, 	458, 	188, 	458, 	188, 	5, 	768*111, 		4, 	4, 	0, 	0x84),
				new HousePlacementEntry( typeof( NewTwoStorySmallPlasterDwelling ), 	1030855, 	470, 	200, 	470, 	200, 	5, 	768*135, 		3, 	3, 	0, 	0x82),
				new HousePlacementEntry( typeof( Wagon ), 								1030870, 	470, 	200, 	470, 	200, 	5, 	768*135, 		0, 	0, 	0, 	0x94),
				new HousePlacementEntry( typeof( NewTwoStorySmallStoneDwelling ), 		1030841, 	470, 	200, 	470, 	200, 	5, 	768*135, 		3, 	3, 	0, 	0x58),
				new HousePlacementEntry( typeof( NewTwoStorySmallStoneHome ), 			1030839, 	470, 	200, 	470, 	200, 	5, 	768*135, 		3, 	3, 	0, 	0x56),
				new HousePlacementEntry( typeof( NewTwoStorySmallStoneHouse ), 			1030840, 	470, 	200, 	470, 	200, 	5, 	768*135, 		3, 	3, 	0, 	0x57),
				new HousePlacementEntry( typeof( NewTwoStorySmallWoodenDwelling ), 		1030842, 	470, 	200, 	470, 	200, 	5, 	768*135, 		3, 	3, 	0, 	0x59),
				new HousePlacementEntry( typeof( LogCabin ), 							1011318, 	478, 	208, 	478, 	208, 	5, 	768*155, 		1, 	6, 	0, 	0x9A),
				new HousePlacementEntry( typeof( NewLogCabin ), 						1030859, 	488, 	218, 	488, 	218, 	6, 	768*155, 		2, 	5, 	0, 	0x86),
				new HousePlacementEntry( typeof( NewSmallStoneShoppe ), 				1030835, 	478, 	208, 	478, 	208, 	5, 	768*155, 		-5, 6, 	0, 	0x52),
				new HousePlacementEntry( typeof( NewWoodenHomePorch ), 					1030836, 	487, 	217, 	487, 	217, 	6, 	768*155, 		2, 	5, 	0, 	0x53),
				new HousePlacementEntry( typeof( SmallTower ), 							1011317, 	500, 	230, 	500, 	230, 	6, 	768*180, 		3, 	4, 	0, 	0x98),
				new HousePlacementEntry( typeof( NewSmallStoneTemple ), 				1030856, 	504, 	234, 	504, 	234, 	6, 	768*240, 		4, 	-3, 0, 	0x83),
				new HousePlacementEntry( typeof( NewBrickHomeWithFrontDeck ), 			1030867, 	518, 	248, 	518, 	248, 	7, 	768*250, 		0, 	7, 	0, 	0x91),
				new HousePlacementEntry( typeof( NewPlasterHousePictureWindow ), 		1030832, 	515, 	245, 	515, 	245, 	7, 	768*265, 		7, 	-6, 0, 	0x4F),
				new HousePlacementEntry( typeof( NewStoneHomeWithEnclosedPatio ), 		1030858, 	516, 	246, 	516, 	246, 	7, 	768*265, 		7, 	0, 	0, 	0x85),
				new HousePlacementEntry( typeof( SandStonePatio ), 						1011320, 	520, 	250, 	520, 	250, 	7, 	768*265, 		-1, 4, 	0, 	0x9C),
				new HousePlacementEntry( typeof( NewOldStoneHomeShoppe ), 				1030864, 	526, 	256, 	526, 	256, 	7, 	768*290, 	8, 	-5, 0, 	0x8E),
				new HousePlacementEntry( typeof( NewBrickHomeWithLargePorch ), 			1030869, 	533, 	263, 	533, 	263, 	7, 	768*295, 	-6, 6, 	0, 	0x93),
				new HousePlacementEntry( typeof( GuildHouse ), 							1011309, 	544, 	274, 	544, 	274, 	7, 	768*310, 	-1, 7, 	0, 	0x74),
				new HousePlacementEntry( typeof( LargePatioHouse ), 					1011315, 	546, 	276, 	546, 	276, 	8, 	768*310, 	-4, 7, 	0, 	0x8C),
				new HousePlacementEntry( typeof( NewTwoStoryWoodenHomeWithPorch ), 		1030834, 	562, 	292, 	562, 	292, 	8, 	768*335, 	6, 	4, 	0, 	0x51),
				new HousePlacementEntry( typeof( TwoStoryVilla ), 						1011319, 	560, 	290, 	560, 	290, 	8, 	768*335, 	3, 	6, 	0, 	0x9E),
				new HousePlacementEntry( typeof( NewTwoStoryBrickHouse ), 				1030831, 	568, 	298, 	568, 	298, 	8, 	768*336, 	-4, 5, 	0, 	0x4E),
				new HousePlacementEntry( typeof( NewFancyStoneWoodHome ), 				1030846, 	580, 	310, 	580, 	310, 	9, 	768*375, 	-4, 5, 	0, 	0x5D),
				new HousePlacementEntry( typeof( NewTwoStoryStoneVilla ), 				1030854, 	589, 	319, 	589, 	319, 	9, 	768*375, 	4, 	8, 	0, 	0x81),
				new HousePlacementEntry( typeof( NewWoodenHomeUpperDeck ), 				1030853, 	590, 	320, 	590, 	320, 	9, 	768*375, 	-4, 5, 	0, 	0x80),
				new HousePlacementEntry( typeof( NewBrickArena ), 						1030862, 	608, 	338, 	608, 	338, 	10, 768*415, 	-8, 11, 0, 	0x8A),
				new HousePlacementEntry( typeof( NewMarbleShoppe ), 					1030868, 	608, 	338, 	608, 	338, 	10, 768*415, 	-5, 6, 	0, 	0x92),
				new HousePlacementEntry( typeof( NewPlasterHomeDirtDeck ), 				1030852, 	622, 	352, 	622, 	352, 	10, 768*465, 	-2, 7, 	0, 	0x63),
				new HousePlacementEntry( typeof( NewTwoStoryBrickHome ), 				1030833, 	625, 	355, 	625, 	355, 	10, 768*470, 	-3, 7, 	0, 	0x50),
				new HousePlacementEntry( typeof( NewBrickHouseWithSteeple ), 			1030830, 	654, 	384, 	654, 	384, 	11, 768*490, 	0, 	6, 	0, 	0x4D),
				new HousePlacementEntry( typeof( NewFancyWoodenStoneHouse ), 			1030847, 	653, 	383, 	653, 	383, 	11, 768*490, 	6, 	-4, 0, 	0x5E),
				new HousePlacementEntry( typeof( TwoStoryHouse ), 						1011310, 	694, 	424, 	694, 	424, 	12, 768*525, 	-3, 7, 	0, 	0x76),
				new HousePlacementEntry( typeof( TwoStoryHouse ), 						1011311, 	694, 	424, 	694, 	424, 	12, 768*525, 	-3, 7, 	0, 	0x78),
				new HousePlacementEntry( typeof( LargeMarbleHouse ), 					1011316, 	700, 	430, 	700, 	430, 	13, 768*525, 	-4, 7, 	0, 	0x96),
				new HousePlacementEntry( typeof( NewTwoStorySandstoneHouse ), 			1030829, 	725, 	455, 	725, 	455, 	14, 768*525, 	7, 	-4, 0, 	0x4C),
				new HousePlacementEntry( typeof( NewSmallStoneTower ), 					1030837, 	731, 	461, 	731, 	461, 	14, 768*685, 	-2, 6, 	0, 	0x54),
				new HousePlacementEntry( typeof( NewSmallBrickCastle ), 				1030865, 	743, 	473, 	743, 	473, 	14, 768*685, 	-5, 6, 	0, 	0x8F),
				new HousePlacementEntry( typeof( NewStoneFort ), 						1030863, 	750, 	480, 	750, 	480, 	14, 768*710, 	-5, 7, 	0, 	0x8B),
				new HousePlacementEntry( typeof( CastleTower ), 						1024781, 	839, 	569, 	839, 	569, 	17, 768*750, 	5, 	7, 	0, 	0x4A),
				new HousePlacementEntry( typeof( NewRaisedBrickHome ), 					1030861, 	866, 	596, 	866, 	596, 	18, 768*755, 	3, 	7, 	0, 	0x89),
				new HousePlacementEntry( typeof( NewSmallWizardTower ), 				1030866, 	869, 	599, 	869, 	599, 	18, 768*925, 	-2, 6, 	0, 	0x90),
				new HousePlacementEntry( typeof( NewWoodenMansion ), 					1030843, 	920, 	650, 	920, 	650, 	20, 768*970, 	6, 	7, 	0, 	0x5A),
				new HousePlacementEntry( typeof( NewThreeStoryStoneVilla ), 			1030838, 	965, 	695, 	965, 	695, 	22, 768*1110, 	-6, 7, 	0, 	0x55),
				new HousePlacementEntry( typeof( Tower ), 								1011312, 	1476, 	1206, 	1476, 	1206, 	24, 768*1200, 	0, 	7, 	0, 	0x7A),
				new HousePlacementEntry( typeof( LargeTent ), 							1024851, 	1572, 	1302, 	1572, 	1302, 	28, 768*1400, 	1, 	13, 0, 	0x49),
				new HousePlacementEntry( typeof( Keep ), 								1011313, 	2847, 	2577, 	2847, 	2577, 	30, 768*2000, 	0, 	11, 0, 	0x7C),
				new HousePlacementEntry( typeof( Pyramid ), 							1024788, 	3856, 	3586, 	3856, 	3586, 	32, 768*2100, 	3, 	16, 0, 	0x48),
				new HousePlacementEntry( typeof( Castle ), 								1011314, 	4777, 	4507, 	4777, 	4507, 	34, (768*3000), 	0, 	16, 0, 	0x7E),
				new HousePlacementEntry( typeof( Fortress ), 							1024869, 	6448, 	6178, 	6448, 	6178, 	36, (768*3900), 	4, 	16, 0, 	0x4B)
			};

		public static HousePlacementEntry[] ClassicHouses{ get{ return m_ClassicHouses; } }





		private static HousePlacementEntry[] m_TwoStoryFoundations = new HousePlacementEntry[]
			{

				new HousePlacementEntry( typeof( HouseFoundation ),		1060241,	425,	212,	489,	244,	10,	(7*7*768),		0,	4,	0,	0x13EC	), // 7x7 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060242,	580,	290,	667,	333,	14,	(7*8*768),		0,	5,	0,	0x13ED	), // 7x8 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060243,	650,	325,	748,	374,	16,	(7*9*768),		0,	5,	0,	0x13EE	), // 7x9 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060244,	700,	350,	805,	402,	16,	(7*10*768),		0,	6,	0,	0x13EF	), // 7x10 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060245,	750,	375,	863,	431,	16,	(7*11*768),		0,	6,	0,	0x13F0	), // 7x11 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060246,	800,	400,	920,	460,	18,	(7*12*768),		0,	7,	0,	0x13F1	), // 7x12 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060253,	580,	290,	667,	333,	14,	(8*7*768),		0,	4,	0,	0x13F8	), // 8x7 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060254,	650,	325,	748,	374,	16,	(8*8*768),		0,	5,	0,	0x13F9	), // 8x8 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060255,	700,	350,	805,	402,	16,	(8*9*768),		0,	5,	0,	0x13FA	), // 8x9 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060256,	750,	375,	863,	431,	16,	(8*10*768),		0,	6,	0,	0x13FB	), // 8x10 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060257,	800,	400,	920,	460,	18,	(8*11*768),		0,	6,	0,	0x13FC	), // 8x11 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060258,	850,	425,	1265,	632,	24,	(8*12*768),		0,	7,	0,	0x13FD	), // 8x12 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060259,	1100,	550,	1265,	632,	24,	(8*13*768),		0,	7,	0,	0x13FE	), // 8x13 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060265,	650,	325,	748,	374,	16,	(9*7*768),		0,	4,	0,	0x1404	), // 9x7 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060266,	700,	350,	805,	402,	16,	(9*8*768),		0,	5,	0,	0x1405	), // 9x8 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060267,	750,	375,	863,	431,	16,	(9*9*768),		0,	5,	0,	0x1406	), // 9x9 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060268,	800,	400,	920,	460,	18,	(9*10*768),		0,	6,	0,	0x1407	), // 9x10 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060269,	850,	425,	1265,	632,	24,	(9*11*768),		0,	6,	0,	0x1408	), // 9x11 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060270,	1100,	550,	1265,	632,	24,	(9*12*768),		0,	7,	0,	0x1409	), // 9x12 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060271,	1100,	550,	1265,	632,	24,	(9*13*768),		0,	7,	0,	0x140A	), // 9x13 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060277,	700,	350,	805,	402,	16,	(10*7*768),		0,	4,	0,	0x1410	), // 10x7 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060278,	750,	375,	863,	431,	16,	(10*8*768),		0,	5,	0,	0x1411	), // 10x8 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060279,	800,	400,	920,	460,	18,	(10*9*768),		0,	5,	0,	0x1412	), // 10x9 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060280,	850,	425,	1265,	632,	24,	(10*10*768),		0,	6,	0,	0x1413	), // 10x10 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060281,	1100,	550,	1265,	632,	24,	(10*11*768),		0,	6,	0,	0x1414	), // 10x11 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060282,	1100,	550,	1265,	632,	24,	(10*12*768),		0,	7,	0,	0x1415	), // 10x12 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060283,	1150,	575,	1323,	661,	24,	(10*13*768),		0,	7,	0,	0x1416	), // 10x13 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060289,	750,	375,	863,	431,	16,	(11*7*768),		0,	4,	0,	0x141C	), // 11x7 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060290,	800,	400,	920,	460,	18,	(11*8*768),		0,	5,	0,	0x141D	), // 11x8 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060291,	850,	425,	1265,	632,	24,	(11*9*768),		0,	5,	0,	0x141E	), // 11x9 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060292,	1100,	550,	1265,	632,	24,	(11*10*768),		0,	6,	0,	0x141F	), // 11x10 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060293,	1100,	550,	1265,	632,	24,	(11*11*768),		0,	6,	0,	0x1420	), // 11x11 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060294,	1150,	575,	1323,	661,	24,	(11*12*768),		0,	7,	0,	0x1421	), // 11x12 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060295,	1200,	600,	1380,	690,	26,	(11*13*768),		0,	7,	0,	0x1422	), // 11x13 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060301,	800,	400,	920,	460,	18,	(12*7*768),		0,	4,	0,	0x1428	), // 12x7 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060302,	850,	425,	1265,	632,	24,	(12*8*768),		0,	5,	0,	0x1429	), // 12x8 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060303,	1100,	550,	1265,	632,	24,	(12*9*768),		0,	5,	0,	0x142A	), // 12x9 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060304,	1100,	550,	1265,	632,	24,	(12*10*768),		0,	6,	0,	0x142B	), // 12x10 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060305,	1150,	575,	1323,	661,	24,	(12*11*768),		0,	6,	0,	0x142C	), // 12x11 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060306,	1200,	600,	1380,	690,	26,	(12*12*768),		0,	7,	0,	0x142D	), // 12x12 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060307,	1250,	625,	1438,	719,	26,	(12*13*768),		0,	7,	0,	0x142E	), // 12x13 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060314,	1100,	550,	1265,	632,	24,	(13*8*768),		0,	5,	0,	0x1435	), // 13x8 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060315,	1100,	550,	1265,	632,	24,	(13*9*768),		0,	5,	0,	0x1436	), // 13x9 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060316,	1150,	575,	1323,	661,	24,	(13*10*768),		0,	6,	0,	0x1437	), // 13x10 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060317,	1200,	600,	1380,	690,	26,	(13*11*768),		0,	6,	0,	0x1438	), // 13x11 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060318,	1250,	625,	1438,	719,	26,	(13*12*768),		0,	7,	0,	0x1439	), // 13x12 2-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060319,	1300,	650,	1495,	747,	28,	(13*13*768),		0,	7,	0,	0x143A	)  // 13x13 2-Story Customizable House
			};

		public static HousePlacementEntry[] TwoStoryFoundations{ get{ return m_TwoStoryFoundations; } }





		private static HousePlacementEntry[] m_ThreeStoryFoundations = new HousePlacementEntry[]
			{
				new HousePlacementEntry( typeof( HouseFoundation ),		1060272,	1150,	575,	1323,	661,	24,	(9*14*1536),		0,	8,	0,	0x140B	), // 9x14 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060284,	1200,	600,	1380,	690,	26,	(10*14*1536),		0,	8,	0,	0x1417	), // 10x14 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060285,	1250,	625,	1438,	719,	26,	(10*15*1536),		0,	8,	0,	0x1418	), // 10x15 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060296,	1250,	625,	1438,	719,	26,	(11*14*1536),		0,	8,	0,	0x1423	), // 11x14 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060297,	1300,	650,	1495,	747,	28,	(11*15*1536),		0,	8,	0,	0x1424	), // 11x15 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060298,	1350,	675,	1553,	776,	28,	(11*16*1536),		0,	9,	0,	0x1425	), // 11x16 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060308,	1300,	650,	1495,	747,	28,	(12*14*1536),		0,	8,	0,	0x142F	), // 12x14 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060309,	1350,	675,	1553,	776,	28,	(12*15*1536),		0,	8,	0,	0x1430	), // 12x15 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060310,	1370,	685,	1576,	788,	28,	(12*16*1536),		0,	9,	0,	0x1431	), // 12x16 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060311,	1370,	685,	1576,	788,	28,	(12*17*1536),		0,	9,	0,	0x1432	), // 12x17 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060320,	1350,	675,	1553,	776,	28,	(13*14*1536),		0,	8,	0,	0x143B	), // 13x14 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060321,	1370,	685,	1576,	788,	28,	(13*15*1536),		0,	8,	0,	0x143C	), // 13x15 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060322,	1370,	685,	1576,	788,	28,	(13*16*1536),		0,	9,	0,	0x143D	), // 13x16 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060323,	2119,	1059,	2437,	1218,	42,	(13*17*1536),		0,	9,	0,	0x143E	), // 13x17 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060324,	2119,	1059,	2437,	1218,	42,	(13*18*1536),		0,	10,	0,	0x143F	), // 13x18 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060327,	1150,	575,	1323,	661,	24,	(14*9*1536),		0,	5,	0,	0x1442	), // 14x9 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060328,	1200,	600,	1380,	690,	26,	(14*10*1536),		0,	6,	0,	0x1443	), // 14x10 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060329,	1250,	625,	1438,	719,	26,	(14*11*1536),		0,	6,	0,	0x1444	), // 14x11 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060330,	1300,	650,	1495,	747,	28,	(14*12*1536),		0,	7,	0,	0x1445	), // 14x12 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060331,	1350,	675,	1553,	776,	28,	(14*13*1536),		0,	7,	0,	0x1446	), // 14x13 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060332,	1370,	685,	1576,	788,	28,	(14*14*1536),		0,	8,	0,	0x1447	), // 14x14 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060333,	1370,	685,	1576,	788,	28,	(14*15*1536),		0,	8,	0,	0x1448	), // 14x15 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060334,	2119,	1059,	2437,	1218,	42,	(14*16*1536),		0,	9,	0,	0x1449	), // 14x16 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060335,	2119,	1059,	2437,	1218,	42,	(14*17*1536),		0,	9,	0,	0x144A	), // 14x17 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060336,	2119,	1059,	2437,	1218,	42,	(14*18*1536),		0,	10,	0,	0x144B	), // 14x18 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060340,	1250,	625,	1438,	719,	26,	(15*10*1536),		0,	6,	0,	0x144F	), // 15x10 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060341,	1300,	650,	1495,	747,	28,	(15*11*1536),		0,	6,	0,	0x1450	), // 15x11 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060342,	1350,	675,	1553,	776,	28,	(15*12*1536),		0,	7,	0,	0x1451	), // 15x12 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060343,	1370,	685,	1576,	788,	28,	(15*13*1536),		0,	7,	0,	0x1452	), // 15x13 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060344,	1370,	685,	1576,	788,	28,	(15*14*1536),		0,	8,	0,	0x1453	), // 15x14 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060345,	2119,	1059,	2437,	1218,	42,	(15*15*1536),		0,	8,	0,	0x1454	), // 15x15 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060346,	2119,	1059,	2437,	1218,	42,	(15*16*1536),		0,	9,	0,	0x1455	), // 15x16 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060347,	2119,	1059,	2437,	1218,	42,	(15*17*1536),		0,	9,	0,	0x1456	), // 15x17 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060348,	2119,	1059,	2437,	1218,	42,	(15*18*1536),		0,	10,	0,	0x1457	), // 15x18 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060353,	1350,	675,	1553,	776,	28,	(16*11*1536),		0,	6,	0,	0x145C	), // 16x11 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060354,	1370,	685,	1576,	788,	28,	(16*12*1536),		0,	7,	0,	0x145D	), // 16x12 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060355,	1370,	685,	1576,	788,	28,	(16*13*1536),		0,	7,	0,	0x145E	), // 16x13 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060356,	2119,	1059,	2437,	1218,	42,	(16*14*1536),		0,	8,	0,	0x145F	), // 16x14 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060357,	2119,	1059,	2437,	1218,	42,	(16*15*1536),		0,	8,	0,	0x1460	), // 16x15 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060358,	2119,	1059,	2437,	1218,	42,	(16*16*1536),		0,	9,	0,	0x1461	), // 16x16 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060359,	2119,	1059,	2437,	1218,	42,	(16*17*1536),		0,	9,	0,	0x1462	), // 16x17 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060360,	2119,	1059,	2437,	1218,	42,	(16*18*1536),		0,	10,	0,	0x1463	), // 16x18 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060366,	1370,	685,	1576,	788,	28,	(17*12*1536),		0,	7,	0,	0x1469	), // 17x12 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060367,	2119,	1059,	2437,	1218,	42,	(17*13*1536),		0,	7,	0,	0x146A	), // 17x13 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060368,	2119,	1059,	2437,	1218,	42,	(17*14*1536),		0,	8,	0,	0x146B	), // 17x14 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060369,	2119,	1059,	2437,	1218,	42,	(17*15*1536),		0,	8,	0,	0x146C	), // 17x15 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060370,	2119,	1059,	2437,	1218,	42,	(17*16*1536),		0,	9,	0,	0x146D	), // 17x16 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060371,	2119,	1059,	2437,	1218,	42,	(17*17*1536),		0,	9,	0,	0x146E	), // 17x17 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060372,	2119,	1059,	2437,	1218,	42,	(17*18*1536),		0,	10,	0,	0x146F	), // 17x18 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060379,	2119,	1059,	2437,	1218,	42,	(18*13*1536),		0,	7,	0,	0x1476	), // 18x13 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060380,	2119,	1059,	2437,	1218,	42,	(18*14*1536),		0,	8,	0,	0x1477	), // 18x14 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060381,	2119,	1059,	2437,	1218,	42,	(18*15*1536),		0,	8,	0,	0x1478	), // 18x15 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060382,	2119,	1059,	2437,	1218,	42,	(18*16*1536),		0,	9,	0,	0x1479	), // 18x16 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060383,	2119,	1059,	2437,	1218,	42,	(18*17*1536),		0,	9,	0,	0x147A	), // 18x17 3-Story Customizable House
				new HousePlacementEntry( typeof( HouseFoundation ),		1060384,	2119,	1059,	2437,	1218,	42, (18*18*1536),		0,	10,	0,	0x147B	)  // 18x18 3-Story Customizable House
			};

		public static HousePlacementEntry[] ThreeStoryFoundations{ get{ return m_ThreeStoryFoundations; } }
	}
}
