using System;
using System.IO;
using System.Linq;
using System.Reflection;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: DiagnosticLoader <path_to_BYOJoystick.dll>");
            return 1;
        }

        var path = args[0];
        if (!File.Exists(path))
        {
            Console.WriteLine($"File not found: {path}");
            return 2;
        }

        try
        {
            var asm = Assembly.LoadFrom(path);
            Console.WriteLine($"Loaded assembly: {asm.FullName}");
            try
            {
                var types = asm.GetTypes();
                Console.WriteLine($"Assembly contains {types.Length} types.");
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine("ReflectionTypeLoadException thrown while getting types:");
                Console.WriteLine(ex.Message);
                if (ex.LoaderExceptions != null)
                {
                    Console.WriteLine("LoaderExceptions:");
                    foreach (var e in ex.LoaderExceptions)
                    {
                        Console.WriteLine("- " + (e?.ToString() ?? "<null>"));
                    }
                }
                return 3;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to load assembly:");
            Console.WriteLine(ex.ToString());
            return 4;
        }

        Console.WriteLine("Done.");
        return 0;
    }
}
