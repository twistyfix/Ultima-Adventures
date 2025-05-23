using System;
using System.Collections.Generic;
using Server;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Engines.BulkOrders
{
	public class SmallTailorBOD : SmallBOD
	{
		public override int ComputeFame()
		{
			return TailorRewardCalculator.Instance.ComputeFame( this );
		}

		public override int ComputeGold()
		{
			return TailorRewardCalculator.Instance.ComputeGold( this );
		}

		public override List<Item> ComputeRewards( bool full )
		{
			List<Item> list = new List<Item>();

			RewardGroup rewardGroup = TailorRewardCalculator.Instance.LookupRewards( TailorRewardCalculator.Instance.ComputePoints( this ) );

			if ( rewardGroup != null )
			{
				if ( full )
				{
					for ( int i = 0; i < rewardGroup.Items.Length; ++i )
					{
						Item item = rewardGroup.Items[i].Construct();

						if ( item != null )
							list.Add( item );
					}
				}
				else
				{
					RewardItem rewardItem = rewardGroup.AcquireItem();

					if ( rewardItem != null )
					{
						Item item = rewardItem.Construct();

						if ( item != null )
							list.Add( item );
					}
				}
			}

			return list;
		}

		public static SmallTailorBOD CreateRandomFor( Mobile m, bool defaultMaterialOnly = true )
		{
			SmallBulkEntry[] entries;
			bool useMaterials;

			double theirSkill = m.Skills[SkillName.Tailoring].Base;
			if ( useMaterials = Utility.RandomBool() && theirSkill >= 6.2 ) // Ugly, but the easiest leather BOD is Leather Cap which requires at least 6.2 skill.
				entries = SmallBulkEntry.TailorLeather;
			else
				entries = SmallBulkEntry.TailorCloth;

            if (defaultMaterialOnly) useMaterials = false;

			if ( entries.Length > 0 )
			{
				int amountMax;

				if ( theirSkill >= 70.1 )
					amountMax = Utility.RandomList( 10, 15, 20, 20 );
				else if ( theirSkill >= 50.1 )
					amountMax = Utility.RandomList( 10, 15, 15, 20 );
				else
					amountMax = Utility.RandomList( 10, 10, 15, 20 );

				BulkMaterialType material = BulkMaterialType.None;

				if ( useMaterials && theirSkill >= 70.1 )
				{
					for ( int i = 0; i < 20; ++i )
					{
						BulkMaterialType check = GetRandomMaterial( BulkMaterialType.Horned, BulkMaterialType.Alien );
						double skillReq = 0.0;

						switch ( check )
						{
							case BulkMaterialType.Horned: skillReq = 60.0; break;
                            case BulkMaterialType.Barbed: skillReq = 65.0; break;
                            case BulkMaterialType.Necrotic: skillReq = 70.0; break;
                            case BulkMaterialType.Volcanic: skillReq = 75.0; break;
                            case BulkMaterialType.Frozen: skillReq = 80.0; break;
                            case BulkMaterialType.Spined: skillReq = 85.0; break;
                            case BulkMaterialType.Goliath: skillReq = 90.0; break;
                            case BulkMaterialType.Draconic: skillReq = 95.0; break;
							case BulkMaterialType.Hellish: skillReq = 99.0; break;
							case BulkMaterialType.Dinosaur: skillReq = 105.0; break;
							case BulkMaterialType.Alien: skillReq = 120.0; break;
						}

						if ( theirSkill >= skillReq )
						{
							material = check;
							break;
						}
					}
				}

				double excChance = 0.0;

				if ( theirSkill >= 70.1 )
					excChance = (theirSkill + 80.0) / 200.0;

				bool reqExceptional = ( excChance > Utility.RandomDouble() );

				SmallBulkEntry entry = null;

				CraftSystem system = DefTailoring.CraftSystem;

				for ( int i = 0; i < 150; ++i )
				{
					SmallBulkEntry check = entries[Utility.Random( entries.Length )];

					CraftItem item = system.CraftItems.SearchFor( check.Type );

					if ( item != null )
					{
						bool allRequiredSkills = true;
						double chance = item.GetSuccessChance( m, null, system, false, ref allRequiredSkills );

						if ( allRequiredSkills && chance >= 0.0 )
						{
							if ( reqExceptional )
								chance = item.GetExceptionalChance( system, chance, m );

							if ( chance > 0.0 )
							{
								entry = check;
								break;
							}
						}
					}
				}
					
				if ( entry != null )
					return new SmallTailorBOD( entry, material, amountMax, reqExceptional );
			}

			return null;
		}

		private SmallTailorBOD( SmallBulkEntry entry, BulkMaterialType material, int amountMax, bool reqExceptional )
		{
			this.Hue = 0x483;
			this.AmountMax = amountMax;
			this.Type = entry.Type;
			this.Number = entry.Number;
			this.Graphic = entry.Graphic;
			this.RequireExceptional = reqExceptional;
			this.Material = material;
		}

		[Constructable]
		public SmallTailorBOD()
		{
			SmallBulkEntry[] entries;
			bool useMaterials;

			if ( useMaterials = Utility.RandomBool() )
				entries = SmallBulkEntry.TailorLeather;
			else
				entries = SmallBulkEntry.TailorCloth;

			if ( entries.Length > 0 )
			{
				int hue = 0x483;
				int amountMax = Utility.RandomList( 10, 15, 20 );

				BulkMaterialType material;

				if ( useMaterials )
					material = GetRandomMaterial( BulkMaterialType.Horned, BulkMaterialType.Alien );
				else
					material = BulkMaterialType.None;

				bool reqExceptional = Utility.RandomBool() || (material == BulkMaterialType.None);

				SmallBulkEntry entry = entries[Utility.Random( entries.Length )];

				this.Hue = hue;
				this.AmountMax = amountMax;
				this.Type = entry.Type;
				this.Number = entry.Number;
				this.Graphic = entry.Graphic;
				this.RequireExceptional = reqExceptional;
				this.Material = material;
			}
		}

		public SmallTailorBOD( int amountCur, int amountMax, Type type, int number, int graphic, bool reqExceptional, BulkMaterialType mat )
		{
			this.Hue = 0x483;
			this.AmountMax = amountMax;
			this.AmountCur = amountCur;
			this.Type = type;
			this.Number = number;
			this.Graphic = graphic;
			this.RequireExceptional = reqExceptional;
			this.Material = mat;
		}

		public SmallTailorBOD( Serial serial ) : base( serial )
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
            switch (version)
            {
                case 1:
                    break;

                case 0:
                    if (Material != BulkMaterialType.None)
                    {
                        Material += 7; // Number of Metals added ahead of it in the Enum
                    }
                    break;
            }
        }
	}
}