using System;
using System.IO;
using System.Collections;
using Server;

namespace Server.Engines.BulkOrders
{
	public class LargeBulkEntry
	{
		private LargeBOD m_Owner;
		private int m_Amount;
		private SmallBulkEntry m_Details;

		public LargeBOD Owner{ get{ return m_Owner; } set{ m_Owner = value; } }
		public int Amount{ get{ return m_Amount; } set{ m_Amount = value; if ( m_Owner != null ) m_Owner.InvalidateProperties(); } }
		public SmallBulkEntry Details{ get{ return m_Details; } }

		// Blacksmith
		public static SmallBulkEntry[] LargeRing { get { return GetEntries("Blacksmith", "largering"); } }
		public static SmallBulkEntry[] LargePlate { get { return GetEntries("Blacksmith", "largeplate"); } }
		public static SmallBulkEntry[] LargeChain { get { return GetEntries("Blacksmith", "largechain"); } }
		public static SmallBulkEntry[] LargeAxes { get { return GetEntries("Blacksmith", "largeaxes"); } }
		public static SmallBulkEntry[] LargeFencing { get { return GetEntries("Blacksmith", "largefencing"); } }
		public static SmallBulkEntry[] LargeMaces { get { return GetEntries("Blacksmith", "largemaces"); } }
		public static SmallBulkEntry[] LargePolearms { get { return GetEntries("Blacksmith", "largepolearms"); } }
		public static SmallBulkEntry[] LargeSwords { get { return GetEntries("Blacksmith", "largeswords"); } }

		// Tailor
		public static SmallBulkEntry[] BoneSet { get { return GetEntries("Tailoring", "boneset"); } }
		public static SmallBulkEntry[] Farmer { get { return GetEntries("Tailoring", "farmer"); } }
		public static SmallBulkEntry[] FemaleLeatherSet { get { return GetEntries("Tailoring", "femaleleatherset"); } }
		public static SmallBulkEntry[] FisherGirl { get { return GetEntries("Tailoring", "fishergirl"); } }
		public static SmallBulkEntry[] Gypsy { get { return GetEntries("Tailoring", "gypsy"); } }
		public static SmallBulkEntry[] HatSet { get { return GetEntries("Tailoring", "hatset"); } }
		public static SmallBulkEntry[] Jester { get { return GetEntries("Tailoring", "jester"); } }
		public static SmallBulkEntry[] Lady { get { return GetEntries("Tailoring", "lady"); } }
		public static SmallBulkEntry[] MaleLeatherSet { get { return GetEntries("Tailoring", "maleleatherset"); } }
		public static SmallBulkEntry[] Pirate { get { return GetEntries("Tailoring", "pirate"); } }
		public static SmallBulkEntry[] ShoeSet { get { return GetEntries("Tailoring", "shoeset"); } }
		public static SmallBulkEntry[] StuddedSet { get { return GetEntries("Tailoring", "studdedset"); } }
		public static SmallBulkEntry[] TownCrier { get { return GetEntries("Tailoring", "towncrier"); } }
        public static SmallBulkEntry[] Wizard { get { return GetEntries("Tailoring", "wizard"); } }
        
		// Carpenter
        public static SmallBulkEntry[] LargeArmor { get { return GetEntries("Carpenter", "largearmor"); } }
        public static SmallBulkEntry[] LargeInstrument { get { return GetEntries("Carpenter", "largeinstrument"); } }
        public static SmallBulkEntry[] LargePercussion { get { return GetEntries("Carpenter", "largepercussion"); } }
        public static SmallBulkEntry[] LargeStaff { get { return GetEntries("Carpenter", "largestaff"); } }
        public static SmallBulkEntry[] LargeString { get { return GetEntries("Carpenter", "largestring"); } }

		// Fletcher
        public static SmallBulkEntry[] LargeBow { get { return GetEntries("Fletcher", "largebow"); } }
        public static SmallBulkEntry[] LargeCrossBow { get { return GetEntries("Fletcher", "largecrossbow"); } }

		private static Hashtable m_Cache;

		public static SmallBulkEntry[] GetEntries( string type, string name )
		{
			if ( m_Cache == null )
				m_Cache = new Hashtable();

			Hashtable table = (Hashtable)m_Cache[type];

			if ( table == null )
				m_Cache[type] = table = new Hashtable();

			SmallBulkEntry[] entries = (SmallBulkEntry[])table[name];

			if ( entries == null )
				table[name] = entries = SmallBulkEntry.LoadEntries( type, name );

			return entries;
		}

		public static LargeBulkEntry[] ConvertEntries( LargeBOD owner, SmallBulkEntry[] small )
		{
			LargeBulkEntry[] large = new LargeBulkEntry[small.Length];

			for ( int i = 0; i < small.Length; ++i )
				large[i] = new LargeBulkEntry( owner, small[i] );

			return large;
		}

		public LargeBulkEntry( LargeBOD owner, SmallBulkEntry details )
		{
			m_Owner = owner;
			m_Details = details;
		}

		public LargeBulkEntry( LargeBOD owner, GenericReader reader )
		{
			m_Owner = owner;
			m_Amount = reader.ReadInt();

			Type realType = null;

			string type = reader.ReadString();

			if ( type != null )
				realType = ScriptCompiler.FindTypeByFullName( type );

			m_Details = new SmallBulkEntry( realType, reader.ReadInt(), reader.ReadInt() );
		}

		public void Serialize( GenericWriter writer )
		{
			writer.Write( m_Amount );
			writer.Write( m_Details.Type == null ? null : m_Details.Type.FullName );
			writer.Write( m_Details.Number );
			writer.Write( m_Details.Graphic );
		}
	}
}