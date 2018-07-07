
using System;
using System.IO;
using System.Text;

namespace BKEditor.Base
{
	public class AlignedUnsafeMemoryStreamWritter
	{
		private byte[] bytes = new byte[1024];
		private int offset = 0;

		private void CheckCapacity(int num)
		{
			if (offset + num > bytes.Length)
			{
				var newBytes = new byte[bytes.Length * 2];
				Array.Copy(bytes, newBytes, bytes.Length);
				bytes = newBytes;
			}
		}

		public unsafe void Write(int i)
		{
			CheckCapacity(4);
			fixed (byte* ptr = &bytes[offset])
			{
				*(int*) ptr = i;
				offset += 4;
			}
		}

		public unsafe void Write(uint i)
		{
			CheckCapacity(4);
			fixed (byte* ptr = &bytes[offset])
			{
				*(uint*)ptr = i;
				offset += 4;
			}
		}

		public unsafe void Write(long l)
		{
			CheckCapacity(8);
			fixed (byte* ptr = &bytes[offset])
			{
				*(int*)ptr = (int)(l & 0xffffffff);
				*(int*)(ptr + 4) = (int)((l >> 32) & 0xffffffff);
				offset += 8;
			}
		}

		public unsafe void Write(ulong l)
		{
			CheckCapacity(8);
			fixed (byte* ptr = &bytes[offset])
			{
				*(int*)ptr = (int)(l & 0xffffffff);
				*(int*)(ptr + 4) = (int)((l >> 32) & 0xffffffff);
				offset += 8;
			}
		}

		public unsafe void Write(bool b)
		{
			CheckCapacity(4);
			fixed (byte* ptr = &bytes[offset])
			{
				*(int*)ptr = b?1:0;
				offset += 4;
			}
		}

		public unsafe void Write(float f)
		{
			CheckCapacity(4);
			fixed (byte* ptr = &bytes[offset])
			{
				*(float*)ptr = f;
				offset += 4;
			}
		}

		public unsafe void Write(string s)
		{
			byte[] strBytes = Encoding.UTF8.GetBytes(s);
			ushort safesize = (ushort)((strBytes.Length + 2 + (4 - 1)) & -4);
			CheckCapacity(safesize);
			fixed (byte* ptr = &bytes[offset])
			{
				*(ushort*)ptr = (ushort)s.Length;
				fixed (byte* ptr2 = strBytes)
				{
					for (int i = 0; i < strBytes.Length; i++)
					{
						*(ptr + 2 + i) = ptr2[i];
					}
					// align
					for (int i = strBytes.Length; i < safesize - 2; i++)
					{
						*(ptr + 2 + i) = 0;
					}
				}
				offset += safesize;
			}
		}

		public byte[] ToArray()
		{
			byte[] data = new byte[offset];
			Array.Copy(bytes, data, offset);
			return data;
		}

		public void WriteToStream(Stream sw)
		{
			sw.Write(bytes, 0, offset);
		}

		public void Clear()
		{
			offset = 0;
		}
	}
}