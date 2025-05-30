using System;
using Server.Items;
using Server.Targeting;
using System.Collections;

namespace Server.Mobiles
{
	[CorpseName( "a spider corpse" )]
	public class MonstrousSpider : BaseCreature
	{
		public override int BreathPhysicalDamage{ get{ return 50; } }
		public override int BreathFireDamage{ get{ return 0; } }
		public override int BreathColdDamage{ get{ return 0; } }
		public override int BreathPoisonDamage{ get{ return 50; } }
		public override int BreathEnergyDamage{ get{ return 0; } }
		public override int BreathEffectHue{ get{ return 0; } }
		public override int BreathEffectSound{ get{ return 0x62A; } }
		public override int BreathEffectItemID{ get{ return 0x10D4; } }
		public override bool HasBreath{ get{ return true; } }
		public override double BreathEffectDelay{ get{ return 0.1; } }
		public override void BreathDealDamage( Mobile target, int form ){ base.BreathDealDamage( target, 6 ); }

		[Constructable]
		public MonstrousSpider() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a monstrous spider";
			Body = 173;
			BaseSoundID = 0x388;

			SetStr( 376, 400 );
			SetDex( 376, 395 );
			SetInt( 336, 360 );

			SetHits( 346, 360 );
			SetMana( 0 );

			SetDamage( 22, 28 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 80 );
			SetResistance( ResistanceType.Poison, 100 );

			SetSkill( SkillName.Poisoning, 120.0 );
			SetSkill( SkillName.MagicResist, 60.0 );
			SetSkill( SkillName.Tactics, 80.0 );
			SetSkill( SkillName.Wrestling, 80.0 );

			Fame = 6000;
			Karma = -6000;

			VirtualArmor = 50;
			
			Tamable = true;
			ControlSlots = 3;
			MinTameSkill = 110;

			PackItem( new SpidersSilk( 100 ) );

			Item Venom = new VenomSack();
				Venom.Name = "lethal venom sack";
				AddItem( Venom );
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Rich );
		}

		public override FoodType FavoriteFood{ get{ return FoodType.Meat; } }
		public override Poison PoisonImmune{ get{ return Poison.Deadly; } }
		public override Poison HitPoison{ get{ return Poison.Lethal; } }
		public override bool CanAngerOnTame { get { return true; } }

		public override int GetAttackSound(){ return 0x601; }	// A
		public override int GetDeathSound(){ return 0x602; }	// D
		public override int GetHurtSound(){ return 0x603; }		// H

		public MonstrousSpider( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}