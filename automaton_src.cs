﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace Automaton
{
    class Program
    {

        static void Main(string[] args)
        {
            
        }
    }

    class Plane
    {
        private bool[,] cells;
        public Plane(bool[,] cells)
        {
            this.cells = cells;
        }

        //Функция возвращает допустимые ходы из клетки (x,y)
        //смысл возвращаемого значение|возвращаемое значение|бинарное представление возвращаемого значения
        //нет ходов| 0| 0000

        //n    | 1| 0001
        //e    | 2| 0010
        //s    | 4| 0100
        //w    | 8| 1000

        //ne   | 3| 0011
        //ns   | 5| 0101
        //nw   | 9| 1001
        //es   | 6| 0110
        //we   |10| 1010
        //ws   |12| 1100

        //nes  | 7| 0111
        //new  |11| 1011
        //wsn  |13| 1101
        //wse  |14| 1110

        //nesw |15| 1111

        public int GetAvailableMoves(int x, int y)
        {
            int output = 0;
            if (cells[x + 1, y]) output += 1;
            if (cells[x - 1, y]) output += 4;
            if (cells[x, y + 1]) output += 2;
            if (cells[x, y - 1]) output += 8;
            if (output == 0) return 0;
            return output; 
        }
    }

    class AutomatonOnPlane
    {
        private Plane plane;
        private MooreDiagram automaton;
        private int x;
        private int y;

        AutomatonOnPlane(Plane plane)
        {
            automaton = new MooreDiagram(new Range[] { new Range(0, 15) }, new Range[] { new Range(0, 4) });
        }


        public void SetInitState(string stateId)
        {
            automaton.SetInitState(stateId);
        }
        public void AddState(string stateId)
        {
            automaton.AddNewState(stateId);
        }
        public void AddNewTransition(string curStateId, AIO input, string nextStateId, AIO output)
        {
            automaton.AddNewTransition(curStateId, input, nextStateId, output);
        }
        public void RemoveTransition(string curStateId, AIO input, string nextStateId, AIO output)
        {
            automaton.RemoveTransition(curStateId, input, nextStateId, output);
        }


        public void SetPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void Move()
        {
            int route = plane.GetAvailableMoves(x, y);
            int output = automaton.Process(new AIO(route)).Data[0];
            switch(output)
            {
                case 1: y += 1; break; //north
                case 2: x += 1; break; //east
                case 4: y -= 1; break; //south
                case 8: x -= 1; break; //west
                case 0: break;
            }
        }

        public int X { get => x; }
        public int Y { get => y; }
        public string State { get => automaton.CurrentState; }
    }


    internal static class Hash
    {
        private static byte[] t = new byte[] { 178, 96, 240, 173, 88, 170, 92, 83, 72, 8, 149, 43, 251, 139, 179, 74, 192, 110, 237, 44, 40, 214, 244, 221, 20, 21, 165, 195, 143, 4, 70, 24, 38, 78, 94, 102, 107, 108, 163, 245, 203, 103, 217, 26, 239, 106, 181, 116, 185, 177, 242, 248, 25, 7, 138, 27, 223, 186, 37, 14, 161, 202, 252, 241, 148, 169, 59, 104, 193, 255, 227, 76, 233, 101, 67, 53, 156, 183, 238, 160, 89, 130, 64, 34, 68, 118, 122, 152, 188, 73, 167, 63, 19, 51, 112, 56, 213, 253, 180, 82, 187, 209, 232, 15, 90, 129, 215, 79, 71, 230, 6, 168, 207, 250, 33, 91, 124, 157, 85, 135, 141, 208, 153, 60, 226, 111, 97, 98, 32, 11, 133, 87, 50, 197, 13, 182, 16, 246, 22, 236, 45, 80, 134, 151, 46, 140, 62, 31, 216, 12, 201, 132, 228, 29, 199, 75, 66, 249, 205, 35, 120, 136, 194, 131, 247, 145, 219, 58, 23, 190, 175, 55, 1, 5, 229, 174, 113, 77, 47, 144, 30, 222, 206, 137, 121, 86, 164, 100, 10, 18, 49, 128, 162, 39, 159, 2, 109, 166, 212, 220, 126, 42, 235, 105, 93, 61, 211, 95, 231, 28, 142, 191, 254, 172, 218, 48, 52, 123, 224, 69, 57, 65, 84, 17, 204, 115, 243, 147, 234, 184, 0, 125, 176, 158, 196, 9, 117, 150, 210, 54, 146, 198, 189, 200, 127, 225, 114, 155, 119, 41, 99, 171, 36, 3, 154, 81 };
        private static int[] fb = new int[] { 255, 255 << 8, 255 << 16, 255 << 24 };

        public static byte[] T { get => t; set => t = value; }
        public static int[] Fb { get => fb; set => fb = value; }

        public static int GetHashCode(int[] array)
        {
            byte[] a = new byte[4 * array.Length];
            int index = 0;
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    a[index] = (byte)((array[i] & fb[j]) >> (8 * j));
                    index++;
                }
            }
            int res = 0;
            byte h;
            for (int i = 0; i < 4; i++)
            {
                h = T[(a[0] + i) % 256];
                for (int j = 1; j < a.Length; j++)
                {
                    h = T[h ^ a[j]];
                }
                res <<= 8;
                res += h;
            }
            return res;
        }
    }

    public class AIO
    {
        public int[] Data { get; private set; }
        public int DataLength { get => Data.Length; }

        public AIO(params int[] data)
        {
            Data = data;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!obj.GetType().Equals(typeof(AIO)))
                return false;

            return (obj as AIO).Data.SequenceEqual(Data);
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode(Data);
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", Data) + "]";
        }
    }

    internal class State
    {
        public string Id { get; }
        public Dictionary<AIO, (AIO, State)> Table { get; }

        public State(string id, Dictionary<AIO, (AIO, State)> table)
        {
            Id = id;
            Table = table;
        }



        public (AIO, State) Process(AIO input)
        {
            if (!Table.ContainsKey(input))
                throw new ArgumentException($"Состояние с Id={Id} не содержит выходящей из него стрелки с входом={input}");

            return Table[input];
        }

        

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!obj.GetType().Equals(typeof(State)))
                return false;

            return (obj as State).Id == Id;
        }

        public override string ToString()
        {
            return Id;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    public class Range
    {
        public int Begin { get; }
        public int End { get; }

        public Range(int begin, int end)
        {
            if (begin > end)
                throw new ArgumentException("begin > end", "end");
            Begin = begin;
            End = end;
        }

        public bool Contains(int value)
        {
            return Begin <= value && value <= End;
        }

        public override string ToString()
        {
            return $"[{Begin}, {End}]";
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!obj.GetType().Equals(typeof(Range)))
                return false;

            var o = obj as Range;
            return this.Begin == o.Begin && this.End == o.End;
        }
    }

    public class MooreDiagram
    {
        private State currentState;
        public Range[] InputAlphabet { get; }
        public Range[] OutputAlphabet { get; }

        private List<State> allStates;

        public MooreDiagram(Range[] inputAlphabet, Range[] outputAlphabet)
        {
            InputAlphabet = inputAlphabet;
            OutputAlphabet = outputAlphabet;
            currentState = null;
            allStates = new List<State>();
        }

        public int InputLength { get => InputAlphabet.Length; }
        public int OutputLength { get => OutputAlphabet.Length; }
        public string CurrentState { get => currentState.Id; }
        internal State CurrentState1 { get => currentState; set => currentState = value; }
        internal List<State> AllStates { get => allStates; set => allStates = value; }

        public void SetInitState(string id)
        {
            if (currentState != null)
                throw new InvalidOperationException("Автомат уже инициализирован.");

            currentState = new State(id, new Dictionary<AIO, (AIO, State)>());
            allStates.Add(currentState);
        }

        public void AddNewState(string id)
        {
            if (currentState == null)
                throw new InvalidOperationException("Автомат ещё не инициализирован.");
            if (allStates.Find(x => x.Id == id) != null)
                throw new ArgumentException($"Состояние с Id={id} уже существует", id);

            State s = new State(id, new Dictionary<AIO, (AIO, State)>());
            allStates.Add(s);
        }

        public void AddNewTransition(string curStateId, AIO input, string nextStateId, AIO output)
        {
            if (currentState == null)
                throw new InvalidOperationException("Автомат ещё не инициализирован.");
            if (allStates.Find(x => x.Id == curStateId) == null)
                throw new ArgumentException($"Состояния с Id={curStateId} не существует.", "curStateId");
            if (allStates.Find(x => x.Id == nextStateId) == null)
                throw new ArgumentException($"Состояния с Id={nextStateId} не существует.", "nextStateId");
            if (!doesAIOSatisfyAlphabet(input, InputAlphabet))
                throw new ArgumentException("Не все входные данные находятся в требуемом алфавите.", "input");
            if (!doesAIOSatisfyAlphabet(output, OutputAlphabet))
                throw new ArgumentException("Не все выходные данные находятся в требуемом алфавите.", "output");

            State curState = allStates.Find(x => x.Id == curStateId);
            State nextState = allStates.Find(x => x.Id == nextStateId);

            curState.Table.Add(input, (output, nextState));
            
        }

        public void RemoveTransition(string curStateId, AIO input, string nextStateId, AIO output)
        {
            if (currentState == null)
                throw new InvalidOperationException("Автомат ещё не инициализирован.");
            if (allStates.Find(x => x.Id == curStateId) == null)
                throw new ArgumentException($"Состояния с Id={curStateId} не существует.", "curStateId");
            if (allStates.Find(x => x.Id == nextStateId) == null)
                throw new ArgumentException($"Состояния с Id={nextStateId} не существует.", "nextStateId");

            State curState = allStates.Find(x => x.Id == curStateId);
            State nextState = allStates.Find(x => x.Id == nextStateId);

            if (!curState.Table.Contains(new KeyValuePair<AIO, (AIO, State)>(input, (output, nextState))))
                throw new InvalidOperationException("Не существует перехода, который нужно удалить.");

            curState.Table.Remove(input);
        }

        public AIO Process(AIO input)
        {
            if (currentState == null)
                throw new InvalidOperationException("Автомат ещё не инициализирован.");
            if (!doesAIOSatisfyAlphabet(input, InputAlphabet))
                throw new ArgumentException("Автомат не распознаёт эти входные данные.");

            var output = currentState.Process(input);
            currentState = output.Item2;
            return output.Item1;
        }

        private bool doesAIOSatisfyAlphabet(AIO aio, Range[] alphabet)
        {
            if (aio.DataLength != alphabet.Length)
                return false;

            for (int i = 0; i < aio.DataLength; i++)
            {
                if (!alphabet[i].Contains(aio.Data[i]))
                    return false;
            }

            return true;
        }
    }
}