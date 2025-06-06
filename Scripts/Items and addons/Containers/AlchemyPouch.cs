using System;
using Server;

namespace Server.Items
{
	[Flipable( 0x1C10, 0x1CC6 )]
    public class AlchemyPouch : LargeSack
    {
		[Constructable]
		public AlchemyPouch() : base()
		{
			Weight = 1.0;
			MaxItems = 50;
			Name = "alchemy rucksack";
			Hue = 0x89F;
		}

        public override bool CanAdd( Mobile from, Item item)
		{
            if ( Server.Misc.MaterialInfo.IsReagent( item ) ||
						item is GodBrewing || 
						item is Bottle || 
						item is Jar || 
						item is MortarPestle || 
						item is SurgeonsKnife || 
						item is GardenTool || 
						item is AlchemyPouch )
			{
				return true;
			}

			return false;
		}

		public override bool OnDragDropInto( Mobile from, Item dropped, Point3D p )
        {
			if (CanAdd(from, dropped)) return base.OnDragDropInto(from, dropped, p);

			if ( dropped is Container )
			{
                from.SendMessage("You can only use another alchemy rucksack within this sack.");
			}
			else
			{
				from.SendMessage("This rucksack is for small alchemical crafting items.");
			}

			return false;
        }

        public override bool OnDragDrop( Mobile from, Item dropped )
        {
			if (CanAdd(from, dropped)) return base.OnDragDrop(from, dropped);

			if ( dropped is Container)
			{
                from.SendMessage("You can only use another alchemy rucksack within this sack.");
			}
			else
			{
				from.SendMessage("This rucksack is for small alchemical crafting items.");
			}

			return false;
        }

        public AlchemyPouch( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			Weight = 1.0;
			MaxItems = 50;
			Name = "alchemy rucksack";
		}

		public override int GetTotal(TotalType type)
        {
			if (type != TotalType.Weight)
				return base.GetTotal(type);
			else
			{
				return (int)(TotalItemWeights() * (0.05));
			}
        }

		public override void UpdateTotal(Item sender, TotalType type, int delta)
        {
            if (type != TotalType.Weight)
                base.UpdateTotal(sender, type, delta);
            else
                base.UpdateTotal(sender, type, (int)(delta * (0.05)));
        }

		private double TotalItemWeights()
        {
			double weight = 0.0;

			foreach (Item item in Items)
				weight += (item.Weight * (double)(item.Amount));

			return weight;
        }
	}
}