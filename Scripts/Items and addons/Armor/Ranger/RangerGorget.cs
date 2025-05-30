using System;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Items
{
	public class RangerGorget : BaseArmor, ITailorRepairable
    {
		public override int BasePhysicalResistance{ get{ return 4; } }
		public override int BaseFireResistance{ get{ return 8; } }
		public override int BaseColdResistance{ get{ return 6; } }
		public override int BasePoisonResistance{ get{ return 6; } }
		public override int BaseEnergyResistance{ get{ return 8; } }

		public override int InitMinHits{ get{ return 35; } }
		public override int InitMaxHits{ get{ return 45; } }

		public override int AosStrReq{ get{ return 25; } }
		public override int OldStrReq{ get{ return 25; } }

		public override int ArmorBase{ get{ return 16; } }

		public override ArmorMaterialType MaterialType{ get{ return ArmorMaterialType.Studded; } }
		public override CraftResource DefaultResource{ get{ return CraftResource.RegularLeather; } }

		public override int LabelNumber{ get{ return 1041495; } } // studded gorget, ranger armor

		[Constructable]
		public RangerGorget() : base( 0x13D6 )
		{
			Weight = 1.0;
			Hue = 0x59C;
			SkillBonuses.SetValues( 0, SkillName.Camping, 3 );
			SkillBonuses.SetValues( 1, SkillName.Tracking, 3 );
		}

		public RangerGorget( Serial serial ) : base( serial )
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