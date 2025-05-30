using System;
using Server;
using Server.Targeting;
using Server.Engines.Craft;
using Server.Mobiles;
using Server.Regions;

namespace Server.Items
{
	public interface IRepairDeed
	{
        double SkillLevel { get; }
        bool Check(Mobile from);
		void Delete();
    }

    public class RepairDeed : Item, IRepairDeed
    {
		private class RepairSkillInfo
		{
			private CraftSystem m_System;
			private Type[] m_NearbyTypes;
			private TextDefinition m_NotNearbyMessage, m_Name;

			public TextDefinition NotNearbyMessage{	get { return m_NotNearbyMessage; } }
			public TextDefinition Name { get { return m_Name; } }


			public CraftSystem System { get { return m_System; } }
			public Type[] NearbyTypes { get { return m_NearbyTypes; } }

			public RepairSkillInfo( CraftSystem system, Type[] nearbyTypes, TextDefinition notNearbyMessage, TextDefinition name )
			{
				m_System = system;
				m_NearbyTypes = nearbyTypes;
				m_NotNearbyMessage = notNearbyMessage;
				m_Name = name;
			}

			public RepairSkillInfo( CraftSystem system, Type nearbyType, TextDefinition notNearbyMessage, TextDefinition name )
				: this( system, new Type[] { nearbyType }, notNearbyMessage, name )
			{
			}

			public static RepairSkillInfo[] Table { get { return m_Table; } }
			private static RepairSkillInfo[] m_Table = new RepairSkillInfo[]
				{
					new RepairSkillInfo( DefBlacksmithy.CraftSystem, typeof( Blacksmith ), 1047013, 1023015 ),
					new RepairSkillInfo( DefTailoring.CraftSystem, typeof( Tailor ), 1061132, 1022981 ),
					new RepairSkillInfo( DefTinkering.CraftSystem, typeof( Tinker ), 1061166, 1022983 ),
					new RepairSkillInfo( DefCarpentry.CraftSystem, typeof( Carpenter ), 1061135, 1060774 ),
					new RepairSkillInfo( DefBowFletching.CraftSystem, typeof( Bowyer ), 1061134, 1023005 )
				};

			public static RepairSkillInfo GetInfo( RepairSkillType type )
			{
				int v = (int)type;

				if( v < 0 || v >= m_Table.Length )
					v = 0;

				return m_Table[v];
			}
		}
		public enum RepairSkillType
		{
			Smithing,
			Tailoring,
			Tinkering,
			Carpentry,
			Fletching
		}

		public override bool DisplayLootType { get { return false; } }

		private RepairSkillType m_Skill; 
		private double m_SkillLevel;

		private Mobile m_Crafter;

		[CommandProperty( AccessLevel.GameMaster )]
		public RepairSkillType RepairSkill
		{
			get { return m_Skill; }
			set { m_Skill = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public double SkillLevel
		{
			get { return m_SkillLevel; }
			set { m_SkillLevel = Math.Max( Math.Min( value, 120.0 ), 0 ) ; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Crafter
		{
			get { return m_Crafter; }
			set { m_Crafter = value; InvalidateProperties(); }
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			list.Add( 1061133, String.Format( "{0}\t{1}", GetSkillTitle( m_SkillLevel ).ToString(), RepairSkillInfo.GetInfo( m_Skill ).Name ) ); // A repair service contract from ~1_SKILL_TITLE~ ~2_SKILL_NAME~.
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if( m_Crafter != null )
				list.Add( 1050043, m_Crafter.Name ); // crafted by ~1_NAME~

			//On OSI it says it's exceptional.  Intentional difference.
		}


		[Constructable]
		public RepairDeed() : this( RepairSkillType.Smithing, 100.0, null, true )
		{
		}

		[Constructable]
		public RepairDeed( RepairSkillType skill, double level ) : this( skill, level, null, true )
		{
		}

		[Constructable]
		public RepairDeed( RepairSkillType skill, double level, bool normalizeLevel ) : this( skill, level, null, normalizeLevel )
		{
		}

		public RepairDeed( RepairSkillType skill, double level, Mobile crafter ) : this( skill, level, crafter, true )
		{
		}

		public RepairDeed( RepairSkillType skill, double level, Mobile crafter, bool normalizeLevel ) : base( 0x14F0 )
		{
			if( normalizeLevel )
				SkillLevel = (int)(level/10)*10;
			else
				SkillLevel = level;

			m_Skill = skill;
			m_Crafter = crafter;
			Hue = 0x1BC;
			LootType = LootType.Blessed;
		}

		public RepairDeed( Serial serial ) : base( serial )
		{
		}

		private static TextDefinition GetSkillTitle( double skillLevel )
		{
			int skill = (int)(skillLevel/10);

			if( skill >= 11 )
				return (1062008 + skill-11);
			else if( skill >=5 )
				return (1061123 + skill-5);

			switch( skill )
			{
				case 4:
					return "a Novice";
				case 3:
					return "a Neophyte";
				default:
					return "a Newbie";		//On OSI, it shouldn't go below 50, but, this is for 'custom' support.
			}
		}

		public static RepairSkillType GetTypeFor( CraftSystem s )
		{
			for( int i = 0; i < RepairSkillInfo.Table.Length; i++ )
			{
				if( RepairSkillInfo.Table[i].System == s )
					return (RepairSkillType)i;
			}

			return RepairSkillType.Smithing;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if( Check( from ) )
				Repair.Do( from, RepairSkillInfo.GetInfo( m_Skill ).System, this );
		}

		public bool Check( Mobile from )
		{
			if( !IsChildOf( from.Backpack ) )
				from.SendLocalizedMessage( 1047012 ); // The contract must be in your backpack to use it.
			else if( !VerifyRegion( from ) )
				TextDefinition.SendMessageTo( from, RepairSkillInfo.GetInfo( m_Skill ).NotNearbyMessage );
			else
				return true;

			return false;
		}
		public bool VerifyRegion( Mobile m )
		{
			//TODO: When the entire region system data is in, convert to that instead of a proximity thing.

			if( !m.Region.IsPartOf( typeof( VillageRegion ) ) && !m.Region.IsPartOf( typeof( BardTownRegion ) ) )
				return false;

			return true;

		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)0 ); // version

			writer.Write( (int)m_Skill );
			writer.Write( m_SkillLevel );
			writer.Write( m_Crafter );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch( version )
			{
				case 0:
				{
					m_Skill = (RepairSkillType)reader.ReadInt();
					m_SkillLevel = reader.ReadDouble();
					m_Crafter = reader.ReadMobile();

					break;
				}
			}
		}
	}
}
