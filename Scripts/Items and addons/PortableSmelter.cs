using System;
using Server;
using Server.Network;
using System.Text;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
	public class PortableSmelter : Item
	{
		private int m_Charges;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Charges
		{
			get{ return m_Charges; }
			set{ m_Charges = value; InvalidateProperties(); }
		}

		[Constructable]
		public PortableSmelter() : base( 0x540A )
		{
			Name = "portable smelter";
			Weight = 5;
			ItemID = Utility.RandomMinMax( 0x540A, 0x540B );
			Charges = Utility.RandomMinMax( 50, 100 );
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( m_Charges > 1 ){ list.Add( 1070722, m_Charges.ToString() + " Uses Left"); }
			else { list.Add( 1070722, "1 Use Left"); }
            list.Add( 1049644, "Smelt ore into ingots");
        }

		public override void OnDoubleClick( Mobile from )
		{
			if ( !Movable )
				return;

			if ( !IsChildOf( from.Backpack ) ) 
			{
				from.SendMessage( "This must be in your backpack to use." );
				return;
			}
			else
			{
				from.SendMessage( "Select the ore you want to smelt into ingots." );
				from.Target = new InternalTarget( this );
			}
		}

		private class InternalTarget : Target
		{
			private PortableSmelter m_Tool;

			public InternalTarget( PortableSmelter tool ) :  base ( 2, false, TargetFlags.None )
			{
				m_Tool = tool;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is BaseOre )
				{
					BaseOre m_Ore = (BaseOre)targeted;

					if ( m_Ore.Deleted )
						return;

					if ( !from.InRange( m_Ore.GetWorldLocation(), 2 ) )
					{
						from.SendMessage( "The ore is too far away." );
						return;
					}

					double difficulty = CraftResources.GetMetalProcessDifficulty(m_Ore.Resource);
					double minSkill = difficulty - 25.0;
					double maxSkill = difficulty + 25.0;

					if ( difficulty > 50.0 && difficulty > from.Skills[SkillName.Mining].Value )
					{
						from.SendLocalizedMessage( 501986 ); // You have no idea how to smelt this strange ore!
						return;
					}
					
					if ( m_Ore.Amount <= 1 && m_Ore.ItemID == 0x19B7 )
					{
						from.SendLocalizedMessage( 501987 ); // There is not enough metal-bearing ore in this pile to make an ingot.
						return;
					}

					if ( from.CheckTargetSkill( SkillName.Mining, targeted, minSkill, maxSkill ) )
					{
						if ( m_Ore.Amount <= 0 )
						{
							from.SendLocalizedMessage( 501987 ); // There is not enough metal-bearing ore in this pile to make an ingot.
						}
						else
						{
							int amount = m_Ore.Amount;
							if ( m_Ore.Amount > 30000 )
								amount = 30000;

							BaseIngot ingot = m_Ore.GetIngot();
							
							if ( m_Ore.ItemID == 0x19B7 )
							{
								if ( m_Ore.Amount % 2 == 0 )
								{
									amount /= 2;
									m_Ore.Delete();
								}
								else
								{
									amount /= 2;
									m_Ore.Amount = 1;
								}
							}
								
							else if ( m_Ore.ItemID == 0x19B9 )
							{
								amount *= 2;
								m_Ore.Delete();
							}
							
							else
							{
								amount /= 1;
								m_Ore.Delete();
							}

							ingot.Amount = amount;
							from.AddToBackpack( ingot );
							from.PlaySound( 0x208 );

							from.SendLocalizedMessage( 501988 ); // You smelt the ore removing the impurities and put the metal in your backpack.
							m_Tool.ConsumeCharge( from );
						}
					}
					else if ( m_Ore.Amount < 2 && m_Ore.ItemID == 0x19B9 )
					{
						from.SendLocalizedMessage( 501990 ); // You burn away the impurities but are left with less useable metal.
						m_Ore.ItemID = 0x19B8;
						from.PlaySound( 0x208 );
						m_Tool.ConsumeCharge( from );
					}
					else if ( m_Ore.Amount < 2 && m_Ore.ItemID == 0x19B8 || m_Ore.ItemID == 0x19BA )
					{
						from.SendLocalizedMessage( 501990 ); // You burn away the impurities but are left with less useable metal.
						m_Ore.ItemID = 0x19B7;
						from.PlaySound( 0x208 );
						m_Tool.ConsumeCharge( from );
					}
					else
					{
						from.SendLocalizedMessage( 501990 ); // You burn away the impurities but are left with less useable metal.
						m_Ore.Amount /= 2;
						from.PlaySound( 0x208 );
						m_Tool.ConsumeCharge( from );
					}
				}
				else
				{
					from.SendMessage( "You can only use this on ore." );
				}
			}
		}

		public void ConsumeCharge( Mobile from )
		{
			--Charges;

			if ( Charges == 0 )
			{
				from.SendMessage( "The smelter was used too much and broke." );
				Item MyJunk = new SpaceJunkA();
			  	MyJunk.Hue = this.Hue;
			  	MyJunk.ItemID = this.ItemID;
				MyJunk.Name = Server.Items.SpaceJunk.RandomCondition() + " portable smelter";
				MyJunk.Weight = this.Weight;
			  	from.AddToBackpack ( MyJunk );
				this.Delete();
			}
		}

		public PortableSmelter( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
			writer.Write( (int) m_Charges );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_Charges = (int)reader.ReadInt();
		}
	}
}