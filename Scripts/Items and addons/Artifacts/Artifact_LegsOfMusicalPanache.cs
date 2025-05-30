using System;
using Server;


namespace Server.Items
{
	public class LegsOfMusicalPanache : LeatherLegs
	{
		public override int InitMinHits{ get{ return 80; } }
		public override int InitMaxHits{ get{ return 160; } }
		public override int BasePhysicalResistance{ get{ return 18; } }
		public override int BaseFireResistance{ get{ return 18; } }
		public override int BasePoisonResistance{ get{ return 18; } }
		public override int BaseColdResistance{ get{ return 18; } }
		public override int BaseEnergyResistance{ get{ return 18; } }


		[Constructable]
		public LegsOfMusicalPanache()
		{
			Name = "Leggings Of Musical Panache";
			Hue = 0x632;
			SkillBonuses.SetValues( 0, SkillName.Discordance, 15 );
			SkillBonuses.SetValues( 2, SkillName.Peacemaking, 15 );
			SkillBonuses.SetValues( 3, SkillName.Provocation, 15 );

			Attributes.BonusDex = 10;
			Attributes.RegenStam = 8;
			Attributes.DefendChance = 10;
            Attributes.Luck = 100;
			//remember the powers they give to resist discord and peace
		}


        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			list.Add( 1070722, "Artifact");
        }


		public LegsOfMusicalPanache( Serial serial ) : base( serial )
		{
		}


		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );


			writer.Write( (int) 1 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );


			int version = reader.ReadInt();
		}
	}
}