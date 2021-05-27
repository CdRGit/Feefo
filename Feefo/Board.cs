using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Feefo
{
	public class Board
	{
		char[,] _field;
		List<Pointer> _pointers;

		public Board(char[,] field, List<(int x, int y)> pointerStarts)
		{
			_field = field;

			_pointers = new();

			foreach (var position in pointerStarts)
			{
				var wrappedLeft = Wrap(position.x - 1, position.y);
				if (_field[wrappedLeft.X, wrappedLeft.Y] == '<')
				{
					_pointers.Add(new Pointer(wrappedLeft.X, wrappedLeft.Y));
				}

				var wrappedRight = Wrap(position.x + 1, position.y);
				if (_field[wrappedRight.X, wrappedRight.Y] == '>')
				{
					_pointers.Add(new Pointer(wrappedRight.X, wrappedRight.Y));
				}

				var wrappedUp = Wrap(position.x, position.y - 1);
				if (_field[wrappedUp.X, wrappedUp.Y] == '^')
				{
					_pointers.Add(new Pointer(wrappedUp.X, wrappedUp.Y));
				}

				var wrappedDown = Wrap(position.x, position.y + 1);
				if (_field[wrappedDown.X, wrappedDown.Y] == 'V')
				{
					_pointers.Add(new Pointer(wrappedDown.X, wrappedDown.Y));
				}
			}
		}

		public (int X, int Y) Wrap(int x, int y)
		{
			while (x < 0)
				x += _field.GetLength(0);
			while (x > _field.GetLength(0) - 1)
				x -= _field.GetLength(0);
			while (y < 0)
				y += _field.GetLength(1);
			while (y > _field.GetLength(1) - 1)
				y -= _field.GetLength(1);

			return (x, y);
		}

		public void Display()
		{
			var color = Console.ForegroundColor;

			for (var y = 0; y < _field.GetLength(1); y++)
			{
				for (var x = 0; x < _field.GetLength(0); x++)
				{
					if (_pointers.Where(p => p.X == x && p.Y == y).Any())
					{
						Console.ForegroundColor = ConsoleColor.Green;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}

					Console.Write(_field[x, y]);
				}
				Console.WriteLine();
			}

			Console.ForegroundColor = color;
		}

		List<Task> pointerTasks;

		public async Task RunTillExit()
		{
			pointerTasks = new List<Task>();

			foreach (var pointer in _pointers)
			{
				pointerTasks.Add(pointer.RunTillExit(this));
			}

			await Task.WhenAll(pointerTasks);
		}

		List<Pointer> _waitingPointers = new();

		void PerformTransfers((int X, int Y) wrappedPos, Pointer waiting)
		{
			for (var i = _waitingPointers.Count - 1; i >= 0; i--)
			{
				var other = _waitingPointers[i];
				if (Wrap(other.X, other.Y) == wrappedPos)
				{
					lock (waiting.Queue)
					{
						lock (other.Queue)
						{
							other.Queue.Enqueue(waiting.Queue.Dequeue());
							waiting.State = PointerState.Executing;
							other.State = PointerState.Executing;
							_waitingPointers.Remove(waiting);
							_waitingPointers.Remove(other);
						}
					}
				}
			}
		}

		public void AnnounceExited()
		{
			var running = false;
			foreach (var pointer in _pointers)
			{
				if (pointer.State == PointerState.Executing || pointer.State == PointerState.String || pointer.State == PointerState.StringEscaped)
				{
					running = true;
					break;
				}
			}
			if (!running)
			{
				foreach (var pointer in _pointers)
				{
					pointer.Kill = true;
				}
			}
		}

		public void AnnounceWaiting(Pointer pointer)
		{
			lock (_waitingPointers)
			{
				_waitingPointers.Add(pointer);
				for (var i = _waitingPointers.Count - 1; i >= 0; i--)
				{
					if (i >= _waitingPointers.Count)
						continue;
					var waiting = _waitingPointers[i];
					var wrappedLeft = Wrap(waiting.X - 1, waiting.Y);
					if (_field[wrappedLeft.X, wrappedLeft.Y] == '<')
					{
						var wrappedPos = Wrap(waiting.X - 2, waiting.Y);
						PerformTransfers(wrappedPos, waiting);
						continue;
					}

					var wrappedRight = Wrap(waiting.X + 1, waiting.Y);
					if (_field[wrappedRight.X, wrappedRight.Y] == '>')
					{
						var wrappedPos = Wrap(waiting.X + 2, waiting.Y);
						PerformTransfers(wrappedPos, waiting);
						continue;
					}

					var wrappedUp = Wrap(waiting.X, waiting.Y - 1);
					if (_field[wrappedUp.X, wrappedUp.Y] == '^')
					{
						var wrappedPos = Wrap(waiting.X, waiting.Y - 2);
						PerformTransfers(wrappedPos, waiting);
						continue;
					}

					var wrappedDown = Wrap(waiting.X, waiting.Y + 1);
					if (_field[wrappedDown.X, wrappedDown.Y] == 'V')
					{
						var wrappedPos = Wrap(waiting.X, waiting.Y + 2);
						PerformTransfers(wrappedPos, waiting);
						continue;
					}
				}
			}
		}

		public char GetChar(int x, int y) => _field[Wrap(x, y).X, Wrap(x, y).Y];
	}

	public class Pointer
	{
		public Pointer(int x, int y)
		{
			X = x;
			Y = y;
			State = PointerState.Executing;
			Queue = new();
		}

		public int X { get; private set; }
		public int Y { get; private set; }
		public PointerState State { get; set; }
		public PointerDirection Direction { get; private set; }
		public Queue<double> Queue { get; private set; }

		public bool Kill { get; set; }

		public async Task RunTillExit(Board board)
		{
			await Task.Run(async () =>
			{
				var current = board.GetChar(X, Y);

				while (current != 'x')
				{
					switch (State)
					{
						case PointerState.Executing:
							switch (current)
							{
								// direction control
								case '>': Direction = PointerDirection.Right; break;
								case '<': Direction = PointerDirection.Left; break;
								case 'V': Direction = PointerDirection.Down; break;
								case '^': Direction = PointerDirection.Up; break;

								// control flow
								case '!': Move(); break; // skip next instruction always
								case '?': if (Queue.Dequeue() == 0) Move(); break; // skip next instruction if head is zero

								// noop
								case ' ': break;

								// state control
								case '"': State = PointerState.String; break;

								// queue operations
								case 'r': Queue.Enqueue(Queue.Dequeue()); break;             // recycle first item
								case 'R': Queue = new Queue<double>(Queue.Reverse()); break;
								case 'd': { var temp = Queue.Dequeue(); Queue.Enqueue(temp); Queue.Enqueue(temp); break; } // duplicate
								case 'D': Queue.Enqueue(Queue.Peek()); break; // duplicate without dequeue
								case 'c': Queue.Dequeue(); break; // clear head
								case 'C': Queue.Clear(); break;   // clear queue

								// output
								case 'p': Console.Write((char)Queue.Dequeue()); break;                                     // write first item (ASCII)
								case 'P': Console.WriteLine((char)Queue.Dequeue()); break;                                 // write first item (ASCII newline)
								case 'o': Console.Write(Queue.Dequeue()); break;                                           // write first item (value)
								case 'O': Console.WriteLine(Queue.Dequeue()); break;                                       // write first item (value newline)
								case 'h': Console.Write("0x" + ((uint)Queue.Dequeue()).ToString("X8")); break;        // write first item (hex value)
								case 'H': Console.WriteLine("0x" + ((uint)Queue.Dequeue()).ToString("X8")); break;    // write first item (hex value newline)

								// input
								case 'i': while (!Console.KeyAvailable) { }  Queue.Enqueue(Console.ReadKey().KeyChar); break;
								case 'I': Console.Write("Input (number): ");  Queue.Enqueue(int.Parse(Console.ReadLine())); break;

								// integers
								case '0': Queue.Enqueue(0); break;
								case '1': Queue.Enqueue(1); break;
								case '2': Queue.Enqueue(2); break;
								case '3': Queue.Enqueue(3); break;
								case '4': Queue.Enqueue(4); break;
								case '5': Queue.Enqueue(5); break;
								case '6': Queue.Enqueue(6); break;
								case '7': Queue.Enqueue(7); break;
								case '8': Queue.Enqueue(8); break;
								case '9': Queue.Enqueue(9); break;

								// math
								case '+': Queue.Enqueue(Queue.Dequeue() + Queue.Dequeue()); break;
								case '-': Queue.Enqueue(Queue.Dequeue() - Queue.Dequeue()); break;
								case '*': Queue.Enqueue(Queue.Dequeue() * Queue.Dequeue()); break;
								case '/': Queue.Enqueue(Queue.Dequeue() / Queue.Dequeue()); break;
								case '%': Queue.Enqueue((int)Queue.Dequeue() % (int)Queue.Dequeue()); break;

								// self inspection
								case '#': Queue.Enqueue(Queue.Count); break;

								// data transfer
								case ',': 
									{ 
										State = PointerState.Waiting;
										board.AnnounceWaiting(this);
										break; 
									}

								// default
								default: throw new NotImplementedException($"Unexpected: {current}");
							}
							break;
						case PointerState.String:
							switch (current)
							{
								case '\\': State = PointerState.StringEscaped; break;
								case '"': State = PointerState.Executing; break;
								default: Queue.Enqueue((byte)current); break;
							}
							break;
						case PointerState.StringEscaped:
							switch (current)
							{
								case '\\':
								case '"': Queue.Enqueue((byte)current); break;
								case '0': Queue.Enqueue(0); break;
								case 'n': Queue.Enqueue((byte)'\n'); break;
								case 'r': Queue.Enqueue((byte)'\r'); break;
								case 'a': Queue.Enqueue((byte)'\a'); break;
								default: throw new Exception($"Can't escape {current}");
							}
							State = PointerState.String;
							break;
						case PointerState.Waiting:
							// we wait
							await Task.Delay(10);
							break;
						default: throw new NotImplementedException($"Not implemented yet: {State}");
					}
					lock (Queue)
						if (State != PointerState.Waiting)
							Move();

					if (Kill)
						break;

					current = board.GetChar(X, Y);
				}
				State = PointerState.Exited;
				board.AnnounceExited();
			});
		}

		private void Move()
		{
			switch (Direction)
			{
				case PointerDirection.Right: X++; break;
				case PointerDirection.Left : X--; break;
				case PointerDirection.Down : Y++; break;
				case PointerDirection.Up   : Y--; break;
			}
		}
	}
	public enum PointerDirection
	{
		Up,
		Down,
		Left,
		Right
	}

	public enum PointerState
	{
		Executing,
		String,
		StringEscaped,
		Waiting,
		Exited
	}
}
