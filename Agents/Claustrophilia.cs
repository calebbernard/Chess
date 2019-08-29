using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Agents
{
    class Claustrophilia : Player
    {
        private Move checkForcedLine(Board _b, int depth = 0)
        {
            // max search depth
            if (depth == 5)
            {
                return null;
            } else
            {
                depth++;
            }
            Move forcingMove = null;
            foreach (var m in _b.availableMoves(playerColor))
            {
                var dream = _b.clone();
                dream.move(m.from.pos, m.to.pos, m.special);
                dream.afterTurn();

                if (dream.state == GameState.Active)
                {
                    var enemyMoves = dream.availableMoves(Operations.oppositeColor(playerColor));
                    if (enemyMoves.Count == 1)
                    {
                        dream.move(enemyMoves.First().from.pos, enemyMoves.First().to.pos, enemyMoves.First().special);
                        forcingMove = checkForcedLine(dream, depth);
                    }
                }
                else
                {
                    // win leaf
                    if (Operations.winner(playerColor, dream) == 1)
                    {
                        return m;
                    }
                    // draw or loss leaf
                    return null;
                }
            }
            return forcingMove;
        }

        public override Tuple<Coord, Coord, SpecialMove> chooseMove(List<Move> choices)
        {
            int enemyOptions = int.MaxValue;
            Move bestMove = null;
            foreach (var m in choices)
            {
                var forced = checkForcedLine(b);
                if (forced != null)
                {
                    bestMove = forced;
                    break;
                }
                var dream = b.clone();
                dream.move(m.from.pos, m.to.pos, m.special);
                dream.afterTurn();
                // take checkmate (this might be redundant with checkForcedLine?
                if (dream.state != GameState.Active && Operations.winner(playerColor, dream) == 1)
                {
                    bestMove = m;
                    break;
                }
                var tempEnemyOptions = dream.availableMoves(Operations.oppositeColor(playerColor)).Count;
                if (tempEnemyOptions < enemyOptions)
                {
                    bestMove = m;
                    enemyOptions = tempEnemyOptions;
                }
            }
            return Tuple.Create(bestMove.from.pos, bestMove.to.pos, bestMove.special);
        }
    }
}
