// 
using System.Numerics;

namespace DLS.Simulation
{
	// Helper class for dealing with pin state.
	// Pin state is stored as a BigInteger, with format:
	// Tristate flags (most significant 256 bits) | Bit states (least significant 256 bits)
	// BigIntegers can (practically) store as many bits as needed, so 256-bit values will take up 256-bits in memory and 512-bit values (like this one) will take up 512-bits in memory.
	public static class PinState
	{
		// Each bit has three possible states (tri-state logic):
		public const BigInteger LogicLow = 0;
		public const BigInteger LogicHigh = 1;
		public const BigInteger LogicDisconnected = 2;

		// Mask for single bit value (bit state, and tristate flag)
		public const BigInteger SingleBitMask = 1 | (1 << 256);
		
		public static BigInteger GetBitStates(BigInteger state) => (BigInteger)state; // Is this correct? Shouldn't it be (BigInteger)(state & BigInteger.Parse(0xffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff, NumberStyles.AllowHexSpecifier))?
		public static Biginteger GetTristateFlags(BigInteger state) => (BigInteger)(state >> 256);

		public static void Set(ref uint state, BigInteger bitStates, BigInteger tristateFlags)
		{
			state = (uint)(bitStates | (tristateFlags << 256));
		}

		public static void Set(ref uint state, uint other) => state = other;

		public static ushort GetBitTristatedValue(BigInteger state, int bitIndex)
		{
			ushort bitState = (ushort)((GetBitStates(state) >> bitIndex) & 1);
			ushort tri = (ushort)((GetTristateFlags(state) >> bitIndex) & 1);
			return (ushort)(bitState | (tri << 1)); // Combine to form tri-stated value: 0 = LOW, 1 = HIGH, 2 = DISCONNECTED
		}

		public static bool FirstBitHigh(uint state) => (state & 1) == LogicHigh;

		// ---- Set (smaller bit value) from (larger bit value) Source ----
		// Sets the values in a bit state from a larger one. All required versions can be generated using a series of these ones.
		
