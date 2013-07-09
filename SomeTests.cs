// sometest from mono nunit tests

using NUnit.Framework;
using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace RogerWilco
{

[TestFixture]
public class SomeTests
{
	[Test]
	public void Decoder_ConvertLimitedDestination ()
	{
		char [] chars = new char [10000];
		byte [] bytes = new byte [10000];

		Decoder conv = Encoding.UTF8.GetDecoder ();
		int charsUsed, bytesUsed;
		bool done;

		conv.Convert (bytes, 0, 10000, chars, 0, 1000, true,
			      out charsUsed, out bytesUsed, out done);

		Assert.IsFalse (done, "#1");
		Assert.AreEqual (1000, charsUsed, "#2");
		Assert.AreEqual (1000, bytesUsed, "#3");
	}

	[Test]
	public void Encoder_ConvertLimitedDestination ()
	{
		byte [] bytes = new byte [10000];
		char [] chars = new char [10000];

		Encoder conv = Encoding.UTF8.GetEncoder ();
		int bytesUsed, charsUsed;
		bool done;

		conv.Convert (chars, 0, 10000, bytes, 0, 1000, true,
			      out bytesUsed, out charsUsed, out done);

		Assert.IsFalse (done, "#1");
		Assert.AreEqual (1000, bytesUsed, "#2");
		Assert.AreEqual (1000, charsUsed, "#3");
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Bug10788()
	{
		byte[] bytes = new byte[4096];
		char[] chars = new char[10];

		Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 4096, chars, 9, false);
	}

	[Test]
	public void Bug10789()
	{
		byte[] bytes = new byte[4096];
		char[] chars = new char[10];

		try {
			Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 1, chars, 10, false);
			Assert.Fail ("ArgumentException is expected #1");
		} catch (ArgumentException) {
		}

		try {
			Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 1, chars, 11, false);
			Assert.Fail ("ArgumentOutOfRangeException is expected #2");
		} catch (ArgumentOutOfRangeException) {
		}

		int charactersWritten = Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 0, chars, 10, false);
		Assert.AreEqual (0, charactersWritten, "#3");
	}

	[Test]
	public void ConvertZeroCharacters ()
	{
		int charsUsed, bytesUsed;
		bool completed;
		byte [] bytes = new byte [0];

		Encoding.UTF8.GetEncoder ().Convert (
			new char[0], 0, 0, bytes, 0, bytes.Length, true,
			out charsUsed, out bytesUsed, out completed);

		Assert.IsTrue (completed, "#1");
		Assert.AreEqual (0, charsUsed, "#2");
		Assert.AreEqual (0, bytesUsed, "#3");
	}

	[Test] // bug #565129
	public void SufficientByteArray2 ()
	{
		var u = Encoding.UTF8;
		Assert.AreEqual (3, u.GetByteCount ("\uFFFD"), "#1-1");
		Assert.AreEqual (3, u.GetByteCount ("\uD800"), "#1-2");
		Assert.AreEqual (3, u.GetByteCount ("\uDC00"), "#1-3");
		Assert.AreEqual (4, u.GetByteCount ("\uD800\uDC00"), "#1-4");
		byte [] bytes = new byte [10];
		Assert.AreEqual (3, u.GetBytes ("\uDC00", 0, 1, bytes, 0), "#1-5"); // was bogus

		Assert.AreEqual (3, u.GetBytes ("\uFFFD").Length, "#2-1");
		Assert.AreEqual (3, u.GetBytes ("\uD800").Length, "#2-2");
		Assert.AreEqual (3, u.GetBytes ("\uDC00").Length, "#2-3");
		Assert.AreEqual (4, u.GetBytes ("\uD800\uDC00").Length, "#2-4");

		for (char c = char.MinValue; c < char.MaxValue; c++) {
			byte [] bIn;
			bIn = u.GetBytes (c.ToString ());
		}

		try {
			new UTF8Encoding (false, true).GetBytes (new char [] {'\uDF45', '\uD808'}, 0, 2);
			Assert.Fail ("EncoderFallbackException is expected");
		} catch (EncoderFallbackException) {
		}
	}

	[Test]
	public void NoPreambleOnAppend ()
	{
		MemoryStream ms = new MemoryStream ();
		StreamWriter w = new StreamWriter (ms, Encoding.UTF8);
		w.Write ("a");
		w.Flush ();
		Assert.AreEqual (4, ms.Position, "#1");

		// Append 1 byte, should skip the preamble now.
		w.Write ("a");
		w.Flush ();
		w = new StreamWriter (ms, Encoding.UTF8);
		Assert.AreEqual (5, ms.Position, "#2");
	}

	[Test] // ctor (SByte*, Int32, Int32, Encoding)
	public unsafe void Constructor8_Value_Null ()
	{
		try {
			new String ((sbyte*) null, 0, 0, null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("value", ex.ParamName, "#A5");
		}

		try {
			new String ((sbyte*) null, 0, 1, null);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("value", ex.ParamName, "#B5");
		}

		try {
			new String ((sbyte*) null, 1, 0, null);
			Assert.Fail ("#C1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.AreEqual ("value", ex.ParamName, "#C5");
		}

		Assert.AreEqual (String.Empty, new String ((sbyte*) null, 0, 0, Encoding.Default), "#D");

		try {
			new String ((sbyte*) null, 0, 1, Encoding.Default);
			Assert.Fail ("#E1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#E2");
			Assert.IsNull (ex.InnerException, "#E3");
			Assert.IsNotNull (ex.Message, "#E4");
			//Assert.AreEqual ("value", ex.ParamName, "#E5");
		}

		Assert.AreEqual (String.Empty, new String ((sbyte*) null, 1, 0, Encoding.Default), "#F");
	}

	// DecoderFallbackExceptionTest
	//   This struct describes a DecoderFallbackExceptions test. It
	//   contains the expected indexes (eindex) and bad-bytes lengths
	//   (elen) delivered by the first and subsequent
	//   DecoderFallbackException throwed when the utf8 conversion routines
	//   are exposed by the array of bytes (bytes) contained in this test.
	//   It also has a nice description (description) for documentation and
	//   debugging.
	//
	//   The hardcoded 'eindex' and 'elen' info is the output that you will
	//   got if you run this strings on the MS.NET platform.
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

	// try to convert the all current test's bytes with Getchars()
	// in only one step
	private void DecoderFallbackExceptions_GetChars (
		char [] chars,
		int testno,
		Decoder dec,
		DecoderFallbackExceptionTest t)
	{
		try {
			dec.GetChars (t.bytes, 0, t.bytes.Length, chars, 0, true);
				Assert.IsTrue (
					t.eindex.Length == 0,
					String.Format (
						"test#{0}-1: UNEXPECTED SUCCESS",
						testno));
		} catch(DecoderFallbackException ex) {
			Assert.IsTrue (
				t.eindex.Length > 0,
				String.Format (
					"test#{0}-1: UNEXPECTED FAIL",
					testno));
			Assert.IsTrue (
				ex.Index == t.eindex[0],
				String.Format (
					"test#{0}-1: Expected exception at {1} not {2}.",
					testno,
					t.eindex[0],
					ex.Index));
			Assert.IsTrue (
				ex.BytesUnknown.Length == t.elen[0],
				String.Format (
					"test#{0}-1: Expected BytesUnknown.Length of {1} not {2}.",
					testno,
					t.elen[0],
					ex.BytesUnknown.Length));
			for (int i = 0; i < ex.BytesUnknown.Length; i++)
				Assert.IsTrue (
					ex.BytesUnknown[i] == t.bytes[ex.Index + i],
					String.Format (
						"test#{0}-1: expected byte {1:X} not {2:X} at {3}.",
						testno,
						t.bytes[ex.Index + i],
						ex.BytesUnknown[i],
						ex.Index + i));
			dec.Reset ();
		}
	}

	// convert bytes to string using a fixed blocksize.
	// If something bad happens, try to recover using the
	// DecoderFallbackException info.
	private void DecoderFallbackExceptions_Convert (
		char [] chars,
		int testno,
		Decoder dec,
		DecoderFallbackExceptionTest t,
		int block_size)
	{
		int charsUsed, bytesUsed;
		bool completed;

		int ce = 0; // current exception
		for (int c = 0; c < t.bytes.Length; ) {
			try {
				int bu = c + block_size > t.bytes.Length
						? t.bytes.Length - c
						: block_size;
				dec.Convert (
					t.bytes, c, bu,
					chars, 0, chars.Length,
					c + bu >= t.bytes.Length,
					out bytesUsed, out charsUsed,
					out completed);
				c += bytesUsed;
			} catch (DecoderFallbackException ex) {
				Assert.IsTrue (
					t.eindex.Length > ce,
					String.Format (
						"test#{0}-2-{1}#{2}: UNEXPECTED FAIL (c={3}, eIndex={4}, eBytesUnknwon={5})",
						testno, block_size, ce, c,
						ex.Index,
						ex.BytesUnknown.Length));
				Assert.IsTrue (
					ex.Index + c == t.eindex[ce],
					String.Format (
						"test#{0}-2-{1}#{2}: Expected at {3} not {4}.",
						testno, block_size, ce,
						t.eindex[ce],
						ex.Index + c));
				Assert.IsTrue (
					ex.BytesUnknown.Length == t.elen[ce],
					String.Format (
						"test#{0}-2-{1}#{2}: Expected BytesUnknown.Length of {3} not {4} @{5}.",
						testno, block_size, ce,
						t.elen[0], ex.BytesUnknown.Length, c));
				for (int i = 0; i < ex.BytesUnknown.Length; i++)
					Assert.IsTrue (
						ex.BytesUnknown[i] == t.bytes[ex.Index + i + c],
						String.Format (
							"test#{0}-2-{1}#{2}: Expected byte {3:X} not {4:X} at {5}.",
							testno, block_size, ce,
							t.bytes[ex.Index + i + c],
							ex.BytesUnknown[i],
							ex.Index + i));
				c += ex.BytesUnknown.Length + ex.Index;
				dec.Reset ();
				ce++;
			}
		}
		Assert.IsTrue (
			ce == t.eindex.Length,
			String.Format (
				"test#{0}-2-{1}: UNEXPECTED SUCCESS (expected {2} exceptions, but happened {3})",
				testno, block_size, t.eindex.Length, ce));
	}

	[Test]
	public void DecoderFallbackExceptions ()
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

		for(int t = 0; t < tests.Length; t++) {
			chars = new char [utf8.GetMaxCharCount (tests[t].bytes.Length)];

			// #1 complete conversion
			DecoderFallbackExceptions_GetChars (chars, t+1, dec, tests[t]);

			// #2 convert with several block_sizes
			for (int bs = 1; bs <= tests[t].bytes.Length; bs++)
				DecoderFallbackExceptions_Convert (chars, t+1, dec, tests[t], bs);
		}
	}

		// EncoderFallbackExceptionTest
		//   This struct describes a EncoderFallbackExceptions' test.
		//   It contains an array (index_fail) which is void if it is a
		//   valid UTF16 string.
		//   If it is an invalid string this array contains indexes
		//   (in 'index_fail') which point to the invalid chars in
		//   'str'.
		//   This array is hardcoded in each tests and it contains the
		//   absolute positions found in a sequence of
		//   EncoderFallbackException exceptions thrown if you convert
		//   this strings on a MS.NET platform.
		struct EncoderFallbackExceptionTest
		{
			public string str;
			public int [] eindex;
			public EncoderFallbackExceptionTest (
					string str,
					int [] eindex)
			{
				this.str = str;
				this.eindex = eindex;
			}
		}

		// try to encode some bytes at once with GetBytes
		private void EncoderFallbackExceptions_GetBytes (
			byte [] bytes,
			int testno,
			Encoder enc,
			EncoderFallbackExceptionTest t)
		{
			try {
				enc.GetBytes (
					t.str.ToCharArray (), 0, t.str.Length,
					bytes, 0, true);
				Assert.IsTrue (
					t.eindex.Length == 0,
					String.Format (
						"test#{0}-1: UNEXPECTED SUCCESS",
						testno));
			} catch(EncoderFallbackException ex) {
				Assert.IsTrue (
					t.eindex.Length > 0,
					String.Format (
						"test#{0}-1: UNEXPECTED FAIL",
						testno));
				Assert.IsTrue (
					ex.Index == t.eindex[0],
					String.Format (
						"test#{0}-1: Expected exception at {1} not {2}.",
						testno, t.eindex[0], ex.Index));
				Assert.IsTrue (
					!ex.IsUnknownSurrogate (),
					String.Format (
						"test#{0}-1: Expected false not {1} in IsUnknownSurrogate().",
						testno,
						ex.IsUnknownSurrogate ()));
				// NOTE: I know that in the previous check we
				// have asserted that ex.IsUnknownSurrogate()
				// is always false, but this does not mean that
				// we don't have to take in consideration its
				// real value for the next check.
				if (ex.IsUnknownSurrogate ())
					Assert.IsTrue (
						ex.CharUnknownHigh == t.str[ex.Index]
						&& ex.CharUnknownLow == t.str[ex.Index + 1],
						String.Format (
							"test#{0}-1: expected ({1:X}, {2:X}) not ({3:X}, {4:X}).",
							testno,
							t.str[ex.Index],
							t.str[ex.Index + 1],
							ex.CharUnknownHigh,
							ex.CharUnknownLow));
				else
					Assert.IsTrue (
						ex.CharUnknown == t.str[ex.Index],
						String.Format (
							"test#{0}-1: expected ({1:X}) not ({2:X}).",
							testno,
							t.str[ex.Index],
							ex.CharUnknown));
				enc.Reset ();
			}
		}

		private void EncoderFallbackExceptions_Convert (
			byte [] bytes,
			int testno,
			Encoder enc,
			EncoderFallbackExceptionTest t,
			int block_size)
		{
			int charsUsed, bytesUsed;
			bool completed;

			int ce = 0; // current exception

			for (int c = 0; c < t.str.Length; ) {
				//Console.WriteLine ("test#{0}-2-{1}: c={2}", testno, block_size, c);
				try {
					int bu = c + block_size > t.str.Length
							? t.str.Length - c
							: block_size;
					enc.Convert (
						t.str.ToCharArray (), c, bu,
						bytes, 0, bytes.Length,
						c + bu >= t.str.Length,
						out charsUsed, out bytesUsed,
						out completed);
					c += charsUsed;
				} catch (EncoderFallbackException ex) {
					//Console.WriteLine (
					//	"test#{0}-2-{1}#{2}: Exception (Index={3}, UnknownSurrogate={4})",
					//	testno, block_size, ce,
					//	ex.Index, ex.IsUnknownSurrogate ());
					Assert.IsTrue (
						ce < t.eindex.Length,
						String.Format (
							"test#{0}-2-{1}#{2}: UNEXPECTED EXCEPTION (Index={3}, UnknownSurrogate={4})",
							testno, block_size, ce,
							ex.Index,
							ex.IsUnknownSurrogate ()));
					Assert.IsTrue (
						ex.Index + c == t.eindex[ce],
						String.Format (
							"test#{0}-2-{1}#{2}: Expected exception at {3} not {4}.",
							testno, block_size, ce,
							t.eindex[ce],
							ex.Index + c));
					Assert.IsTrue (
						!ex.IsUnknownSurrogate (),
						String.Format (
							"test#{0}-2-{1}#{2}: Expected false not {3} in IsUnknownSurrogate().",
							testno, block_size, ce,
							ex.IsUnknownSurrogate ()));
					if (ex.IsUnknownSurrogate ()) {
						Assert.IsTrue (
							ex.CharUnknownHigh == t.str[ex.Index + c]
							&& ex.CharUnknownLow == t.str[ex.Index + c + 1],
							String.Format (
								"test#{0}-2-{1}#{2}: expected ({3:X}, {4:X}) not ({5:X}, {6:X}).",
								testno, block_size, ce,
								t.str[ex.Index + c], t.str[ex.Index + c + 1],
								ex.CharUnknownHigh, ex.CharUnknownLow));
						c += ex.Index + 2;
					} else {
						Assert.IsTrue (
							ex.CharUnknown == t.str[ex.Index + c],
							String.Format (
								"test#{0}-2-{1}#{2}: expected ({3:X}) not ({4:X}).",
								testno, block_size, ce,
								t.str[ex.Index + c],
								ex.CharUnknown));
						c += ex.Index + 1;
					}
					enc.Reset ();
					ce++;
				}
			}
			Assert.IsTrue (
				ce == t.eindex.Length,
				String.Format (
					"test#{0}-2-{1}: UNEXPECTED SUCCESS (expected {2} exceptions, but happened {3})",
					testno, block_size, t.eindex.Length, ce));
		}

		[Test]
		public void EncoderFallbackExceptions ()
		{

			EncoderFallbackExceptionTest [] tests = new EncoderFallbackExceptionTest []
			{
				/* #1  */ new EncoderFallbackExceptionTest ( "Zero \u0000.",                                   new int [] { }),
				/* #2  */ new EncoderFallbackExceptionTest ( "Last before leads \uD7FF.",                      new int [] { }),
				/* #3  */ new EncoderFallbackExceptionTest ( "Using lead \uD800 without a surrogate.",         new int [] { 11 }),
				/* #4  */ new EncoderFallbackExceptionTest ( "Using lead \uD877 without a surrogate.",         new int [] { 11 }),
				/* #5  */ new EncoderFallbackExceptionTest ( "Using lead \uDBFF without a surrogate.",         new int [] { 11 }),
				/* #6  */ new EncoderFallbackExceptionTest ( "Using trail \uDC00 without a lead.",             new int [] { 12 }),
				/* #7  */ new EncoderFallbackExceptionTest ( "Using trail \uDBFF without a lead.",             new int [] { 12 }),
				/* #8  */ new EncoderFallbackExceptionTest ( "First-plane 2nd block \uE000.",                  new int [] { }),
				/* #9  */ new EncoderFallbackExceptionTest ( "First-plane 2nd block \uFFFF.",                  new int [] { }),
				/* #10 */ new EncoderFallbackExceptionTest ( "Playing with first surrogate \uD800\uDC00.",     new int [] { }),
				/* #11 */ new EncoderFallbackExceptionTest ( "Playing before first surrogate \uD800\uDBFF.",   new int [] { 31, 32 }),
				/* #12 */ new EncoderFallbackExceptionTest ( "Playing with last of first plane \uD800\uDFFF.", new int [] { }),
				/* #13 */ new EncoderFallbackExceptionTest ( "Playing with first of last plane \uDBFF\uDC00.", new int [] { }),
				/* #14 */ new EncoderFallbackExceptionTest ( "Playing with last surrogate \uDBFF\uDFFF.",      new int [] { }),
				/* #15 */ new EncoderFallbackExceptionTest ( "Playing after last surrogate \uDBFF\uE000.",     new int [] { 29 }),
				/* #16 */ new EncoderFallbackExceptionTest ( "Incomplete string \uD800",                       new int [] { 18 }),
				/* #17 */ new EncoderFallbackExceptionTest ( "Horrible thing \uD800\uD800.",                   new int [] { 15, 16 }),
			};
			Encoding utf8 = Encoding.GetEncoding (
						"utf-8",
						new EncoderExceptionFallback(),
						new DecoderExceptionFallback());
			Encoder enc = utf8.GetEncoder ();
			byte [] bytes;

			for(int t = 0; t < tests.Length; t++) {
				bytes = new byte [utf8.GetMaxByteCount (tests[t].str.Length)];

				// #1 complete conversion
				EncoderFallbackExceptions_GetBytes (bytes, t+1, enc, tests[t]);

				// #2 convert in two rounds
				for (int bs = 1; bs <= tests[t].str.Length; bs++)
					EncoderFallbackExceptions_Convert (bytes, t+1, enc, tests[t], bs);
			}
		}
}

}
