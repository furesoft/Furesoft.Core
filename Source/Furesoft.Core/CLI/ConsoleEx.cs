namespace Furesoft.Core.CLI;

public static class ConsoleEx
{
    public static string Prompt(string message)
    {
        Console.Write(message);
        return Console.ReadLine();
    }

    public static int PromptInt(string message)
    {
        var value = Prompt(message);
        if (int.TryParse(value, out var res)) return res;

        Console.WriteLine("Invalid Value");

        return -1;
    }

    public static float PromptDecimal(string message)
    {
        var value = Prompt(message);
        if (float.TryParse(value, out var res)) return res;

        Console.WriteLine("Invalid Value");

        return -1;
    }
}