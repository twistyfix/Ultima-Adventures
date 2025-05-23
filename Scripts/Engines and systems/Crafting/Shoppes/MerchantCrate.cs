using System;
using Server;
using System.Collections;
using System.Collections.Generic;
using Server.Multis;
using Server.ContextMenus;
using Server.Misc;
using Server.Network;
using Server.Items;
using Server.Gumps;
using Server.Mobiles;
using Server.Commands;

namespace Server.Items
{
	[Furniture]
	[Flipable( 0xE3D, 0xE3C )]
	public class MerchantCrate : Container
	{
		public override bool DisplaysContent{ get{ return false; } }
		public override bool DisplayWeight{ get{ return false; } }

		public override int DefaultMaxWeight{ get{ return 0; } } // A value of 0 signals unlimited weight

		public override bool IsDecoContainer{ get{ return false; } }

		public int CrateGold;

		[CommandProperty(AccessLevel.Owner)]
		public int Crate_Gold{ get { return CrateGold; } set { CrateGold = value; InvalidateProperties(); } }

		[Constructable]
		public MerchantCrate() : base( 0xE3D )
		{
			Name = "merchant crate";
			Hue = 0x83F;
			Weight = 1.0;
		}

		public MerchantCrate( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
            writer.Write( CrateGold );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			CrateGold = reader.ReadInt();
			QuickTimer thisTimer = new QuickTimer( this ); 
			thisTimer.Start();
		}

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list ) 
		{ 
			base.GetContextMenuEntries( from, list ); 
			list.Add( new SpeechGumpEntry( from ) );
			if ( !this.Movable && BaseHouse.CheckAccessible( from, this ) == true ){ list.Add( new CashOutEntry( from, this ) ); }
		} 

		public class SpeechGumpEntry : ContextMenuEntry
		{
			private Mobile m_Mobile;
			
			public SpeechGumpEntry( Mobile from ) : base( 6121, 3 )
			{
				m_Mobile = from;
			}

			public override void OnClick()
			{
			    if( !( m_Mobile is PlayerMobile ) )
				return;
				
				PlayerMobile mobile = (PlayerMobile) m_Mobile;
				{
					if ( ! mobile.HasGump( typeof( SpeechGump ) ) )
					{
						mobile.SendGump(new SpeechGump( "A Hard Day's Work", SpeechFunctions.SpeechText( m_Mobile.Name, m_Mobile.Name, "MerchantCrate" ) ));
					}
				}
            }
        }

		public class CashOutEntry : ContextMenuEntry
		{
			private Mobile m_Mobile;
			private MerchantCrate m_Crate;
	
			public CashOutEntry( Mobile from, MerchantCrate crate ) : base( 6113, 3 )
			{
				m_Mobile = from;
				m_Crate = crate;
			}

			public override void OnClick()
			{
			    if( !( m_Mobile is PlayerMobile ) )
				return;
				
				PlayerMobile mobile = (PlayerMobile) m_Mobile;
				{
					if ( m_Crate.CrateGold > 0 )
					{
						int barter = (int)m_Mobile.Skills[SkillName.ItemID].Value;
						if ( mobile.NpcGuild == NpcGuild.MerchantsGuild ){ barter = barter + 25; } // FOR GUILD MEMBERS

						int cash = (int)( m_Crate.CrateGold + (m_Crate.CrateGold * (barter / 100) ) );

						m_Mobile.AddToBackpack( new BankCheck( cash ) );
						m_Mobile.SendMessage("You now have a check for " + cash.ToString() + " gold.");
						m_Crate.CrateGold = 0;
						m_Crate.InvalidateProperties();
					}
					else
					{
						m_Mobile.SendMessage("There is no gold in this crate!");
					}
				}
            }
        }

