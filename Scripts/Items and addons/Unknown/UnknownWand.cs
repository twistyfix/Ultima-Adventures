using System;
using Server;
using Server.Network;
using System.Text;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
	public class UnknownWand : Item
	{
		public int WandLevel;
		public string WandOrigin;
		public int WandColor;
		public int WandID;
		public string WandMetal;

		[CommandProperty(AccessLevel.Owner)]
		public int Wand_Level { get { return WandLevel; } set { WandLevel = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public string Wand_Origin { get { return WandOrigin; } set { WandOrigin = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public int Wand_Color { get { return WandColor; } set { WandColor = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public int Wand_ID { get { return WandID; } set { WandID = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public string Wand_Metal { get { return WandMetal; } set { WandMetal = value; InvalidateProperties(); } }

		[Constructable]
		public UnknownWand() : base( 0xDF2 )
		{
			ItemID = Utility.RandomList( 0xDF2, 0xDF3, 0xDF4, 0xDF5 );
			string sWand = "a strange";
			switch( Utility.RandomMinMax( 0, 6 ) )
			{
				case 0: sWand = "an odd"; break;
				case 1: sWand = "an unusual"; break;
				case 2: sWand = "a bizarre"; break;
				case 3: sWand = "a curious"; break;
				case 4: sWand = "a peculiar"; break;
				case 5: sWand = "a strange"; break;
				case 6: sWand = "a weird"; break;
			}

			Weight = 1.0;

			Name = "wand";

			Server.Misc.MaterialInfo.ColorMetal( this, 0 );

			WandMetal = Name.Replace(" wand", "");
			WandColor = this.Hue;
			WandID = ItemID;

			Name = sWand + " " + Name;

			WandLevel = Utility.RandomMinMax( 1, 8 );

			string sLanguage = "pixie";
			switch( Utility.RandomMinMax( 0, 35 ) )
			{
				case 0: sLanguage = "balron"; break;
				case 1: sLanguage = "pixie"; break;
				case 2: sLanguage = "centaur"; break;
				case 3: sLanguage = "demonic"; break;
				case 4: sLanguage = "dragon"; break;
				case 5: sLanguage = "dwarvish"; break;
				case 6: sLanguage = "elven"; break;
				case 7: sLanguage = "fey"; break;
				case 8: sLanguage = "gargoyle"; break;
				case 9: sLanguage = "cyclops"; break;
				case 10: sLanguage = "gnoll"; break;
				case 11: sLanguage = "goblin"; break;
				case 12: sLanguage = "gremlin"; break;
				case 13: sLanguage = "druidic"; break;
				case 14: sLanguage = "tritun"; break;
				case 15: sLanguage = "minotaur"; break;
				case 16: sLanguage = "naga"; break;
				case 17: sLanguage = "ogrish"; break;
				case 18: sLanguage = "orkish"; break;
				case 19: sLanguage = "sphinx"; break;
				case 20: sLanguage = "treekin"; break;
				case 21: sLanguage = "trollish"; break;
				case 22: sLanguage = "undead"; break;
				case 23: sLanguage = "vampire"; break;
				case 24: sLanguage = "dark elf"; break;
				case 25: sLanguage = "magic"; break;
				case 26: sLanguage = "human"; break;
				case 27: sLanguage = "symbolic"; break;
				case 28: sLanguage = "runic"; break;
			}

			string sPart = "strange ";
			switch( Utility.RandomMinMax( 0, 5 ) )
			{
				case 0:	sPart = "strange ";	break;
				case 1:	sPart = "odd ";		break;
				case 2:	sPart = "ancient ";	break;
				case 3:	sPart = "unknown ";	break;
				case 4:	sPart = "cryptic ";	break;
				case 5:	sPart = "mystical ";	break;
			}

			WandOrigin = "etched with " + sPart + sLanguage + " symbols";
		}

		public static void WandType( Item wand, Mobile m, Mobile vendor )
		{
			if ( Utility.RandomMinMax( 1, 100 ) > 10 )
			{
				UnknownWand stick = (UnknownWand)wand;
				int stickLevel = stick.WandLevel;
				int stickType = Utility.RandomMinMax( 1, (8 * stickLevel) );
				string stickName = "";

				Item newWand = new ClumsyMagicStaff();
				newWand.Delete();

				if ( stickType == 1 ){ newWand = new ClumsyMagicStaff(); stickName = "clumsiness"; } // CIRCLE 1
				else if ( stickType == 2 ){ newWand = new CreateFoodMagicStaff(); stickName = "food creation"; } // CIRCLE 1
				else if ( stickType == 3 ){ newWand = new FeebleMagicStaff(); stickName = "feeble minds"; } // CIRCLE 1
				else if ( stickType == 4 ){ newWand = new HealMagicStaff(); stickName = "healing"; } // CIRCLE 1
				else if ( stickType == 5 ){ newWand = new MagicArrowMagicStaff(); stickName = "magical arrow"; } // CIRCLE 1
				else if ( stickType == 6 ){ newWand = new NightSightMagicStaff(); stickName = "night sight"; } // CIRCLE 1
				else if ( stickType == 7 ){ newWand = new ReactiveArmorMagicStaff(); stickName = "reactive armor"; } // CIRCLE 1
				else if ( stickType == 8 ){ newWand = new WeaknessMagicStaff(); stickName = "weakness"; } // CIRCLE 1
				else if ( stickType == 9 ){ newWand = new AgilityMagicStaff(); stickName = "agility"; } // CIRCLE 2
				else if ( stickType == 10 ){ newWand = new CunningMagicStaff(); stickName = "cunning"; } // CIRCLE 2
				else if ( stickType == 11 ){ newWand = new CureMagicStaff(); stickName = "curing"; } // CIRCLE 2
				else if ( stickType == 12 ){ newWand = new HarmMagicStaff(); stickName = "harming"; } // CIRCLE 2
				else if ( stickType == 13 ){ newWand = new MagicTrapMagicStaff(); stickName = "magical traps"; } // CIRCLE 2
				else if ( stickType == 14 ){ newWand = new MagicUntrapMagicStaff(); stickName = "trap removal"; } // CIRCLE 2
				else if ( stickType == 15 ){ newWand = new ProtectionMagicStaff(); stickName = "protection"; } // CIRCLE 2
				else if ( stickType == 16 ){ newWand = new StrengthMagicStaff(); stickName = "strength"; } // CIRCLE 2
				else if ( stickType == 17 ){ newWand = new BlessMagicStaff(); stickName = "blessing"; } // CIRCLE 3
				else if ( stickType == 18 ){ newWand = new FireballMagicStaff(); stickName = "fireballs"; } // CIRCLE 3
				else if ( stickType == 19 ){ newWand = new MagicLockMagicStaff(); stickName = "magical locks"; } // CIRCLE 3
				else if ( stickType == 20 ){ newWand = new MagicUnlockMagicStaff(); stickName = "unlocking"; } // CIRCLE 3
				else if ( stickType == 21 ){ newWand = new PoisonMagicStaff(); stickName = "poisoning"; } // CIRCLE 3
				else if ( stickType == 22 ){ newWand = new TelekinesisMagicStaff(); stickName = "telekinesis"; } // CIRCLE 3
				else if ( stickType == 23 ){ newWand = new TeleportMagicStaff(); stickName = "teleporting"; } // CIRCLE 3
				else if ( stickType == 24 ){ newWand = new WallofStoneMagicStaff(); stickName = "stone wall"; } // CIRCLE 3
				else if ( stickType == 25 ){ newWand = new ArchCureMagicStaff(); stickName = "arch curing"; } // CIRCLE 4
				else if ( stickType == 26 ){ newWand = new ArchProtectionMagicStaff(); stickName = "arch protection"; } // CIRCLE 4
				else if ( stickType == 27 ){ newWand = new CurseMagicStaff(); stickName = "curses"; } // CIRCLE 4
				else if ( stickType == 28 ){ newWand = new FireFieldMagicStaff(); stickName = "fire fields"; } // CIRCLE 4
				else if ( stickType == 29 ){ newWand = new GreaterHealMagicStaff(); stickName = "greater healing"; } // CIRCLE 4
				else if ( stickType == 30 ){ newWand = new LightningMagicStaff(); stickName = "lightning bolts"; } // CIRCLE 4
				else if ( stickType == 31 ){ newWand = new ManaDrainMagicStaff(); stickName = "mana draining"; } // CIRCLE 4
				else if ( stickType == 32 ){ newWand = new RecallMagicStaff(); stickName = "recalling"; } // CIRCLE 4
				else if ( stickType == 33 ){ newWand = new BladeSpiritsMagicStaff(); stickName = "blade spirits"; } // CIRCLE 5
				else if ( stickType == 34 ){ newWand = new DispelFieldMagicStaff(); stickName = "dispelling fields"; } // CIRCLE 5
				else if ( stickType == 35 ){ newWand = new IncognitoMagicStaff(); stickName = "disguises"; } // CIRCLE 5
				else if ( stickType == 36 ){ newWand = new MagicReflectionMagicStaff(); stickName = "magical reflection"; } // CIRCLE 5
				else if ( stickType == 37 ){ newWand = new MindBlastMagicStaff(); stickName = "mind blasting"; } // CIRCLE 5
				else if ( stickType == 38 ){ newWand = new ParalyzeMagicStaff(); stickName = "paralyzing"; } // CIRCLE 5
				else if ( stickType == 39 ){ newWand = new PoisonFieldMagicStaff(); stickName = "poisonous fields"; } // CIRCLE 5
				else if ( stickType == 40 ){ newWand = new SummonCreatureMagicStaff(); stickName = "creature summoning"; } // CIRCLE 5
				else if ( stickType == 41 ){ newWand = new DispelMagicStaff(); stickName = "dispelling"; } // CIRCLE 6
				else if ( stickType == 42 ){ newWand = new EnergyBoltMagicStaff(); stickName = "energy bolts"; } // CIRCLE 6
				else if ( stickType == 43 ){ newWand = new ExplosionMagicStaff(); stickName = "explosions"; } // CIRCLE 6
				else if ( stickType == 44 ){ newWand = new InvisibilityMagicStaff(); stickName = "invisibility"; } // CIRCLE 6
				else if ( stickType == 45 ){ newWand = new MarkMagicStaff(); stickName = "marking"; } // CIRCLE 6
				else if ( stickType == 46 ){ newWand = new MassCurseMagicStaff(); stickName = "mass curses"; } // CIRCLE 6
				else if ( stickType == 47 ){ newWand = new ParalyzeFieldMagicStaff(); stickName = "paralyzing fields"; } // CIRCLE 6
				else if ( stickType == 48 ){ newWand = new RevealMagicStaff(); stickName = "revealing"; } // CIRCLE 6
				else if ( stickType == 49 ){ newWand = new ChainLightningMagicStaff(); stickName = "chain lightning"; } // CIRCLE 7
				else if ( stickType == 50 ){ newWand = new EnergyFieldMagicStaff(); stickName = "energy fields"; } // CIRCLE 7
				else if ( stickType == 51 ){ newWand = new FlameStrikeMagicStaff(); stickName = "flame striking"; } // CIRCLE 7
				else if ( stickType == 52 ){ newWand = new GateTravelMagicStaff(); stickName = "gate travels"; } // CIRCLE 7
				else if ( stickType == 53 ){ newWand = new ManaVampireMagicStaff(); stickName = "mana vampire"; } // CIRCLE 7
				else if ( stickType == 54 ){ newWand = new MassDispelMagicStaff(); stickName = "mass dispelling"; } // CIRCLE 7
				else if ( stickType == 55 ){ newWand = new MeteorSwarmMagicStaff(); stickName = "meteor swarms"; } // CIRCLE 7
				else if ( stickType == 56 ){ newWand = new PolymorphMagicStaff(); stickName = "polymorphing"; } // CIRCLE 7
				else if ( stickType == 57 ){ newWand = new AirElementalMagicStaff(); stickName = "air elementals"; } // CIRCLE 8
				else if ( stickType == 58 ){ newWand = new EarthElementalMagicStaff(); stickName = "earth elementals"; } // CIRCLE 8
				else if ( stickType == 59 ){ newWand = new EarthquakeMagicStaff(); stickName = "earthquakes"; } // CIRCLE 8
				else if ( stickType == 60 ){ newWand = new EnergyVortexMagicStaff(); stickName = "vortex summoning"; } // CIRCLE 8
				else if ( stickType == 61 ){ newWand = new FireElementalMagicStaff(); stickName = "fire elementals"; } // CIRCLE 8
				else if ( stickType == 62 ){ newWand = new ResurrectionMagicStaff(); stickName = "resurrecting"; } // CIRCLE 8
				else if ( stickType == 63 ){ newWand = new SummonDaemonMagicStaff(); stickName = "daemon summoning"; } // CIRCLE 8
				else { newWand = new WaterElementalMagicStaff(); stickName = "water elementals"; } // CIRCLE 8

				string wandOwner = "";
					if ( Utility.RandomMinMax( 1, 3 ) == 1 ){ wandOwner = Server.LootPackEntry.MagicWandOwner() + " "; }

				newWand.Name = wandOwner + stick.WandMetal + " wand of " + stickName;
				newWand.Hue = stick.WandColor;
				newWand.ItemID = stick.WandID;
				ItemIdentification.ReplaceItemOrAddToBackpack(wand, newWand, m);

				m.SendMessage("This seems to be a " + newWand.Name + ".");
				if ( m != vendor ){ vendor.SayTo(m, "This seems to be a " + newWand.Name + "."); }
			}
			else
			{
				int nJunk = Utility.RandomMinMax( 1, 5 );
				string stickName = "";
				switch( nJunk )
				{
					case 1: stickName = "a useless stick"; break;
					case 2: stickName = "a wand that was never enchanted"; break;
					case 3: stickName = "a fake wand"; break;
					case 4: stickName = "nothing magical at all"; break;
					case 5: stickName = "a simple metal rod"; break;
				}

				m.SendMessage( "This seems to be " + stickName + ", so you throw it away." );
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !Movable )
			{
				from.SendMessage( "That cannot move so you cannot identify it." );
				return;
			}
			else if ( !IsChildOf( from.Backpack ) && Server.Misc.MyServerSettings.IdentifyItemsOnlyInPack() ) 
			{
				from.SendMessage( "This must be in your backpack to identify." );
				return;
			}
			else if ( !from.InRange( this.GetWorldLocation(), 3 ) )
			{
				from.SendMessage( "You will need to get closer to identify that." );
				return;
			}
			else
			{
				Server.Items.ItemIdentification.IDItem( from, this, this, false );
			}
		}

		public UnknownWand( Serial serial ) : base( serial )
		{
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			list.Add( 1070722, WandOrigin);
			list.Add( 1049644, "Unidentified"); // PARENTHESIS
        }

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
            writer.Write( WandLevel );
            writer.Write( WandOrigin );
            writer.Write( WandColor );
            writer.Write( WandID );
            writer.Write( WandMetal );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
            WandLevel = reader.ReadInt();
            WandOrigin = reader.ReadString();
			WandColor = reader.ReadInt();
			WandID = reader.ReadInt();
			WandMetal = reader.ReadString();
		}
	}
}