using System;
using Server;
using Server.Network;
using System.Text;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
	public class KegRemovalTimer : Timer 
	{ 
		private Item i_item; 
		public KegRemovalTimer( Item item ) : base( TimeSpan.FromSeconds( 10.0 ) ) 
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

	public class Throwup : Item 
	{ 
		[Constructable] 
		public Throwup() : base( Utility.RandomList( 0xf3b, 0xf3c ) ) 
		{ 
			Name = "a puddle of vomit"; 
			Hue = 0x557; 
			Movable = false;

			KegRemovalTimer thisTimer = new KegRemovalTimer( this ); 
			thisTimer.Start(); 
		} 

		public override void OnSingleClick( Mobile from ) 
		{ 
			this.LabelTo( from, this.Name ); 
		} 
  
		public Throwup( Serial serial ) : base( serial ) 
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

	public class UnknownKeg : Item
	{
		public int KegFilled;

		[CommandProperty(AccessLevel.Owner)]
		public int Keg_Filled { get { return KegFilled; } set { KegFilled = value; InvalidateProperties(); } }

		[Constructable]
		public UnknownKeg() : base( 0x1AD6 )
		{
			ItemID = Utility.RandomList( 0x1AD6, 0x1AD7 );
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
			Name = sLiquid + " keg of liquid";
			KegFilled = Utility.RandomMinMax( 10, 100 );
			Weight = 20 + ((KegFilled * 80) / 100);
			Hue = 0x96D;
		}

		public UnknownKeg( Serial serial ) : base( serial )
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
			else if ( from.InRange( this.GetWorldLocation(), 1 ) )
			{
				if ( from.CheckSkill( SkillName.TasteID, -5, 125 ) )
				{
					if ( from.Body.IsHuman && !from.Mounted )
						from.Animate( 34, 5, 1, true, false, 0 );

					from.PlaySound( 0x2D6 );

					Server.Items.UnknownKeg.GiveKeg( from, this );
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
						Throwup puke = new Throwup(); 
						puke.Map = from.Map; 
						puke.Location = from.Location;
						from.SendMessage("You fail to identify the liquid, convulsing and spilling the keg.");
					}
					else if ( nReaction == 2 )
					{
						from.PlaySound( from.Female ? 798 : 1070 );
						from.Say( "*hiccup!*" );
						from.SendMessage("You fail to identify the liquid, spasming and spilling the keg.");
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
						from.SendMessage("You fail to identify the liquid, coughing and spilling the keg.");
					}
					else if ( nReaction == 5 )
					{
						from.PlaySound( from.Female ? 784 : 1055 );
						from.Say( "*clears throat*" );
						if ( !from.Mounted )
							from.Animate( 33, 5, 1, true, false, 0 );
						from.SendMessage("You fail to identify the liquid, hurting your throat...you dump out the keg.");
					}
					else if ( nReaction == 6 )
					{
						from.PlaySound( from.Female ? 782 : 1053 );
						from.Say( "*burp!*" );
						if ( !from.Mounted )
							from.Animate( 33, 5, 1, true, false, 0 );
						from.SendMessage("You fail to identify the liquid, accidentally drinking the entire keg.");
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
						from.SendMessage("You fail to identify the liquid, spitting it out and dumping the keg.");
					}

					from.AddToBackpack( new Keg() );
				}

				this.Delete();
			}
			else
			{
				from.SendLocalizedMessage( 502138 ); // That is too far away for you to use
			}
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);

			string Kword;

			if ( KegFilled <= 0 ){ Kword = "The keg is empty"; }
			else if ( KegFilled < 5 ){ Kword = "The keg is nearly empty"; }
			else if ( KegFilled < 20 ){ Kword = "The keg is not very full"; }
			else if ( KegFilled < 30 ){ Kword = "The keg is about one quarter full"; }
			else if ( KegFilled < 40 ){ Kword = "The keg is about one third full"; }
			else if ( KegFilled < 47 ){ Kword = "The keg is almost half full"; }
			else if ( KegFilled < 54 ){ Kword = "The keg is approximately half full"; }
			else if ( KegFilled < 70 ){ Kword = "The keg is more than half full"; }
			else if ( KegFilled < 80 ){ Kword = "The keg is about three quarters full"; }
			else if ( KegFilled < 96 ){ Kword = "The keg is very full"; }
			else if ( KegFilled < 100 ){ Kword = "The liquid is almost to the top of the keg"; }
			else { Kword = "The keg is completely full"; }

			list.Add( 1070722, Kword);

			list.Add( 1049644, "Unidentified"); // PARENTHESIS
        }

		public static void GiveKeg( Mobile from, UnknownKeg keg )
		{
			Item item = new PotionKeg();
			PotionKeg barrel = (PotionKeg)item;
			barrel.Held = keg.KegFilled;

			int potionType = Utility.RandomMinMax( 1, 31 );

			if ( Utility.RandomMinMax( 1, 125 ) <= from.Skills[SkillName.Cooking].Value ) // COOKS CAN FIND A POTION 1 LEVEL HIGHER
			{
				if ( potionType == 2 ){ potionType++; }
				else if ( potionType == 3 ){ potionType++; }
				else if ( potionType == 5 ){ potionType++; }
				else if ( potionType == 7 ){ potionType++; }
				else if ( potionType == 9 ){ potionType++; }
				else if ( potionType == 10 ){ potionType++; }
				else if ( potionType == 11 ){ potionType++; }
				else if ( potionType == 12 ){ potionType = 30; }
				else if ( potionType == 13 ){ potionType++; }
				else if ( potionType == 15 ){ potionType++; }
				else if ( potionType == 16 ){ potionType++; }
				else if ( potionType == 18 ){ potionType++; }
				else if ( potionType == 19 ){ potionType++; }
				else if ( potionType == 21 ){ potionType++; }
				else if ( potionType == 22 ){ potionType++; }
				else if ( potionType == 24 ){ potionType++; }
				else if ( potionType == 25 ){ potionType++; }
				else if ( potionType == 27 ){ potionType++; }
				else if ( potionType == 28 ){ potionType++; }
			}

			if ( potionType == 1 ){ barrel.Type = PotionEffect.Nightsight; }
			else if ( potionType == 2 ){ barrel.Type = PotionEffect.CureLesser; }
			else if ( potionType == 3 ){ barrel.Type = PotionEffect.Cure; }
			else if ( potionType == 4 ){ barrel.Type = PotionEffect.CureGreater; }
			else if ( potionType == 5 ){ barrel.Type = PotionEffect.Agility; }
			else if ( potionType == 6 ){ barrel.Type = PotionEffect.AgilityGreater; }
			else if ( potionType == 7 ){ barrel.Type = PotionEffect.Strength; }
			else if ( potionType == 8 ){ barrel.Type = PotionEffect.StrengthGreater; }
			else if ( potionType == 9 ){ barrel.Type = PotionEffect.PoisonLesser; }
			else if ( potionType == 10 ){ barrel.Type = PotionEffect.Poison; }
			else if ( potionType == 11 ){ barrel.Type = PotionEffect.PoisonGreater; }
			else if ( potionType == 12 ){ barrel.Type = PotionEffect.PoisonDeadly; }
			else if ( potionType == 13 ){ barrel.Type = PotionEffect.Refresh; }
			else if ( potionType == 14 ){ barrel.Type = PotionEffect.RefreshTotal; }
			else if ( potionType == 15 ){ barrel.Type = PotionEffect.HealLesser; }
			else if ( potionType == 16 ){ barrel.Type = PotionEffect.Heal; }
			else if ( potionType == 17 ){ barrel.Type = PotionEffect.HealGreater; }
			else if ( potionType == 18 ){ barrel.Type = PotionEffect.ExplosionLesser; }
			else if ( potionType == 19 ){ barrel.Type = PotionEffect.Explosion; }
			else if ( potionType == 20 ){ barrel.Type = PotionEffect.ExplosionGreater; }
			else if ( potionType == 21 ){ barrel.Type = PotionEffect.InvisibilityLesser; }
			else if ( potionType == 22 ){ barrel.Type = PotionEffect.Invisibility; }
			else if ( potionType == 23 ){ barrel.Type = PotionEffect.InvisibilityGreater; }
			else if ( potionType == 24 ){ barrel.Type = PotionEffect.RejuvenateLesser; }
			else if ( potionType == 25 ){ barrel.Type = PotionEffect.Rejuvenate; }
			else if ( potionType == 26 ){ barrel.Type = PotionEffect.RejuvenateGreater; }
			else if ( potionType == 27 ){ barrel.Type = PotionEffect.ManaLesser; }
			else if ( potionType == 28 ){ barrel.Type = PotionEffect.Mana; }
			else if ( potionType == 29 ){ barrel.Type = PotionEffect.ManaGreater; }
			else if ( potionType == 30 ){ barrel.Type = PotionEffect.PoisonLethal; }
			else { barrel.Type = PotionEffect.Invulnerability; }

			Server.Items.PotionKeg.SetColorKeg( item, item );
			from.SendMessage("This seems to be a " + barrel.Name + ".");
			ItemIdentification.ReplaceItemOrAddToBackpack(keg, barrel, from);
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
            writer.Write( KegFilled );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
            KegFilled = reader.ReadInt();
				if ( KegFilled < 1 ){ KegFilled = Utility.RandomMinMax( 10, 100 ); }
				Weight = 20 + ((KegFilled * 80) / 100);
		}
	}
}