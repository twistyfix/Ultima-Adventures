using System;
using Server;
using Server.Engines.Craft;

namespace Server.Items
{
    public class GiftMetalShield : BaseGiftShield, IBlacksmithRepairable
    {
		public override int BasePhysicalResistance{ get{ return 0; } }
		public override int BaseFireResistance{ get{ return 1; } }
		public override int BaseColdResistance{ get{ return 0; } }
		public override int BasePoisonResistance{ get{ return 0; } }
		public override int BaseEnergyResistance{ get{ return 0; } }

		public override int InitMinHits{ get{ return 50; } }
		public override int InitMaxHits{ get{ return 65; } }

		public override int AosStrReq{ get{ return 45; } }

		public override int ArmorBase{ get{ return 11; } }

        [Constructable]
		public GiftMetalShield() : base( 0x1B7B )
		{
			Name = "metal shield";
			Weight = 6.0;
		}

		public GiftMetalShield( Serial serial ) : base(serial)
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
	}
}
