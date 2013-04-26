// Links:
//
//	- http://www.cl.cam.ac.uk/~mgk25/unicode.html
using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

public abstract class Tester
{
	string cnvout;

	static public bool console = false;
	static public bool console_utf8 = true;

	static public int min_block = 1;
	static public int inc_block = 13;
	static public int max_block = min_block + inc_block * 1;

	protected StreamWriter flog;
	protected BinaryWriter fout;
	protected BinaryWriter ferr;
	protected BinaryReader data;
	protected BinaryReader dataread;

	protected abstract void Test (int blocksize);

	private string TesterName { get { return this.GetType().Name; } }

	protected void msg (byte b)
	{
		if (b == 0x0a) {
			msg ((byte) 0x5c);
			msg ((byte) 0x6e);
		} else {
			flog.Write (b);
			if (console && console_utf8)
				ferr.Write (b);
		}
	}

	protected void msg (string s)
	{
		flog.Write (s);
		if (console)
			Console.Write (s);
	}

	private string getoutfile (string testfile, int blocksize, string type)
	{
		return String.Format(
			cnvout + "/{0}-{1}-{2}{3}.txt",
			this.TesterName,
			Regex.Replace (
				Regex.Replace (testfile, "^.*/", ""),
				"[^a-zA-Z0-9]", "_"),
			blocksize,
			type != null ? "." + type : "");
	}

	public byte [] ReadBlock (int blocksize)
	{
		byte [] r = data.ReadBytes (blocksize);

		return r.Length > 0 ? r : null;
	}

	public void RunTest (string testfile, int blocksize)
	{
		// open output and input files
		using (data = new BinaryReader (File.Open (testfile, FileMode.Open))) {
			using (flog = new StreamWriter (getoutfile (testfile, blocksize, null))) {
				using (fout = new BinaryWriter (File.Open (getoutfile (testfile, blocksize, "utf8"), FileMode.Create))) {
					// run test
					Test (blocksize);
				}
			}
		}
	}

	public void RunAll (string [] testfiles)
	{
		Console.WriteLine ("{0}:", this.TesterName);
		foreach (string testfile in testfiles) {
			Console.WriteLine ("  - Processing file '{0}'...", testfile);
			for (int blocksize = min_block; blocksize <= max_block; blocksize += inc_block) {
				Console.WriteLine ("    - block size = {0}", blocksize);
				RunTest (testfile, blocksize);
			}
		}
	}

	public Tester (string cnvout)
	{
		// get a handler for binary writing to console
		ferr = new BinaryWriter (Console.OpenStandardOutput ());
		this.cnvout = cnvout;
	}

}

public class DecoderTest : Tester
{
	protected Decoder dec;

	public DecoderTest (string cnvout, Encoding encoding) : base (cnvout)
	{
		dec = encoding.GetDecoder ();
	}

