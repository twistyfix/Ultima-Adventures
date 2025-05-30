using System;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Items
{
	public class MistressCollar : BaseArmor, ITailorRepairable
	{
		public override int ArtifactRarity{ get{ return 58; } }

		public override int BasePhysicalResistance{ get{ return 12; } }
		public override int BaseFireResistance{ get{ return 9; } }
		public override int BaseColdResistance{ get{ return 12; } }
		public override int BasePoisonResistance{ get{ return 12; } }
		public override int BaseEnergyResistance{ get{ return 12; } }

		public override int InitMinHits{ get{ return Utility.RandomMinMax(100, 125); } }
		public override int InitMaxHits{ get{ return Utility.RandomMinMax(126, 150); } }

		public override int AosStrReq{ get{ return 20; } }

		public override int ArmorBase{ get{ return 15; } }

		public override ArmorMaterialType MaterialType{ get{ return ArmorMaterialType.Leather; } }
		public override CraftResource DefaultResource{ get{ return CraftResource.RegularLeather; } }

		public override ArmorMeditationAllowance DefMedAllowance{ get{ return ArmorMeditationAllowance.All; } }

		public override bool AllowMaleWearer{ get{ return false; } }

		[Constructable]
		public MistressCollar() : base( 0x13C7 )
		{
			Weight = 1.0;
			Name = "Mistress Collar";
			Hue = 1950;

			Attributes.DefendChance = 15;
			Attributes.EnhancePotions = 10;
			Attributes.LowerManaCost = 8;
			Attributes.LowerRegCost = 20;
			Attributes.Luck = 150;
			Attributes.NightSight = 1;
			Attributes.RegenHits = 2;
			Attributes.SpellDamage = 5;
		}

		public MistressCollar( Serial serial ) : base( serial )
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