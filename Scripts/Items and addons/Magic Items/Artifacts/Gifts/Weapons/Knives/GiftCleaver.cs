using System;
using Server.Network;
using Server.Items;
using Server.Engines.Craft;

namespace Server.Items
{
	[FlipableAttribute( 0xEC3, 0xEC2 )]
    public class GiftCleaver : BaseGiftKnife, ITinkerRepairable
    {
		public override WeaponAbility PrimaryAbility{ get{ return WeaponAbility.BleedAttack; } }
		public override WeaponAbility SecondaryAbility{ get{ return WeaponAbility.InfectiousStrike; } }
		public override WeaponAbility ThirdAbility{ get{ return WeaponAbility.MeleeProtection2; } }
		public override WeaponAbility FourthAbility{ get{ return WeaponAbility.ZapDexStrike; } }
		public override WeaponAbility FifthAbility{ get{ return WeaponAbility.ZapStamStrike; } }

		public override int AosStrengthReq{ get{ return 10; } }
		public override int AosMinDamage{ get{ return 11; } }
		public override int AosMaxDamage{ get{ return 13; } }
		public override int AosSpeed{ get{ return 46; } }
		public override float MlSpeed{ get{ return 2.50f; } }

		public override int OldStrengthReq{ get{ return 10; } }
		public override int OldMinDamage{ get{ return 2; } }
		public override int OldMaxDamage{ get{ return 13; } }
		public override int OldSpeed{ get{ return 40; } }

		public override int InitMinHits{ get{ return 31; } }
		public override int InitMaxHits{ get{ return 50; } }

		[Constructable]
		public GiftCleaver() : base( 0xEC3 )
		{
			Weight = 2.0;
		}

		public GiftCleaver( Serial serial ) : base( serial )
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

			if ( Weight == 1.0 )
				Weight = 2.0;
		}
	}
}