using System;
using Server;
using Server.Engines.Craft;
using Server.Guilds;

namespace Server.Items
{
	// Based off a ChaosShield
	[FlipableAttribute( 0x4228, 0x4229 )]
	public class GargishChaosShield : BaseShield, IBlacksmithRepairable
    {
		public override int BasePhysicalResistance{ get{ return 1; } }
		public override int BaseFireResistance{ get{ return 0; } }
		public override int BaseColdResistance{ get{ return 0; } }
		public override int BasePoisonResistance{ get{ return 0; } }
		public override int BaseEnergyResistance{ get{ return 0; } }

		public override int InitMinHits{ get{ return 100; } }
		public override int InitMaxHits{ get{ return 125; } }

		public override int AosStrReq{ get{ return 95; } }

		public override int ArmorBase{ get{ return 32; } }

		[Constructable]
		public GargishChaosShield() : base( 0x4228 )
		{
			if ( !Core.AOS )
				LootType = LootType.Newbied;

			//Weight = 5.0;
		}

		public GargishChaosShield( Serial serial ) : base(serial)
		{
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)0 );//version
		}

		public override bool OnEquip( Mobile from )
		{
			return Validate( from ) && base.OnEquip( from );
		}

		public override void OnSingleClick( Mobile from )
		{
			if ( Validate( Parent as Mobile ) )
				base.OnSingleClick( from );
		}

		public virtual bool Validate( Mobile m )
		{
			if ( m == null || !m.Player || m.AccessLevel != AccessLevel.Player || Core.AOS )
				return true;

			Guild g = m.Guild as Guild;

			if ( g == null || g.Type != GuildType.Chaos )
			{
				m.FixedEffect( 0x3728, 10, 13 );
				Delete();

				return false;
			}

			return true;
		}
	}
}
