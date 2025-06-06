using System;
using System.Collections;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Necromancy
{
	public class BloodOathSpell : NecromancerSpell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Blood Oath", "In Jux Mani Xen",
				203,
				9031,
				Reagent.DaemonBlood
			);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1.5 ); } }

		public override double RequiredSkill{ get{ return 20.0; } }
		public override int RequiredMana{ get{ return 13; } }

		public BloodOathSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( Mobile m )
		{
			if ( Caster == m || !(m is PlayerMobile || m is BaseCreature) ) // only PlayerMobile and BaseCreature implement blood oath checking
			{
				Caster.SendLocalizedMessage( 1060508 ); // You can't curse that.
			}
			else if ( m_OathTable.Contains( Caster ) )
			{
				Caster.SendLocalizedMessage( 1061607 ); // You are already bonded in a Blood Oath.
			}
			else if ( m_OathTable.Contains( m ) )
			{
				if ( m.Player )
					Caster.SendLocalizedMessage( 1061608 ); // That player is already bonded in a Blood Oath.
				else
					Caster.SendLocalizedMessage( 1061609 ); // That creature is already bonded in a Blood Oath.
			}
			else if ( CheckHSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );

				/* Temporarily creates a dark pact between the caster and the target.
				 * Any damage dealt by the target to the caster is increased, but the target receives the same amount of damage.
				 * The effect lasts for ((Spirit Speak skill level - target's Resist Magic skill level) / 80 ) + 8 seconds.
				 * 
				 * NOTE: The above algorithm must be fixed point, it should be:
				 * ((ss-rm)/8)+8
				 */

				ExpireTimer timer = (ExpireTimer)m_Table[m];
				if ( timer != null )
				timer.DoExpire();

				m_OathTable[Caster] = Caster;
				m_OathTable[m] = Caster;

				 if ( m.Spell != null )
					m.Spell.OnCasterHurt();
				
				Caster.PlaySound( 0x175 );

				Caster.FixedParticles( 0x375A, 1, 17, 9919, 33, 7, EffectLayer.Waist );
				Caster.FixedParticles( 0x3728, 1, 13, 9502, 33, 7, (EffectLayer)255 );

				m.FixedParticles( 0x375A, 1, 17, 9919, 33, 7, EffectLayer.Waist );
				m.FixedParticles( 0x3728, 1, 13, 9502, 33, 7, (EffectLayer)255 );

				TimeSpan duration = TimeSpan.FromSeconds( ((GetDamageSkill( Caster ) - GetResistSkill( m )) / 8) + 8 );
				m.CheckSkill( SkillName.MagicResist, 0.0, 120.0 );	//Skill check for gain

				timer = new ExpireTimer ( Caster, m, duration );
				timer.Start ();

				BuffInfo.AddBuff ( Caster, new BuffInfo ( BuffIcon.BloodOathCaster, 1075659, duration, Caster, m.Name.ToString () ) );
				BuffInfo.AddBuff ( m, new BuffInfo ( BuffIcon.BloodOathCurse, 1075661, duration, m, Caster.Name.ToString () ) );

				m_Table[m] = timer;
				HarmfulSpell( m );
			}

			FinishSequence();
		}

			public static bool RemoveCurse( Mobile m )
			{
			ExpireTimer t = (ExpireTimer)m_Table[m];

				if ( t == null )
					return false;

			t.DoExpire();
			return true;
		}

		private static Hashtable m_OathTable = new Hashtable();
		private static Hashtable m_Table = new Hashtable ();

		public static Mobile GetBloodOath( Mobile m )
		{
			if ( m == null )
				return null;

			Mobile oath = (Mobile)m_OathTable[m];

			if ( oath == m )
				oath = null;

			return oath;
		}

		private class ExpireTimer : Timer
		{
			private Mobile m_Caster;
			private Mobile m_Target;
			private DateTime m_End;

			public ExpireTimer( Mobile caster, Mobile target, TimeSpan delay ) : base( delay )
			{
				m_Caster = caster;
				m_Target = target;
				m_End = DateTime.UtcNow + delay;

				Priority = TimerPriority.TwoFiftyMS;
			}

			protected override void OnTick()
			{
				if ( m_Caster.Deleted || m_Target.Deleted || !m_Caster.Alive || !m_Target.Alive || DateTime.UtcNow >= m_End )
				{
					DoExpire ();
				}
			}
			public void DoExpire()
			{
				if( m_OathTable.Contains( m_Caster ) )
				{
					m_Caster.SendLocalizedMessage( 1061620 ); // Your Blood Oath has been broken.
					m_OathTable.Remove ( m_Caster );
				}

				if( m_OathTable.Contains( m_Target ) )
				{
					m_Target.SendLocalizedMessage( 1061620 ); // Your Blood Oath has been broken.
					m_OathTable.Remove ( m_Target );
				}

				BuffInfo.RemoveBuff ( m_Caster, BuffIcon.BloodOathCaster );
				BuffInfo.RemoveBuff ( m_Target, BuffIcon.BloodOathCurse );

				m_Table.Remove ( m_Caster );
			}
		}

		private class InternalTarget : Target
		{
			private BloodOathSpell m_Owner;

			public InternalTarget( BloodOathSpell owner ) : base( Core.ML ? 10 : 12, false, TargetFlags.Harmful )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
					m_Owner.Target( (Mobile) o );
				else
					from.SendLocalizedMessage( 1060508 ); // You can't curse that.
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}