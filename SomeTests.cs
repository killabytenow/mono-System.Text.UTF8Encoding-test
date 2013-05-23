// sometest from mono nunit tests

using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

public class SomeTests
{
	public static void DecoderConvertLimitedDestination ()
	{
		char [] chars = new char [10000];
		byte [] bytes = new byte [10000];

		Decoder conv = Encoding.UTF8.GetDecoder ();
		int charsUsed, bytesUsed;
		bool done;

		conv.Convert (bytes, 0, 10000, chars, 0, 1000, true,
			out charsUsed, out bytesUsed, out done);

		Console.Write (String.Format ("DecoderConvertLimitedDestination: #1 done should be false <{0}>\n", done));
		Console.Write (String.Format ("DecoderConvertLimitedDestination: #2 charsUsed should be 1000 <{0}>\n", charsUsed));
		Console.Write (String.Format ("DecoderConvertLimitedDestination: #3 bytesUsed should be 1000 <{0}>\n", bytesUsed));
	}

	public static void EncoderConvertLimitedDestination ()
	{
		byte [] bytes = new byte [10000];
		char [] chars = new char [10000];

		Encoder conv = Encoding.UTF8.GetEncoder ();
		int bytesUsed, charsUsed;
		bool done;

		conv.Convert (chars, 0, 10000, bytes, 0, 1000, true,
			out bytesUsed, out charsUsed, out done);

		Console.Write (String.Format ("EncoderConvertLimitedDestination: #1 done should be false <{0}>\n", done));
		Console.Write (String.Format ("EncoderConvertLimitedDestination: #2 bytesUsed should be 1000 <{0}>\n", bytesUsed));
		Console.Write (String.Format ("EncoderConvertLimitedDestination: #3 charsUsed should be 1000 <{0}>\n", charsUsed));
	}

