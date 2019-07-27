using System.Text;
using System.IO;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader("F:\\OneDrive - Lancaster University\\programming\\c#\\visual studio projects\\pluto\\Pluto\\ConsoleApp1\\test.pluto"))
            //using (StreamReader sr = new StreamReader("c:\\users\\44778\\OneDrive - Lancaster University\\programming\\c#\\visual studio projects\\pluto\\Pluto\\ConsoleApp1\\test.pluto"))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            string program = sb.ToString();
            PlutoRuntime runtime = new PlutoRuntime();
            runtime.compile(program);
            Console.ReadLine();
        }
    }
}