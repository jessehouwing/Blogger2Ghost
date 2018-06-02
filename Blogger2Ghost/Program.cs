using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ManyConsole;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;

namespace Blogger2Ghost
{
    class Program
    {
        public static async Task Main(string[] args)
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
