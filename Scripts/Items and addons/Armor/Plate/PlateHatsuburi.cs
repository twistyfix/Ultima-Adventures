using System;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Items
{
	public class PlateHatsuburi : BaseArmor, IBlacksmithRepairable
    {
		public override int BasePhysicalResistance{ get{ return 12; } }
		public override int BaseFireResistance{ get{ return 5; } }
		public override int BaseColdResistance{ get{ return 5; } }
		public override int BasePoisonResistance{ get{ return 5; } }
		public override int BaseEnergyResistance{ get{ return 5; } }

		public override int InitMinHits{ get{ return 55; } }
		public override int InitMaxHits{ get{ return 75; } }

		public override int AosStrReq{ get{ return 65; } }
		public override int OldStrReq{ get{ return 65; } }

		public override int ArmorBase{ get{ return 4; } }

		public override ArmorMaterialType MaterialType{ get{ return ArmorMaterialType.Plate; } }

		[Constructable]
		public PlateHatsuburi() : base( 0x2775 )
		{
			Weight = 7.0;
		}

		public PlateHatsuburi( Serial serial ) : base( serial )
		{
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}