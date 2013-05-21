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

	public static void SufficientByteArray2 ()
	{
		var u = Encoding.UTF8;
		Console.Write (String.Format ("SufficientByteArray2: #1-1 u.GetByteCount should be 3 <{0}>\n", u.GetByteCount ("\uFFFD")));
		Console.Write (String.Format ("SufficientByteArray2: #1-2 u.GetByteCount should be 3 <{0}>\n", u.GetByteCount ("\uD800")));
		Console.Write (String.Format ("SufficientByteArray2: #1-3 u.GetByteCount should be 3 <{0}>\n", u.GetByteCount ("\uDC00")));
		Console.Write (String.Format ("SufficientByteArray2: #1-4 u.GetByteCount should be 4 <{0}>\n", u.GetByteCount ("\uD800\uDC00")));
		byte [] bytes = new byte [10];
		Console.Write (String.Format ("SufficientByteArray2: #1-5 u.GetBytes should be 3 <{0}>\n", u.GetBytes ("\uDC00", 0, 1, bytes, 0))); // was bogus

		Console.Write (String.Format ("SufficientByteArray2: #2-1 u.GetBytes.Length should be 3 <{0}>\n", u.GetBytes ("\uFFFD").Length));
		Console.Write (String.Format ("SufficientByteArray2: #2-2 u.GetBytes.Length should be 3 <{0}>\n", u.GetBytes ("\uD800").Length));
		Console.Write (String.Format ("SufficientByteArray2: #2-3 u.GetBytes.Length should be 3 <{0}>\n", u.GetBytes ("\uDC00").Length));
		Console.Write (String.Format ("SufficientByteArray2: #2-4 u.GetBytes.Length should be 4 <{0}>\n", u.GetBytes ("\uD800\uDC00").Length));

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


	public static void Main ()
	{
		DecoderConvertLimitedDestination ();
		EncoderConvertLimitedDestination ();
		Bug10788 ();
		Bug10789 ();
		ConvertZeroCharacters ();
		SufficientByteArray2 ();
	}
}

