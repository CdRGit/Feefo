using System;
using System.Collections.Generic;
using System.Linq;

namespace Feefo
{
	public static class Processor
	{
		public static Board Process(string input)
		{
			// idk how to deal with tabs properly lol so you get this
			if (input.Contains('\t'))
				throw new ArgumentException("NO TABS YOU FUCKWIT");

			input = input.Replace("\r", "");

			// I'm not even gonna bother
			var boardHeight = input.Split('\n').Length;
			var boardWidth = input.Split('\n').Select(s => s.Length).Max();

			var field = new char[boardWidth, boardHeight];
			var pointerStarts = new List<(int, int)>();

			for (var x = 0; x < boardWidth; x++)
			{
				for (var y = 0; y < boardHeight; y++)
				{
					field[x, y] = ' ';
				}
			}

			// splitting it twice who gives a shit
			foreach (var line in input.Split('\n').Select((value, index) => (value, index)))
			{
				foreach (var chr in line.value.Select((value, index) => (value, index)))
				{
					if (chr.value != '.')
						field[chr.index, line.index] = chr.value;
					else
						pointerStarts.Add((chr.index, line.index));
				}
			}

			return new Board(field, pointerStarts);
		}
	}
}