	protected override void Test (int blocksize)
	{
		int  bytesUsed, charsUsed,
			offset = 0, line = 1, lchar = 0;
		bool completed;
		char [] buffer = new char [blocksize > 2 ? blocksize : 2];
		byte [] bytes;

		while ((bytes = ReadBlock (blocksize)) != null) {
			for (int i = 0; i < bytes.Length; ) {
				try {
					dec.Convert (
						bytes, i, blocksize + i < bytes.Length ? blocksize : bytes.Length - i,
						buffer, 0, buffer.Length,
						false,
						out bytesUsed, out charsUsed,
						out completed);
					msg (String.Format ("byte {0}: bytesUsed={1} charsUsed={2} completed={3} line={4}:{5} <",
								offset, bytesUsed, charsUsed, completed, line, lchar));
					for (int j = 0; j < charsUsed; j++) {
						msg (String.Format ("({0:X})", (uint) buffer [j]));
						if (((uint) buffer [j]) > (uint) 0xffff) {
							msg (String.Format ("byte[{0}]: bad char={1:X} line={2}:{3} <",
										offset, (uint) buffer [j], line, lchar));
						} else {
							if (((uint) buffer [j]) > (uint) 0xff) {
								fout.Write ((((byte) buffer [j]) >> 8) & ((uint) 0xff));
								fout.Write ((((byte) buffer [j])     ) & ((uint) 0xff));
							} else {
								fout.Write ((byte) buffer [j]);
							}
						}
					}
					msg (">\n");
					if (bytesUsed == 0 && charsUsed == 0) {
						msg ("Oh no puta mierda!");
						return;
					}
				} catch (DecoderFallbackException e) {
					msg (String.Format ("byte {0}:ERROR: line={1}:{2} ", offset, line, lchar));
					msg (String.Format ("Unable to decode {0} byte{1} ",
								e.BytesUnknown.Length,
								e.BytesUnknown.Length > 0 ? "s" : ""));
					foreach (byte unknown in e.BytesUnknown)
						msg (String.Format ("0x{0:X2} ", (uint) unknown));
					msg (String.Format ("at index {0}.\n", e.Index));
					msg (String.Format ("byte {0}:ERROR: line={1}:{2} caca=", offset, line, lchar));
					for (int j = e.Index; j < (e.Index + 6 < bytes.Length ? e.Index + 6 : bytes.Length); j++) {
						if (j < 0)
							msg (String.Format (" ({0})", j));
						else
							msg (String.Format (" {0:X}", (uint) bytes [j]));
					}
					msg ("\n");
					bytesUsed = e.BytesUnknown.Length;
					dec.Reset ();
				}
				for (int j = 0; j < bytesUsed && i + j < bytes.Length; j++)
					if(bytes [i + j] == 0x0a) {
						line++;
						lchar=0;
					} else
						lchar++;
				i += bytesUsed;
				offset += bytesUsed;
			}
		}
		dec.Reset ();
	}
}

public abstract class EncoderTest : Tester
{
	protected Encoder enc;

	public EncoderTest (string cnvout, Encoding encoding) : base (cnvout)
	{
		enc = encoding.GetEncoder ();
	}

	protected override void Test (int blocksize)
	{
		int  bytesUsed, charsUsed, again,
			line = 1, lchar = 0, offset = 0;
		bool completed;
		byte [] bytes;
		char [] chars;
		byte [] buffer = new byte [blocksize * 4];

		while ((bytes = ReadBlock (blocksize << 1)) != null) {
			// if odd elements, fix array
			if ((bytes.Length & 0x01) != 0) {
				Console.WriteLine ("1 dangling byte!!!!\n  offset: {0}", offset << 1);
				for (int i = 0; i < bytes.Length; i++)
					Console.Write ("{0} ", bytes [i]);
				Console.WriteLine ();
				Array.Resize (ref bytes, bytes.Length - 1);
			}

			// decode content directly as chars
			chars = new char [bytes.Length >> 1];
			for (int j = 0; j < chars.Length; j++)
			  chars [j] = (char) ((((uint) bytes [(j << 1) + 0]) << 8)
			  		    | (((uint) bytes [(j << 1) + 1])));

			// encode things into UTF8
			again = 0;
			for (int i = 0; i < chars.Length; ) {
				try {
					enc.Convert (
						chars,
						i,
						again == 0
							? (blocksize + i < chars.Length
								? blocksize
								: chars.Length - i)
							: again,
						buffer, 0, buffer.Length,
						false,
						out charsUsed, out bytesUsed,
						out completed);
					again = 0;
					msg (String.Format ("char[{0}]: charsUsed={1} bytesUsed={2} completed={3} line={4}:{5} <",
								offset, charsUsed, bytesUsed, completed, line, lchar));
					for (int j = 0; j < bytesUsed; j++)
					{
						msg (buffer [j]);
						fout.Write ((byte) buffer [j]);
					}
					msg ("> <");
					for (int j = 0; j < bytesUsed; j++)
						msg (String.Format ("({0:X})", (uint) buffer [j]));
					msg (">\n");
					if (bytesUsed == 0 && charsUsed == 0) {
						msg ("Oh no puta mierda!");
						break;
					}
				} catch (EncoderFallbackException e) {
					msg (String.Format ("char[{0}]:ERROR: line={1}:{2} ",
								offset, line, lchar));
					msg (e.IsUnknownSurrogate ()
						? String.Format ("Unable to encode pair {0:X} {1:X} at index {2}, ",
									(uint) e.CharUnknownHigh,
									(uint) e.CharUnknownLow,
									e.Index)
						: String.Format ("Unable to encode char {0:X} at index {1}, ",
									(uint) e.CharUnknown,
									e.Index));
					if (e.Index > 0)
					{
						again = e.Index;
						charsUsed = 0;
						msg (String.Format (" repeat first {0} chars.\n", again));
					} else {
						charsUsed = e.IsUnknownSurrogate () ? 2 : 1;
						msg (String.Format (" skipping {0} chars.\n", charsUsed));
					}
					enc.Reset ();
				}
				for (int j = 0; j < charsUsed; j++)
					if (chars [i + j] == '\n') {
						line++;
						lchar=0;
					} else
						lchar++;
				i += charsUsed;
				offset += charsUsed;
			}
		}
		flog.Close ();
		fout.Close ();
	}
}

