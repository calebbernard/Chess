using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Agents
{
    class HumanPlayer : Player
    {
        public HumanPlayer() : base()
        {
            name = "Human";
        }
        public override Tuple<Coord, Coord, SpecialMove> chooseMove(List<Move> moves)
        {
            bool success = false;
            while (!success)
            {
                Console.Clear();
                b.display();
                string input = Console.ReadLine().ToUpper();
                string from = input.Substring(0, 2);
                string to = input.Substring(2, 2);
                SpecialMove special = SpecialMove.None;
                if (input.Length >= 5)
                {
                    string s = input.Substring(4);
                    if (s == "Q")
                    {
                        special = SpecialMove.PromoteToQueen;
                    }
                    else if (s == "R")
                    {
                        special = SpecialMove.PromoteToRook;
                    }
                    else if (s == "N")
                    {
                        special = SpecialMove.PromoteToKnight;
                    }
                    else if (s == "B")
                    {
                        special = SpecialMove.PromoteToBishop;
                    }
                }
                foreach (Move m in moves)
                {
                    if (m.from.pos.ToString() == from && m.to.pos.ToString() == to && m.special == special)
                    {
                        return Tuple.Create(m.from.pos, m.to.pos, m.special);
                    }
                }
                Console.WriteLine("Invalid move. Try again.");
            }
            return null;
        }
    }
}