		public static void Set4BitFrom8BitSource(ref BigInteger state, BigInteger source8bit, bool firstNibble)
		{
			ushort sourceBitStates = (ushort)GetBitStates(source8bit);
			ushort sourceTristateFlags = (ushort)GetTristateFlags(source8bit);

			if (firstNibble)
			{
				const ushort mask = 0x0f;
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const ushort mask = 0xf0;
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 4), (BigInteger)((sourceTristateFlags & mask) >> 4));
			}
		}

		public static void Set8BitFrom16BitSource(ref BigInteger state, BigInteger source16bit, bool firstByte)
		{
			ushort sourceBitStates = (ushort)GetBitStates(source16bit);
			ushort sourceTristateFlags = (ushort)GetTristateFlags(source16bit);

			if (firstByte)
			{
				const ushort mask = 0xff;
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const ushort mask = 0xff00;
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 8), (BigInteger)((sourceTristateFlags & mask) >> 8));
			}
		}
		public void Set16BitFrom4BitSources(PinState a, PinState b, PinState c, PinState d)
		{
			bitStates = (a.bitStates << 4) | (b.bitStates) | (c.bitStates << 8) | (d.bitStates << 12);
			tristateFlags = (a.tristateFlags << 4) | (b.tristateFlags) | (c.tristateFlags << 8) | (d.tristateFlags << 12);
		}
		public void Set16BitFrom8BitSources(PinState a, PinState b)
		{
			bitStates = a.bitStates | (b.bitStates << 8);
			tristateFlags = a.tristateFlags | (b.tristateFlags << 8);
		}

		public void Set8BitFrom16BitSource(PinState source16bit, bool firstByte)
		{
			if (firstByte)
			{
				const uint mask = 0b111111111111;
				bitStates = source16bit.bitStates & mask;
				tristateFlags = source16bit.tristateFlags & mask;
			}
			else
			{
				const uint mask = 0b1111111100000000;
				bitStates = (source16bit.bitStates & mask) >> 8;
				tristateFlags = (source16bit.tristateFlags & mask) >> 8;
			}
		}
		public void Set4BitFrom16BitSource(PinState source16bit, byte whichNibble)
		{
			if (whichNibble == 3)
			{
				const uint mask = 0b1111;
				bitStates = source16bit.bitStates & mask;
				tristateFlags = source16bit.tristateFlags & mask;
			}
			else if (whichNibble == 2)
			{
				const uint mask = 0b11110000;
				bitStates = (source16bit.bitStates & mask) >> 4;
				tristateFlags = (source16bit.tristateFlags & mask) >> 4;
			}
			else if (whichNibble == 1)
			{
				const uint mask = 0b111100000000;
				bitStates = (source16bit.bitStates & mask) >> 8;
				tristateFlags = (source16bit.tristateFlags & mask) >> 8;
			}
			else if (whichNibble == 0)
			{
				const uint mask = 0b1111000000000000;
				bitStates = (source16bit.bitStates & mask) >> 12;
				tristateFlags = (source16bit.tristateFlags & mask) >> 12;
			}
			else
			{
				throw new System.ArgumentOutOfRangeException(nameof(whichNibble), "Nibble index must be between 0 and 3.");
			}
		}

		public void Set4BitFrom32BitSource(PinState source16bit, byte whichNibble)
		{
			if (whichNibble == 0)
			{
				const uint mask = 0b1111;
				bitStates = source16bit.bitStates & mask;
				tristateFlags = source16bit.tristateFlags & mask;
			}
			else if (whichNibble == 1)
			{
				const uint mask = 0b11110000;
				bitStates = (source16bit.bitStates & mask) >> 4;
				tristateFlags = (source16bit.tristateFlags & mask) >> 4;
			}
			else if (whichNibble == 2)
			{
				const uint mask = 0b111100000000;
				bitStates = (source16bit.bitStates & mask) >> 8;
				tristateFlags = (source16bit.tristateFlags & mask) >> 8;
			}
			else if (whichNibble == 3)
			{
				const uint mask = 0b1111000000000000;
				bitStates = (source16bit.bitStates & mask) >> 12;
				tristateFlags = (source16bit.tristateFlags & mask) >> 12;
			}
			else if (whichNibble == 4)
			{
				const uint mask = 0b11110000000000000000;
				bitStates = (source16bit.bitStates & mask) >> 16;
				tristateFlags = (source16bit.tristateFlags & mask) >> 16;
			}
			else if (whichNibble == 5)
			{
				const uint mask = 0b111100000000000000000000;
				bitStates = (source16bit.bitStates & mask) >> 20;
				tristateFlags = (source16bit.tristateFlags & mask) >> 20;
			}
			else if (whichNibble == 6)
			{
				const uint mask = 0b11110000000000000000000000;
				bitStates = (source16bit.bitStates & mask) >> 24;
				tristateFlags = (source16bit.tristateFlags & mask) >> 24;
			}
			else if (whichNibble == 7)
			{
				const uint mask = 0b1111000000000000000000000000;
				bitStates = (source16bit.bitStates & mask) >> 28;
				tristateFlags = (source16bit.tristateFlags & mask) >> 28;
			}
			else if (whichNibble == 8)
			{
				const uint mask = 0b11110000000000000000000000000000;
				bitStates = (source16bit.bitStates & mask) >> 32;
				tristateFlags = (source16bit.tristateFlags & mask) >> 32;
			}
			else
			{
				throw new System.ArgumentOutOfRangeException(nameof(whichNibble), "Nibble index must be between 0 and 3.");
			}
		}


		public void Set32BitFrom4BitSources(PinState a, PinState b, PinState c, PinState d, PinState e, PinState f, PinState g, PinState h)
		{
			bitStates = a.bitStates | (b.bitStates << 4) | (c.bitStates << 8) | (d.bitStates << 12) |
						(e.bitStates << 16) | (f.bitStates << 20) | (g.bitStates << 24) | (h.bitStates << 28);
			tristateFlags = a.tristateFlags | (b.tristateFlags << 4) | (c.tristateFlags << 8) | (d.tristateFlags << 12) |
							(e.tristateFlags << 16) | (f.tristateFlags << 20) | (g.tristateFlags << 24) | (h.tristateFlags << 28);
		}
		public void Set32BitFrom8BitSources(PinState a, PinState b, PinState c, PinState d){
			bitStates = a.bitStates | (b.bitStates << 8) | (c.bitStates << 16) | (d.bitStates << 24);
			tristateFlags = a.tristateFlags | (b.tristateFlags << 8) | (c.tristateFlags << 16) | (d.tristateFlags << 24);
		}
		public void Set32BitFrom16BitSources(PinState a, PinState b)
		{
			bitStates = a.bitStates | (b.bitStates << 16);
			tristateFlags = a.tristateFlags | (b.tristateFlags << 16);
		}


		public void Set8BitFrom32BitSource(PinState source16bit, byte whichByte)
		{
			if (whichByte == 0)
			{
				const uint mask = 0b11111111;
				bitStates = source16bit.bitStates & mask;
				tristateFlags = source16bit.tristateFlags & mask;
			}
			else if (whichByte == 1)
			{
				const uint mask = 0b1111111100000000;
				bitStates = (source16bit.bitStates & mask) >> 8;
				tristateFlags = (source16bit.tristateFlags & mask) >> 8;
			}
			else if (whichByte == 2)
			{
				const uint mask = 0b11111111000000000000;
				bitStates = (source16bit.bitStates & mask) >> 16;
				tristateFlags = (source16bit.tristateFlags & mask) >> 16;
			}
			else if (whichByte == 3)
			{
				const uint mask = 0b111111110000000000000000;
				bitStates = (source16bit.bitStates & mask) >> 24;
				tristateFlags = (source16bit.tristateFlags & mask) >> 24;
			}
			else
			{
				throw new System.ArgumentOutOfRangeException(nameof(whichByte), "Byte index must be between 0 and 3.");
			}
		}
		public void Set16BitFrom32BitSource(PinState source32bit, bool firstByte)
		{
			if (firstByte)
			{
				const uint mask = 0b1111111111111111;
				bitStates = source32bit.bitStates & mask;
				tristateFlags = source32bit.tristateFlags & mask;
			}
			else
			{
				const uint mask = 0b11111111111111110000000000000000;
				bitStates = (source32bit.bitStates & mask) >> 16;
				tristateFlags = (source32bit.tristateFlags & mask) >> 16;
			}
		}


		public static void Set16BitFrom32BitSource(ref BigInteger state, BigInteger source32bit, bool firstBytePair)
		{
			uint sourceBitStates = (uint)GetBitStates(source32bit);
			uint sourceTristateFlags = (uint)GetTristateFlags(source32bit);

			if (firstBytePair)
			{
				const ushort mask = 0xffff;
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const uint mask = 0xffff0000;
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 16), (BigInteger)((sourceTristateFlags & mask) >> 16);
			}
		}

		public static void Set32BitFrom64BitSource(ref BigInteger state, BigInteger source64bit, bool firstByteQuad)
		{
			ulong sourceBitStates = (ulong)GetBitStates(source64bit);
			ulong sourceTristateFlags = (ulong)GetTristateFlags(source64bit);

			if (firstByteQuad)
			{
				const uint mask = 0xffffffff;
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const ulong mask = 0xffffffff00000000;
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 32), (BigInteger)((sourceTristateFlags & mask) >> 32);
			}
		}

		public static void Set64BitFrom128BitSource(ref BigInteger state, BigInteger source128bit, bool firstByteOctuplet)
		{
			BigInteger sourceBitStates = (BigInteger)GetBitStates(source128bit);
			BigInteger sourceTristateFlags = (BigInteger)GetTristateFlags(source128bit);

			if (firstByteOctuplet)
			{
				const ulong mask = 0xffffffffffffffff;
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const BigInteger mask = BigInteger.Parse("0xffffffffffffffff0000000000000000", NumberStyles.AllowHexSpecifier);
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 64), (BigInteger)((sourceTristateFlags & mask) >> 64);
			}
		}

		public static void Set128BitFrom256BitSource(ref BigInteger state, BigInteger source256bit, bool firstByteQuadQuad)
		{
			BigInteger sourceBitStates = (BigInteger)GetBitStates(source256bit);
			BigInteger sourceTristateFlags = (BigInteger)GetTristateFlags(source256bit);

			if (firstByteQuadQuad)
			{
				const BigInteger mask = BigInteger.Parse("0xffffffffffffffffffffffffffffffff", NumberStyles.AllowHexSpecifier);
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const BigInteger mask = BigInteger.Parse("0xffffffffffffffffffffffffffffffff00000000000000000000000000000000", NumberStyles.AllowHexSpecifier);
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 128), (BigInteger)((sourceTristateFlags & mask) >> 128);
			}
		}
		
		// ---- Set (larger bit value) From (smaller bit value) Sources ----
		// Sets the values in a bit state from smaller ones. All required versions can be generated using a series of these ones.

		public static void Set8BitFrom4BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			ushort bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 4));
			ushort tristateFlags = (BigInteger)((GetTristateFlags(a) & 0b1111) | ((GetTristateFlags(b) & 0b1111) << 4));
			Set(ref state, (BigInteger)bitStates, (BigInteger)tristateFlags);
		}

		public static void Set16BitFrom8BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			ushort bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 8));
			ushort tristateFlags = (BigInteger)((GetTristateFlags(a) & 0xff) | ((GetTristateFlags(b) & 0xff) << 8));
			Set(ref state, (BigInteger)bitStates, (BigInteger)tristateFlags);
		}
			
		public static void Set32BitFrom16BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			uint bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 16));
			uint tristateFlags = (BigInteger)((GetTristateFlags(a) & 0xffff) | ((GetTristateFlags(b) & 0xffff) << 16));
			Set(ref state, (BigInteger)bitStates, (BigInteger)tristateFlags);
		}

		public static void Set64BitFrom32BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			ulong bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 32));
			ulong tristateFlags = (BigInteger)((GetTristateFlags(a) & 0xffffffff) | ((GetTristateFlags(b) & 0xffffffff) << 32));
			Set(ref state, (BigInteger)bitStates, (BigInteger)tristateFlags);
		}

		public static void Set128BitFrom64BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			BigInteger bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 64));
			BigInteger tristateFlags = (BigInteger)((GetTristateFlags(a) & 0xffffffffffffffff) | ((GetTristateFlags(b) & 0xffffffffffffffff) << 64));
			Set(ref state, bitStates, tristateFlags);
		}
		
		public static void Set256BitFrom128BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			BigInteger bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 128));
			BigInteger tristateFlags = (BigInteger)((GetTristateFlags(a) & BigInteger.Parse("0xffffffffffffffffffffffffffffffff", NumberStyles.AllowHexSpecifier)) | ((GetTristateFlags(b) & BigInteger.Parse("0xffffffffffffffffffffffffffffffff", NumberStyles.AllowHexSpecifier)) << 128));
			Set(ref state, bitStates, tristateFlags);
		}
		
		public static void Toggle(ref BigInteger state, int bitIndex)
		{
			BigInteger bitStates = GetBitStates(state);
			bitStates ^= (BigInteger)(1u << bitIndex);

			// Clear tristate flags (can't be disconnected if toggling as only input dev pins are allowed)
			Set(ref state, bitStates, (BigInteger)0);
		}

		public static void SetAllDisconnected(ref uint state) => Set(ref state, 0, ushort.MaxValue);
	}
}
