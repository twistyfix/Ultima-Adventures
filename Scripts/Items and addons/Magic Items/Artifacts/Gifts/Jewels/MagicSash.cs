using System;
using Server;
using Server.Engines.Craft;
using Server.Misc;

namespace Server.Items
{
	public class GiftSash : GiftGoldRing, ITailorRepairable, IClothingStub
	{
		[Constructable]
		public GiftSash()
		{
			Resource = CraftResource.None;
			Name = "sash";
			ItemID = 0x1541;
			Hue = RandomThings.GetRandomColor(0);
			Layer = Layer.MiddleTorso;
			Weight = 2.0;
		}

		public GiftSash( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}