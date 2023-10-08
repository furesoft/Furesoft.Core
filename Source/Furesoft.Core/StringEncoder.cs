using System.Text;

namespace Furesoft.Core;

/// <summary>
///     A Class To Encode Strings in a SerialKey Format
/// </summary>
public static class StringEncoder
{
    private const string Alphabet = "ACBZXJHFRTKNLMUV";

    /// <summary>
    ///     Encode Integer Values To String
    /// </summary>
    /// <param name="values">The Values To Encode</param>
    /// <returns></returns>
    public static string Encode(params int[] values)
    {
        var sb = new StringBuilder();

        //convert values to string
        foreach (var v in values) sb.Append(GetLetter(v)).Append("-");

        var result = sb.ToString();

        //build checksum and append to stringbuilder
        var checksum = (result.Length - values.Length) ^ 3;
        var checksumConverted = GetLetter(checksum);
        sb.Append(checksumConverted);

        return sb.ToString();
    }

    /// <summary>
    ///     Validates A Encoded String
    /// </summary>
    /// <param name="code">The encoded String</param>
    /// <returns></returns>
    public static bool Validate(string code)
    {
        var spl = code.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (spl.Length == 0) return false;
        if (spl.Length == 1) return false;

        var checksum = (code.Length - spl.Length) ^ 3;
        var checksumLetter = GetLetter(checksum);

        return spl[^1] == checksumLetter;
    }

    /// <summary>
    ///     Decode a EncodedString to Integer Values
    /// </summary>
    /// <param name="code">The Encoded String to Decode</param>
    /// <returns></returns>
    public static int[] Decode(string code)
    {
        var spl = code.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var buffer = new int[spl.Length - 1];

        for (var i = 0; i < spl.Length - 1; i++) buffer[i] = int.Parse(GetNumberString(spl[i]));

        var checksum = (code.Length - spl.Length) ^ 3;
        var checksumLetter = GetLetter(checksum);

        if (spl[^1] == checksumLetter) return buffer;

        return null;
    }

    private static string GetNumberString(string v)
    {
        var sb = new StringBuilder();

        foreach (var l in v)
        {
            var index = Alphabet.IndexOf(l);
            var digit = index.ToString();

            sb.Append(digit);
        }

        return sb.ToString();
    }

    private static string GetLetter(int v)
    {
        var str = v.ToString().ToCharArray();

        var sb = new StringBuilder();

        foreach (var d in str) sb.Append(Alphabet[d - '0']);

        return sb.ToString();
    }
}