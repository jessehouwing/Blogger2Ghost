using System;
using System.Threading.Tasks;
using ManyConsole;

namespace Blogger2Ghost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            int result;

            try
            {
                var commandToExecute = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
                result = ConsoleCommandDispatcher.DispatchCommand(commandToExecute, args, Console.Out);
            }
            catch (Exception e)
            {
                result = -1;
                Console.Error.WriteLine(e.Message);
                if (e.InnerException != null)
                {
                    Console.Error.WriteLine(e.InnerException.Message);
                }
            }

            Environment.Exit(result);
        }

    }
}