	public static void Bug10788 ()
	{
		byte[] bytes = new byte[4096];
		char[] chars = new char[10];

		try {
			Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 4096, chars, 9, false);
			Console.Write ("Bug10788: FAIL: Expected ArgumentException.\n");
		} catch(ArgumentException) {
			Console.Write ("Bug10788: Received expected ArgumentException.\n");
		}
	}

	public static void Bug10789()
	{
		byte[] bytes = new byte[4096];
		char[] chars = new char[10];

		try {
			Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 1, chars, 10, false);
			Console.Write ("Bug10789: FAIL: Expected ArgumentException.\n");
		} catch (ArgumentException) {
			Console.Write ("Bug10789: Received expected ArgumentException.\n");
		}
		try {
			Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 1, chars, 11, false);
			Console.Write ("Bug10789: FAIL: Expected ArgumentOutOfRangeException.\n");
		} catch (ArgumentOutOfRangeException) {
			Console.Write ("Bug10789: Received expected ArgumentOutOfRangeException.\n");
		}

		int charactersWritten = Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 0, chars, 10, false);
		Console.Write (String.Format ("Bug10789: #3 charactersWritten should be 0 <{0}>\n", charactersWritten));
	}

	public static void ConvertZeroCharacters ()
	{
		int charsUsed, bytesUsed;
		bool completed;
		byte [] bytes = new byte [0];

		Encoding.UTF8.GetEncoder ().Convert (
			new char[0], 0, 0, bytes, 0, bytes.Length, true,
			out charsUsed, out bytesUsed, out completed);

		Console.Write (String.Format ("ConvertZeroCharacters: #1 completed should be TRUE <{0}>\n", completed));
		Console.Write (String.Format ("ConvertZeroCharacters: #2 charsUsed should be 0 <{0}>\n", charsUsed));
		Console.Write (String.Format ("ConvertZeroCharacters: #3 bytesUsed should be 0 <{0}>\n", bytesUsed));
	}

	public static void SufficientByteArray ()
	{
		Encoder e = Encoding.UTF8.GetEncoder ();
		byte [] bytes = new byte [0];

		char [] chars = new char [] {'\uD800'};
		e.GetBytes (chars, 0, 1, bytes, 0, false);
		try {
			int ret = e.GetBytes (chars, 1, 0, bytes, 0, true);
#if NET_2_0
			Console.Write (String.Format ("SufficientByteArray: drop insufficient char in 2.0: char[]; ret should be 0 <{0}>\n", ret));
#else
			Console.Write ("SufficientByteArray: FAIL: drop insufficient char in 1.0: char[]; Expected ArgumentException.\n");
#endif
		} catch (ArgumentException) {
#if !NET_2_0
			Console.Write ("SufficientByteArray: FAIL: drop insufficient char in 1.0: char[]; Unexpected ArgumentException.\n");
#endif
		}

		string s = "\uD800";
		try {
			int ret = Encoding.UTF8.GetBytes (s, 0, 1, bytes, 0);
#if NET_2_0
			Console.Write (String.Format ("SufficientByteArray: drop insufficient char in 2.0: string; ret should be 0 <{0}>\n", ret));
#else
			Console.Write ("SufficientByteArray: FAIL: drop insufficient char in 1.0: string; Expected ArgumentException.\n");
#endif
		} catch (ArgumentException) {
#if !NET_2_0
			Console.Write ("SufficientByteArray: FAIL: drop insufficient char in 1.0: string; Unexpected ArgumentException.\n");
#endif
		}
	}

	public static void SufficientByteArray2 ()
	{
		var u = Encoding.UTF8;
		Console.WriteLine ("SufficientByteArray2: #1-1 u.GetByteCount should be 3 <{0}>", u.GetByteCount ("\uFFFD"));
		Console.WriteLine ("SufficientByteArray2: #1-2 u.GetByteCount should be 3 <{0}>", u.GetByteCount ("\uD800"));
		Console.WriteLine ("SufficientByteArray2: #1-3 u.GetByteCount should be 3 <{0}>", u.GetByteCount ("\uDC00"));
		Console.WriteLine ("SufficientByteArray2: #1-4 u.GetByteCount should be 4 <{0}>", u.GetByteCount ("\uD800\uDC00"));
		byte [] bytes = new byte [10];
		Console.WriteLine ("SufficientByteArray2: #1-5 u.GetBytes should be 3 <{0}>", u.GetBytes ("\uDC00", 0, 1, bytes, 0)); // was bogus

		Console.WriteLine ("SufficientByteArray2: #2-1 u.GetBytes.Length should be 3 <{0}>", u.GetBytes ("\uFFFD").Length);
		Console.WriteLine ("SufficientByteArray2: #2-2 u.GetBytes.Length should be 3 <{0}>", u.GetBytes ("\uD800").Length);
		Console.WriteLine ("SufficientByteArray2: #2-3 u.GetBytes.Length should be 3 <{0}>", u.GetBytes ("\uDC00").Length);
		Console.WriteLine ("SufficientByteArray2: #2-4 u.GetBytes.Length should be 4 <{0}>", u.GetBytes ("\uD800\uDC00").Length);

		for (char c = char.MinValue; c < char.MaxValue; c++) {
			byte [] bIn;
			bIn = u.GetBytes (c.ToString ());
		}

		try {
			new UTF8Encoding (false, true).GetBytes (new char [] {'\uDF45', '\uD808'}, 0, 2);
			Console.Write ("SufficientByteArray2: FAIL: EncoderFallbackException is expected.\n");
		} catch (EncoderFallbackException) {
			Console.Write ("SufficientByteArray2: Received expected EncoderFallbackException.\n");
		}
	}

	public static void NoPreambleOnAppend ()
	{
		MemoryStream ms = new MemoryStream ();
		StreamWriter w = new StreamWriter (ms, Encoding.UTF8);
		w.Write ("a");
		w.Flush ();
		Console.WriteLine ("NoPreambleOnAppend: #1 ms.Position should be 4 <{0}>.", ms.Position);

		// Append 1 byte, should skip the preamble now.
		w.Write ("a");
		w.Flush ();
		w = new StreamWriter (ms, Encoding.UTF8);
		Console.WriteLine ("NoPreambleOnAppend: #2 ms.Position should be 5 <{0}>.", ms.Position);
	}

	public static unsafe void Constructor8_Value_Null ()
	{
		string s1, s2;
		
		Console.WriteLine ("Constructor8_Value_Null: #1");
		s1 = String.Empty;
		Console.WriteLine ("Constructor8_Value_Null: #2");
		s2 = new String ((sbyte*) null, 0, 0, Encoding.UTF8);
		Console.WriteLine ("Constructor8_Value_Null: ##");
	}

	struct EncoderFallbackExceptionTest
	{
		public bool valid;
		public string str;
		public int index, index_fail, length;
		public EncoderFallbackExceptionTest (
				bool valid,
				string str,
				int index,
				int index_fail, int length)
		{
			this.valid = valid;
			this.str = str;
			this.index = index;
			this.index_fail = index_fail;
			this.length = length;
		}
	}

	public static void FallbackExceptions ()
	{

		EncoderFallbackExceptionTest [] tests = new EncoderFallbackExceptionTest []
		{
			new EncoderFallbackExceptionTest ( false, "Using surrogate \uD800 without a surrogate.", 16, 16, 1),
			new EncoderFallbackExceptionTest ( false, "Using surrogate \uD877 without a surrogate.", 16, 16, 1),
			new EncoderFallbackExceptionTest ( false, "Using surrogate \uD8FF without a surrogate.", 16, 16, 1),
			new EncoderFallbackExceptionTest ( true,  "Playing with limit of \uDBFF\uDC00.",         22, 22, 1),
			new EncoderFallbackExceptionTest ( false, "Playing before limit of \uD800\uD7FF.",       24, 24, 1),
			new EncoderFallbackExceptionTest ( false, "Playing after limit of \uDFFF\uE000.",        23, 23, 1),
			new EncoderFallbackExceptionTest ( true,  "Playing with first surrogate \uD800\uDC00.",  29, 29, 1),
			new EncoderFallbackExceptionTest ( false, "Playing with last surrogate \uDBFF\uFFFF.",   28, 28, 1),

			new EncoderFallbackExceptionTest ( true,  "Last value \uFFFF.", 11, -1, 0),
			new EncoderFallbackExceptionTest ( true,  "Zero \u0000.", 5, -1, 0)
		};
		Encoding utf8 = Encoding.GetEncoding (
					"utf-8",
					new EncoderExceptionFallback(),
					new DecoderExceptionFallback());
		Encoder enc = utf8.GetEncoder ();
		byte [] bytes;
		int byteIndex, charsUsed, bytesUsed;
		bool completed;

		int testno = 1;
		int b, c;
		foreach (EncoderFallbackExceptionTest t in tests) {
			bytes = new byte [ utf8.GetMaxByteCount (t.str.Length) ];

			// #1 complete conversion
			try {
				enc.GetBytes (t.str.ToCharArray (), 0, t.str.Length, bytes, 0, true);
				if (!t.valid)
					Console.WriteLine ("FallbackExceptions: test#{0}-1: UNEXPECTED SUCCESS", testno);
			} catch(EncoderFallbackException ex) {
				if (!t.valid)
				{
					int expected_index_fail = t.index_fail;
					if (ex.Index != expected_index_fail)
						Console.WriteLine ("FallbackExceptions: test#{0}-1: Expected exception at {1} not {2}.", testno, expected_index_fail, ex.Index);
					c = ex.Index + 1;
				} else {
					Console.WriteLine ("FallbackExceptions: test#{0}-1: UNEXPECTED FAIL", testno);
				}
				enc.Reset ();
			}

			// #2 convert in two rounds
			for (b = 0; b < t.str.Length; b += 1)
			{
				try {
					enc.Convert (
						t.str.ToCharArray (), 0, b,
						bytes, 0, bytes.Length,
						false,
						out charsUsed, out bytesUsed,
						out completed);
					c = charsUsed;
				} catch(EncoderFallbackException ex) {
					if (!t.valid)
					{
						int expected_index_fail = t.index_fail;
						if (ex.Index != expected_index_fail)
							Console.WriteLine ("FallbackExceptions: test#{0}-2-{1}/try 1: Expected exception at {2} not {3}.", testno, b,expected_index_fail, ex.Index);
						c = ex.Index + 1;
					} else {
						Console.WriteLine ("FallbackExceptions: test#{0}-2-{1}/try 1:  UNEXPECTED EXCEPTION (Index={2})", testno, b, ex.Index);
					}
					enc.Reset ();
					continue;
				}
				try {
					enc.Convert (
						t.str.ToCharArray (), c, t.str.Length - c,
						bytes, 0, bytes.Length,
						true,
						out charsUsed, out byteIndex,
						out completed);
				} catch(EncoderFallbackException ex) {
					if (!t.valid)
					{
						int expected_index_fail = t.index_fail - c;
						if (ex.Index != expected_index_fail)
							Console.WriteLine ("FallbackExceptions: test#{0}-2-{1}/try 2: Expected exception at {2} not {3}.", testno, b, expected_index_fail, ex.Index);
						//Console.WriteLine (
						//	ex.IsUnknownSurrogate ()
						//		? String.Format ("  ++ Unable to encode pair {0:X} {1:X} at index {2}, ",
						//					(uint) ex.CharUnknownHigh,
						//					(uint) ex.CharUnknownLow,
						//					ex.Index)
						//		: String.Format ("  ++ Unable to encode char {0:X} at index {1}, ",
						//					(uint) ex.CharUnknown,
						//					ex.Index));
					} else {
						Console.WriteLine ("FallbackExceptions: test#{0}-2-{1}/try 2: UNEXPECTED FAIL (Index={2})", testno, b, ex.Index);
					}
					enc.Reset ();
					continue;
				}
				if (!t.valid)
					Console.WriteLine ("FallbackExceptions: test#{0}-2-{1}: UNEXPECTED SUCCESS", testno, b);
			}
			testno++;
		}
	}

	public static void Main ()
	{
#if NET_2_0
		Console.Write ("NET 2.0\n");
#else
		Console.Write ("NET 1.0\n");
#endif
		byte [] a = new byte[0];
		Console.WriteLine ("a is null? = {0}", a == null);
		if (a == null)
		DecoderConvertLimitedDestination ();
		EncoderConvertLimitedDestination ();
		Bug10788 ();
		Bug10789 ();
		ConvertZeroCharacters ();
		SufficientByteArray ();
		SufficientByteArray2 ();
		NoPreambleOnAppend ();
		Constructor8_Value_Null ();
		FallbackExceptions ();
	}
}

