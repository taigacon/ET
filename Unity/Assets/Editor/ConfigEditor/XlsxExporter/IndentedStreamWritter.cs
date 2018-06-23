using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace BKEditor.Config.Export
{
	public class IndentedStreamWritter : StreamWriter
	{
		public int Indent { get; private set; }

		public IndentedStreamWritter([NotNull] Stream stream) : base(stream)
		{
		}

		public IndentedStreamWritter([NotNull] Stream stream, [NotNull] Encoding encoding) : base(stream, encoding)
		{
		}

		public IndentedStreamWritter([NotNull] Stream stream, [NotNull] Encoding encoding, int bufferSize) : base(stream, encoding, bufferSize)
		{
		}

		public IndentedStreamWritter([NotNull] Stream stream, [NotNull] Encoding encoding, int bufferSize, bool leaveOpen) : base(stream, encoding, bufferSize, leaveOpen)
		{
		}

		public IndentedStreamWritter([NotNull] string path) : base(path)
		{
		}

		public IndentedStreamWritter([NotNull] string path, bool append) : base(path, append)
		{
		}

		public IndentedStreamWritter([NotNull] string path, bool append, [NotNull] Encoding encoding) : base(path, append, encoding)
		{
		}

		public IndentedStreamWritter([NotNull] string path, bool append, [NotNull] Encoding encoding, int bufferSize) : base(path, append, encoding, bufferSize)
		{
		}

		public override void Write(string value)
		{
			var index = value.IndexOf("\r\n", StringComparison.Ordinal);
			if (index >= 0)
			{
				var start = 0;
				do
				{
					base.Write(value.Substring(start, index));
					base.WriteLine();
					start = index + 2;
					index = value.IndexOf("\r\n", start, StringComparison.Ordinal);
				}
				while (index >= 0);
				base.Write(value.Substring(start));
			}
			else
			{
				base.Write(value);
			}
		}

		public override void WriteLine()
		{
			base.WriteLine();
			if(Indent > 0)
                base.Write(new string('\t', Indent));
		}

		public void AddIndent(int value = 1)
		{
			Indent += value;
		}

		public void DecIndent(int value = 1)
		{
			Indent -= value;
		}
	}
}