		public override void OnDoubleClick( Mobile from )
		{
			if ( CrateGold >= 500000 )
			{
                from.SendMessage("There is too much gold in here. You need to transfer it out first.");
			}
            else if ( this.Movable )
			{
                from.SendMessage("This must be locked down in a house to use!");
			}
			else if ( from.AccessLevel > AccessLevel.Player || from.InRange( this.GetWorldLocation(), 2 ) )
			{
				Open( from );
			}
			else
			{
				from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 1019045 ); // I can't reach that.
			}
		}

		public virtual void Open( Mobile from )
		{
			DisplayTo( from );
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			list.Add( 1049644, "Contains: " + CrateGold.ToString() + " Gold");
			list.Add( 1070722, "For Sale: " + Sale().ToString() + " Gold");
        }

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if ( CrateGold >= 500000 )
			{
                from.SendMessage("There is too much gold in here. You need to transfer it out first.");
				return false;
			}
            else if (this.Movable)
			{
                from.SendMessage("This must be locked down in a house to use!");
				return false;
			}
			else if ( from.Kills > 0 && !(this.Map == Map.Ilshenar && this.X <= 1007 && this.Y <= 1280))
			{
                from.SendMessage("This is useless since no one deals with murderers!");
				return false;
			
			}

			if ( !base.OnDragDrop( from, dropped ) )
				return false;

			from.SendMessage( "The item will be picked up in about a day" );
			PublicOverheadMessage (MessageType.Regular, 0x3B2, true, "Worth " + GetItemValue( dropped, dropped.Amount ).ToString() + " gold");

			if ( m_Timer != null )
				m_Timer.Stop();
			else
				m_Timer = new EmptyTimer( this );

			m_Timer.Start();

			return true;
		}

		public override bool OnDragDropInto( Mobile from, Item item, Point3D p )
		{
			if ( CrateGold >= 500000 )
			{
                from.SendMessage("There is too much gold in here. You need to transfer it out first.");
				return false;
			}
            else if (this.Movable)
			{
                from.SendMessage("This must be locked down in a house to use!");
				return false;
			}

			if ( !base.OnDragDropInto( from, item, p ) )
				return false;

			from.SendMessage( "The item will be picked up in about a day" );
			PublicOverheadMessage (MessageType.Regular, 0x3B2, true, "Worth " + GetItemValue( item, item.Amount ).ToString() + " gold");

			if ( m_Timer != null )
				m_Timer.Stop();
			else
				m_Timer = new EmptyTimer( this );

			m_Timer.Start();

			return true;
		}

		public void Empty()
		{
            if (!this.Movable)
			{
				List<Item> items = this.Items;

				if ( items.Count > 0 )
				{
					PublicOverheadMessage (MessageType.Regular, 0x3B2, true, "The items have been picked up");

					for ( int i = items.Count - 1; i >= 0; --i )
					{
						if ( i >= items.Count )
							continue;

						CrateGold = CrateGold + GetItemValue( items[i], items[i].Amount );
						items[i].Delete();
					}
				}
			}

			if ( m_Timer != null )
				m_Timer.Stop();

			m_Timer = null;
		}

		public int Sale()
		{
			int gold = 0;

			List<Item> items = this.Items;

			if ( items.Count > 0 )
			{
				for ( int i = items.Count - 1; i >= 0; --i )
				{
					if ( i >= items.Count )
						continue;

					gold = gold + GetItemValue( items[i], items[i].Amount );
				}
			}

			return gold;
		}

		private Timer m_Timer;

		private class EmptyTimer : Timer
		{
			private MerchantCrate m_Crate;

			public EmptyTimer( MerchantCrate crate ) : base( TimeSpan.FromHours( 2.0 ) )
			{
				m_Crate = crate;
				Priority = TimerPriority.FiveSeconds;
			}

			protected override void OnTick()
			{
				m_Crate.Empty();
			}
		}

		private class QuickTimer : Timer
		{
			private MerchantCrate m_Crate;

			public QuickTimer( MerchantCrate crate ) : base( TimeSpan.FromSeconds( 60.0 ) )
			{
				m_Crate = crate;
				Priority = TimerPriority.FiveSeconds;
			}

			protected override void OnTick()
			{
				m_Crate.Empty();
			}
		}

		public static int GetPrice( Item item, int price )
		{
			if ( item is BaseArmor ) {
				BaseArmor armor = (BaseArmor)item;

				if ( armor.Quality == ArmorQuality.Low )
					price = (int)( price * 0.60 );
				else if ( armor.Quality == ArmorQuality.Exceptional )
					price = (int)( price * 1.25 );

				price = (int)(price * CraftResources.GetValueMultiplier(armor.Resource));

				price += 100 * (int)armor.Durability;

				price += 100 * (int)armor.ProtectionLevel;

				if ( price < 1 )
					price = 1;

				if ( armor.PlayerConstructed == false )
					price = 0;
			}
			else if ( item is BaseWeapon ) {
				BaseWeapon weapon = (BaseWeapon)item;

				if ( weapon.Quality == WeaponQuality.Low )
					price = (int)( price * 0.60 );
				else if ( weapon.Quality == WeaponQuality.Exceptional )
					price = (int)( price * 1.25 );

                price = (int)(price * CraftResources.GetValueMultiplier(weapon.Resource));

				price += 100 * (int)weapon.DurabilityLevel;

				price += 100 * (int)weapon.DamageLevel;

				if ( price < 1 )
					price = 1;

				if ( weapon.PlayerConstructed == false )
					price = 0;
			}
			else if ( item is BaseInstrument ) {
				BaseInstrument lute = (BaseInstrument)item;

				if ( lute.Quality == InstrumentQuality.Low )
					price = (int)( price * 0.60 );
				else if ( lute.Quality == InstrumentQuality.Exceptional )
					price = (int)( price * 1.25 );

                price = (int)(price * CraftResources.GetValueMultiplier(lute.Resource));

				if ( price < 1 )
					price = 1;

				if ( lute.UsesRemaining < 300 )
					price = 0;

				if ( lute.Crafter == null )
					price = 0;
			}
			else if ( item is BaseClothing ) {
				BaseClothing cloth = (BaseClothing)item;

				if ( cloth.PlayerConstructed == false )
					price = 0;
			}
			else if ( item is BaseTool ) {
				BaseTool tool = (BaseTool)item;

				if ( tool.UsesRemaining < 50 )
					price = 0;
			}

			return price;
		}

		public static int GetItemValue( Item item, int amount )
		{
			int gold = 0;

			if ( item is AbbatoirDeed ){ gold = 220 * amount; }
			else if ( item is AlchemyTub ){ gold = 500 * amount; }
			else if ( item is WildStaff ){ gold = 31 * amount; }
			else if ( item is MagicalShortbow ){ gold = 18 * amount; }
			else if ( item is ElvenCompositeLongbow ){ gold = 18 * amount; }
			else if ( item is WarCleaver ){ gold = 10 * amount; }
			else if ( item is AssassinSpike ){ gold = 10 * amount; }
			else if ( item is Leafblade ){ gold = 12 * amount; }
			else if ( item is OrnateAxe ){ gold = 21 * amount; }
			else if ( item is RuneBlade ){ gold = 27 * amount; }
			else if ( item is RadiantScimitar ){ gold = 17 * amount; }
			else if ( item is ElvenSpellblade ){ gold = 16 * amount; }
			else if ( item is ElvenMachete ){ gold = 17 * amount; }
			else if ( item is DiamondMace ){ gold = 15 * amount; }
			else if ( item is AgapiteIngot ){ gold = 10 * amount; }
			else if ( item is AgilityPotion ){ gold = 7 * amount; }
			else if ( item is AgilityScroll ){ gold = 10 * amount; }
			else if ( item is AmethystFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is AmethystIngot ){ gold = 120 * amount; }
			else if ( item is AmethystPlateArms ){ gold = 470 * amount; }
			else if ( item is AmethystPlateChest ){ gold = 605 * amount; }
			else if ( item is AmethystPlateGloves ){ gold = 360 * amount; }
			else if ( item is AmethystPlateGorget ){ gold = 260 * amount; }
			else if ( item is AmethystPlateHelm ){ gold = 330 * amount; }
			else if ( item is AmethystPlateLegs ){ gold = 545 * amount; }
			else if ( item is AmethystShield ){ gold = 575 * amount; }
			else if ( item is AniLargeVioletFlask ){ gold = 50 * amount; }
			else if ( item is AnimateDeadScroll ){ gold = 15 * amount; }
			else if ( item is AniRedRibbedFlask ){ gold = 55 * amount; }
			else if ( item is AniSmallBlueFlask ){ gold = 50 * amount; }
			else if ( item is AnvilEastDeed ){ gold = 26 * amount; }
			else if ( item is AnvilSouthDeed ){ gold = 26 * amount; }
			else if ( item is ApplePie ){ gold = 15 * amount; }
			else if ( item is ArchCureScroll ){ gold = 20 * amount; }
			else if ( item is ArchProtectionScroll ){ gold = 20 * amount; }
			else if ( item is Armoire ){ gold = 88 * amount; }
			else if ( item is Arrow ){ gold = 1 * amount; }
			else if ( item is AutoResPotion ){ gold = 94 * amount; }
			else if ( item is AwaseMisoSoup ){ gold = 16 * amount; }
			else if ( item is Axe ){ gold = 20 * amount; }
			else if ( item is Axle ){ gold = 1 * amount; }
			else if ( item is AxleGears ){ gold = 1 * amount; }
			else if ( item is BallotBoxDeed ){ gold = 22 * amount; }
			else if ( item is BambooChair ){ gold = 33 * amount; }
			else if ( item is BambooFlute ){ gold = 47 * amount; }
			else if ( item is BambooScreen ){ gold = 167 * amount; }
			else if ( item is Bandana ){ gold = 3 * amount; }
			else if ( item is BarrelHoops ){ gold = 4 * amount; }
			else if ( item is BarrelLid ){ gold = 14 * amount; }
			else if ( item is BarrelStaves ){ gold = 14 * amount; }
			else if ( item is BarrelTap ){ gold = 2 * amount; }
			else if ( item is Bascinet ){ gold = 9 * amount; }
			else if ( item is BaseMixture ){ gold = 28 * amount; }
			else if ( item is BaseElixir ){ gold = 28 * amount; }
			else if ( item is BaseLiquid ){ gold = 28 * amount; }
			else if ( item is BattleAxe ){ gold = 13 * amount; }
			else if ( item is Beads ){ gold = 13 * amount; }
			else if ( item is Beeswax ){ gold = 50 * amount; }
			else if ( item is BladedStaff ){ gold = 16 * amount; }
			else if ( item is BladeSpiritsScroll ){ gold = 25 * amount; }
			else if ( item is BlendWithForestPotion ){ gold = 28 * amount; }
			else if ( item is BlessScroll ){ gold = 15 * amount; }
			else if ( item is BloodOathScroll ){ gold = 5 * amount; }
			else if ( item is BloodPactScroll ){ gold = 28 * amount; }
			else if ( item is Board ){ gold = 3 * amount; }
			else if ( item is AshBoard ){ gold = 4 * amount; }
			else if ( item is CherryBoard ){ gold = 5 * amount; }
			else if ( item is EbonyBoard ){ gold = 6 * amount; }
			else if ( item is GoldenOakBoard ){ gold = 7 * amount; }
			else if ( item is HickoryBoard ){ gold = 8 * amount; }
			else if ( item is MahoganyBoard ){ gold = 9 * amount; }
			else if ( item is OakBoard ){ gold = 10 * amount; }
			else if ( item is PineBoard ){ gold = 11 * amount; }
			else if ( item is GhostBoard ){ gold = 11 * amount; }
			else if ( item is RosewoodBoard ){ gold = 12 * amount; }
			else if ( item is WalnutBoard ){ gold = 13 * amount; }
			else if ( item is ElvenBoard ){ gold = 26 * amount; }
			else if ( item is PetrifiedBoard ){ gold = 14 * amount; }
			else if ( item is BodySash ){ gold = 3 * amount; }
			else if ( item is Bokuto ){ gold = 10 * amount; }
			else if ( item is Bokuto ){ gold = 27 * amount; }
			else if ( item is BolaBall ){ gold = 3 * amount; }
			else if ( item is Bolt ){ gold = 1 * amount; }
			else if ( item is BoneArms ){ gold = 43 * amount; }
			else if ( item is BoneChest ){ gold = 64 * amount; }
			else if ( item is BoneGloves ){ gold = 39 * amount; }
			else if ( item is BoneHarvester ){ gold = 17 * amount; }
			else if ( item is BoneHelm ){ gold = 12 * amount; }
			else if ( item is BoneLegs ){ gold = 51 * amount; }
			else if ( item is BoneSkirt ){ gold = 51 * amount; }
			else if ( item is Bonnet ){ gold = 4 * amount; }
			else if ( item is Boots ){ gold = 5 * amount; }
			else if ( item is BottleOfAcid ){ gold = 32 * amount; }
			else if ( item is Bow ){ gold = 17 * amount; }
			else if ( item is BreadLoaf ){ gold = 15 * amount; }
			else if ( item is Broadsword ){ gold = 17 * amount; }
			else if ( item is BrocadeGozaMatEastDeed ){ gold = 35 * amount; }
			else if ( item is BrocadeGozaMatSouthDeed ){ gold = 35 * amount; }
			else if ( item is BrocadeSquareGozaMatEastDeed ){ gold = 35 * amount; }
			else if ( item is BrocadeSquareGozaMatSouthDeed ){ gold = 35 * amount; }
			else if ( item is BronzeIngot ){ gold = 8 * amount; }
			else if ( item is BronzeShield ){ gold = 33 * amount; }
			else if ( item is Buckler ){ gold = 25 * amount; }
			else if ( item is Engines.BulkOrders.BulkOrderBook ){ gold = 25 * amount; }
			else if ( item is ButcherKnife ){ gold = 7 * amount; }
			else if ( item is CaddelliteFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is CaddellitePlateArms ){ gold = 470 * amount; }
			else if ( item is CaddellitePlateChest ){ gold = 605 * amount; }
			else if ( item is CaddellitePlateGloves ){ gold = 360 * amount; }
			else if ( item is CaddellitePlateGorget ){ gold = 260 * amount; }
			else if ( item is CaddellitePlateHelm ){ gold = 330 * amount; }
			else if ( item is CaddellitePlateLegs ){ gold = 545 * amount; }
			else if ( item is CaddelliteShield ){ gold = 575 * amount; }
			else if ( item is Cake ){ gold = 15 * amount; }
			else if ( item is CakeMix ){ gold = 15 * amount; }
			else if ( item is ColorCandleShort ){ gold = 85 * amount; }
			else if ( item is ColorCandleLong ){ gold = 90 * amount; }
			else if ( item is Candle ){ gold = 3 * amount; }
			else if ( item is CandleLarge ){ gold = 70 * amount; }
			else if ( item is Candelabra ){ gold = 140 * amount; }
			else if ( item is CandelabraStand ){ gold = 210 * amount; }
			else if ( item is CandleLong ){ gold = 80 * amount; }
			else if ( item is CandleShort ){ gold = 75 * amount; }
			else if ( item is CandleSkull ){ gold = 95 * amount; }
			else if ( item is CandleReligious ){ gold = 120 * amount; }
			else if ( item is Cap ){ gold = 5 * amount; }
			else if ( item is CarvedPumpkin ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin2 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin3 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin4 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin5 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin6 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin7 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin8 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin9 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin10 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin11 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin12 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin13 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin14 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin15 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin16 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin17 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin18 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin19 ){ gold = 17 * amount; }
			else if ( item is CarvedPumpkin20 ){ gold = 20 * amount; }
			else if ( item is WoodenCoffin ){ gold = 25 * amount; }
			else if ( item is WoodenCasket ){ gold = 25 * amount; }
			else if ( item is StoneCoffin ){ gold = 45 * amount; }
			else if ( item is StoneCasket ){ gold = 45 * amount; }
			else if ( item is RockUrn ){ gold = 30 * amount; }
			else if ( item is RockVase ){ gold = 30 * amount; }
			else if ( item is StoneOrnateUrn ){ gold = 40 * amount; }
			else if ( item is StoneOrnateAmphora ){ gold = 50 * amount; }
			else if ( item is ChainChest ){ gold = 71 * amount; }
			else if ( item is ChainCoif ){ gold = 6 * amount; }
			else if ( item is ChainHatsuburi ){ gold = 38 * amount; }
			else if ( item is ChainLegs ){ gold = 74 * amount; }
			else if ( item is ChainSkirt ){ gold = 74 * amount; }
			else if ( item is ChainLightningScroll ){ gold = 35 * amount; }
			else if ( item is ChaosShield ){ gold = 115 * amount; }
			else if ( item is CheesePizza ){ gold = 15 * amount; }
			else if ( item is CherryArmoire ){ gold = 99 * amount; }
			else if ( item is ChickenLeg ){ gold = 15 * amount; }
			else if ( item is CityMap ){ gold = 3 * amount; }
			else if ( item is Cleaver ){ gold = 7 * amount; }
			else if ( item is Cloak ){ gold = 4 * amount; }
			else if ( item is ClockFrame ){ gold = 12 * amount; }
			else if ( item is ClockLeft ){ gold = 5 * amount; }
			else if ( item is ClockParts ){ gold = 1 * amount; }
			else if ( item is ClockRight ){ gold = 5 * amount; }
			else if ( item is CloseHelm ){ gold = 9 * amount; }
			else if ( item is ClothNinjaHood ){ gold = 16 * amount; }
			else if ( item is ClothNinjaJacket ){ gold = 12 * amount; }
			else if ( item is ClothHood ){ gold = 6 * amount; }
			else if ( item is ClothCowl ){ gold = 6 * amount; }
			else if ( item is FancyHood ){ gold = 6 * amount; }
			else if ( item is Club ){ gold = 8 * amount; }
			else if ( item is ClumsyScroll ){ gold = 5 * amount; }
			else if ( item is CompositeBow ){ gold = 23 * amount; }
			else if ( item is ConflagrationPotion ){ gold = 7 * amount; }
			else if ( item is FrostbitePotion ){ gold = 7 * amount; }
			else if ( item is ConfusionBlastPotion ){ gold = 7 * amount; }
			else if ( item is CookedBird ){ gold = 15 * amount; }
			else if ( item is CookieMix ){ gold = 15 * amount; }
			else if ( item is Cookies ){ gold = 15 * amount; }
			else if ( item is CopperIngot ){ gold = 7 * amount; }
			else if ( item is CorpseSkinScroll ){ gold = 5 * amount; }
			else if ( item is CreateFoodScroll ){ gold = 5 * amount; }
			else if ( item is CrescentBlade ){ gold = 18 * amount; }
			else if ( item is Crossbow ){ gold = 25 * amount; }
			else if ( item is CunningScroll ){ gold = 10 * amount; }
			else if ( item is CurePotion ){ gold = 14 * amount; }
			else if ( item is CureScroll ){ gold = 10 * amount; }
			else if ( item is CurseScroll ){ gold = 20 * amount; }
			else if ( item is CurseWeaponScroll ){ gold = 5 * amount; }
			else if ( item is CurvedFlask ){ gold = 35 * amount; }
			else if ( item is Cutlass ){ gold = 12 * amount; }
			else if ( item is Daisho ){ gold = 33 * amount; }
			else if ( item is Dagger ){ gold = 10 * amount; }
			else if ( item is DartBoardEastDeed ){ gold = 16 * amount; }
			else if ( item is DartBoardSouthDeed ){ gold = 16 * amount; }
			else if ( item is DeadlyPoisonPotion ){ gold = 28 * amount; }
			else if ( item is DecorativePlateKabuto ){ gold = 47 * amount; }
			else if ( item is DispelFieldScroll ){ gold = 25 * amount; }
			else if ( item is DispelScroll ){ gold = 30 * amount; }
			else if ( item is DoubleAxe ){ gold = 26 * amount; }
			else if ( item is DoubleBladedStaff ){ gold = 17 * amount; }
			else if ( item is Doublet ){ gold = 7 * amount; }
			else if ( item is Dough ){ gold = 15 * amount; }
			else if ( item is DovetailSaw ){ gold = 7 * amount; }
			else if ( item is DragonArms ){ gold = 94 * amount; }
			else if ( item is DragonChest ){ gold = 121 * amount; }
			else if ( item is DragonGloves ){ gold = 72 * amount; }
			else if ( item is DragonHelm ){ gold = 10 * amount; }
			else if ( item is DragonLegs ){ gold = 109 * amount; }
			else if ( item is DrawKnife ){ gold = 6 * amount; }
			else if ( item is Dressform ){ gold = 64 * amount; }
			else if ( item is DruidicRunePotion ){ gold = 28 * amount; }
			else if ( item is Drums ){ gold = 53 * amount; }
			else if ( item is DullCopperIngot ){ gold = 5 * amount; }
			else if ( item is EarthquakeScroll ){ gold = 40 * amount; }
			else if ( item is Easle ){ gold = 58 * amount; }
			else if ( item is EggBomb ){ gold = 17 * amount; }
			else if ( item is ElegantArmoire ){ gold = 99 * amount; }
			else if ( item is ElegantLowTable ){ gold = 87 * amount; }
			else if ( item is EmeraldFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is EmeraldIngot ){ gold = 120 * amount; }
			else if ( item is EmeraldPlateArms ){ gold = 470 * amount; }
			else if ( item is EmeraldPlateChest ){ gold = 605 * amount; }
			else if ( item is EmeraldPlateGloves ){ gold = 360 * amount; }
			else if ( item is EmeraldPlateGorget ){ gold = 260 * amount; }
			else if ( item is EmeraldPlateHelm ){ gold = 330 * amount; }
			else if ( item is EmeraldPlateLegs ){ gold = 545 * amount; }
			else if ( item is EmeraldShield ){ gold = 575 * amount; }
			else if ( item is EmptyBookcase ){ gold = 59 * amount; }
			else if ( item is EmptyVialsWRack ){ gold = 60 * amount; }
			else if ( item is EnergyBoltScroll ){ gold = 30 * amount; }
			else if ( item is EnergyFieldScroll ){ gold = 35 * amount; }
			else if ( item is EnergyVortexScroll ){ gold = 40 * amount; }
			else if ( item is EvilOmenScroll ){ gold = 5 * amount; }
			else if ( item is ExecutionersAxe ){ gold = 15 * amount; }
			else if ( item is ExorcismScroll ){ gold = 35 * amount; }
			else if ( item is ExplosionPotion ){ gold = 14 * amount; }
			else if ( item is ExplosionScroll ){ gold = 30 * amount; }
			else if ( item is FancyArmoire ){ gold = 88 * amount; }
			else if ( item is FancyDress ){ gold = 12 * amount; }
			else if ( item is FancyShirt ){ gold = 10 * amount; }
			else if ( item is FancyWindChimes ){ gold = 10 * amount; }
			else if ( item is FancyWoodenChairCushion ){ gold = 41 * amount; }
			else if ( item is FeatheredHat ){ gold = 5 * amount; }
			else if ( item is FeeblemindScroll ){ gold = 5 * amount; }
			else if ( item is FemaleKimono ){ gold = 9 * amount; }
			else if ( item is FemaleLeatherChest ){ gold = 18 * amount; }
			else if ( item is FemalePlateChest ){ gold = 113 * amount; }
			else if ( item is FemaleStuddedChest ){ gold = 25 * amount; }
			else if ( item is FinishedWoodenChest ){ gold = 79 * amount; }
			else if ( item is FireballScroll ){ gold = 15 * amount; }
			else if ( item is FireFieldScroll ){ gold = 20 * amount; }
			else if ( item is FireflyPotion ){ gold = 28 * amount; }
			else if ( item is FishingPole ){ gold = 25 * amount; }
			else if ( item is FishSteak ){ gold = 15 * amount; }
			else if ( item is FlamestrikeScroll ){ gold = 35 * amount; }
			else if ( item is FletcherTools ){ gold = 1 * amount; }
			else if ( item is FloppyHat ){ gold = 3 * amount; }
			else if ( item is FlourMillEastDeed ){ gold = 219 * amount; }
			else if ( item is FlourMillSouthDeed ){ gold = 219 * amount; }
			else if ( item is FlourSifter ){ gold = 1 * amount; }
			else if ( item is FlowerGarland ){ gold = 5 * amount; }
			else if ( item is FootStool ){ gold = 24 * amount; }
			else if ( item is ForkLeft ){ gold = 1 * amount; }
			else if ( item is ForkRight ){ gold = 1 * amount; }
			else if ( item is FormalShirt ){ gold = 7 * amount; }
			else if ( item is FriedEggs ){ gold = 15 * amount; }
			else if ( item is Froe ){ gold = 6 * amount; }
			else if ( item is FruitPie ){ gold = 15 * amount; }
			else if ( item is Fukiya ){ gold = 10 * amount; }
			else if ( item is Fukiya ){ gold = 26 * amount; }
			else if ( item is FukiyaDarts ){ gold = 1 * amount; }
			else if ( item is FullApron ){ gold = 5 * amount; }
			else if ( item is FullBookcase ){ gold = 59 * amount; }
			else if ( item is FullVialsWRack ){ gold = 65 * amount; }
			else if ( item is FurArms ){ gold = 50 * amount; }
			else if ( item is FurTunic ){ gold = 60 * amount; }
			else if ( item is FurLegs ){ gold = 50 * amount; }
			else if ( item is FurBoots ){ gold = 5 * amount; }
			else if ( item is FurCap ){ gold = 4 * amount; }
			else if ( item is FurCape ){ gold = 8 * amount; }
			else if ( item is FurRobe ){ gold = 10 * amount; }
			else if ( item is FurSarong ){ gold = 7 * amount; }
			else if ( item is GardenTool ){ gold = 6 * amount; }
			else if ( item is GarnetFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is GarnetIngot ){ gold = 120 * amount; }
			else if ( item is GarnetPlateArms ){ gold = 470 * amount; }
			else if ( item is GarnetPlateChest ){ gold = 605 * amount; }
			else if ( item is GarnetPlateGloves ){ gold = 360 * amount; }
			else if ( item is GarnetPlateGorget ){ gold = 260 * amount; }
			else if ( item is GarnetPlateHelm ){ gold = 330 * amount; }
			else if ( item is GarnetPlateLegs ){ gold = 545 * amount; }
			else if ( item is GarnetShield ){ gold = 575 * amount; }
			else if ( item is GateTravelScroll ){ gold = 35 * amount; }
			else if ( item is Gears ){ gold = 1 * amount; }
			else if ( item is GhostlyImagesScroll ){ gold = 28 * amount; }
			else if ( item is GhostPhaseScroll ){ gold = 28 * amount; }
			else if ( item is GildedDress ){ gold = 12 * amount; }
			else if ( item is GildedWoodenChest ){ gold = 79 * amount; }
			else if ( item is Globe ){ gold = 4 * amount; }
			else if ( item is GnarledStaff ){ gold = 31 * amount; }
			else if ( item is Goblet ){ gold = 2 * amount; }
			else if ( item is GoldBeadNecklace ){ gold = 13 * amount; }
			else if ( item is GoldBracelet ){ gold = 13 * amount; }
			else if ( item is GoldEarrings ){ gold = 13 * amount; }
			else if ( item is GoldIngot ){ gold = 9 * amount; }
			else if ( item is GoldNecklace ){ gold = 13 * amount; }
			else if ( item is GoldRing ){ gold = 13 * amount; }
			else if ( item is GozaMatEastDeed ){ gold = 35 * amount; }
			else if ( item is GozaMatSouthDeed ){ gold = 35 * amount; }
			else if ( item is GraspingRootsPotion ){ gold = 28 * amount; }
			else if ( item is GraveShovel ){ gold = 12 * amount; }
			else if ( item is GraveyardGatewayScroll ){ gold = 28 * amount; }
			else if ( item is GreaterAgilityPotion ){ gold = 28 * amount; }
			else if ( item is GreaterConflagrationPotion ){ gold = 28 * amount; }
			else if ( item is GreaterFrostbitePotion ){ gold = 28 * amount; }
			else if ( item is GreaterConfusionBlastPotion ){ gold = 28 * amount; }
			else if ( item is GreaterCurePotion ){ gold = 28 * amount; }
			else if ( item is GreaterExplosionPotion ){ gold = 28 * amount; }
			else if ( item is GreaterHealPotion ){ gold = 28 * amount; }
			else if ( item is GreaterHealScroll ){ gold = 20 * amount; }
			else if ( item is GreaterInvisibilityPotion ){ gold = 28 * amount; }
			else if ( item is GreaterManaPotion ){ gold = 28 * amount; }
			else if ( item is GreaterPoisonPotion ){ gold = 28 * amount; }
			else if ( item is GreaterRejuvenatePotion ){ gold = 28 * amount; }
			else if ( item is GreaterStrengthPotion ){ gold = 28 * amount; }
			else if ( item is GreenTea ){ gold = 18 * amount; }
			else if ( item is Hakama ){ gold = 6 * amount; }
			else if ( item is HakamaShita ){ gold = 8 * amount; }
			else if ( item is HalfApron ){ gold = 5 * amount; }
			else if ( item is Hammer ){ gold = 14 * amount; }
			else if ( item is HammerPick ){ gold = 13 * amount; }
			else if ( item is HarmScroll ){ gold = 10 * amount; }
			else if ( item is Harp ){ gold = 87 * amount; }
			else if ( item is Hatchet ){ gold = 13 * amount; }
			else if ( item is HealPotion ){ gold = 14 * amount; }
			else if ( item is HealScroll ){ gold = 5 * amount; }
			else if ( item is HeaterShield ){ gold = 115 * amount; }
			else if ( item is GuardsmanShield ){ gold = 115 * amount; }
			else if ( item is ElvenShield ){ gold = 115 * amount; }
			else if ( item is DarkShield ){ gold = 115 * amount; }
			else if ( item is CrestedShield ){ gold = 115 * amount; }
			else if ( item is ChampionShield ){ gold = 115 * amount; }
			else if ( item is JeweledShield ){ gold = 115 * amount; }
			else if ( item is HeatingStand ){ gold = 2 * amount; }
			else if ( item is HeavyCrossbow ){ gold = 27 * amount; }
			else if ( item is HeavyPlateJingasa ){ gold = 38 * amount; }
			else if ( item is HellsBrandScroll ){ gold = 28 * amount; }
			else if ( item is HellsGateScroll ){ gold = 28 * amount; }
			else if ( item is Helmet ){ gold = 9 * amount; }
			else if ( item is HerbalHealingPotion ){ gold = 28 * amount; }
			else if ( item is HerbalistCauldron ){ gold = 123 * amount; }
			else if ( item is Hinge ){ gold = 1 * amount; }
			else if ( item is HorrificBeastScroll ){ gold = 15 * amount; }
			else if ( item is IceFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is IceIngot ){ gold = 120 * amount; }
			else if ( item is IcePlateArms ){ gold = 470 * amount; }
			else if ( item is IcePlateChest ){ gold = 605 * amount; }
			else if ( item is IcePlateGloves ){ gold = 360 * amount; }
			else if ( item is IcePlateGorget ){ gold = 260 * amount; }
			else if ( item is IcePlateHelm ){ gold = 330 * amount; }
			else if ( item is IcePlateLegs ){ gold = 545 * amount; }
			else if ( item is IceShield ){ gold = 575 * amount; }
			else if ( item is IncognitoScroll ){ gold = 25 * amount; }
			else if ( item is Inshave ){ gold = 6 * amount; }
			else if ( item is WoodworkingTools ){ gold = 6 * amount; }
			else if ( item is InvisibilityPotion ){ gold = 14 * amount; }
			else if ( item is InvisibilityScroll ){ gold = 30 * amount; }
			else if ( item is InvulnerabilityPotion ){ gold = 53 * amount; }
			else if ( item is IronIngot ){ gold = 4 * amount; }
			else if ( item is JadeFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is JadeIngot ){ gold = 120 * amount; }
			else if ( item is JadePlateArms ){ gold = 470 * amount; }
			else if ( item is JadePlateChest ){ gold = 605 * amount; }
			else if ( item is JadePlateGloves ){ gold = 360 * amount; }
			else if ( item is JadePlateGorget ){ gold = 260 * amount; }
			else if ( item is JadePlateHelm ){ gold = 330 * amount; }
			else if ( item is JadePlateLegs ){ gold = 545 * amount; }
			else if ( item is JadeShield ){ gold = 575 * amount; }
			else if ( item is JarHoney ){ gold = 300 * amount; }
			else if ( item is JarsOfWaxInstrument ){ gold = 80 * amount; }
			else if ( item is JarsOfWaxLeather ){ gold = 80 * amount; }
			else if ( item is JarsOfWaxMetal ){ gold = 80 * amount; }
			else if ( item is JesterHat ){ gold = 6 * amount; }
			else if ( item is JesterSuit ){ gold = 13 * amount; }
			else if ( item is JinBaori ){ gold = 10 * amount; }
			else if ( item is JointingPlane ){ gold = 6 * amount; }
			else if ( item is Kama ){ gold = 30 * amount; }
			else if ( item is Kamishimo ){ gold = 8 * amount; }
			else if ( item is Kasa ){ gold = 15 * amount; }
			else if ( item is Katana ){ gold = 16 * amount; }
			else if ( item is Keg ){ gold = 19 * amount; }
			else if ( item is Key ){ gold = 1 * amount; }
			else if ( item is KeyRing ){ gold = 4 * amount; }
			else if ( item is Kilt ){ gold = 5 * amount; }
			else if ( item is KnifeLeft ){ gold = 1 * amount; }
			else if ( item is KnifeRight ){ gold = 1 * amount; }
			else if ( item is Kryss ){ gold = 16 * amount; }
			else if ( item is Lajatang ){ gold = 54 * amount; }
			else if ( item is LambLeg ){ gold = 15 * amount; }
			else if ( item is Lance ){ gold = 17 * amount; }
			else if ( item is Lantern ){ gold = 1 * amount; }
			else if ( item is LapHarp ){ gold = 54 * amount; }
			else if ( item is LargeBattleAxe ){ gold = 16 * amount; }
			else if ( item is LargeBedEastDeed ){ gold = 319 * amount; }
			else if ( item is LargeBedSouthDeed ){ gold = 319 * amount; }
			else if ( item is LargeCrate ){ gold = 48 * amount; }
			else if ( item is LargeFlask ){ gold = 50 * amount; }
			else if ( item is LargeForgeEastDeed ){ gold = 27 * amount; }
			else if ( item is LargeForgeSouthDeed ){ gold = 27 * amount; }
			else if ( item is LargeStoneTableEastDeed ){ gold = 480 * amount; }
			else if ( item is LargeStoneTableSouthDeed ){ gold = 480 * amount; }
			else if ( item is StoneWizardTable ){ gold = 580 * amount; }
			else if ( item is LargeTable ){ gold = 72 * amount; }
			else if ( item is LargeVase ){ gold = 268 * amount; }
			else if ( item is LeatherArms ){ gold = 40 * amount; }
			else if ( item is LeatherBustierArms ){ gold = 11 * amount; }
			else if ( item is LeatherCap ){ gold = 5 * amount; }
			else if ( item is LeatherChest ){ gold = 52 * amount; }
			else if ( item is LeatherCloak ){ gold = 60 * amount; }
			else if ( item is LeatherSandals ){ gold = 30 * amount; }
			else if ( item is LeatherShoes ){ gold = 37 * amount; }
			else if ( item is LeatherBoots ){ gold = 45 * amount; }
			else if ( item is LeatherThighBoots ){ gold = 52 * amount; }
			else if ( item is LeatherSoftBoots ){ gold = 60 * amount; }
			else if ( item is LeatherRobe ){ gold = 80 * amount; }
			else if ( item is LeatherDo ){ gold = 42 * amount; }
			else if ( item is LeatherGloves ){ gold = 30 * amount; }
			else if ( item is LeatherGorget ){ gold = 37 * amount; }
			else if ( item is LeatherHaidate ){ gold = 28 * amount; }
			else if ( item is LeatherHiroSode ){ gold = 23 * amount; }
			else if ( item is LeatherJingasa ){ gold = 11 * amount; }
			else if ( item is LeatherLegs ){ gold = 40 * amount; }
			else if ( item is LeatherMempo ){ gold = 14 * amount; }
			else if ( item is LeatherNinjaBelt ){ gold = 12 * amount; }
			else if ( item is LeatherNinjaHood ){ gold = 8 * amount; }
			else if ( item is LeatherNinjaJacket ){ gold = 26 * amount; }
			else if ( item is LeatherNinjaMitts ){ gold = 9 * amount; }
			else if ( item is LeatherNinjaPants ){ gold = 25 * amount; }
			else if ( item is LeatherShorts ){ gold = 14 * amount; }
			else if ( item is LeatherSkirt ){ gold = 11 * amount; }
			else if ( item is LeatherSuneate ){ gold = 26 * amount; }
			else if ( item is LesserCurePotion ){ gold = 7 * amount; }
			else if ( item is LesserExplosionPotion ){ gold = 7 * amount; }
			else if ( item is LesserHealPotion ){ gold = 7 * amount; }
			else if ( item is LesserInvisibilityPotion ){ gold = 7 * amount; }
			else if ( item is LesserManaPotion ){ gold = 7 * amount; }
			else if ( item is LesserPoisonPotion ){ gold = 7 * amount; }
			else if ( item is LesserRejuvenatePotion ){ gold = 7 * amount; }
			else if ( item is LichFormScroll ){ gold = 30 * amount; }
			else if ( item is light_wall_torch ){ gold = 25 * amount; }
			else if ( item is WallTorch ){ gold = 25 * amount; }
			else if ( item is ColoredWallTorch ){ gold = 35 * amount; }
			else if ( item is LightningScroll ){ gold = 20 * amount; }
			else if ( item is LightPlateJingasa ){ gold = 28 * amount; }
			else if ( item is LocalMap ){ gold = 6 * amount; }
			else if ( item is Lockpick ){ gold = 6 * amount; }
			else if ( item is Log ){ gold = 2 * amount; }
			else if ( item is AshLog ){ gold = 3 * amount; }
			else if ( item is CherryLog ){ gold = 4 * amount; }
			else if ( item is EbonyLog ){ gold = 5 * amount; }
			else if ( item is GoldenOakLog ){ gold = 6 * amount; }
			else if ( item is HickoryLog ){ gold = 7 * amount; }
			else if ( item is MahoganyLog ){ gold = 8 * amount; }
			else if ( item is OakLog ){ gold = 9 * amount; }
			else if ( item is PineLog ){ gold = 10 * amount; }
			else if ( item is GhostLog ){ gold = 10 * amount; }
			else if ( item is RosewoodLog ){ gold = 11 * amount; }
			else if ( item is WalnutLog ){ gold = 12 * amount; }
			else if ( item is ElvenLog ){ gold = 24 * amount; }
			else if ( item is PetrifiedLog ){ gold = 13 * amount; }
			else if ( item is LongFlask ){ gold = 40 * amount; }
			else if ( item is LongPants ){ gold = 5 * amount; }
			else if ( item is Longsword ){ gold = 27 * amount; }
			else if ( item is LoomEastDeed ){ gold = 188 * amount; }
			else if ( item is LoomSouthDeed ){ gold = 188 * amount; }
			else if ( item is LureStonePotion ){ gold = 28 * amount; }
			else if ( item is Lute ){ gold = 65 * amount; }
			else if ( item is Mace ){ gold = 14 * amount; }
			else if ( item is MagicArrowScroll ){ gold = 5 * amount; }
			else if ( item is MagicLockScroll ){ gold = 15 * amount; }
			else if ( item is MagicReflectScroll ){ gold = 25 * amount; }
			else if ( item is MagicTrapScroll ){ gold = 10 * amount; }
			else if ( item is MagicUnTrapScroll ){ gold = 10 * amount; }
			else if ( item is MaleKimono ){ gold = 9 * amount; }
			else if ( item is ManaDrainScroll ){ gold = 20 * amount; }
			else if ( item is ManaLeechScroll ){ gold = 28 * amount; }
			else if ( item is ManaPotion ){ gold = 14 * amount; }
			else if ( item is ManaVampireScroll ){ gold = 35 * amount; }
			else if ( item is MapleArmoire ){ gold = 99 * amount; }
			else if ( item is MapmakersPen ){ gold = 4 * amount; }
			else if ( item is MapWorld ){ gold = 30 * amount; }
			else if ( item is MarbleFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is MarbleIngot ){ gold = 120 * amount; }
			else if ( item is MarblePlateArms ){ gold = 470 * amount; }
			else if ( item is MarblePlateChest ){ gold = 605 * amount; }
			else if ( item is MarblePlateGloves ){ gold = 360 * amount; }
			else if ( item is MarblePlateGorget ){ gold = 260 * amount; }
			else if ( item is MarblePlateHelm ){ gold = 330 * amount; }
			else if ( item is MarblePlateLegs ){ gold = 545 * amount; }
			else if ( item is MarbleShields ){ gold = 575 * amount; }
			else if ( item is MarkScroll ){ gold = 30 * amount; }
			else if ( item is MarlinSouthAddonDeed ){ gold = 900 * amount; }
			else if ( item is MassCurseScroll ){ gold = 30 * amount; }
			else if ( item is MassDispelScroll ){ gold = 35 * amount; }
			else if ( item is Maul ){ gold = 10 * amount; }
			else if ( item is MeatPie ){ gold = 15 * amount; }
			else if ( item is MediumCrate ){ gold = 39 * amount; }
			else if ( item is MediumFlask ){ gold = 30 * amount; }
			else if ( item is MediumStoneTableEastDeed ){ gold = 380 * amount; }
			else if ( item is MediumStoneTableSouthDeed ){ gold = 380 * amount; }
			else if ( item is MetalKiteShield ){ gold = 62 * amount; }
			else if ( item is MetalShield ){ gold = 60 * amount; }
			else if ( item is MeteorSwarmScroll ){ gold = 35 * amount; }
			else if ( item is MindBlastScroll ){ gold = 25 * amount; }
			else if ( item is MindRotScroll ){ gold = 10 * amount; }
			else if ( item is MisoSoup ){ gold = 16 * amount; }
			else if ( item is MixingCauldron ){ gold = 123 * amount; }
			else if ( item is MixingSpoon ){ gold = 20 * amount; }
			else if ( item is Monocle ){ gold = 6 * amount; }
			else if ( item is MortarPestle ){ gold = 4 * amount; }
			else if ( item is MouldingPlane ){ gold = 6 * amount; }
			else if ( item is Muffins ){ gold = 15 * amount; }
			else if ( item is MushroomGatewayPotion ){ gold = 28 * amount; }
			else if ( item is NaturesPassagePotion ){ gold = 28 * amount; }
			else if ( item is Necklace ){ gold = 13 * amount; }
			else if ( item is NecroCurePoisonScroll ){ gold = 28 * amount; }
			else if ( item is NecromancerSpellbook ){ gold = 30 * amount; }
			else if ( item is NecroPoisonScroll ){ gold = 28 * amount; }
			else if ( item is NecroUnlockScroll ){ gold = 28 * amount; }
			else if ( item is NewArmoireA ){ gold = 59 * amount; }
			else if ( item is NewArmoireB ){ gold = 59 * amount; }
			else if ( item is NewArmoireC ){ gold = 59 * amount; }
			else if ( item is NewArmoireD ){ gold = 59 * amount; }
			else if ( item is NewArmoireE ){ gold = 59 * amount; }
			else if ( item is NewArmoireF ){ gold = 59 * amount; }
			else if ( item is NewArmoireG ){ gold = 59 * amount; }
			else if ( item is NewArmoireH ){ gold = 59 * amount; }
			else if ( item is NewArmoireI ){ gold = 59 * amount; }
			else if ( item is NewArmoireJ ){ gold = 59 * amount; }
			else if ( item is NewArmorShelfA ){ gold = 59 * amount; }
			else if ( item is NewArmorShelfB ){ gold = 59 * amount; }
			else if ( item is NewArmorShelfC ){ gold = 59 * amount; }
			else if ( item is NewArmorShelfD ){ gold = 59 * amount; }
			else if ( item is NewArmorShelfE ){ gold = 59 * amount; }
			else if ( item is NewBakerShelfA ){ gold = 59 * amount; }
			else if ( item is NewBakerShelfB ){ gold = 59 * amount; }
			else if ( item is NewBakerShelfC ){ gold = 59 * amount; }
			else if ( item is NewBakerShelfD ){ gold = 59 * amount; }
			else if ( item is NewBakerShelfE ){ gold = 59 * amount; }
			else if ( item is NewBakerShelfF ){ gold = 59 * amount; }
			else if ( item is NewBakerShelfG ){ gold = 59 * amount; }
			else if ( item is NewBlacksmithShelfA ){ gold = 59 * amount; }
			else if ( item is NewBlacksmithShelfB ){ gold = 59 * amount; }
			else if ( item is NewBlacksmithShelfC ){ gold = 59 * amount; }
			else if ( item is NewBlacksmithShelfD ){ gold = 59 * amount; }
			else if ( item is NewBlacksmithShelfE ){ gold = 59 * amount; }
			else if ( item is NewBookShelfA ){ gold = 59 * amount; }
			else if ( item is NewBookShelfB ){ gold = 59 * amount; }
			else if ( item is NewBookShelfC ){ gold = 59 * amount; }
			else if ( item is NewBookShelfD ){ gold = 59 * amount; }
			else if ( item is NewBookShelfE ){ gold = 59 * amount; }
			else if ( item is NewBookShelfF ){ gold = 59 * amount; }
			else if ( item is NewBookShelfG ){ gold = 59 * amount; }
			else if ( item is NewBookShelfH ){ gold = 59 * amount; }
			else if ( item is NewBookShelfI ){ gold = 59 * amount; }
			else if ( item is NewBookShelfJ ){ gold = 59 * amount; }
			else if ( item is NewBookShelfK ){ gold = 59 * amount; }
			else if ( item is NewBookShelfL ){ gold = 59 * amount; }
			else if ( item is NewBookShelfM ){ gold = 59 * amount; }
			else if ( item is NewBowyerShelfA ){ gold = 59 * amount; }
			else if ( item is NewBowyerShelfB ){ gold = 59 * amount; }
			else if ( item is NewBowyerShelfC ){ gold = 59 * amount; }
			else if ( item is NewBowyerShelfD ){ gold = 59 * amount; }
			else if ( item is NewCarpenterShelfA ){ gold = 59 * amount; }
			else if ( item is NewCarpenterShelfB ){ gold = 59 * amount; }
			else if ( item is NewCarpenterShelfC ){ gold = 59 * amount; }
			else if ( item is NewClothShelfA ){ gold = 59 * amount; }
			else if ( item is NewClothShelfB ){ gold = 59 * amount; }
			else if ( item is NewClothShelfC ){ gold = 59 * amount; }
			else if ( item is NewClothShelfD ){ gold = 59 * amount; }
			else if ( item is NewClothShelfE ){ gold = 59 * amount; }
			else if ( item is NewClothShelfF ){ gold = 59 * amount; }
			else if ( item is NewClothShelfG ){ gold = 59 * amount; }
			else if ( item is NewClothShelfH ){ gold = 59 * amount; }
			else if ( item is NewDarkBookShelfA ){ gold = 59 * amount; }
			else if ( item is NewDarkBookShelfB ){ gold = 59 * amount; }
			else if ( item is NewDarkShelf ){ gold = 59 * amount; }
			else if ( item is NewDrawersA ){ gold = 59 * amount; }
			else if ( item is NewDrawersB ){ gold = 59 * amount; }
			else if ( item is NewDrawersC ){ gold = 59 * amount; }
			else if ( item is NewDrawersD ){ gold = 59 * amount; }
			else if ( item is NewDrawersE ){ gold = 59 * amount; }
			else if ( item is NewDrawersF ){ gold = 59 * amount; }
			else if ( item is NewDrawersG ){ gold = 59 * amount; }
			else if ( item is NewDrawersH ){ gold = 59 * amount; }
			else if ( item is NewDrawersI ){ gold = 59 * amount; }
			else if ( item is NewDrawersJ ){ gold = 59 * amount; }
			else if ( item is NewDrawersK ){ gold = 59 * amount; }
			else if ( item is NewDrawersL ){ gold = 59 * amount; }
			else if ( item is NewDrawersM ){ gold = 59 * amount; }
			else if ( item is NewDrawersN ){ gold = 59 * amount; }
			else if ( item is NewDrinkShelfA ){ gold = 59 * amount; }
			else if ( item is NewDrinkShelfB ){ gold = 59 * amount; }
			else if ( item is NewDrinkShelfC ){ gold = 59 * amount; }
			else if ( item is NewDrinkShelfD ){ gold = 59 * amount; }
			else if ( item is NewDrinkShelfE ){ gold = 59 * amount; }
			else if ( item is NewHelmShelf ){ gold = 59 * amount; }
			else if ( item is NewHunterShelf ){ gold = 59 * amount; }
			else if ( item is NewKitchenShelfA ){ gold = 59 * amount; }
			else if ( item is NewKitchenShelfB ){ gold = 59 * amount; }
			else if ( item is NewOldBookShelf ){ gold = 59 * amount; }
			else if ( item is NewPotionShelf ){ gold = 59 * amount; }
			else if ( item is NewRuinedBookShelf ){ gold = 59 * amount; }
			else if ( item is NewShelfA ){ gold = 59 * amount; }
			else if ( item is NewShelfB ){ gold = 59 * amount; }
			else if ( item is NewShelfC ){ gold = 59 * amount; }
			else if ( item is NewShelfD ){ gold = 59 * amount; }
			else if ( item is NewShelfE ){ gold = 59 * amount; }
			else if ( item is NewShelfF ){ gold = 59 * amount; }
			else if ( item is NewShelfG ){ gold = 59 * amount; }
			else if ( item is NewShelfH ){ gold = 59 * amount; }
			else if ( item is NewShoeShelfA ){ gold = 59 * amount; }
			else if ( item is NewShoeShelfB ){ gold = 59 * amount; }
			else if ( item is NewShoeShelfC ){ gold = 59 * amount; }
			else if ( item is NewShoeShelfD ){ gold = 59 * amount; }
			else if ( item is NewSorcererShelfA ){ gold = 59 * amount; }
			else if ( item is NewSorcererShelfB ){ gold = 59 * amount; }
			else if ( item is NewSorcererShelfC ){ gold = 59 * amount; }
			else if ( item is NewSorcererShelfD ){ gold = 59 * amount; }
			else if ( item is NewSupplyShelfA ){ gold = 59 * amount; }
			else if ( item is NewSupplyShelfB ){ gold = 59 * amount; }
			else if ( item is NewSupplyShelfC ){ gold = 59 * amount; }
			else if ( item is NewTailorShelfA ){ gold = 59 * amount; }
			else if ( item is NewTailorShelfB ){ gold = 59 * amount; }
			else if ( item is NewTailorShelfC ){ gold = 59 * amount; }
			else if ( item is NewTailorShelfD ){ gold = 59 * amount; }
			else if ( item is NewTannerShelfA ){ gold = 59 * amount; }
			else if ( item is NewTannerShelfB ){ gold = 59 * amount; }
			else if ( item is NewTavernShelfC ){ gold = 59 * amount; }
			else if ( item is NewTavernShelfD ){ gold = 59 * amount; }
			else if ( item is NewTavernShelfE ){ gold = 59 * amount; }
			else if ( item is NewTavernShelfF ){ gold = 59 * amount; }
			else if ( item is NewTinkerShelfA ){ gold = 59 * amount; }
			else if ( item is NewTinkerShelfB ){ gold = 59 * amount; }
			else if ( item is NewTinkerShelfC ){ gold = 59 * amount; }
			else if ( item is NewTortureShelf ){ gold = 59 * amount; }
			else if ( item is NewWizardShelfA ){ gold = 59 * amount; }
			else if ( item is NewWizardShelfB ){ gold = 59 * amount; }
			else if ( item is NewWizardShelfC ){ gold = 59 * amount; }
			else if ( item is NewWizardShelfD ){ gold = 59 * amount; }
			else if ( item is NewWizardShelfE ){ gold = 59 * amount; }
			else if ( item is NewWizardShelfF ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetE ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetF ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetG ){ gold = 59 * amount; }
			else if ( item is ColoredDresserE ){ gold = 59 * amount; }
			else if ( item is ColoredDresserF ){ gold = 59 * amount; }
			else if ( item is ColoredShelfK ){ gold = 59 * amount; }
			else if ( item is ColoredShelfL ){ gold = 59 * amount; }
			else if ( item is ColoredShelfM ){ gold = 59 * amount; }
			else if ( item is ColoredShelfN ){ gold = 59 * amount; }
			else if ( item is ColoredDresserG ){ gold = 59 * amount; }
			else if ( item is ColoredDresserH ){ gold = 59 * amount; }
			else if ( item is ColoredArmoireA ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetA ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetB ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetC ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetD ){ gold = 59 * amount; }
			else if ( item is ColoredDresserC ){ gold = 59 * amount; }
			else if ( item is ColoredDresserD ){ gold = 59 * amount; }
			else if ( item is ColoredDresserI ){ gold = 59 * amount; }
			else if ( item is ColoredArmoireB ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetH ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetI ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetJ ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetK ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetL ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetM ){ gold = 59 * amount; }
			else if ( item is ColoredCabinetN ){ gold = 59 * amount; }
			else if ( item is ColoredDresserA ){ gold = 59 * amount; }
			else if ( item is ColoredDresserB ){ gold = 59 * amount; }
			else if ( item is ColoredDresserJ ){ gold = 59 * amount; }
			else if ( item is ColoredShelf1 ){ gold = 59 * amount; }
			else if ( item is ColoredShelf2 ){ gold = 59 * amount; }
			else if ( item is ColoredShelf3 ){ gold = 59 * amount; }
			else if ( item is ColoredShelf4 ){ gold = 59 * amount; }
			else if ( item is ColoredShelf5 ){ gold = 59 * amount; }
			else if ( item is ColoredShelf6 ){ gold = 59 * amount; }
			else if ( item is ColoredShelf7 ){ gold = 59 * amount; }
			else if ( item is ColoredShelf8 ){ gold = 59 * amount; }
			else if ( item is ColoredShelfA ){ gold = 59 * amount; }
			else if ( item is ColoredShelfB ){ gold = 59 * amount; }
			else if ( item is ColoredShelfC ){ gold = 59 * amount; }
			else if ( item is ColoredShelfD ){ gold = 59 * amount; }
			else if ( item is ColoredShelfE ){ gold = 59 * amount; }
			else if ( item is ColoredShelfF ){ gold = 59 * amount; }
			else if ( item is ColoredShelfG ){ gold = 59 * amount; }
			else if ( item is ColoredShelfH ){ gold = 59 * amount; }
			else if ( item is ColoredShelfI ){ gold = 59 * amount; }
			else if ( item is ColoredShelfJ ){ gold = 59 * amount; }
			else if ( item is ColoredShelfO ){ gold = 59 * amount; }
			else if ( item is ColoredShelfP ){ gold = 59 * amount; }
			else if ( item is ColoredShelfQ ){ gold = 59 * amount; }
			else if ( item is ColoredShelfR ){ gold = 59 * amount; }
			else if ( item is ColoredShelfS ){ gold = 59 * amount; }
			else if ( item is ColoredShelfT ){ gold = 59 * amount; }
			else if ( item is ColoredShelfU ){ gold = 59 * amount; }
			else if ( item is ColoredShelfV ){ gold = 59 * amount; }
			else if ( item is ColoredShelfW ){ gold = 59 * amount; }
			else if ( item is ColoredShelfX ){ gold = 59 * amount; }
			else if ( item is ColoredShelfY ){ gold = 59 * amount; }
			else if ( item is ColoredShelfZ ){ gold = 59 * amount; }
			else if ( item is MountedTrophyHead ){ gold = 500 * amount; }
			else if ( item is NightSightPotion ){ gold = 14 * amount; }
			else if ( item is NightSightScroll ){ gold = 5 * amount; }
			else if ( item is Nightstand ){ gold = 45 * amount; }
			else if ( item is NinjaTabi ){ gold = 6 * amount; }
			else if ( item is NoDachi ){ gold = 41 * amount; }
			else if ( item is NorseHelm ){ gold = 9 * amount; }
			else if ( item is Nunchaku ){ gold = 17 * amount; }
			else if ( item is Obi ){ gold = 5 * amount; }
			else if ( item is OilCloth ){ gold = 4 * amount; }
			else if ( item is OnyxFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is OnyxIngot ){ gold = 120 * amount; }
			else if ( item is OnyxPlateArms ){ gold = 470 * amount; }
			else if ( item is OnyxPlateChest ){ gold = 605 * amount; }
			else if ( item is OnyxPlateGloves ){ gold = 360 * amount; }
			else if ( item is OnyxPlateGorget ){ gold = 260 * amount; }
			else if ( item is OnyxPlateHelm ){ gold = 330 * amount; }
			else if ( item is OnyxPlateLegs ){ gold = 545 * amount; }
			else if ( item is OnyxShield ){ gold = 575 * amount; }
			else if ( item is OrcHelm ){ gold = 12 * amount; }
			else if ( item is OrderShield ){ gold = 115 * amount; }
			else if ( item is OreShovel ){ gold = 5 * amount; }
			else if ( item is OrnateWoodenChest ){ gold = 79 * amount; }
			else if ( item is PainSpikeScroll ){ gold = 5 * amount; }
			else if ( item is PaperLantern ){ gold = 8 * amount; }
			else if ( item is ParalyzeFieldScroll ){ gold = 30 * amount; }
			else if ( item is ParalyzeScroll ){ gold = 25 * amount; }
			else if ( item is PeachCobbler ){ gold = 15 * amount; }
			else if ( item is PentagramDeed ){ gold = 220 * amount; }
			else if ( item is PewterMug ){ gold = 2 * amount; }
			else if ( item is PhantasmScroll ){ gold = 28 * amount; }
			else if ( item is Pickaxe ){ gold = 11 * amount; }
			else if ( item is PickpocketDipEastDeed ){ gold = 146 * amount; }
			else if ( item is PickpocketDipSouthDeed ){ gold = 146 * amount; }
			else if ( item is Pike ){ gold = 19 * amount; }
			else if ( item is Pitchfork ){ gold = 9 * amount; }
			else if ( item is PlainDress ){ gold = 7 * amount; }
			else if ( item is ExquisiteRobe ){ gold = 19 * amount; }
			else if ( item is ProphetRobe ){ gold = 19 * amount; }
			else if ( item is ElegantRobe ){ gold = 19 * amount; }
			else if ( item is FormalRobe ){ gold = 19 * amount; }
			else if ( item is ArchmageRobe ){ gold = 19 * amount; }
			else if ( item is PriestRobe ){ gold = 19 * amount; }
			else if ( item is CultistRobe ){ gold = 19 * amount; }
			else if ( item is GildedDarkRobe ){ gold = 19 * amount; }
			else if ( item is GildedLightRobe ){ gold = 19 * amount; }
			else if ( item is SageRobe ){ gold = 19 * amount; }
			else if ( item is RoyalCoat ){ gold = 10 * amount; }
			else if ( item is RoyalShirt ){ gold = 10 * amount; }
			else if ( item is RusticShirt ){ gold = 10 * amount; }
			else if ( item is SquireShirt ){ gold = 10 * amount; }
			else if ( item is FormalCoat ){ gold = 10 * amount; }
			else if ( item is WizardShirt ){ gold = 10 * amount; }
			else if ( item is BeggarVest ){ gold = 6 * amount; }
			else if ( item is RoyalVest ){ gold = 6 * amount; }
			else if ( item is RusticVest ){ gold = 6 * amount; }
			else if ( item is SailorPants ){ gold = 3 * amount; }
			else if ( item is PiratePants ){ gold = 5 * amount; }
			else if ( item is RoyalSkirt ){ gold = 5 * amount; }
			else if ( item is Skirt ){ gold = 6 * amount; }
			else if ( item is RoyalLongSkirt ){ gold = 6 * amount; }
			else if ( item is BarbarianBoots ){ gold = 7 * amount; } 
			else if ( item is DeadMask ){ gold = 11 * amount; }
			else if ( item is WizardHood ){ gold = 6 * amount; }
			else if ( item is PlainLowTable ){ gold = 87 * amount; }
			else if ( item is PlainWoodenChest ){ gold = 79 * amount; }
			else if ( item is Plate ){ gold = 2 * amount; }
			else if ( item is PlateArms ){ gold = 94 * amount; }
			else if ( item is PlateBattleKabuto ){ gold = 47 * amount; }
			else if ( item is PlateChest ){ gold = 121 * amount; }
			else if ( item is PlateDo ){ gold = 155 * amount; }
			else if ( item is PlateGloves ){ gold = 72 * amount; }
			else if ( item is PlateGorget ){ gold = 52 * amount; }
			else if ( item is PlateHaidate ){ gold = 117 * amount; }
			else if ( item is PlateHatsuburi ){ gold = 38 * amount; }
			else if ( item is PlateHelm ){ gold = 10 * amount; }
			else if ( item is DreadHelm ){ gold = 10 * amount; }
			else if ( item is PlateHiroSode ){ gold = 111 * amount; }
			else if ( item is PlateLegs ){ gold = 109 * amount; }
			else if ( item is PlateSkirt ){ gold = 109 * amount; }
			else if ( item is PlateMempo ){ gold = 38 * amount; }
			else if ( item is PlateSuneate ){ gold = 112 * amount; }
			else if ( item is PlayerBBEast ){ gold = 118 * amount; }
			else if ( item is PlayerBBSouth ){ gold = 118 * amount; }
			else if ( item is PoisonFieldScroll ){ gold = 25 * amount; }
			else if ( item is PoisonPotion ){ gold = 14 * amount; }
			else if ( item is PoisonScroll ){ gold = 15 * amount; }
			else if ( item is PoisonStrikeScroll ){ gold = 20 * amount; }
			else if ( item is PolymorphScroll ){ gold = 35 * amount; }
			else if ( item is PotionKeg ){ gold = 30 * amount; }
			else if ( item is PresetMapEntry ){ gold = 6 * amount; }
			else if ( item is ProtectionScroll ){ gold = 10 * amount; }
			else if ( item is ProtectiveFairyPotion ){ gold = 28 * amount; }
			else if ( item is PugilistMits ){ gold = 9 * amount; }
			else if ( item is ThrowingGloves ){ gold = 9 * amount; }
			else if ( item is PumpkinPie ){ gold = 15 * amount; }
			else if ( item is QuarterStaff ){ gold = 28 * amount; }
			else if ( item is QuartzFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is QuartzIngot ){ gold = 120 * amount; }
			else if ( item is QuartzPlateArms ){ gold = 470 * amount; }
			else if ( item is QuartzPlateChest ){ gold = 605 * amount; }
			else if ( item is QuartzPlateGloves  ){ gold = 360 * amount; }
			else if ( item is QuartzPlateGorget ){ gold = 260 * amount; }
			else if ( item is QuartzPlateHelm ){ gold = 330 * amount; }
			else if ( item is QuartzPlateLegs ){ gold = 545 * amount; }
			else if ( item is QuartzShield ){ gold = 575 * amount; }
			else if ( item is Quiche ){ gold = 15 * amount; }
			else if ( item is ReactiveArmorScroll ){ gold = 5 * amount; }
			else if ( item is RecallScroll ){ gold = 20 * amount; }
			else if ( item is RedArmoire ){ gold = 99 * amount; }
			else if ( item is RedHangingLantern ){ gold = 25 * amount; }
			else if ( item is RedMisoSoup ){ gold = 16 * amount; }
			else if ( item is RefreshPotion ){ gold = 14 * amount; }
			else if ( item is RejuvenatePotion ){ gold = 28 * amount; }
			else if ( item is RepeatingCrossbow ){ gold = 22 * amount; }
			else if ( item is RestorativeSoilPotion ){ gold = 28 * amount; }
			else if ( item is ResurrectionScroll ){ gold = 40 * amount; }
			else if ( item is RetchedAirScroll ){ gold = 28 * amount; }
			else if ( item is RevealScroll ){ gold = 30 * amount; }
			else if ( item is Ribs ){ gold = 15 * amount; }
			else if ( item is RingmailArms ){ gold = 42 * amount; }
			else if ( item is RingmailChest ){ gold = 60 * amount; }
			else if ( item is RingmailGloves ){ gold = 26 * amount; }
			else if ( item is RingmailLegs ){ gold = 45 * amount; }
			else if ( item is RingmailSkirt ){ gold = 45 * amount; }
			else if ( item is Robe ){ gold = 9 * amount; }
			else if ( item is RollingPin ){ gold = 1 * amount; }
			else if ( item is RoundPaperLantern ){ gold = 8 * amount; }
			else if ( item is RoyalArms ){ gold = 94 * amount; }
			else if ( item is RoyalBoots ){ gold = 20 * amount; }
			else if ( item is RoyalCape ){ gold = 8 * amount; }
			else if ( item is RoyalChest ){ gold = 121 * amount; }
			else if ( item is RoyalGloves ){ gold = 72 * amount; }
			else if ( item is RoyalGorget ){ gold = 52 * amount; }
			else if ( item is RoyalHelm ){ gold = 10 * amount; }
			else if ( item is RoyalShield ){ gold = 115 * amount; }
			else if ( item is RoyalsLegs ){ gold = 109 * amount; }
			else if ( item is RubyFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is RubyIngot ){ gold = 120 * amount; }
			else if ( item is RubyPlateArms ){ gold = 470 * amount; }
			else if ( item is RubyPlateChest ){ gold = 605 * amount; }
			else if ( item is RubyPlateGloves ){ gold = 360 * amount; }
			else if ( item is RubyPlateGorget ){ gold = 260 * amount; }
			else if ( item is RubyPlateHelm ){ gold = 330 * amount; }
			else if ( item is RubyPlateLegs ){ gold = 545 * amount; }
			else if ( item is RubyShield ){ gold = 575 * amount; }
			else if ( item is Runebook ){ gold = 1000 * amount; }
			else if ( item is SackFlour ){ gold = 15 * amount; }
			else if ( item is Sai ){ gold = 28 * amount; }
			else if ( item is SamuraiTabi ){ gold = 8 * amount; }
			else if ( item is Sandals ){ gold = 2 * amount; }
			else if ( item is SapphireFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is SapphireIngot ){ gold = 120 * amount; }
			else if ( item is SapphirePlateArms ){ gold = 470 * amount; }
			else if ( item is SapphirePlateChest ){ gold = 605 * amount; }
			else if ( item is SapphirePlateGloves ){ gold = 360 * amount; }
			else if ( item is SapphirePlateGorget ){ gold = 260 * amount; }
			else if ( item is SapphirePlateHelm ){ gold = 330 * amount; }
			else if ( item is SapphirePlateLegs ){ gold = 545 * amount; }
			else if ( item is SapphireShield ){ gold = 575 * amount; }
			else if ( item is SausagePizza ){ gold = 15 * amount; }
			else if ( item is Saw ){ gold = 9 * amount; }
			else if ( item is Scales ){ gold = 4 * amount; }
			else if ( item is Scepter ){ gold = 18 * amount; }
			else if ( item is Scimitar ){ gold = 18 * amount; }
			else if ( item is Scissors ){ gold = 6 * amount; }
			else if ( item is Scorp ){ gold = 6 * amount; }
			else if ( item is ScribesPen ){ gold = 4 * amount; }
			else if ( item is Scythe ){ gold = 19 * amount; }
			else if ( item is SeaChart ){ gold = 9 * amount; }
			else if ( item is SewingKit ){ gold = 1 * amount; }
			else if ( item is Sextant ){ gold = 6 * amount; }
			else if ( item is SextantParts ){ gold = 2 * amount; }
			else if ( item is SextantParts ){ gold = 1 * amount; }
			else if ( item is ShadowIronIngot ){ gold = 6 * amount; }
			else if ( item is ShepherdsCrook ){ gold = 31 * amount; }
			else if ( item is ShieldOfEarthPotion ){ gold = 28 * amount; }
			else if ( item is ShinySilverIngot ){ gold = 120 * amount; }
			else if ( item is Shirt ){ gold = 6 * amount; }
			else if ( item is Shoes ){ gold = 4 * amount; }
			else if ( item is ShojiLantern ){ gold = 8 * amount; }
			else if ( item is ShojiScreen ){ gold = 167 * amount; }
			else if ( item is ShortCabinet ){ gold = 89 * amount; }
			else if ( item is ShortMusicStand ){ gold = 47 * amount; }
			else if ( item is ShortPants ){ gold = 3 * amount; }
			else if ( item is ShortSpear ){ gold = 11 * amount; }
			else if ( item is Shovel ){ gold = 6 * amount; }
			else if ( item is Shuriken ){ gold = 9 * amount; }
			else if ( item is SilverBeadNecklace ){ gold = 10 * amount; }
			else if ( item is SilverBracelet ){ gold = 10 * amount; }
			else if ( item is SilverEarrings ){ gold = 10 * amount; }
			else if ( item is SilverFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is SilverNecklace ){ gold = 10 * amount; }
			else if ( item is SilverPlateArms ){ gold = 470 * amount; }
			else if ( item is SilverPlateChest ){ gold = 605 * amount; }
			else if ( item is SilverPlateGloves ){ gold = 360 * amount; }
			else if ( item is SilverPlateGorget ){ gold = 260 * amount; }
			else if ( item is SilverPlateHelm ){ gold = 330 * amount; }
			else if ( item is SilverPlateLegs ){ gold = 545 * amount; }
			else if ( item is SilverRing ){ gold = 10 * amount; }
			else if ( item is SilverShield ){ gold = 575 * amount; }
			else if ( item is Skillet ){ gold = 1 * amount; }
			else if ( item is SkinDemonArms ){ gold = 400 * amount; }
			else if ( item is SkinDemonChest ){ gold = 500 * amount; }
			else if ( item is SkinDemonGloves ){ gold = 300 * amount; }
			else if ( item is SkinDemonGorget ){ gold = 370 * amount; }
			else if ( item is SkinDemonHelm ){ gold = 100 * amount; }
			else if ( item is SkinDemonLegs ){ gold = 400 * amount; }
			else if ( item is SkinDragonArms ){ gold = 400 * amount; }
			else if ( item is SkinDragonChest ){ gold = 500 * amount; }
			else if ( item is SkinDragonGloves ){ gold = 300 * amount; }
			else if ( item is SkinDragonGorget ){ gold = 370 * amount; }
			else if ( item is SkinDragonHelm ){ gold = 100 * amount; }
			else if ( item is SkinDragonLegs ){ gold = 400 * amount; }
			else if ( item is SkinNightmareArms ){ gold = 400 * amount; }
			else if ( item is SkinNightmareChest ){ gold = 500 * amount; }
			else if ( item is SkinNightmareGloves ){ gold = 300 * amount; }
			else if ( item is SkinNightmareGorget ){ gold = 370 * amount; }
			else if ( item is SkinNightmareHelm ){ gold = 100 * amount; }
			else if ( item is SkinNightmareLegs ){ gold = 400 * amount; }
			else if ( item is SkinningKnife ){ gold = 7 * amount; }
			else if ( item is SkinSerpentArms ){ gold = 400 * amount; }
			else if ( item is SkinSerpentChest ){ gold = 500 * amount; }
			else if ( item is SkinSerpentGloves ){ gold = 300 * amount; }
			else if ( item is SkinSerpentGorget ){ gold = 370 * amount; }
			else if ( item is SkinSerpentHelm ){ gold = 100 * amount; }
			else if ( item is SkinSerpentLegs ){ gold = 400 * amount; }
			else if ( item is SkinTrollArms ){ gold = 400 * amount; }
			else if ( item is SkinTrollChest ){ gold = 500 * amount; }
			else if ( item is SkinTrollGloves ){ gold = 300 * amount; }
			else if ( item is SkinTrollGorget ){ gold = 370 * amount; }
			else if ( item is SkinTrollHelm ){ gold = 100 * amount; }
			else if ( item is SkinTrollLegs ){ gold = 400 * amount; }
			else if ( item is SkinUnicornArms ){ gold = 400 * amount; }
			else if ( item is SkinUnicornChest ){ gold = 500 * amount; }
			else if ( item is SkinUnicornGloves ){ gold = 300 * amount; }
			else if ( item is SkinUnicornGorget ){ gold = 370 * amount; }
			else if ( item is SkinUnicornHelm ){ gold = 100 * amount; }
			else if ( item is SkinUnicornLegs ){ gold = 400 * amount; }
			else if ( item is Skirt ){ gold = 5 * amount; }
			else if ( item is SkullCap ){ gold = 3 * amount; }
			else if ( item is SledgeHammer ){ gold = 15 * amount; }
			else if ( item is SmallBedEastDeed ){ gold = 219 * amount; }
			else if ( item is SmallBedSouthDeed ){ gold = 219 * amount; }
			else if ( item is SmallCrate ){ gold = 21 * amount; }
			else if ( item is SmallFlask ){ gold = 25 * amount; }
			else if ( item is SmallForgeDeed ){ gold = 26 * amount; }
			else if ( item is SmallPlateJingasa ){ gold = 28 * amount; }
			else if ( item is SmallTowerSculpture ){ gold = 388 * amount; }
			else if ( item is SmallUrn ){ gold = 388 * amount; }
			else if ( item is SmithHammer ){ gold = 10 * amount; }
			else if ( item is SmoothingPlane ){ gold = 6 * amount; }
			else if ( item is Spear ){ gold = 15 * amount; }
			else if ( item is Harpoon ){ gold = 15 * amount; }
			else if ( item is SpectreShadowScroll ){ gold = 28 * amount; }
			else if ( item is Spellbook ){ gold = 30 * amount; }
			else if ( item is SpinelFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is SpinelIngot ){ gold = 120 * amount; }
			else if ( item is SpinelPlateArms ){ gold = 470 * amount; }
			else if ( item is SpinelPlateChest ){ gold = 605 * amount; }
			else if ( item is SpinelPlateGloves ){ gold = 360 * amount; }
			else if ( item is SpinelPlateGorget ){ gold = 260 * amount; }
			else if ( item is SpinelPlateHelm ){ gold = 330 * amount; }
			else if ( item is SpinelPlateLegs ){ gold = 545 * amount; }
			else if ( item is SpinelShield ){ gold = 575 * amount; }
			else if ( item is SpinningHourglass ){ gold = 70 * amount; }
			else if ( item is SpinningwheelEastDeed ){ gold = 166 * amount; }
			else if ( item is SpinningwheelSouthDeed ){ gold = 166 * amount; }
			else if ( item is SpoonLeft ){ gold = 1 * amount; }
			else if ( item is SpoonRight ){ gold = 1 * amount; }
			else if ( item is Springs ){ gold = 1 * amount; }
			else if ( item is Spyglass ){ gold = 6 * amount; }
			else if ( item is SquareGozaMatEastDeed ){ gold = 35 * amount; }
			else if ( item is SquareGozaMatSouthDeed ){ gold = 35 * amount; }
			else if ( item is StandardPlateKabuto ){ gold = 37 * amount; }
			else if ( item is StarRubyFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is StarRubyIngot ){ gold = 120 * amount; }
			else if ( item is StarRubyPlateArms ){ gold = 470 * amount; }
			else if ( item is StarRubyPlateChest ){ gold = 605 * amount; }
			else if ( item is StarRubyPlateGloves ){ gold = 360 * amount; }
			else if ( item is StarRubyPlateGorget ){ gold = 260 * amount; }
			else if ( item is StarRubyPlateHelm ){ gold = 330 * amount; }
			else if ( item is StarRubyPlateLegs ){ gold = 545 * amount; }
			else if ( item is StarRubyShield ){ gold = 575 * amount; }
			else if ( item is StatueEast ){ gold = 300 * amount; }
			else if ( item is StatueNorth ){ gold = 300 * amount; }
			else if ( item is StatuePegasus ){ gold = 360 * amount; }
			else if ( item is StatueSouth ){ gold = 300 * amount; }
			else if ( item is StoneChair ){ gold = 300 * amount; }
			else if ( item is StoneCirclePotion ){ gold = 28 * amount; }
			else if ( item is StoneOvenEastDeed ){ gold = 185 * amount; }
			else if ( item is StoneOvenSouthDeed ){ gold = 185 * amount; }
			else if ( item is Stool ){ gold = 24 * amount; }
			else if ( item is StrangleScroll ){ gold = 30 * amount; }
			else if ( item is StrawHat ){ gold = 3 * amount; }
			else if ( item is StrengthPotion ){ gold = 14 * amount; }
			else if ( item is StrengthScroll ){ gold = 10 * amount; }
			else if ( item is StuddedArms ){ gold = 43 * amount; }
			else if ( item is StuddedBustierArms ){ gold = 27 * amount; }
			else if ( item is StuddedChest ){ gold = 64 * amount; }
			else if ( item is StuddedDo ){ gold = 66 * amount; }
			else if ( item is StuddedGloves ){ gold = 39 * amount; }
			else if ( item is StuddedGorget ){ gold = 36 * amount; }
			else if ( item is StuddedHaidate ){ gold = 37 * amount; }
			else if ( item is StuddedHiroSode ){ gold = 32 * amount; }
			else if ( item is StuddedLegs ){ gold = 51 * amount; }
			else if ( item is StuddedSkirt ){ gold = 51 * amount; }
			else if ( item is StuddedMempo ){ gold = 28 * amount; }
			else if ( item is StuddedSuneate ){ gold = 40 * amount; }
			else if ( item is SummonAirElementalScroll ){ gold = 40 * amount; }
			else if ( item is SummonCreatureScroll ){ gold = 25 * amount; }
			else if ( item is SummonDaemonScroll ){ gold = 40 * amount; }
			else if ( item is SummonEarthElementalScroll ){ gold = 40 * amount; }
			else if ( item is SummonFamiliarScroll ){ gold = 10 * amount; }
			else if ( item is SummonFireElementalScroll ){ gold = 40 * amount; }
			else if ( item is SummonWaterElementalScroll ){ gold = 40 * amount; }
			else if ( item is Surcoat ){ gold = 7 * amount; }
			else if ( item is SurgeonsKnife ){ gold = 7 * amount; }
			else if ( item is SushiPlatter ){ gold = 17 * amount; }
			else if ( item is SushiRolls ){ gold = 17 * amount; }
			else if ( item is SwarmOfInsectsPotion ){ gold = 28 * amount; }
			else if ( item is SweetDough ){ gold = 15 * amount; }
			else if ( item is TallCabinet ){ gold = 89 * amount; }
			else if ( item is TallMusicStand ){ gold = 57 * amount; }
			else if ( item is TallStrawHat ){ gold = 4 * amount; }
			else if ( item is Tambourine ){ gold = 43 * amount; }
			else if ( item is TambourineTassel ){ gold = 43 * amount; }
			else if ( item is TattsukeHakama ){ gold = 8 * amount; }
			else if ( item is Tekagi ){ gold = 22 * amount; }
			else if ( item is TelekinisisScroll ){ gold = 15 * amount; }
			else if ( item is TeleportScroll ){ gold = 15 * amount; }
			else if ( item is Tessen ){ gold = 41 * amount; }
			else if ( item is Tetsubo ){ gold = 21 * amount; }
			else if ( item is Tetsubo ){ gold = 43 * amount; }
			else if ( item is ThighBoots ){ gold = 7 * amount; }
			else if ( item is ThinLongsword ){ gold = 13 * amount; }
			else if ( item is Throne ){ gold = 54 * amount; }
			else if ( item is TinkerTools ){ gold = 3 * amount; }
			else if ( item is Tongs ){ gold = 7 * amount; }
			else if ( item is TopazFemalePlateChest ){ gold = 565 * amount; }
			else if ( item is TopazIngot ){ gold = 120 * amount; }
			else if ( item is TopazPlateArms ){ gold = 470 * amount; }
			else if ( item is TopazPlateChest ){ gold = 605 * amount; }
			else if ( item is TopazPlateGloves ){ gold = 360 * amount; }
			else if ( item is TopazPlateGorget ){ gold = 260 * amount; }
			else if ( item is TopazPlateHelm ){ gold = 330 * amount; }
			else if ( item is TopazPlateLegs ){ gold = 545 * amount; }
			else if ( item is TopazShield ){ gold = 575 * amount; }
			else if ( item is TotalRefreshPotion ){ gold = 28 * amount; }
			else if ( item is TrainingDummyEastDeed ){ gold = 125 * amount; }
			else if ( item is TrainingDummySouthDeed ){ gold = 125 * amount; }
			else if ( item is TrapKit ){ gold = 210 * amount; }
			else if ( item is TreefellowPotion ){ gold = 28 * amount; }
			else if ( item is TribalPaint ){ gold = 25 * amount; }
			else if ( item is TricorneHat ){ gold = 4 * amount; }
			else if ( item is PirateHat ){ gold = 4 * amount; }
			else if ( item is Tunic ){ gold = 9 * amount; }
			else if ( item is TwoHandedAxe ){ gold = 16 * amount; }
			else if ( item is UnbakedApplePie ){ gold = 15 * amount; }
			else if ( item is UnbakedFruitPie ){ gold = 15 * amount; }
			else if ( item is UnbakedMeatPie ){ gold = 15 * amount; }
			else if ( item is UnbakedPeachCobbler ){ gold = 15 * amount; }
			else if ( item is UnbakedPumpkinPie ){ gold = 15 * amount; }
			else if ( item is UnbakedQuiche ){ gold = 15 * amount; }
			else if ( item is UncookedCheesePizza ){ gold = 15 * amount; }
			else if ( item is UncookedSausagePizza ){ gold = 15 * amount; }
			else if ( item is UndeadEyesScroll ){ gold = 28 * amount; }
			else if ( item is UnlockScroll ){ gold = 15 * amount; }
			else if ( item is ValoriteIngot ){ gold = 12 * amount; }
			else if ( item is SteelIngot ){ gold = 13 * amount; }
			else if ( item is BrassIngot ){ gold = 14 * amount; }
			else if ( item is MithrilIngot ){ gold = 15 * amount; }
			else if ( item is XormiteIngot ){ gold = 15 * amount; }
			else if ( item is DwarvenIngot ){ gold = 30 * amount; }
			else if ( item is VampireGiftScroll ){ gold = 28 * amount; }
			else if ( item is VampiricEmbraceScroll ){ gold = 45 * amount; }
			else if ( item is Vase ){ gold = 228 * amount; }
			else if ( item is VengefulSpiritScroll ){ gold = 35 * amount; }
			else if ( item is VeriteIngot ){ gold = 11 * amount; }
			else if ( item is VikingSword ){ gold = 27 * amount; }
			else if ( item is VolcanicEruptionPotion ){ gold = 28 * amount; }
			else if ( item is Wakizashi ){ gold = 19 * amount; }
			else if ( item is WallOfSpikesScroll ){ gold = 28 * amount; }
			else if ( item is WallOfStoneScroll ){ gold = 15 * amount; }
			else if ( item is WarAxe ){ gold = 14 * amount; }
			else if ( item is WarHammer ){ gold = 12 * amount; }
			else if ( item is WarMace ){ gold = 15 * amount; }
			else if ( item is WasabiClumps ){ gold = 17 * amount; }
			else if ( item is WaterTroughEastDeed ){ gold = 319 * amount; }
			else if ( item is WaterTroughSouthDeed ){ gold = 319 * amount; }
			else if ( item is WaxingPot ){ gold = 25 * amount; }
			else if ( item is WaxPainting ){ gold = 500 * amount; }
			else if ( item is WaxPaintingA ){ gold = 500 * amount; }
			else if ( item is WaxPaintingB ){ gold = 500 * amount; }
			else if ( item is WaxPaintingC ){ gold = 500 * amount; }
			else if ( item is WaxPaintingD ){ gold = 500 * amount; }
			else if ( item is WaxPaintingE ){ gold = 500 * amount; }
			else if ( item is WaxPaintingF ){ gold = 500 * amount; }
			else if ( item is WaxPaintingG ){ gold = 500 * amount; }
			else if ( item is WeakenScroll ){ gold = 5 * amount; }
			else if ( item is WhiteFurArms ){ gold = 50 * amount; }
			else if ( item is WhiteFurTunic ){ gold = 60 * amount; }
			else if ( item is WhiteFurLegs ){ gold = 50 * amount; }
			else if ( item is WhiteFurBoots ){ gold = 6 * amount; }
			else if ( item is WhiteFurCap ){ gold = 8 * amount; }
			else if ( item is WhiteFurCape ){ gold = 9 * amount; }
			else if ( item is WhiteFurRobe ){ gold = 12 * amount; }
			else if ( item is WhiteFurSarong ){ gold = 8 * amount; }
			else if ( item is WhiteHangingLantern ){ gold = 25 * amount; }
			else if ( item is WhiteMisoSoup ){ gold = 16 * amount; }
			else if ( item is WideBrimHat ){ gold = 4 * amount; }
			else if ( item is WindChimes ){ gold = 10 * amount; }
			else if ( item is WitherScroll ){ gold = 25 * amount; }
			else if ( item is WitchHat ){ gold = 5 * amount; }
			else if ( item is WizardsHat ){ gold = 5 * amount; }
			else if ( item is WoodenBench ){ gold = 46 * amount; }
			else if ( item is WoodenBox ){ gold = 27 * amount; }
			else if ( item is WoodenChair ){ gold = 33 * amount; }
			else if ( item is WoodenChairCushion ){ gold = 37 * amount; }
			else if ( item is WoodenChest ){ gold = 56 * amount; }
			else if ( item is WoodenFootLocker ){ gold = 79 * amount; }
			else if ( item is WoodenKiteShield ){ gold = 35 * amount; }
			else if ( item is WoodenPlateArms ){ gold = 94 * amount; }
			else if ( item is WoodenPlateChest ){ gold = 121 * amount; }
			else if ( item is WoodenPlateGloves ){ gold = 72 * amount; }
			else if ( item is WoodenPlateGorget ){ gold = 52 * amount; }
			else if ( item is WoodenPlateHelm ){ gold = 10 * amount; }
			else if ( item is WoodenPlateLegs ){ gold = 109 * amount; }
			else if ( item is WoodenShield ){ gold = 30 * amount; }
			else if ( item is WoodenThrone ){ gold = 46 * amount; }
			else if ( item is WoodlandProtectionPotion ){ gold = 28 * amount; }
			else if ( item is WorldMap ){ gold = 12 * amount; }
			else if ( item is WorldMapAmbrosia ){ gold = 30 * amount; }
			else if ( item is WorldMapBottle ){ gold = 30 * amount; }
			else if ( item is WorldMapIslesOfDread ){ gold = 30 * amount; }
			else if ( item is WorldMapLodor ){ gold = 30 * amount; }
			else if ( item is WorldMapSerpent ){ gold = 30 * amount; }
			else if ( item is WorldMapSosaria ){ gold = 30 * amount; }
			else if ( item is WorldMapSavage ){ gold = 30 * amount; }
			else if ( item is WorldMapUmber ){ gold = 30 * amount; }
			else if ( item is WraithFormScroll ){ gold = 35 * amount; }
			else if ( item is WritingTable ){ gold = 48 * amount; }
			else if ( item is YewWoodTable ){ gold = 60 * amount; }
			else if ( item is Yumi ){ gold = 26 * amount; }
			else if ( item is StoneVase ){ gold = 30 * amount; }
			else if ( item is StoneLargeVase ){ gold = 35 * amount; }
			else if ( item is StoneAmphora ){ gold = 30 * amount; }
			else if ( item is StoneLargeAmphora ){ gold = 35 * amount; }
			else if ( item is StoneOrnateVase ){ gold = 35 * amount; }
			else if ( item is StoneOrnateAmphora ){ gold = 35 * amount; }
			else if ( item is StoneGargoyleVase ){ gold = 50 * amount; }
			else if ( item is StoneBuddhistSculpture ){ gold = 45 * amount; }
			else if ( item is StoneMingSculpture ){ gold = 40 * amount; }
			else if ( item is StoneYuanSculpture ){ gold = 40 * amount; }
			else if ( item is StoneQinSculpture ){ gold = 40 * amount; }
			else if ( item is StoneMingUrn ){ gold = 30 * amount; }
			else if ( item is StoneQinUrn ){ gold = 30 * amount; }
			else if ( item is StoneYuanUrn ){ gold = 30 * amount; }
			else if ( item is StoneChairs ){ gold = 50 * amount; }
			else if ( item is StoneBenchLong ){ gold = 50 * amount; }
			else if ( item is StoneBenchShort ){ gold = 50 * amount; }
			else if ( item is StoneTableLong ){ gold = 50 * amount; }
			else if ( item is StoneTableShort ){ gold = 50 * amount; }
			else if ( item is StonePedestal ){ gold = 40 * amount; }
			else if ( item is StoneFancyPedestal ){ gold = 50 * amount; }
			else if ( item is StoneRoughPillar ){ gold = 200 * amount; }
			else if ( item is StoneColumn ){ gold = 100 * amount; }
			else if ( item is StoneGothicColumn ){ gold = 200 * amount; }
			else if ( item is StoneSteps ){ gold = 35 * amount; }
			else if ( item is StoneBlock ){ gold = 35 * amount; }
			else if ( item is StoneSarcophagus ){ gold = 200 * amount; }
			else if ( item is SmallStatueAngel ){ gold = 150 * amount; }
			else if ( item is GargoyleStatue ){ gold = 150 * amount; }
			else if ( item is SmallStatueMan ){ gold = 150 * amount; }
			else if ( item is StatueGargoyleBust ){ gold = 150 * amount; }
			else if ( item is StatueBust ){ gold = 150 * amount; }
			else if ( item is SmallStatueNoble ){ gold = 150 * amount; }
			else if ( item is SmallStatuePegasus ){ gold = 150 * amount; }
			else if ( item is SmallStatueSkull ){ gold = 150 * amount; }
			else if ( item is SmallStatueWoman ){ gold = 150 * amount; }
			else if ( item is SmallStatueDragon ){ gold = 150 * amount; }
			else if ( item is StatueAdventurer ){ gold = 250 * amount; }
			else if ( item is StatueAmazon ){ gold = 250 * amount; }
			else if ( item is StatueDemonicFace ){ gold = 250 * amount; }
			else if ( item is StatueDruid ){ gold = 250 * amount; }
			else if ( item is StatueElvenKnight ){ gold = 250 * amount; }
			else if ( item is StatueElvenPriestess ){ gold = 250 * amount; }
			else if ( item is StatueElvenSorceress ){ gold = 250 * amount; }
			else if ( item is StatueElvenWarrior ){ gold = 250 * amount; }
			else if ( item is StatueGryphon ){ gold = 250 * amount; }
			else if ( item is StatueDwarf ){ gold = 250 * amount; }
			else if ( item is StatueMermaid ){ gold = 250 * amount; }
			else if ( item is StatueSeaHorse ){ gold = 250 * amount; }
			else if ( item is StatueFighter ){ gold = 250 * amount; }
			else if ( item is GargoyleFlightStatue ){ gold = 250 * amount; }
			else if ( item is SphinxStatue ){ gold = 250 * amount; }
			else if ( item is SmallStatueLion ){ gold = 250 * amount; }
			else if ( item is MedusaStatue ){ gold = 250 * amount; }
			else if ( item is StatueNoble ){ gold = 250 * amount; }
			else if ( item is StatuePriest ){ gold = 250 * amount; }
			else if ( item is StatueSwordsman ){ gold = 250 * amount; }
			else if ( item is StatueWizard ){ gold = 250 * amount; }
			else if ( item is StatueDesertGod ){ gold = 350 * amount; }
			else if ( item is StatueHorseRider ){ gold = 350 * amount; }
			else if ( item is StatueGargoyleTall ){ gold = 350 * amount; }
			else if ( item is StatueWolfWinged ){ gold = 350 * amount; }
			else if ( item is StatueMinotaurDefend ){ gold = 350 * amount; }
			else if ( item is StatueMinotaurAttack ){ gold = 350 * amount; }
			else if ( item is MediumStatueLion ){ gold = 350 * amount; }
			else if ( item is MinotaurStatue ){ gold = 350 * amount; }
			else if ( item is LargePegasusStatue ){ gold = 350 * amount; }
			else if ( item is StatueWomanWarriorPillar ){ gold = 350 * amount; }
			else if ( item is StatueAngelTall ){ gold = 500 * amount; }
			else if ( item is LargeStatueLion ){ gold = 500 * amount; }
			else if ( item is TallStatueLion ){ gold = 500 * amount; }
			else if ( item is StatueDaemon ){ gold = 500 * amount; }
			else if ( item is StatueCapeWarrior ){ gold = 500 * amount; }
			else if ( item is StatueWiseManTall ){ gold = 500 * amount; }
			else if ( item is LargeStatueWolf ){ gold = 500 * amount; }
			else if ( item is StatueWomanTall ){ gold = 500 * amount; }
			else if ( item is StatueGateGuardian ){ gold = 750 * amount; }
			else if ( item is StatueGuardian ){ gold = 750 * amount; }
			else if ( item is StatueGiantWarrior ){ gold = 750 * amount; }
			else if ( item is StoneTombStoneA ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneB ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneC ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneD ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneE ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneF ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneG ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneH ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneI ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneJ ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneK ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneL ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneM ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneN ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneO ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneP ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneQ ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneR ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneS ){ gold = 35 * amount; }
			else if ( item is StoneTombStoneT ){ gold = 35 * amount; }

			gold = GetPrice( item, gold );

			return gold;
		}
	}
}
