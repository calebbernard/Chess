using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Agents
{
    class RandomPlayer : Player
    {
        public RandomPlayer(Random _rng)
        {
            rng = _rng;
        }
        public override Tuple<Coord, Coord, SpecialMove> chooseMove(List<Move> moves)
        {
            Move choice = moves[rng.Next(moves.Count)];
            Coord from = new Coord(choice.agent.tile.pos.row, choice.agent.tile.pos.col);
            Coord to = new Coord(choice.to.pos.row, choice.to.pos.col);
            SpecialMove special = choice.special;
            return Tuple.Create(from, to, special);
        }
    }
}
