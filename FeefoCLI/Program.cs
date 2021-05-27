using Feefo;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FeefoCLI
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var text = File.ReadAllText(args.Length > 0 ? args[0] : Console.ReadLine());

			var board = Processor.Process(text);

			board.Display();

			try
			{
				await board.RunTillExit();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				board.Display();
			}

			Console.WriteLine("END");

			Console.ReadLine();
		}
	}
}
