using System;
using Server.Items;
using Server.Network;
using System.Collections.Generic;
using System.Collections;
using Server.Mobiles;
using Server.Misc;
using Server.Targeting;
using Server.Multis;
using Server.Regions;

namespace Server.Items
{
	public class TaskManagerDaily : Item
	{
		
		private static DateTime lastrun;
		
		[Constructable]
		public TaskManagerDaily () : base( 0x0EDE )
		{
			Movable = false;
			Name = "Task Manager Daily";
			Visible = false;
			lastrun = DateTime.UtcNow;
			TaskTimer thisTimer = new TaskTimer( this ); 
			thisTimer.Start(); 
		}

        public TaskManagerDaily(Serial serial) : base(serial)
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
            writer.Write( (int) 0 ); // version
			writer.Write( (DateTime)lastrun );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
            int version = reader.ReadInt();
			lastrun = reader.ReadDateTime();
			
			if ( Server.Misc.MyServerSettings.RunRoutinesAtStartup() )
			{
				FirstTimer thisTimer = new FirstTimer( this ); 
				thisTimer.Start();
			}
		}

		public class TaskTimer : Timer 
		{
			private Item i_item; 
			public TaskTimer( Item task ) : base( TimeSpan.FromMinutes( 1440 ) )
			{ 
				Priority = TimerPriority.OneMinute; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				TaskTimer thisTimer = new TaskTimer( i_item ); 
				thisTimer.Start(); 
				RunThis();
			} 
		}

		public class FirstTimer : Timer 
		{ 
			private Item i_item; 
			public FirstTimer( Item task ) : base( TimeSpan.FromSeconds( 10.0 ) )
			{ 
				Priority = TimerPriority.OneSecond; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				TaskTimer thisTimer = new TaskTimer( i_item ); 
				thisTimer.Start(); 
				RunThis();
			} 
		}

