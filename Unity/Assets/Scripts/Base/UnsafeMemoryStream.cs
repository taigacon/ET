
using System;
using System.IO;
using System.Text;

namespace BK.Base
{
	public class AlignedUnsafeMemoryStreamReader
	{
		private readonly byte[] bytes;
		private int offset = 0;

		private void CheckSize(int size)
		{
			if (offset + size > bytes.Length)
			{
				throw new InvalidDataException();
			}
		}

		public AlignedUnsafeMemoryStreamReader(byte[] bytes)
		{
			this.bytes = bytes;
		}

		public unsafe int ReadInt()
		{
			CheckSize(4);
			fixed (byte* ptr = &bytes[offset])
			{
				offset += 4;
				return *(int*) ptr;
			}
		}

		public unsafe uint ReadUInt()
		{
			CheckSize(4);
			fixed (byte* ptr = &bytes[offset])
			{
				offset += 4;
				return *(uint*)ptr;
			}
		}

		public unsafe float ReadFloat()
		{
			CheckSize(4);
			fixed (byte* ptr = &bytes[offset])
			{
				offset += 4;
				return *(float*)ptr;
			}
		}

		public unsafe bool ReadBool()
		{
			CheckSize(4);
			fixed (byte* ptr = &bytes[offset])
			{
				offset += 4;
				return *(int*)ptr != 0;
			}
		}

		public unsafe long ReadLong()
		{
			CheckSize(8);
			fixed (byte* ptr = &bytes[offset])
			{
				offset += 8;
				int low = *(int*) ptr;
				int high = *(int*)(ptr + 4);
				return (long)low | ((long)high << 32);
			}
		}

		public unsafe ulong ReadULong()
		{
			CheckSize(8);
			fixed (byte* ptr = &bytes[offset])
			{
				offset += 8;
				uint low = *(uint*)ptr;
				uint high = *(uint*)(ptr + 4);
				return (ulong)low | ((ulong)high << 32);
			}
		}

		public unsafe string ReadString()
		{
			CheckSize(2);
			fixed (byte* ptr = &bytes[offset])
			{
				offset += 2;
				ushort size = *(ushort*) ptr;
				ushort safesize = (ushort)((size + 2 + (4 - 1)) & -4 - 2);
				CheckSize(safesize);
				string s = Encoding.UTF8.GetString(bytes, offset, size);
				offset += safesize;
				return s;
			}
		}
	}
}