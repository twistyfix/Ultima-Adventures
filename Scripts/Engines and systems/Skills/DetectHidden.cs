using System;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Targeting;
using Server.Regions;

namespace Server.SkillHandlers
{
	public class DetectHidden
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.DetectHidden].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile src )
		{
			if ( src.Blessed )
			{
				src.SendMessage( "You cannot search while in this state." );
			}
			else 
			{
				src.SendLocalizedMessage( 500819 );//Where will you search?
				src.Target = new InternalTarget();
			}

			return TimeSpan.FromSeconds( 6.0 );
		}

		private class InternalTarget : Target
		{
			public InternalTarget() : base( 12, true, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile src, object targ )
			{
				bool foundAnyone = false;

				Point3D p;
				if ( targ is Mobile )
					p = ((Mobile)targ).Location;
				else if ( targ is Item )
					p = ((Item)targ).Location;
				else if ( targ is IPoint3D )
					p = new Point3D( (IPoint3D)targ );
				else 
					p = src.Location;

				double srcSkill = src.Skills[SkillName.DetectHidden].Value;
				int range = (int)(srcSkill / 10.0);

				if ( !src.CheckSkill( SkillName.DetectHidden, 0.0, 100.0 ) )
					range /= 2;

				BaseHouse house = BaseHouse.FindHouseAt( p, src.Map, 16 );

				bool inHouse = ( house != null && house.IsFriend( src ) );

				if ( inHouse )
					range = 22;

				if ( range > 0 )
				{
					IPooledEnumerable inRange = src.Map.GetMobilesInRange( p, range );

					foreach ( Mobile trg in inRange )
					{
						if ( trg.Hidden && src != trg )
						{
							double ss = srcSkill + Utility.Random( 21 ) - 10;
							double ts = trg.Skills[SkillName.Hiding].Value + Utility.Random( 21 ) - 10;

							if ( src.AccessLevel >= trg.AccessLevel && ( ss >= ts || ( inHouse && house.IsInside( trg ) ) ) )
							{
								trg.RevealingAction();
								trg.SendLocalizedMessage( 500814 ); // You have been revealed!
								foundAnyone = true;
							}
						}
					}

					inRange.Free();

					/// WIZARD WANTS THIS TO WORK FOR NORMAL TRAPS, HIDDEN TRAPS, & HIDDEN CONTAINERS ///
					IPooledEnumerable TitemsInRange = src.Map.GetItemsInRange( p, range );
					foreach ( Item item in TitemsInRange )
					{
						if ( Server.SkillHandlers.DetectHidden.DetectSomething( item, src, false ) )
							foundAnyone = true;
					}
					TitemsInRange.Free();
				}

				if ( !foundAnyone )
				{
					src.SendLocalizedMessage( 500817 ); // You can see nothing hidden there.
				}
			}
		}

		public static bool DetectSomething( Item item, Mobile m, bool skillCheck )
		{
			bool foundAnyone = false;
			string sTrap;

			if ( item is BaseTrap )
			{
				BaseTrap trap = (BaseTrap) item;

				if ( trap is FireColumnTrap ){ sTrap = "(fire column trap)"; }
				else if ( trap is FlameSpurtTrap ){ sTrap = "(fire spurt trap)"; }
				else if ( trap is GasTrap ){ sTrap = "(poison gas trap)"; }
				else if ( trap is GiantSpikeTrap ){ sTrap = "(giant spike trap)"; }
				else if ( trap is MushroomTrap ){ sTrap = "(odd mushroom)"; }
				else if ( trap is SawTrap ){ sTrap = "(saw blade trap)"; }
				else if ( trap is SpikeTrap ){ sTrap = "(spike trap)"; }
				else if ( trap is StoneFaceTrap ){ sTrap = "(stone face trap)"; }
				else { sTrap = ""; }

				if ( m.Alive && ( !skillCheck || m.CheckSkill( SkillName.DetectHidden, 0, 100 ) ) )
				{
					Effects.SendLocationParticles( EffectItem.Create( item.Location, item.Map, EffectItem.DefaultDuration ), 0x376A, 9, 32, 5024 );
					Effects.PlaySound( item.Location, item.Map, 0x1FA );
					m.SendMessage( "There is a trap nearby! " + sTrap + "" );
					foundAnyone = true;
				}
			}
			else if ( item is KillerTile )
			{
				if ( m.Alive && ( !skillCheck || m.CheckSkill( SkillName.DetectHidden, 0, 100 ) ) )
				{
					Effects.SendLocationParticles( EffectItem.Create( item.Location, item.Map, EffectItem.DefaultDuration ), 0x376A, 9, 32, 5024 );
					Effects.PlaySound( item.Location, item.Map, 0x1FA );
					foundAnyone = true;
				}
			}
			else if ( item is BaseDoor && (	item.ItemID == 0x35E || 
											item.ItemID == 0xF0 || 
											item.ItemID == 0xF2 || 
											item.ItemID == 0x326 || 
											item.ItemID == 0x324 || 
											item.ItemID == 0x32E || 
											item.ItemID == 0x32C || 
											item.ItemID == 0x314 || 
											item.ItemID == 0x316 || 
											item.ItemID == 0x31C || 
											item.ItemID == 0x31E || 
											item.ItemID == 0xE8 || 
											item.ItemID == 0xEA || 
											item.ItemID == 0x34C || 
											item.ItemID == 0x356 || 
											item.ItemID == 0x35C || 
											item.ItemID == 0x354 || 
											item.ItemID == 0x344 || 
											item.ItemID == 0x346 || 
											item.ItemID == 0x34E || 
											item.ItemID == 0x334 || 
											item.ItemID == 0x336 || 
											item.ItemID == 0x33C || 
											item.ItemID == 0x33E ) )
			{
				if ( m.Alive && ( !skillCheck || m.CheckSkill( SkillName.DetectHidden, 0, 100 ) ) )
				{
					Effects.SendLocationParticles( EffectItem.Create( item.Location, item.Map, EffectItem.DefaultDuration ), 0x376A, 9, 32, 5024 );
					Effects.PlaySound( item.Location, item.Map, 0x1FA );
					m.SendMessage( "There is a hidden door nearby!" );
					foundAnyone = true;
				}
			}
			else if ( item is HiddenTrap )
			{
				if ( m.Alive && ( !skillCheck || m.CheckSkill( SkillName.DetectHidden, 0, 100 ) ) )
				{
					string textSay = "There is a hidden floor trap somewhere nearby!";
					if ( Server.Misc.Worlds.IsOnSpaceship( item.Location, item.Map ) )
					{
						textSay = "There is a dangerous area nearby!";
					}
					Effects.SendLocationParticles( EffectItem.Create( item.Location, item.Map, EffectItem.DefaultDuration ), 0x376A, 9, 32, 5024 );
					Effects.PlaySound( item.Location, item.Map, 0x1FA );
					m.SendMessage( textSay );
					foundAnyone = true;
					
					item.Name = "(detected)";
					item.Visible = true;
					
				}
			}
			else if ( item is HiddenChest )
			{
				if ( m.Alive && ( !skillCheck || m.CheckSkill( SkillName.DetectHidden, 0, 100 ) ) )
				{
					m.SendMessage( "Your eye catches something nearby." );
					Map map = m.Map;
					string where = Server.Misc.Worlds.GetRegionName( m.Map, m.Location );

					int money = Utility.RandomMinMax( 100, 200 );

					int level = (int)(m.Skills[SkillName.DetectHidden].Value / 10);
						if (level < 1){level = 1;}
						if (level > 10){level = 10;}

					switch( Utility.RandomMinMax( 1, level ) )
					{
						case 1: level = 1; break;
						case 2: level = 2; break;
						case 3: level = 3; break;
						case 4: level = 4; break;
						case 5: level = 5; break;
						case 6: level = 6; break;
						case 7: level = 7; break;
						case 8: level = 8; break;
						case 9: level = 9; break;
						case 10: level = 10; break;
					}

					if ( Utility.RandomMinMax( 1, 3 ) > 1 )
					{
						Item coins = new Gold( ( money * level ) );

						if ( Server.Misc.Worlds.IsOnSpaceship( item.Location, item.Map ) )
						{
							coins.Delete(); coins = new DDXormite(); coins.Amount = (int)( ( money * level ) / 3 );
						}
						else if ( Server.Misc.Worlds.GetMyWorld( item.Map, item.Location, item.X, item.Y ) == "the Underworld" )
						{
							coins.Delete(); coins = new DDJewels(); coins.Amount = (int)( ( money * level ) / 2 );
						}
						else if ( Utility.RandomMinMax( 1, 100 ) > 99 )
						{
							coins.Delete(); coins = new DDGemstones(); coins.Amount = (int)( ( money * level ) / 2 );
						}
						else if ( Utility.RandomMinMax( 1, 100 ) > 95 )
						{
							coins.Delete(); coins = new DDGoldNuggets(); coins.Amount = (int)( ( money * level ) );
						}
						else if ( Utility.RandomMinMax( 1, 100 ) > 80 )
						{
							coins.Delete(); coins = new DDSilver(); coins.Amount = (int)( ( money * level ) * 5 );
						}
						else if ( Utility.RandomMinMax( 1, 100 ) > 60 )
						{
							coins.Delete(); coins = new DDCopper(); coins.Amount = (int)( ( money * level ) * 10 );
						}
						if (coins is Gold && m is PlayerMobile) {
							if (((PlayerMobile)m).SoulBound) {
								Phylactery phylactery = ((PlayerMobile)m).FindPhylactery();
								if (phylactery != null) {
									coins.Amount += phylactery.CalculateExtraGold(coins.Amount);
								}
							}
						}
						Point3D loc = item.Location;
						coins.MoveToWorld( loc, map );
						Effects.SendLocationParticles( EffectItem.Create( coins.Location, coins.Map, EffectItem.DefaultDuration ), 0x376A, 9, 32, 5024 );
						Effects.PlaySound( coins.Location, coins.Map, 0x1FA );
						Engines.ExpirationTracker.AutoDelete(coins.Serial, TimeSpan.FromDays(2));
					}
					else
					{
						HiddenBox mBox = new HiddenBox( level, where, m );

						Point3D loc = item.Location;
						mBox.MoveToWorld( loc, map );
						Effects.SendLocationParticles( EffectItem.Create( mBox.Location, mBox.Map, EffectItem.DefaultDuration ), 0x376A, 9, 32, 5024 );
						Effects.PlaySound( mBox.Location, mBox.Map, 0x1FA );
						Engines.ExpirationTracker.AutoDelete(mBox.Serial, TimeSpan.FromDays(2));
					}

					foundAnyone = true;
					item.Delete();
				}
			}
			return foundAnyone;
		}
	}
}
