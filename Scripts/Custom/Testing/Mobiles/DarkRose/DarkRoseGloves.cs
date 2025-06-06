using System;
using Server;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Items
{

	public class DarkRoseGloves : BaseArmor, IBlacksmithRepairable
    {
        public override int BasePhysicalResistance { get { return 5; } }
        public override int BaseFireResistance { get { return 3; } }
        public override int BaseColdResistance { get { return 4; } }
        public override int BasePoisonResistance { get { return 7; } }
        public override int BaseEnergyResistance { get { return 6; } }

        public override int InitMinHits { get { return 150; } }
        public override int InitMaxHits { get { return 200; } }

        public override int AosStrReq { get { return 70; } }
        public override int OldStrReq { get { return 30; } }

        public override int ArmorBase { get { return 40; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Plate; } }
		
		[Constructable]
        public DarkRoseGloves(): base(0x1414)
        {
            Name = "Dark Rose Gloves";
            Hue = 2949;

            Attributes.CastSpeed = 8;
            Attributes.DefendChance = 12;
            Attributes.LowerManaCost = 7;
            Attributes.Luck = 100;
            Attributes.NightSight = 1;
            Attributes.ReflectPhysical = 10;
            Attributes.RegenMana = 8;
            Attributes.WeaponDamage = 10;

            ColdBonus = 9;
            FireBonus = 12;
            PhysicalBonus = 14;


		}

        public DarkRoseGloves(Serial serial)
            : base(serial)
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