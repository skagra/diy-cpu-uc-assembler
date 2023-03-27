using System;
using System.IO;

public class Cmp
{
    public static int Main(string[] args)
    {
        var refFileName = args[0];
        var testFileName = args[1];

        Console.WriteLine("Comparing files");
        Console.WriteLine($"Reference file '{refFileName}'.");
        Console.WriteLine($"Test file '{testFileName}'.");

        var refBytes = File.ReadAllBytes(refFileName);
        var cmpBytes = File.ReadAllBytes(testFileName);

        int result = 0;

        if (refBytes.Length == cmpBytes.Length)
        {
            for (var loc = 0; loc < refBytes.Length; loc++)
            {
                if (refBytes[loc] != cmpBytes[loc])
                {
                    Console.WriteLine($"Files differ at byte {loc}, reference value {refBytes[loc]} test value {cmpBytes[loc]}.");
                    result = -1;
                    break;
                }
            }
        }
        else
        {
            Console.WriteLine("Files are of different length.");
            result = -1;
        }

        if (result == 0)
        {
            Console.WriteLine("PASS");
        }
        else
        {
            Console.WriteLine("FAIL");
        }

        return result;
    }
}
