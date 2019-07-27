using System.Text;
using System.IO;
using System;

class Program
{
    static void Main(string[] args)
    {
        // Load the program into a string
        StringBuilder sb = new StringBuilder();
        using (StreamReader sr = new StreamReader("F:\\test.pluto"))
        {
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                sb.AppendLine(line);
            }
        }
        string program = sb.ToString();
        
        // Create a new Pluto runtime and execute the program
        PlutoRuntime runtime = new PlutoRuntime();
        runtime.compile(program);

        Console.ReadLine();
    }
}