		public static void RunThis()
		{
			
			Server.Items.WorkingSpots.PopulateVillages();
						
			if ( DateTime.UtcNow > ( lastrun + TimeSpan.FromHours( 23.0 )) ) 			
			{
				World.Broadcast( 0x35, true, "Executing daily routine" ); // Let the players know lag is incoming
				Console.WriteLine( "Begin Daily Tasks" );
				LoggingFunctions.LogServer( "Start - Arrange Quest Search Crates" );
				
				///// MOVE THE SEARCH PEDESTALS //////////////////////////////////////
				BuildQuests.SearchCreate();

				LoggingFunctions.LogServer( "Done - Arrange Quest Search Crates" );
				
				LoggingFunctions.LogServer( "Start - Change Stealing Pedestals" );
				
				///// MAKE THE STEAL PEDS LOOK DIFFERENT /////////////////////////////
				BuildSteadPeds.CreateStealPeds();

				LoggingFunctions.LogServer( "Done - Change Stealing Pedestals" );

				LoggingFunctions.LogServer( "Start - Remove Spread Out Monsters, Drinkers, And Healers" );
				
				///// CLEANUP THE CREATURES MASS SPREAD OUT IN THE LAND //////////////

				ArrayList targets = new ArrayList();
				ArrayList healers = new ArrayList();
				ArrayList exodus = new ArrayList();
				ArrayList serpent = new ArrayList();
				ArrayList gargoyle = new ArrayList();
				ArrayList internalinfected = new ArrayList();
				foreach ( Mobile creature in World.Mobiles.Values )
				{
					if ( creature is CodexGargoyleA || creature is CodexGargoyleB )
					{
						gargoyle.Add( creature );
					}
					else if ( creature is BaseCreature && ((BaseCreature)creature).CanInfect && creature.Map == Map.Internal )
						internalinfected.Add( creature );
					else if ( creature.WhisperHue == 999 || creature.WhisperHue == 911 )
					{
						if ( creature != null )
						{
							if ( creature is Adventurers || creature is WanderingHealer || creature is Courier || creature is Syth || creature is Jedi )
							{ healers.Add( creature ); }
							else if ( creature is Exodus ){ exodus.Add( creature ); }
							else if ( creature is Jormungandr ){ serpent.Add( creature ); }
							else { targets.Add( creature ); }
						}
					}
				}
				if (gargoyle.Count == 0)
				{
					CodexGargoyleA gargA = new CodexGargoyleA();
					Worlds.MoveToRandomDungeon( gargA );
					CodexGargoyleB gargB = new CodexGargoyleB();
					Worlds.MoveToRandomDungeon( gargB );
				}

				for ( int i = 0; i < internalinfected.Count; ++i )
				{
					Mobile creature = ( Mobile )internalinfected[ i ];
					creature.Delete();
				}										
					
				Server.Multis.BaseBoat.ClearShip(); // SAFETY CATCH TO CLEAR THE SHIPS OFF THE SEA
				for ( int i = 0; i < targets.Count; ++i )
				{
					Mobile creature = ( Mobile )targets[ i ];
					if ( creature.Hidden == false )
					{
						if ( creature.WhisperHue == 911 )
						{
							Effects.SendLocationEffect( creature.Location, creature.Map, 0x3400, 60, 0x6E4, 0 );
							Effects.PlaySound( creature.Location, creature.Map, 0x108 );
						}
						else
						{
							creature.PlaySound( 0x026 );
							Effects.SendLocationEffect( creature.Location, creature.Map, 0x352D, 16, 4 );
						}
					}
					creature.Delete();
				}
				for ( int i = 0; i < exodus.Count; ++i )
				{
					Mobile creature = ( Mobile )exodus[ i ];
					Server.Misc.IntelligentAction.BurnAway( creature );
					Worlds.MoveToRandomDungeon( creature );
					Server.Misc.IntelligentAction.BurnAway( creature );
				}
				for ( int i = 0; i < serpent.Count; ++i )
				{
					Mobile creature = ( Mobile )serpent[ i ];
					creature.PlaySound( 0x026 );
					Effects.SendLocationEffect( creature.Location, creature.Map, 0x352D, 16, 4 );
					Worlds.MoveToRandomOcean( creature );
					creature.PlaySound( 0x026 );
					Effects.SendLocationEffect( creature.Location, creature.Map, 0x352D, 16, 4 );
				}
				for ( int i = 0; i < gargoyle.Count; ++i )
				{
					Mobile creature = ( Mobile )gargoyle[ i ];
					Server.Misc.IntelligentAction.BurnAway( creature );
					Worlds.MoveToRandomDungeon( creature );
					Server.Misc.IntelligentAction.BurnAway( creature );
				}
				for ( int i = 0; i < healers.Count; ++i )
				{
					Mobile healer = ( Mobile )healers[ i ];
					Effects.SendLocationParticles( EffectItem.Create( healer.Location, healer.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 2023 );
					healer.PlaySound( 0x1FE );
					healer.Delete();
				}

				ArrayList drinkers = new ArrayList();
				foreach ( Mobile drunk in World.Mobiles.Values )
				if ( drunk is AdventurerWest || drunk is AdventurerSouth || drunk is AdventurerNorth || drunk is AdventurerEast || drunk is TavernPatronWest || drunk is TavernPatronSouth || drunk is TavernPatronNorth || drunk is TavernPatronEast )
				{
					if ( drunk != null )
					{
						drinkers.Add( drunk );
					}
				}
				for ( int i = 0; i < drinkers.Count; ++i )
				{
					Mobile drunk = ( Mobile )drinkers[ i ];
					Effects.SendLocationParticles( EffectItem.Create( drunk.Location, drunk.Map, EffectItem.DefaultDuration ), 0x3728, 8, 20, 5042 );
					Effects.PlaySound( drunk, drunk.Map, 0x201 );
					drunk.Delete();
				}

// Checking on investments	
	
				Console.WriteLine( "investment payout" );
				World.Broadcast( 0, true, "Investment bags have been calculated for the day!" );
				
				AetherGlobe.ChangeCurse( 0 );

				ArrayList bankBoxes = new ArrayList(); // start finding investments
				ArrayList stones = new ArrayList(); 
				ArrayList bones = new ArrayList(); // 
				
				foreach( Item bb in World.Items.Values )
				{

					
					if ( bb is BankBox )
					{
							bankBoxes.Add( bb );
					}
					if ( (bb is TombStone || bb is Corpse) && bb.RootParentEntity == null )
					{
						Region region = Region.Find( bb.Location, bb.Map );
						if (bb.Map != null && bb.Map != Map.Internal && !(region.IsPartOf( typeof( BardTownRegion ) ) ) && !(region.IsPartOf( typeof( BardDungeonRegion ) )) )
							stones.Add(bb);
					}
					if (bb is EssenceBones)
						bones.Add(bb);
				}

				foreach( BankBox ibb in bankBoxes )
				{

						double interest = 0;
						double evilinterest = 0;
						
						foreach( Item item in ibb.Items )
						{

							if( item is InterestBag )
							{
		
								List<Item> ItemsInBag = item.Items;
								for( int z = 0; z < ItemsInBag.Count; z++ )
								{
									Item inBag = ItemsInBag[z];

									if( inBag is InvestmentCheck ) // found an investment, make the changes!
									{
										if (Utility.RandomMinMax(1, 5000) == 69)
										{
											((InvestmentCheck)inBag).Worth = 0; // lose it all
											LoggingFunctions.LogInvestments( ibb.Owner, ((InvestmentCheck)inBag).Worth, false );	
										}
										else 
										{											
											interest = ((InvestmentCheck)inBag).Worth * AetherGlobe.rateofreturn ;
											((InvestmentCheck)inBag).Worth += (int)interest;
											
											if (interest > 0)
												LoggingFunctions.LogInvestments( ibb.Owner, (int)interest, true );
											else 
												LoggingFunctions.LogInvestments( ibb.Owner, (int)interest, false );
										}
									}
								}
							}
							if( item is EvilInterestBag )
							{
		
								List<Item> ItemsInBag = item.Items;
								for( int z = 0; z < ItemsInBag.Count; z++ )
								{
									Item inBag = ItemsInBag[z];

									if( inBag is InvestmentCheck ) // found an investment, make the changes!
									{
	
											if (AetherGlobe.rateofreturn >0)
												evilinterest = -(AetherGlobe.rateofreturn);
											else if (AetherGlobe.rateofreturn <0)
												evilinterest = Math.Abs(AetherGlobe.rateofreturn);
											
											interest = ((InvestmentCheck)inBag).Worth * evilinterest ;
											((InvestmentCheck)inBag).Worth += (int)interest;
											
											if (interest > 0)
												LoggingFunctions.LogInvestments( ibb.Owner, (int)interest, true );
											else 
												LoggingFunctions.LogInvestments( ibb.Owner, (int)interest, false );
										
									}
								}
							}
						}
				}
				foreach( Item stone in stones )
				{
					Region reg = Region.Find( stone.Location, stone.Map );
					BaseHouse house = BaseHouse.FindHouseAt( stone.Location, stone.Map, 16 );

					if ( Utility.RandomMinMax(1, 200) == 69 && house == null && !(reg.IsPartOf( typeof( SafeRegion )) || reg.IsPartOf( typeof ( PublicRegion )) || reg.IsPartOf( typeof ( ProtectedRegion )) || reg.IsPartOf( typeof ( HouseRegion )) ) )
					{
						AdventuresFunctions.PopulateStones( stone );
					}
				}
				foreach( EssenceBones bns in bones)
				{
					AdventuresFunctions.PopulateBones(bns);
				}

				//Decay BalanceEffect and crown champion
				World.Broadcast( 0, true, "Balance Champions have been chosen!" );
				Console.WriteLine( "Balance Champs chosen" );
				PlayerMobile goodchamp = null;
				PlayerMobile evilchamp = null;
				
/*				AetherGlobe.GoodChamp = null;
				AetherGlobe.EvilChamp = null;*/
				
				ArrayList players = new ArrayList();
				foreach ( Mobile player in World.Mobiles.Values )
				{
					if ( player is PlayerMobile && player.AccessLevel == AccessLevel.Player  )
					{
						if ( player != null && ((PlayerMobile)player).BalanceEffect != 0 && ((PlayerMobile)player).BalanceStatus != 0 )
						{
							players.Add( player );
						}
					}
				}
				for ( int i = 0; i < players.Count; ++i )
				{
					
						PlayerMobile m = (PlayerMobile)players[ i ];

						if ((Mobile)m == AetherGlobe.GoodChamp || (Mobile)m == AetherGlobe.EvilChamp)
						{
							double loss = (double)m.BalanceEffect * 0.10;
							if (m.SoulBound)
								loss = (double)m.BalanceEffect * 0.05;
								
							m.BalanceEffect -= (int)Math.Ceiling(loss); // decay the amount
						}

						// count portal decay values
						foreach ( Item itm in World.Items.Values )
						{
							if ( itm is PortalGood && ((PortalGood)itm).Owner == (Mobile)m )
							{
								int decayupkeep = (int)( 0.03  * (double)m.BalanceEffect);

								if (decayupkeep < 250) //player doesnt have enough bp to maintain this shard
								{
									((PortalGood)itm).Sleep = true;
								}
								else if (((PortalGood)itm).Sleep && decayupkeep > 250)
								{
									((PortalGood)itm).Sleep = false;
									m.BalanceEffect -= decayupkeep;
								}
								else 
									m.BalanceEffect -= decayupkeep;
							}
							else if ( itm is PortalEvil && ((PortalEvil)itm).Owner == (Mobile)m )
							{
								int decayupkeep = (int)( 0.03  * (double)m.BalanceEffect);

								if (decayupkeep < 250) //player doesnt have enough bp to maintain this shard
								{
									((PortalEvil)itm).Sleep = true;
								}
								else if (((PortalEvil)itm).Sleep && decayupkeep > 250)
								{
									((PortalEvil)itm).Sleep = false;
									m.BalanceEffect -= decayupkeep;
								}
								else 
									m.BalanceEffect -= decayupkeep;
							}
						}
								
						if (m.BalanceEffect < 0 && m.Avatar && m.BalanceStatus != 0 && m.Kills == 0 ) //contributed to good
						{
							if (goodchamp == null )
							{
								
								goodchamp = m;
							}
							else if (m.BalanceEffect < goodchamp.BalanceEffect )
							{
								
								goodchamp = m;
							}
						}
						else if (m.BalanceEffect > 0 && m.Avatar && m.BalanceStatus != 0 ) //contributed to evil
						{
							
							if (evilchamp == null)
							{
								
								evilchamp = m;
							}
							else if (m.BalanceEffect > evilchamp.BalanceEffect)
							{
								evilchamp = m;
								
							}
						}
				}
				
				
				if (goodchamp != null && goodchamp != AetherGlobe.GoodChamp)
				{
					AetherGlobe.GoodChamp = (Mobile)goodchamp;
					goodchamp.InvalidateProperties();
				}
				if (evilchamp != null && evilchamp != AetherGlobe.EvilChamp)
				{
					AetherGlobe.EvilChamp = (Mobile)evilchamp;
					evilchamp.InvalidateProperties();
				}

				if (AetherGlobe.GoodChamp != null)
					World.Broadcast( 0, true, goodchamp.Name + " valiantly fights for the side of Good" );
				if (AetherGlobe.EvilChamp != null)
					World.Broadcast( 0, true, evilchamp.Name + " continues to cause anarchy" );

				AdventuresFunctions.CheckInfection();
				if (AetherGlobe.invasionstage == 1)
					AdventuresFunctions.InvasionRoutine();

				Cleanup.Run();

				Farms.PlantGardens();
				Server.Mobiles.Citizens.PopulateCities();
				ThiefGuildmaster.WipeFlaggedList();		
				Server.Misc.AdventuresFunctions.CleanupInternalObjects( null, true );	
				Server.Misc.AdventuresFunctions.OldCharCleanup();
				LoggingFunctions.LogServer( "Done - Remove Spread Out Monsters, Drinkers, And Healers" );
				Console.WriteLine( "End Daily Tasks" );
				lastrun = DateTime.UtcNow;
			}
		}

	}
}