public class DecoderTestDefaultFallback : DecoderTest
{
	public DecoderTestDefaultFallback (string cnvout)
		: base (cnvout, Encoding.GetEncoding ("utf-8"))
	{
	}
}

public class DecoderTestExceptionFallback : DecoderTest
{
	public DecoderTestExceptionFallback (string cnvout)
		: base (cnvout, Encoding.GetEncoding ("utf-8",
					new EncoderExceptionFallback(),
					new DecoderExceptionFallback()))
	{
	}
}

public class EncoderTestDefaultFallback : EncoderTest
{
	public EncoderTestDefaultFallback (string cnvout)
		: base (cnvout, Encoding.GetEncoding ("utf-8"))
	{
	}
}

public class EncoderTestExceptionFallback : EncoderTest
{
	public EncoderTestExceptionFallback (string cnvout)
		: base (cnvout, Encoding.GetEncoding ("utf-8",
					new EncoderExceptionFallback(),
					new DecoderExceptionFallback()))
	{
	}
}

public class Example
{
	static string cnvout;

	public static bool IsRunningOnMono ()
	{
		return Type.GetType ("Mono.Runtime") != null;
	}

	public static void TestEncoding ()
	{
		string [] testfiles = {
			"samples/utf16be.txt",
			"samples/utf16-all-bad-dc00-dfff.txt",
			"samples/utf16-all-good-after-dfff.txt",
			"samples/utf16-all-good-before-dc00.txt",
			"samples/utf16-all-surrogates.txt",
		};
		EncoderTestDefaultFallback etdf = new EncoderTestDefaultFallback (cnvout);
		EncoderTestExceptionFallback etef = new EncoderTestExceptionFallback (cnvout);

		etdf.RunAll (testfiles);
		etef.RunAll (testfiles);
	}

	public static void TestDecoding ()
	{
		string [] testfiles = {
			"samples/euro.txt-u",
			"samples/lyrics-ipa.txt",
			"samples/TeX.txt",
			"samples/UTF-8-demo.txt",
			"samples/UTF-8-KERMIT-SAMPLER.html",
			"samples/UTF-8-test.txt",
			"samples/wgl4.txt",
		};
		DecoderTestDefaultFallback dtdf = new DecoderTestDefaultFallback (cnvout);
		DecoderTestExceptionFallback dtef = new DecoderTestExceptionFallback (cnvout);

		dtdf.RunAll (testfiles);
		dtef.RunAll (testfiles);
	}

	public static void Main ()
	{
		// decide output directory
		cnvout = "cnvout-"
		       + (IsRunningOnMono () ? "mono" : "other");

		// create output dir
		if (!Directory.Exists (cnvout))
			Directory.CreateDirectory (cnvout);

		TestEncoding ();
		TestDecoding ();
	}
}

