using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Agents
{
    class Greedy : Player
    {
        public Greedy(Random _rng)
        {
            rng = _rng;
        }

        public override Tuple<Coord, Coord, SpecialMove> chooseMove(List<Move> moves)
        {
            Move choice = null;
            foreach (Move m in moves)
            {
                var b2 = b.clone();
                b2.move(m.from.pos, m.to.pos, m.special);
                if (b2.state == GameState.BlackVictory && playerColor == Color.Black ||
                    b2.state == GameState.WhiteVictory && playerColor == Color.White)
                {
                    choice = m;
                    break;
                }
            }
            bool captureFound = false;
            foreach (Move m in moves)
            {
                if (!captureFound && m.removeMe != null)
                {
                    captureFound = true;
                    choice = m;
                } else if (m.removeMe != null && m.removeMe.GetType() == typeof(Queen))
                {
                    choice = m;
                } else if (m.removeMe != null && m.removeMe.GetType() == typeof(Rook))
                {
                    choice = m;
                }
                else if (m.removeMe != null && m.removeMe.GetType() == typeof(Bishop))
                {
                    choice = m;
                }
                else if (m.removeMe != null && m.removeMe.GetType() == typeof(Knight))
                {
                    choice = m;
                }
                else if (m.removeMe != null && m.removeMe.GetType() == typeof(Pawn))
                {
                    choice = m;
                }
            }
            if (choice == null)
            {
                choice = moves[rng.Next(moves.Count)];
                
            }

            Coord from = new Coord(choice.agent.tile.pos.row, choice.agent.tile.pos.col);
            Coord to = new Coord(choice.to.pos.row, choice.to.pos.col);
            SpecialMove special = choice.special;
            return Tuple.Create(from, to, special);
        }
    }
}
