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

	struct DecoderFallbackExceptionTest
	{
		public string description;
		public byte [] bytes;
		public int [] eindex;
		public int [] elen;
		public DecoderFallbackExceptionTest (
				string description,
				int [] eindex,
				int [] elen,
				byte [] bytes)
		{
			this.description = description;
			this.bytes = bytes;
			if (eindex.Length != elen.Length)
				throw new ApplicationException ("eindex.Length != elen.Length in test '" + description + "'");
			this.eindex = eindex;
			this.elen = elen;
		}
	}

	public static void DecoderFallbackExceptions ()
	{

		DecoderFallbackExceptionTest [] tests = new DecoderFallbackExceptionTest []
		{
			/* #1  */
			new DecoderFallbackExceptionTest (
				"Greek word 'kosme'",
				new int [] { },
				new int [] { },
				new byte [] {
					0xce, 0xba, 0xe1, 0xbd, 0xb9, 0xcf,
					0x83, 0xce, 0xbc, 0xce, 0xb5 }),
			/* #2  */
			new DecoderFallbackExceptionTest (
				"First possible sequence of 1 byte",
				new int [] { },
				new int [] { },
				new byte [] { 0x00 }),
			/* #3  */
			new DecoderFallbackExceptionTest (
				"First possible sequence of 2 bytes",
				new int [] { },
				new int [] { },
				new byte [] { 0xc2, 0x80 }),
			/* #4  */
			new DecoderFallbackExceptionTest (
				"First possible sequence of 3 bytes",
				new int [] { },
				new int [] { },
				new byte [] { 0xe0, 0xa0, 0x80 }),
			/* #5  */
			new DecoderFallbackExceptionTest (
				"First possible sequence of 4 bytes",
				new int [] { },
				new int [] { },
				new byte [] { 0xf0, 0x90, 0x80, 0x80 }),
			/* #6  */
			new DecoderFallbackExceptionTest (
				"First possible sequence of 5 bytes",
				new int [] { 0, 1, 2, 3, 4 },
				new int [] { 1, 1, 1, 1, 1 },
				new byte [] { 0xf8, 0x88, 0x80, 0x80, 0x80 }),
			/* #7  */
			new DecoderFallbackExceptionTest (
				"First possible sequence of 6 bytes",
				new int [] { 0, 1, 2, 3, 4, 5 },
				new int [] { 1, 1, 1, 1, 1, 1 },
				new byte [] {
					0xfc, 0x84, 0x80, 0x80, 0x80, 0x80 }),
			/* #8  */
			new DecoderFallbackExceptionTest (
				"Last possible sequence of 1 byte",
				new int [] { },
				new int [] { },
				new byte [] { 0x7f }),
			/* #9  */
			new DecoderFallbackExceptionTest (
				"Last possible sequence of 2 bytes",
				new int [] { },
				new int [] { },
				new byte [] { 0xdf, 0xbf }),
			/* #10 */
			new DecoderFallbackExceptionTest (
				"Last possible sequence of 3 bytes",
				new int [] { },
				new int [] { },
				new byte [] { 0xef, 0xbf, 0xbf }),
			/* #11 */
			new DecoderFallbackExceptionTest (
				"Last possible sequence of 4 bytes",
				new int [] { 0, 1, 2, 3 },
				new int [] { 1, 1, 1, 1 },
				new byte [] { 0xf7, 0xbf, 0xbf, 0xbf }),
			/* #12 */
			new DecoderFallbackExceptionTest (
				"Last possible sequence of 5 bytes",
				new int [] { 0, 1, 2, 3, 4 },
				new int [] { 1, 1, 1, 1, 1 },
				new byte [] { 0xfb, 0xbf, 0xbf, 0xbf, 0xbf }),
			/* #13 */
			new DecoderFallbackExceptionTest (
				"Last possible sequence of 6 bytes",
				new int [] { 0, 1, 2, 3, 4, 5 },
				new int [] { 1, 1, 1, 1, 1, 1 },
				new byte [] { 0xfd, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf }),
			/* #14 */
			new DecoderFallbackExceptionTest (
				"U-0000D7FF = ed 9f bf",
				new int [] { },
				new int [] { },
				new byte [] { 0xed, 0x9f, 0xbf }),
			/* #15 */
			new DecoderFallbackExceptionTest (
				"U-0000E000 = ee 80 80",
				new int [] { },
				new int [] { },
				new byte [] { 0xee, 0x80, 0x80 }),
			/* #16 */
			new DecoderFallbackExceptionTest (
				"U-0000FFFD = ef bf bd",
				new int [] { },
				new int [] { },
				new byte [] { 0xef, 0xbf, 0xbd }),
			/* #17 */
			new DecoderFallbackExceptionTest (
				"U-0010FFFF = f4 8f bf bf",
				new int [] { },
				new int [] { },
				new byte [] { 0xf4, 0x8f, 0xbf, 0xbf }),
			/* #18 */
			new DecoderFallbackExceptionTest (
				"U-00110000 = f4 90 80 80",
				new int [] { 0, 2, 3 },
				new int [] { 2, 1, 1 },
				new byte [] { 0xf4, 0x90, 0x80, 0x80 }),
			/* #19 */
			new DecoderFallbackExceptionTest (
				"First continuation byte 0x80",
				new int [] { 0 },
				new int [] { 1 },
				new byte [] { 0x80 }),
			/* #20 */
			new DecoderFallbackExceptionTest (
				"Last  continuation byte 0xbf",
				new int [] { 0 },
				new int [] { 1 },
				new byte [] { 0xbf }),
			/* #21 */
			new DecoderFallbackExceptionTest (
				"2 continuation bytes",
				new int [] { 0, 1 },
				new int [] { 1, 1 },
				new byte [] { 0x80, 0xbf }),
			/* #22 */
			new DecoderFallbackExceptionTest (
				"3 continuation bytes",
				new int [] { 0, 1, 2 },
				new int [] { 1, 1, 1 },
				new byte [] { 0x80, 0xbf, 0x80 }),
			/* #23 */
			new DecoderFallbackExceptionTest (
				"4 continuation bytes",
				new int [] { 0, 1, 2, 3 },
				new int [] { 1, 1, 1, 1 },
				new byte [] { 0x80, 0xbf, 0x80, 0xbf }),
			/* #24 */
			new DecoderFallbackExceptionTest (
				"5 continuation bytes",
				new int [] { 0, 1, 2, 3, 4 },
				new int [] { 1, 1, 1, 1, 1 },
				new byte [] { 0x80, 0xbf, 0x80, 0xbf, 0x80 }),
			/* #25 */
			new DecoderFallbackExceptionTest (
				"6 continuation bytes",
				new int [] { 0, 1, 2, 3, 4, 5 },
				new int [] { 1, 1, 1, 1, 1, 1 },
				new byte [] {
					0x80, 0xbf, 0x80, 0xbf, 0x80, 0xbf }),
			/* #26 */
			new DecoderFallbackExceptionTest (
				"7 continuation bytes",
				new int [] { 0, 1, 2, 3, 4, 5, 6 },
				new int [] { 1, 1, 1, 1, 1, 1, 1 },
				new byte [] {
					0x80, 0xbf, 0x80, 0xbf, 0x80, 0xbf,
					0x80 }),
			/* #27 */
			new DecoderFallbackExceptionTest (
				"Sequence of all 64 continuation bytes",
				new int [] {
					 0,  1,  2,  3,  4,  5,  6,  7,  8,  9,
					10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
					20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
					30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
					40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
					50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
					60, 61, 62, 63 },
				new int [] {
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1 },
				new byte [] {
					0x80, 0x81, 0x82, 0x83, 0x84, 0x85,
					0x86, 0x87, 0x88, 0x89, 0x8a, 0x8b,
					0x8c, 0x8d, 0x8e, 0x8f, 0x90, 0x91,
					0x92, 0x93, 0x94, 0x95, 0x96, 0x97,
					0x98, 0x99, 0x9a, 0x9b, 0x9c, 0x9d,
					0x9e, 0x9f, 0xa0, 0xa1, 0xa2, 0xa3,
					0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9,
					0xaa, 0xab, 0xac, 0xad, 0xae, 0xaf,
					0xb0, 0xb1, 0xb2, 0xb3, 0xb4, 0xb5,
					0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xbb,
					0xbc, 0xbd, 0xbe, 0xbf }),
			/* #28 */
			new DecoderFallbackExceptionTest (
				"All 32 first bytes of 2-byte sequences (0xc0-0xdf), each followed by a space character",
				new int [] {
					 0,  2,  4,  6,  8,
					10, 12, 14, 16, 18,
					20, 22, 24, 26, 28,
					30, 32, 34, 36, 38,
					40, 42, 44, 46, 48,
					50, 52, 54, 56, 58,
					60, 62 },
				new int [] {
					1, 1, 1, 1, 1,
					1, 1, 1, 1, 1,
					1, 1, 1, 1, 1,
					1, 1, 1, 1, 1,
					1, 1, 1, 1, 1,
					1, 1, 1, 1, 1,
					1, 1 },
				new byte [] {
					0xc0, 0x20, 0xc1, 0x20, 0xc2, 0x20,
					0xc3, 0x20, 0xc4, 0x20, 0xc5, 0x20,
					0xc6, 0x20, 0xc7, 0x20, 0xc8, 0x20,
					0xc9, 0x20, 0xca, 0x20, 0xcb, 0x20,
					0xcc, 0x20, 0xcd, 0x20, 0xce, 0x20,
					0xcf, 0x20, 0xd0, 0x20, 0xd1, 0x20,
					0xd2, 0x20, 0xd3, 0x20, 0xd4, 0x20,
					0xd5, 0x20, 0xd6, 0x20, 0xd7, 0x20,
					0xd8, 0x20, 0xd9, 0x20, 0xda, 0x20,
					0xdb, 0x20, 0xdc, 0x20, 0xdd, 0x20,
					0xde, 0x20, 0xdf, 0x20 }),
			/* #29 */
			new DecoderFallbackExceptionTest (
				"All 16 first bytes of 3-byte sequences (0xe0-0xef), each followed by a space character",
				new int [] {
					 0,  2,  4,  6,  8,
					10, 12, 14, 16, 18,
					20, 22, 24, 26, 28,
					30 },
				new int [] {
					1, 1, 1, 1, 1,
					1, 1, 1, 1, 1,
					1, 1, 1, 1, 1,
					1 },
				new byte [] {
					0xe0, 0x20, 0xe1, 0x20, 0xe2, 0x20,
					0xe3, 0x20, 0xe4, 0x20, 0xe5, 0x20,
					0xe6, 0x20, 0xe7, 0x20, 0xe8, 0x20,
					0xe9, 0x20, 0xea, 0x20, 0xeb, 0x20,
					0xec, 0x20, 0xed, 0x20, 0xee, 0x20,
					0xef, 0x20 }),
			/* #30 */
			new DecoderFallbackExceptionTest (
				"All 8 first bytes of 4-byte sequences (0xf0-0xf7), each followed by a space character",
				new int [] { 0,  2,  4,  6,  8, 10, 12, 14 },
				new int [] { 1, 1, 1, 1, 1, 1, 1, 1 },
				new byte [] {
					0xf0, 0x20, 0xf1, 0x20, 0xf2, 0x20,
					0xf3, 0x20, 0xf4, 0x20, 0xf5, 0x20,
					0xf6, 0x20, 0xf7, 0x20 }),
			/* #31 */
			new DecoderFallbackExceptionTest (
				"All 4 first bytes of 5-byte sequences (0xf8-0xfb), each followed by a space character",
				new int [] { 0, 2, 4, 6 },
				new int [] { 1, 1, 1, 1 },
				new byte [] {
					0xf8, 0x20, 0xf9, 0x20, 0xfa, 0x20,
					0xfb, 0x20 }),
			/* #32 */
			new DecoderFallbackExceptionTest (
				"All 2 first bytes of 6-byte sequences (0xfc-0xfd), each followed by a space character",
				new int [] { 0, 2 },
				new int [] { 1, 1 },
				new byte [] { 0xfc, 0x20, 0xfd, 0x20 }),
			/* #33 */
			new DecoderFallbackExceptionTest (
				"2-byte sequence with last byte missing",
				new int [] { 0 },
				new int [] { 1 },
				new byte [] { 0xc0 }),
			/* #34 */
			new DecoderFallbackExceptionTest (
				"3-byte sequence with last byte missing",
				new int [] { 0 },
				new int [] { 2 },
				new byte [] { 0xe0, 0x80 }),
			/* #35 */
			new DecoderFallbackExceptionTest (
				"4-byte sequence with last byte missing",
				new int [] { 0, 2 },
				new int [] { 2, 1 },
				new byte [] { 0xf0, 0x80, 0x80 }),
			/* #36 */
			new DecoderFallbackExceptionTest (
				"5-byte sequence with last byte missing",
				new int [] { 0, 1, 2, 3 },
				new int [] { 1, 1, 1, 1 },
				new byte [] { 0xf8, 0x80, 0x80, 0x80 }),
			/* #37 */
			new DecoderFallbackExceptionTest (
				"6-byte sequence with last byte missing",
				new int [] { 0, 1, 2, 3, 4 },
				new int [] { 1, 1, 1, 1, 1 },
				new byte [] { 0xfc, 0x80, 0x80, 0x80, 0x80 }),
			/* #38 */
			new DecoderFallbackExceptionTest (
				"2-byte sequence with last byte missing",
				new int [] { 0 },
				new int [] { 1 },
				new byte [] { 0xdf }),
			/* #39 */
			new DecoderFallbackExceptionTest (
				"3-byte sequence with last byte missing",
				new int [] { 0 },
				new int [] { 2 },
				new byte [] { 0xef, 0xbf }),
			/* #40 */
			new DecoderFallbackExceptionTest (
				"4-byte sequence with last byte missing",
				new int [] { 0, 1, 2 },
				new int [] { 1, 1, 1 },
				new byte [] { 0xf7, 0xbf, 0xbf }),
			/* #41 */
			new DecoderFallbackExceptionTest (
				"5-byte sequence with last byte missing",
				new int [] { 0, 1, 2, 3 },
				new int [] { 1, 1, 1, 1 },
				new byte [] { 0xfb, 0xbf, 0xbf, 0xbf }),
			/* #42 */
			new DecoderFallbackExceptionTest (
				"6-byte sequence with last byte missing",
				new int [] { 0, 1, 2, 3, 4 },
				new int [] { 1, 1, 1, 1, 1 },
				new byte [] { 0xfd, 0xbf, 0xbf, 0xbf, 0xbf }),
			/* #43 */
			new DecoderFallbackExceptionTest (
				"All the 10 sequences of 3.3 concatenated",
				new int [] {
					 0,  1,      3,
					 5,  6,  7,  8,  9,
					10, 11, 12, 13, 14,
					15, 16,     18, 19,
					20, 21, 22, 23, 24,
					25, 26, 27, 28, 29 },
				new int [] {
					 1,  2,      2,
					 1,  1,  1,  1,  1,
					 1,  1,  1,  1,  1,
					 1,  2,      1,  1,
					 1,  1,  1,  1,  1,
					 1,  1,  1,  1,  1 },
				new byte [] {
					0xc0, 0xe0, 0x80, 0xf0, 0x80, 0x80,
					0xf8, 0x80, 0x80, 0x80, 0xfc, 0x80,
					0x80, 0x80, 0x80, 0xdf, 0xef, 0xbf,
					0xf7, 0xbf, 0xbf, 0xfb, 0xbf, 0xbf,
					0xbf, 0xfd, 0xbf, 0xbf, 0xbf, 0xbf }),
			/* #44 */
			new DecoderFallbackExceptionTest (
				"Bad chars fe",
				new int [] { 0 },
				new int [] { 1 },
				new byte [] { 0xfe }),
			/* #45 */
			new DecoderFallbackExceptionTest (
				"Bad chars ff",
				new int [] { 0 },
				new int [] { 1 },
				new byte [] { 0xff }),
			/* #46 */
			new DecoderFallbackExceptionTest (
				"Bad chars fe fe ff ff",
				new int [] { 0, 1, 2, 3 },
				new int [] { 1, 1, 1, 1 },
				new byte [] { 0xfe, 0xfe, 0xff, 0xff }),
			/* #47 */
			new DecoderFallbackExceptionTest (
				"Overlong U+002F = c0 af",
				new int [] { 0, 1 },
				new int [] { 1, 1 },
				new byte [] { 0xc0, 0xaf }),
			/* #48 */
			new DecoderFallbackExceptionTest (
				"Overlong U+002F = e0 80 af",
				new int [] { 0, 2 },
				new int [] { 2, 1 },
				new byte [] { 0xe0, 0x80, 0xaf }),
			/* #49 */
			new DecoderFallbackExceptionTest (
				"Overlong U+002F = f0 80 80 af",
				new int [] { 0, 2, 3 },
				new int [] { 2, 1, 1 },
				new byte [] { 0xf0, 0x80, 0x80, 0xaf }),
			/* #50 */
			new DecoderFallbackExceptionTest (
				"Overlong U+002F = f8 80 80 80 af",
				new int [] { 0, 1, 2, 3, 4 },
				new int [] { 1, 1, 1, 1, 1 },
				new byte [] { 0xf8, 0x80, 0x80, 0x80, 0xaf }),
			/* #51 */
			new DecoderFallbackExceptionTest (
				"Overlong U+002F = fc 80 80 80 80 af",
				new int [] { 0, 1, 2, 3, 4, 5 },
				new int [] { 1, 1, 1, 1, 1, 1 },
				new byte [] {
					0xfc, 0x80, 0x80, 0x80, 0x80, 0xaf }),
			/* #52 */
			new DecoderFallbackExceptionTest (
				"Maximum overlong U-0000007F",
				new int [] { 0, 1 },
				new int [] { 1, 1 },
				new byte [] { 0xc1, 0xbf }),
			/* #53 */
			new DecoderFallbackExceptionTest (
				"Maximum overlong U-000007FF",
				new int [] { 0, 2 },
				new int [] { 2, 1, },
				new byte [] { 0xe0, 0x9f, 0xbf }),
			/* #54 */
			new DecoderFallbackExceptionTest (
				"Maximum overlong U-0000FFFF",
				new int [] { 0, 2, 3 },
				new int [] { 2, 1, 1 },
				new byte [] { 0xf0, 0x8f, 0xbf, 0xbf }),
			/* #55 */
			new DecoderFallbackExceptionTest (	
				"Maximum overlong U-001FFFFF",
				new int [] { 0, 1, 2, 3, 4 },
				new int [] { 1, 1, 1, 1, 1 },
				new byte [] { 0xf8, 0x87, 0xbf, 0xbf, 0xbf }),
			/* #56 */
			new DecoderFallbackExceptionTest (
				"Maximum overlong U-03FFFFFF",
				new int [] { 0, 1, 2, 3, 4, 5 },
				new int [] { 1, 1, 1, 1, 1, 1 },
				new byte [] {
					0xfc, 0x83, 0xbf, 0xbf, 0xbf, 0xbf }),
			/* #57 */
			new DecoderFallbackExceptionTest (
				"Null overlong c0 80",
				new int [] { 0, 1 },
				new int [] { 1, 1 },
				new byte [] { 0xc0, 0x80, 0x22 }),
			/* #58 */
			new DecoderFallbackExceptionTest (
				"Null overlong e0 80 80",
				new int [] { 0, 2 },
				new int [] { 2, 1 },
				new byte [] { 0xe0, 0x80, 0x80 }),
			/* #59 */
			new DecoderFallbackExceptionTest (
				"Null overlong f0 80 80 80",
				new int [] { 0, 2, 3 },
				new int [] { 2, 1, 1 },
				new byte [] { 0xf0, 0x80, 0x80, 0x80 }),
			/* #60 */
			new DecoderFallbackExceptionTest (
				"Null overlong f8 80 80 80 80",
				new int [] { 0, 1, 2, 3, 4 },
				new int [] { 1, 1, 1, 1, 1 },
				new byte [] { 0xf8, 0x80, 0x80, 0x80, 0x80 }),
			/* #61 */
			new DecoderFallbackExceptionTest (
				"Null overlong fc 80 80 80 80 80",
				new int [] { 0, 1, 2, 3, 4, 5 },
				new int [] { 1, 1, 1, 1, 1, 1 },
				new byte [] {
					0xfc, 0x80, 0x80, 0x80, 0x80, 0x80 }),
			/* #62 */
			new DecoderFallbackExceptionTest (
				"Single UTF-16 surrogate U+D800",
				new int [] { 0, 2 },
				new int [] { 2, 1 },
				new byte [] { 0xed, 0xa0, 0x80 }),
			/* #63 */
			new DecoderFallbackExceptionTest (
				"Single UTF-16 surrogate U+DB7F",
				new int [] { 0, 2 },
				new int [] { 2, 1 },
				new byte [] { 0xed, 0xad, 0xbf }),
			/* #64 */
			new DecoderFallbackExceptionTest (
				"Single UTF-16 surrogate U+DB80",
				new int [] { 0, 2 },
				new int [] { 2, 1 },
				new byte [] { 0xed, 0xae, 0x80 }),
			/* #65 */
			new DecoderFallbackExceptionTest (
				"Single UTF-16 surrogate U+DBFF",
				new int [] { 0, 2 },
				new int [] { 2, 1 },
				new byte [] { 0xed, 0xaf, 0xbf }),
			/* #66 */
			new DecoderFallbackExceptionTest (
				"Single UTF-16 surrogate U+DC00",
				new int [] { 0, 2 },
				new int [] { 2, 1 },
				new byte [] { 0xed, 0xb0, 0x80 }),
			/* #67 */
			new DecoderFallbackExceptionTest (
				"Single UTF-16 surrogate U+DF80",
				new int [] { 0, 2 },
				new int [] { 2, 1 },
				new byte [] { 0xed, 0xbe, 0x80 }),
			/* #68 */
			new DecoderFallbackExceptionTest (
				"Single UTF-16 surrogate U+DFFF",
				new int [] { 0, 2 },
				new int [] { 2, 1 },
				new byte [] { 0xed, 0xbf, 0xbf }),
			/* #69 */
			new DecoderFallbackExceptionTest (
				"Paired UTF-16 surrogate U+D800 U+DC00",
				new int [] { 0, 2, 3, 5 },
				new int [] { 2, 1, 2, 1 },
				new byte [] {
					0xed, 0xa0, 0x80, 0xed, 0xb0, 0x80 }),
			/* #70 */
			new DecoderFallbackExceptionTest (
				"Paired UTF-16 surrogate U+D800 U+DFFF",
				new int [] { 0, 2, 3, 5 },
				new int [] { 2, 1, 2, 1 },
				new byte [] {
					0xed, 0xa0, 0x80, 0xed, 0xbf, 0xbf }),
			/* #71 */
			new DecoderFallbackExceptionTest (
				"Paired UTF-16 surrogate U+DB7F U+DC00",
				new int [] { 0, 2, 3, 5 },
				new int [] { 2, 1, 2, 1 },
				new byte [] {
					0xed, 0xad, 0xbf, 0xed, 0xb0, 0x80 }),
			/* #72 */
			new DecoderFallbackExceptionTest (
				"Paired UTF-16 surrogate U+DB7F U+DFFF",
				new int [] { 0, 2, 3, 5 },
				new int [] { 2, 1, 2, 1 },
				new byte [] {
					0xed, 0xad, 0xbf, 0xed, 0xbf, 0xbf }),
			/* #73 */
			new DecoderFallbackExceptionTest (
				"Paired UTF-16 surrogate U+DB80 U+DC00",
				new int [] { 0, 2, 3, 5 },
				new int [] { 2, 1, 2, 1 },
				new byte [] {
					0xed, 0xae, 0x80, 0xed, 0xb0, 0x80 }),
			/* #74 */
			new DecoderFallbackExceptionTest (
				"Paired UTF-16 surrogate U+DB80 U+DFFF",
				new int [] { 0, 2, 3, 5 },
				new int [] { 2, 1, 2, 1 },
				new byte [] {
					0xed, 0xae, 0x80, 0xed, 0xbf, 0xbf }),
			/* #75 */
			new DecoderFallbackExceptionTest (
				"Paired UTF-16 surrogate U+DBFF U+DC00",
				new int [] { 0, 2, 3, 5 },
				new int [] { 2, 1, 2, 1 },
				new byte [] {
					0xed, 0xaf, 0xbf, 0xed, 0xb0, 0x80 }),
			/* #76 */
			new DecoderFallbackExceptionTest (
				"Paired UTF-16 surrogate U+DBFF U+DFFF",
				new int [] { 0, 2, 3, 5 },
				new int [] { 2, 1, 2, 1 },
				new byte [] {
					0xed, 0xaf, 0xbf, 0xed, 0xbf, 0xbf }),
			/* #77 */
			new DecoderFallbackExceptionTest (
				"Illegal code position U+FFFE",
				new int [] { },
				new int [] { },
				new byte [] { 0xef, 0xbf, 0xbe }),
			/* #78 */
			new DecoderFallbackExceptionTest (
				"Illegal code position U+FFFF",
				new int [] { },
				new int [] { },
				new byte [] { 0xef, 0xbf, 0xbf }),
		};
		Encoding utf8 = Encoding.GetEncoding (
					"utf-8",
					new EncoderExceptionFallback(),
					new DecoderExceptionFallback());
		Decoder dec = utf8.GetDecoder ();
		char [] chars;
		int charsUsed, bytesUsed;
		bool completed;

		int testno = 1;
		int b, c, ce, bu;
		foreach (DecoderFallbackExceptionTest t in tests) {
			chars = new char [ utf8.GetMaxCharCount (t.bytes.Length) ];

			// #1 complete conversion
			try {
				dec.GetChars (t.bytes, 0, t.bytes.Length, chars, 0, true);
				if (t.eindex.Length > 0)
					Console.WriteLine ("DecoderFallbackExceptions: test#{0}-1: UNEXPECTED SUCCESS", testno);
			} catch(DecoderFallbackException ex) {
				if (t.eindex.Length > 0)
				{
					if (ex.Index != t.eindex[0])
						Console.WriteLine (
							"DecoderFallbackExceptions: test#{0}-1: Expected exception at {1} not {2}.",
							testno, t.eindex[0], ex.Index);
					if (ex.BytesUnknown.Length != t.elen[0])
						Console.WriteLine (
							"DecoderFallbackExceptions: test#{0}-1: Expected BytesUnknown.Length of {1} not {2}.",
							testno, t.elen[0], ex.BytesUnknown.Length);
					for (int i = 0; i < ex.BytesUnknown.Length; i++)
						if (ex.BytesUnknown[i] != t.bytes[ex.Index + i])
							Console.WriteLine (
								"DecoderFallbackExceptions: test#{0}-1: expected byte {1:X} not {2:X} at {3}.",
								testno,
								t.bytes[ex.Index + i],
								ex.BytesUnknown[i],
								ex.Index + i);
					c = ex.Index + 1;
				} else {
					Console.WriteLine ("DecoderFallbackExceptions: test#{0}-1: UNEXPECTED FAIL", testno);
				}
				dec.Reset ();
			}

			// #2 convert in several rounds
			for (b = 1; b < t.bytes.Length; b += 1) {
				ce = 0; // current exception
				for (c = 0; c < t.bytes.Length; ) {
					try {
						bu = c + b > t.bytes.Length
							? t.bytes.Length - c
							: b;
						dec.Convert (
							t.bytes, c, bu,
							chars, 0, chars.Length,
							c + bu >= t.bytes.Length,
							out bytesUsed, out charsUsed,
							out completed);
						c += bytesUsed;
					} catch(DecoderFallbackException ex) {
						if (t.eindex.Length > ce)
						{
							if (ex.Index + c != t.eindex[ce])
								Console.WriteLine (
									"DecoderFallbackExceptions: test#{0}-2-{1}#{2}: Expected at {3} not {4}.",
									testno, b, ce,
									t.eindex[ce],
									ex.Index + c);
							if (ex.BytesUnknown.Length != t.elen[ce])
								Console.WriteLine (
									"DecoderFallbackExceptions: test#{0}-2-{1}#{2}: Expected BytesUnknown.Length of {3} not {4} @{5}.",
									testno, b, ce,
									t.elen[0], ex.BytesUnknown.Length, c);
							for (int i = 0; i < ex.BytesUnknown.Length; i++)
								if (ex.BytesUnknown[i] != t.bytes[ex.Index + i + c])
									Console.WriteLine (
										"DecoderFallbackExceptions: test#{0}-2-{1}#{2}: Expected byte {3:X} not {4:X} at {5}.",
										testno, b, ce,
										t.bytes[ex.Index + i + c],
										ex.BytesUnknown[i],
										ex.Index + i);
						} else {
							Console.WriteLine (
								"DecoderFallbackExceptions: test#{0}-2-{1}#{2}: UNEXPECTED FAIL (c={3}, eIndex={4}, eBytesUnknwon={5})",
								testno, b, ce, c,
								ex.Index,
								ex.BytesUnknown.Length);
						}
						c += ex.BytesUnknown.Length + ex.Index;
						ce++;
						dec.Reset ();
						continue;
					}
				}
				if (t.eindex.Length > ce)
					Console.WriteLine ("DecoderFallbackExceptions: test#{0}-2-{1}: UNEXPECTED SUCCESS", testno, b);
			}
			testno++;
		}
	}

	struct EncoderFallbackExceptionTest
	{
		public bool valid, unknown_surrogate;
		public string str;
		public int index_fail;
		public EncoderFallbackExceptionTest (
				bool valid,
				string str,
				int index_fail, bool unknown_surrogate)
		{
			this.valid = valid;
			this.str = str;
			this.index_fail = index_fail;
			this.unknown_surrogate = unknown_surrogate;
		}
	}

	public static void EncoderFallbackExceptions ()
	{

		EncoderFallbackExceptionTest [] tests = new EncoderFallbackExceptionTest []
		{
			/* #1  */ new EncoderFallbackExceptionTest ( true,  "Zero \u0000.",                                    5, false),
			/* #2  */ new EncoderFallbackExceptionTest ( true,  "Last before leads \uD7FF.",                      18, false),
			/* #3  */ new EncoderFallbackExceptionTest ( false, "Using lead \uD800 without a surrogate.",         11, false),
			/* #4  */ new EncoderFallbackExceptionTest ( false, "Using lead \uD877 without a surrogate.",         11, false),
			/* #5  */ new EncoderFallbackExceptionTest ( false, "Using lead \uDBFF without a surrogate.",         11, false),
			/* #6  */ new EncoderFallbackExceptionTest ( false, "Using trail \uDC00 without a lead.",             12, false),
			/* #7  */ new EncoderFallbackExceptionTest ( false, "Using trail \uDBFF without a lead.",             12, false),
			/* #8  */ new EncoderFallbackExceptionTest ( true,  "First-plane 2nd block \uE000.",                  22, false),
			/* #9  */ new EncoderFallbackExceptionTest ( true,  "First-plane 2nd block \uFFFF.",                  22, false),
			/* #10 */ new EncoderFallbackExceptionTest ( true,  "Playing with first surrogate \uD800\uDC00.",     29, false),
			/* #11 */ new EncoderFallbackExceptionTest ( false, "Playing before first surrogate \uD800\uDBFF.",   31, false),
			/* #12 */ new EncoderFallbackExceptionTest ( true,  "Playing with last of first plane \uD800\uDFFF.", 33, false),
			/* #13 */ new EncoderFallbackExceptionTest ( true,  "Playing with first of last plane \uDBFF\uDC00.", 33, false),
			/* #14 */ new EncoderFallbackExceptionTest ( true,  "Playing with last surrogate \uDBFF\uDFFF.",      28, false),
			/* #15 */ new EncoderFallbackExceptionTest ( false, "Playing after last surrogate \uDBFF\uE000.",     29, false),
			/* #16 */ new EncoderFallbackExceptionTest ( false, "Incomplete string \uD800",                       18, false),
			/* #17 */ new EncoderFallbackExceptionTest ( false, "Horrible thing \uD800\uD800.",                   15, false),
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
					Console.WriteLine ("EncoderFallbackExceptions: test#{0}-1: UNEXPECTED SUCCESS", testno);
			} catch(EncoderFallbackException ex) {
				if (!t.valid)
				{
					int expected_index_fail = t.index_fail;
					if (ex.Index != expected_index_fail)
						Console.WriteLine ("EncoderFallbackExceptions: test#{0}-1: Expected exception at {1} not {2}.", testno, expected_index_fail, ex.Index);
					if (ex.IsUnknownSurrogate () != t.unknown_surrogate)
						Console.WriteLine ("EncoderFallbackExceptions: test#{0}-1: Expected {1} not {2} in IsUnknownSurrogate().", testno, t.unknown_surrogate, ex.IsUnknownSurrogate ());
					else {
						if (ex.IsUnknownSurrogate ())
						{
							if (ex.CharUnknownHigh != t.str[ex.Index] || ex.CharUnknownLow  != t.str[ex.Index + 1])
								Console.WriteLine ("EncoderFallbackExceptions: test#{0}-1: expected ({1:X}, {2:X}) not ({3:X}, {4:X}).",
											testno,
											t.str[ex.Index], t.str[ex.Index + 1],
											ex.CharUnknownHigh, ex.CharUnknownLow);
						} else {
							if (ex.CharUnknown != t.str[ex.Index])
								Console.WriteLine ("EncoderFallbackExceptions: test#{0}-1: expected ({1:X}) not ({2:X}).",
											testno, t.str[ex.Index], ex.CharUnknown);
						}
					}
					c = ex.Index + 1;
				} else {
					Console.WriteLine ("EncoderFallbackExceptions: test#{0}-1: UNEXPECTED FAIL", testno);
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
							Console.WriteLine ("EncoderFallbackExceptions: test#{0}-2-{1}/try 1: Expected exception at {2} not {3}.", testno, b,expected_index_fail, ex.Index);
						if (ex.IsUnknownSurrogate () != t.unknown_surrogate)
							Console.WriteLine ("EncoderFallbackExceptions: test#{0}-2-{1}/try 1: Expected {2} not {3} in IsUnknownSurrogate().", testno, t.unknown_surrogate, ex.IsUnknownSurrogate ());
						else {
							if (ex.IsUnknownSurrogate ())
							{
								if (ex.CharUnknownHigh != t.str[ex.Index] || ex.CharUnknownLow  != t.str[ex.Index + 1])
									Console.WriteLine ("EncoderFallbackExceptions: test#{0}-2-{1}/try 2: expected ({2:X}, {3:X}) not ({4:X}, {5:X}).",
												testno, b,
												t.str[ex.Index], t.str[ex.Index + 1],
												ex.CharUnknownHigh, ex.CharUnknownLow);
							} else {
								if (ex.CharUnknown != t.str[ex.Index])
									Console.WriteLine ("EncoderFallbackExceptions: test#{0}-2-{1}/try 2: expected ({2:X}) not ({3:X}).",
												testno, b, t.str[ex.Index], ex.CharUnknown);
							}
						}
						c = ex.Index + 1;
					} else {
						Console.WriteLine ("EncoderFallbackExceptions: test#{0}-2-{1}/try 1:  UNEXPECTED EXCEPTION (Index={2})", testno, b, ex.Index);
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
							Console.WriteLine ("EncoderFallbackExceptions: test#{0}-2-{1}/try 2: Expected exception at {2} not {3}.", testno, b, expected_index_fail, ex.Index);
						if (ex.IsUnknownSurrogate () != t.unknown_surrogate)
							Console.WriteLine ("EncoderFallbackExceptions: test#{0}-2-{1}/try 2: Expected {2} not {3} in IsUnknownSurrogate().", testno, t.unknown_surrogate, ex.IsUnknownSurrogate ());
						else {
							if (ex.IsUnknownSurrogate ())
							{
								if (ex.CharUnknownHigh != t.str[c + ex.Index] || ex.CharUnknownLow  != t.str[c + ex.Index + 1])
									Console.WriteLine ("EncoderFallbackExceptions: test#{0}-2-{1}/try 2: expected ({2:X}, {3:X}) not ({4:X}, {5:X}).",
												testno, b,
												t.str[c + ex.Index], t.str[c + ex.Index + 1],
												ex.CharUnknownHigh, ex.CharUnknownLow);
							} else {
								if (ex.CharUnknown != t.str[c + ex.Index])
									Console.WriteLine ("EncoderFallbackExceptions: test#{0}-2-{1}/try 2: expected ({2:X}) not ({3:X}).",
												testno, b, t.str[c + ex.Index], ex.CharUnknown);
							}
						}
					} else {
						Console.WriteLine ("EncoderFallbackExceptions: test#{0}-2-{1}/try 2: UNEXPECTED FAIL (Index={2})", testno, b, ex.Index);
					}
					enc.Reset ();
					continue;
				}
				if (!t.valid)
					Console.WriteLine ("EncoderFallbackExceptions: test#{0}-2-{1}: UNEXPECTED SUCCESS", testno, b);
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
		EncoderFallbackExceptions ();
		DecoderFallbackExceptions ();
	}
}

