using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Agents
{
    class Materialist : Player
    {
        public Materialist(Random _rng)
        {
            rng = _rng;
        }

        public override Tuple<Coord, Coord, SpecialMove> chooseMove(List<Move> moves)
        {
            /*
             *  1. Take immediate checkmates (copy code from Greedy)
             *  2. weigh out the merit of attacking
             *  3. weight out the merit of defending
             *  4. if any of these have positive merit, do the most meritorious
             *  5. if nothing is going on in that department, choose the move that, 
             *          if it were immediately our turn again, would provide quantitatively
             *          the most options (expanionism)
             */


            // Starting with checkmate + expansionism
            Move checkmate = null;
            Move expansionist = null;
            Move attack = null;

            int moveCount = 0;
            foreach (Move m in moves)
            {
                if (m.removeMe != null)
                {
                    var b3 = b.clone();
                    b3.removePiece(m.removeMe.tile.pos);
                    var dreamMoves = b3.availableMoves(Operations.oppositeColor(playerColor));
                    foreach (Move m2 in dreamMoves)
                    {
                        if (m2.to.pos.row == m.removeMe.tile.pos.row &&
                            m2.to.pos.col == m.removeMe.tile.pos.col)
                        {
                            //
                        }
                    }
                }
                var b2 = b.clone();
                b2.move(m.from.pos, m.to.pos, m.special);
                if (b2.state == GameState.BlackVictory && playerColor == Color.Black ||
                    b2.state == GameState.WhiteVictory && playerColor == Color.White)
                {
                    checkmate = m;
                    break;
                }
                int thisCount = b2.availableMoves(playerColor).Count;
                if (thisCount > moveCount)
                {
                    moveCount = thisCount;
                    expansionist = m;
                }
            }

            Move random = moves[rng.Next(moves.Count)];



            Move choice = checkmate;
            if (choice == null)
            {
                choice = expansionist;
            }
            if (choice == null)
            {
                choice = random;
            }
            Coord from = new Coord(choice.agent.tile.pos.row, choice.agent.tile.pos.col);
            Coord to = new Coord(choice.to.pos.row, choice.to.pos.col);
            SpecialMove special = choice.special;
            return Tuple.Create(from, to, special);
        }
    }
}
