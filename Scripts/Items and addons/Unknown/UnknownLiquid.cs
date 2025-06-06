using System;
using Server;
using Server.Network;
using System.Text;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
	/*
	public class ItemRemovalTimer : Timer 
	{ 
		private Item i_item; 
		public ItemRemovalTimer( Item item ) : base( TimeSpan.FromSeconds( 10.0 ) ) 
		{ 
			Priority = TimerPriority.OneSecond; 
			i_item = item; 
		} 

		protected override void OnTick() 
		{ 
			if (( i_item != null ) && ( !i_item.Deleted )) 
				i_item.Delete(); 
		} 
	} 
	*/
	public class Vomit : Item 
	{ 
		[Constructable] 
		public Vomit() : base( Utility.RandomList( 0xf3b, 0xf3c ) ) 
		{ 
			Name = "a puddle of vomit"; 
			Hue = 0x557; 
			Movable = false;

			ItemRemovalTimer thisTimer = new ItemRemovalTimer( this ); 
			thisTimer.Start(); 
		} 

		public override void OnSingleClick( Mobile from ) 
		{ 
			this.LabelTo( from, this.Name ); 
		} 
  
		public Vomit( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void Serialize(GenericWriter writer) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 0 ); 
		} 

		public override void Deserialize(GenericReader reader) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt(); 

			this.Delete(); // none when the world starts 
		} 
	}

	public class UnknownLiquid : Item
	{
		[Constructable]
		public UnknownLiquid() : base( 0x0EFC )
		{
			ItemID = Utility.RandomList( 0x0EFC, 0x0E2B, 0x0E2C, 0x0E2A, 0x0E26, 0x0E27, 0x0E25, 0x0E24, 0x09C7, 0x099F, 0x099B );
			string sLiquid = "a strange";
			switch( Utility.RandomMinMax( 0, 6 ) )
			{
				case 0: sLiquid = "an odd"; break;
				case 1: sLiquid = "an unusual"; break;
				case 2: sLiquid = "a bizarre"; break;
				case 3: sLiquid = "a curious"; break;
				case 4: sLiquid = "a peculiar"; break;
				case 5: sLiquid = "a strange"; break;
				case 6: sLiquid = "a weird"; break;
			}
			Name = sLiquid + " bottle of liquid";
			Hue = RandomThings.GetRandomColor(0);
			Amount = 1;
			Stackable = false;
			Weight = 1.0;
		}

		public UnknownLiquid( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !Movable )
			{
				from.SendMessage( "That cannot move so you cannot identify it." );
				return;
			}
			else if ( !from.InRange( this.GetWorldLocation(), 3 ) )
			{
				from.SendMessage( "You will need to get closer to identify that." );
				return;
			}
			else if ( !IsChildOf( from.Backpack ) && Server.Misc.MyServerSettings.IdentifyItemsOnlyInPack() ) 
			{
				from.SendMessage( "This must be in your backpack to identify." );
				return;
			}
			else
			{
				if ( from.CheckSkill( SkillName.TasteID, -5, 125 ) )
				{
					if ( from.Body.IsHuman && !from.Mounted )
						from.Animate( 34, 5, 1, true, false, 0 );

					from.PlaySound( 0x2D6 );

					Server.Items.UnknownLiquid.GivePotion( from, this );
				}
				else
				{
					int nReaction = Utility.RandomMinMax( 0, 10 );

					if ( nReaction == 1 )
					{
						from.PlaySound( from.Female ? 813 : 1087 );
						from.Say( "*vomits*" );
						if ( !from.Mounted ) 
							from.Animate( 32, 5, 1, true, false, 0 );                     
						Vomit puke = new Vomit(); 
						puke.Map = from.Map; 
						puke.Location = from.Location;
						from.SendMessage("You fail to identify the liquid, convulsing and spilling the bottle.");
					}
					else if ( nReaction == 2 )
					{
						from.PlaySound( from.Female ? 798 : 1070 );
						from.Say( "*hiccup!*" );
						from.SendMessage("You fail to identify the liquid, spasming and spilling the bottle.");
					}
					else if ( nReaction == 3 )
					{
						from.PlaySound( from.Female ? 792 : 1064 );
						from.Say( "*farts*" );
						from.SendMessage("You fail to identify the liquid, feeling gassy...you dump it out.");
					}
					else if ( nReaction == 4 )
					{
						from.PlaySound( from.Female ? 785 : 1056 );
						from.Say( "*cough!*" );				
						if ( !from.Mounted )
							from.Animate( 33, 5, 1, true, false, 0 );
						from.SendMessage("You fail to identify the liquid, coughing and spilling the bottle.");
					}
					else if ( nReaction == 5 )
					{
						from.PlaySound( from.Female ? 784 : 1055 );
						from.Say( "*clears throat*" );
						if ( !from.Mounted )
							from.Animate( 33, 5, 1, true, false, 0 );
						from.SendMessage("You fail to identify the liquid, hurting your throat...you dump out the bottle.");
					}
					else if ( nReaction == 6 )
					{
						from.PlaySound( from.Female ? 782 : 1053 );
						from.Say( "*burp!*" );
						if ( !from.Mounted )
							from.Animate( 33, 5, 1, true, false, 0 );
						from.SendMessage("You fail to identify the liquid, accidentally drinking the entire bottle.");
					}
					else if ( nReaction > 6 )
					{
						int nPoison = Utility.RandomMinMax( 0, 10 );
						from.Say( "Poison!" );
						Effects.SendLocationParticles( EffectItem.Create( from.Location, from.Map, EffectItem.DefaultDuration ), 0x36B0, 1, 14, 63, 7, 9915, 0 );
						from.PlaySound( Utility.RandomList( 0x30, 0x2D6 ) );
						if ( nPoison > 9 ) { from.ApplyPoison( from, Poison.Deadly ); }
						else if ( nPoison > 7 ) { from.ApplyPoison( from, Poison.Greater ); }
						else if ( nPoison > 4 ) { from.ApplyPoison( from, Poison.Regular ); }
						else { from.ApplyPoison( from, Poison.Lesser ); }
						from.SendMessage( "Poison!");
					}
					else
					{
						from.PlaySound( from.Female ? 820 : 1094 );
						from.Say( "*spits*" );
						if ( !from.Mounted )
							from.Animate( 6, 5, 1, true, false, 0 );
						from.SendMessage("You fail to identify the liquid, spitting it out and dumping the bottle.");
					}
				}

				this.Delete();
			}
		}

		public static void MakeSpaceAceLiquid( Item item )
		{
			item.ItemID = 0x1FDD;
			item.Hue = Server.Misc.RandomThings.GetRandomColor(0);

			string sLiquid = "a strange";
			switch( Utility.RandomMinMax( 0, 6 ) )
			{
				case 0: sLiquid = "an odd"; break;
				case 1: sLiquid = "an unusual"; break;
				case 2: sLiquid = "a bizarre"; break;
				case 3: sLiquid = "a curious"; break;
				case 4: sLiquid = "a peculiar"; break;
				case 5: sLiquid = "a strange"; break;
				case 6: sLiquid = "a weird"; break;
			}
			item.Name = sLiquid + " vial of liquid";
		}

		public static void GivePotion( Mobile from, Item usedItem )
		{
			if ( Server.Misc.IntelligentAction.TestForReagent( from, "undertaker" ) && Utility.RandomMinMax( 1, 5 ) == 1 )
			{
				Item jar = new BottleOfParts();
				Server.Items.BottleOfParts.FillJar( "", Utility.RandomMinMax( 1, 90 ), jar );
				ItemIdentification.ReplaceItemOrAddToBackpack(usedItem, jar, from);
				from.SendMessage("This seems to be a " + jar.Name + ".");
			}
			else
			{
				int min = 0;
				int max = 40;
				int extras = 0;

				if ( from.Skills[SkillName.Forensics].Value >= 50 && from.Skills[SkillName.Necromancy].Value >= 50 && from.Skills[SkillName.SpiritSpeak].Value >= 50 )
				{
					extras++;
					max = 56;
				}
				if ( from.Skills[SkillName.Cooking].Value >= 50 && from.Skills[SkillName.AnimalLore].Value >= 50 && from.Skills[SkillName.AnimalTaming].Value >= 50 )
				{
					extras++;
					extras++;
					max = 56;
				}

				if ( extras > 2 ){ extras = Utility.RandomMinMax( 1, 2 ); max = 55; }

				int potionType = Utility.RandomMinMax( min, max );
				string potionName = "";

				if ( Utility.RandomMinMax( 1, 125 ) <= from.Skills[SkillName.Cooking].Value ) // COOKS CAN FIND A POTION 1 LEVEL HIGHER
				{
					if ( potionType == 1 ){ potionType++; }
					else if ( potionType == 2 ){ potionType++; }
					else if ( potionType == 4 ){ potionType++; }
					else if ( potionType == 6 ){ potionType++; }
					else if ( potionType == 8 ){ potionType++; }
					else if ( potionType == 9 ){ potionType++; }
					else if ( potionType == 10 ){ potionType++; }
					else if ( potionType == 11 ){ potionType = 38; }
					else if ( potionType == 12 ){ potionType++; }
					else if ( potionType == 14 ){ potionType++; }
					else if ( potionType == 15 ){ potionType++; }
					else if ( potionType == 17 ){ potionType++; }
					else if ( potionType == 18 ){ potionType++; }
					else if ( potionType == 20 ){ potionType++; }
					else if ( potionType == 21 ){ potionType++; }
					else if ( potionType == 23 ){ potionType++; }
					else if ( potionType == 24 ){ potionType++; }
					else if ( potionType == 26 ){ potionType++; }
					else if ( potionType == 27 ){ potionType++; }
				}

				Item item = null;
				if ( potionType == 0 ){ item = new NightSightPotion(); potionName = "night sight potion"; }
				else if ( potionType == 1 ){ item = new LesserCurePotion(); potionName = "lesser cure potion"; }
				else if ( potionType == 2 ){ item = new CurePotion(); potionName = "cure potion"; }
				else if ( potionType == 3 ){ item = new GreaterCurePotion(); potionName = "greater cure potion"; }
				else if ( potionType == 4 ){ item = new AgilityPotion(); potionName = "agility potion"; }
				else if ( potionType == 5 ){ item = new GreaterAgilityPotion(); potionName = "greater agility potion"; }
				else if ( potionType == 6 ){ item = new StrengthPotion(); potionName = "strength"; }
				else if ( potionType == 7 ){ item = new GreaterStrengthPotion(); potionName = "greater strength potion"; }
				else if ( potionType == 8 ){ item = new LesserPoisonPotion(); potionName = "lesser poison"; }
				else if ( potionType == 9 ){ item = new PoisonPotion(); potionName = "poison"; }
				else if ( potionType == 10 ){ item = new GreaterPoisonPotion(); potionName = "greater poison"; }
				else if ( potionType == 11 ){ item = new DeadlyPoisonPotion(); potionName = "deadly poison"; }
				else if ( potionType == 12 ){ item = new RefreshPotion(); potionName = "refresh potion"; }
				else if ( potionType == 13 ){ item = new TotalRefreshPotion(); potionName = "total refresh potion"; }
				else if ( potionType == 14 ){ item = new LesserHealPotion(); potionName = "lesser heal potion"; }
				else if ( potionType == 15 ){ item = new HealPotion(); potionName = "heal potion"; }
				else if ( potionType == 16 ){ item = new GreaterHealPotion(); potionName = "greater heal potion"; }
				else if ( potionType == 17 ){ item = new LesserExplosionPotion(); potionName = "lesser explosion potion"; }
				else if ( potionType == 18 ){ item = new ExplosionPotion(); potionName = "explosion potion"; }
				else if ( potionType == 19 ){ item = new GreaterExplosionPotion(); potionName = "greater explosion potion"; }
				else if ( potionType == 20 ){ item = new LesserInvisibilityPotion(); potionName = "lesser invisibility potion"; }
				else if ( potionType == 21 ){ item = new InvisibilityPotion(); potionName = "invisibility potion"; }
				else if ( potionType == 22 ){ item = new GreaterInvisibilityPotion(); potionName = "greater invisibility potion"; }
				else if ( potionType == 23 ){ item = new LesserRejuvenatePotion(); potionName = "lesser rejuvenation potion"; }
				else if ( potionType == 24 ){ item = new RejuvenatePotion(); potionName = "rejuvenation potion"; }
				else if ( potionType == 25 ){ item = new GreaterRejuvenatePotion(); potionName = "greater rejuvenation potion"; }
				else if ( potionType == 26 ){ item = new LesserManaPotion(); potionName = "lesser mana potion"; }
				else if ( potionType == 27 ){ item = new ManaPotion(); potionName = "mana potion"; }
				else if ( potionType == 28 ){ item = new GreaterManaPotion(); potionName = "greater mana potion"; }
				else if ( potionType == 29 ){ item = new InvulnerabilityPotion(); potionName = "invulnerability potion"; }
				else if ( potionType == 30 ){ item = new AutoResPotion(); potionName = "resurrection potion"; }
				else if ( potionType == 31 ){ item = new OilMetal(); potionName = "metal enhancement oil"; }
				else if ( potionType == 32 ){ item = new OilLeather(); potionName = "leather enhancement oil"; }
				else if ( potionType == 33 ){ item = new BottleOfAcid(); potionName = "acid"; }
				else if ( potionType == 34 ){ item = new MagicalDyes(); potionName = "magical dye"; }
				else if ( potionType == 35 ){ item = new BeverageBottle(BeverageType.Ale); potionName = "ale"; }
				else if ( potionType == 36 ){ item = new BeverageBottle(BeverageType.Wine); potionName = "wine"; }
				else if ( potionType == 37 ){ item = new BeverageBottle(BeverageType.Liquor); potionName = "liquor"; }
				else if ( potionType == 38 ){ item = new LethalPoisonPotion(); potionName = "lethal poison"; }
				else if ( potionType == 39 ){ item = new OilWood(); potionName = "wood enhancement oil"; }
				else if ( potionType == 40 ){ item = new PackGrenade(); potionName = "Pack Grenade!!"; }

				if ( extras == 1 )
				{
					if ( potionType == 41 ){ item = new HellsGateScroll(); potionName = "demonic fire ooze"; }
					else if ( potionType == 42 ){ item = new ManaLeechScroll(); potionName = "lich leech mixture"; }
					else if ( potionType == 43 ){ item = new NecroCurePoisonScroll(); potionName = "disease curing concoction"; }
					else if ( potionType == 44 ){ item = new NecroPoisonScroll(); potionName = "disease draught"; }
					else if ( potionType == 45 ){ item = new NecroUnlockScroll(); potionName = "tomb raiding concoction"; }
					else if ( potionType == 46 ){ item = new PhantasmScroll(); potionName = "phantasm elixir"; }
					else if ( potionType == 47 ){ item = new RetchedAirScroll(); potionName = "retched air elixir"; }
					else if ( potionType == 48 ){ item = new SpectreShadowScroll(); potionName = "spectre shadow elixir"; }
					else if ( potionType == 49 ){ item = new UndeadEyesScroll(); potionName = "eyes of the dead mixture"; }
					else if ( potionType == 50 ){ item = new VampireGiftScroll(); potionName = "vampire blood draught"; }
					else if ( potionType == 51 ){ item = new WallOfSpikesScroll(); potionName = "wall of spikes draught"; }
					else if ( potionType == 52 ){ item = new BloodPactScroll(); potionName = "blood pact elixir"; }
					else if ( potionType == 53 ){ item = new GhostlyImagesScroll(); potionName = "ghostly images draught"; }
					else if ( potionType == 54 ){ item = new GhostPhaseScroll(); potionName = "ghost phase concoction"; }
					else if ( potionType == 55 ){ item = new GraveyardGatewayScroll(); potionName = "black gate draught"; }
					else if ( potionType == 56 ){ item = new HellsBrandScroll(); potionName = "hellish branding ooze"; }
				}

				if ( extras == 2 )
				{
					if ( potionType == 41 ){ item = new ShieldOfEarthPotion(); potionName = "shield of earth liquid"; }
					else if ( potionType == 42 ){ item = new WoodlandProtectionPotion(); potionName = "woodland protection oil"; }
					else if ( potionType == 43 ){ item = new ProtectiveFairyPotion(); potionName = "fairy in a bottle"; }
					else if ( potionType == 44 ){ item = new HerbalHealingPotion(); potionName = "herbal healing elixir"; }
					else if ( potionType == 45 ){ item = new GraspingRootsPotion(); potionName = "grasping roots mixture"; }
					else if ( potionType == 46 ){ item = new BlendWithForestPotion(); potionName = "forest blending oil"; }
					else if ( potionType == 47 ){ item = new SwarmOfInsectsPotion(); potionName = "bottle of insects"; }
					else if ( potionType == 48 ){ item = new VolcanicEruptionPotion(); potionName = "volcanic fluid"; }
					else if ( potionType == 49 ){ item = new TreefellowPotion(); potionName = "treant fertilizer"; }
					else if ( potionType == 50 ){ item = new StoneCirclePotion(); potionName = "stone rising concoction"; }
					else if ( potionType == 51 ){ item = new DruidicRunePotion(); potionName = "druidic marking oil"; }
					else if ( potionType == 52 ){ item = new LureStonePotion(); potionName = "stone in a bottle"; }
					else if ( potionType == 53 ){ item = new NaturesPassagePotion(); potionName = "nature passage mixture"; }
					else if ( potionType == 54 ){ item = new MushroomGatewayPotion(); potionName = "mushroom gateway growth"; }
					else if ( potionType == 55 ){ item = new RestorativeSoilPotion(); potionName = "bottle of magical mud"; }
					else if ( potionType == 56 ){ item = new FireflyPotion(); potionName = "bottle of fireflies"; }
				}

				if ( item == null ){ item = new BeverageBottle(BeverageType.Ale); potionName = "ale"; }

				ItemIdentification.ReplaceItemOrAddToBackpack(usedItem, item, from);
				from.SendMessage("This seems to be a bottle of " + potionName + ".");
			}
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			list.Add( 1049644, "Unidentified"); // PARENTHESIS
        }

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}