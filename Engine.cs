using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Chess.Library;

namespace Chess
{

    static class Operations
    {
        public static int winner(Color c, Board b)
        {
            if (b.state == GameState.BlackVictory && c == Color.Black ||
                b.state == GameState.WhiteVictory && c == Color.White)
            {
                return 1;
            } else if (b.state == GameState.WhiteVictory || b.state == GameState.BlackVictory)
            {
                return -1;
            }
            // active or draw
            return 0;
        }

        public static Color oppositeColor(Color c)
        {
            if (c == Color.Black)
            {
                return Color.White;
            }
            return Color.Black;
        }
    }

    enum Color
    {
        Black,
        White
    }

    // eventually change this into a WLD basic state and a different enum for detailed results
    enum GameState
    {
        Active,
        Draw,
        DrawThreefold,
        DrawInsufficientMaterial,
        DrawStalemate,
        DrawFiftyTurns,
        BlackVictory,
        WhiteVictory
    }

    enum SpecialMove
    {
        None,
        PromoteToQueen,
        PromoteToRook,
        PromoteToBishop,
        PromoteToKnight
    }

    enum Pieces
    {
        Queen,
        Pawn,
        Bishop,
        Rook,
        Knight,
        King,
        None
    }

    class Coord
    {
        public int row, col;
        public Coord(int _row, int _col)
        {
            row = _row;
            col = _col;
        }

        public override string ToString()
        {
            string output = "";
            if (col == 0)
            {
                output += "A";
            } else if (col == 1)
            {
                output += "B";
            } else if (col == 2)
            {
                output += "C";
            } else if (col == 3)
            {
                output += "D";
            } else if (col == 4)
            {
                output += "E";
            } else if (col == 5)
            {
                output += "F";
            } else if (col == 6)
            {
                output += "G";
            } else
            {
                output += "H";
            }
            output += Math.Abs(row - 8).ToString();
            return output;
        }

        public static Coord fromString(string sample)
        {
            sample = sample.ToUpper();
            if (sample.Length != 2)
            {
                return null;
            }
            int row, col;
            switch (sample[0])
            {
                case 'A':
                    col = 0;
                    break;
                case 'B':
                    col = 1;
                    break;
                case 'C':
                    col = 2;
                    break;
                case 'D':
                    col = 3;
                    break;
                case 'E':
                    col = 4;
                    break;
                case 'F':
                    col = 5;
                    break;
                case 'G':
                    col = 6;
                    break;
                case 'H':
                    col = 7;
                    break;
                default:
                    return null;
            }
            switch (sample[1])
            {
                case '1':
                    row = 7;
                    break;
                case '2':
                    row = 6;
                    break;
                case '3':
                    row = 5;
                    break;
                case '4':
                    row = 4;
                    break;
                case '5':
                    row = 3;
                    break;
                case '6':
                    row = 2;
                    break;
                case '7':
                    row = 1;
                    break;
                case '8':
                    row = 0;
                    break;
                default:
                    return null;
            }
            return new Coord(row, col);
        }

        public Coord flip()
        {
            return new Coord(7 - row, 7 - col);
        }

        public static Coord fromInt(int sample)
        {
            return new Coord(sample / 8, sample % 8);
        }

        public int toInt()
        {
            return row * 8 + col;
        }
    }

    class Move
    {
        public Piece agent;
        public Tile from;
        public Tile to;
        public Piece removeMe;
        public SpecialMove special;

        public Move(Piece _agent, Tile _from, Tile _to, SpecialMove _special = SpecialMove.None, Piece _removeMe = null)
        {
            agent = _agent;
            from = _from;
            to = _to;
            special = _special;
            removeMe = _removeMe;
        }

        public override string ToString()
        {
            return agent.representation + ": " + from.pos.ToString() + " - " + to.pos.ToString();
        }
    }

    abstract class Piece
    {
        public int id;
        public Color color;
        public Tile tile;
        public Pieces kind;
        public char representation;
        public string fullName;
        public Piece(Color _color, int _id)
        {
            id = _id;
            color = _color;
            tile = null;
        }

        // Return available tiles
        public abstract List<Move> moves(Board b);

        public List<Move> filterOutCheckMoves(List<Move> startList, Board b, Color playerColor)
        {
            var finalList = new List<Move>();
            foreach (Move m in startList)
            {
                if (!b.wouldThisMoveResultInMeBeingInCheck(playerColor, m)){
                    finalList.Add(m);
                }
            }
            return finalList;
        }

        public virtual bool commitMove(Move m, Board b)
        {
            if (m.removeMe != null && m.removeMe.tile != null)
            {
                m.removeMe.tile.piece = null;
                m.removeMe.tile = null;
            }
            m.agent.tile.piece = null;
            m.agent.tile = m.to;
            m.agent.tile.piece = m.agent;
            b.history.Add(m);
            return true;
        }

        public override string ToString()
        {
            return representation.ToString();
        }

        public string longRepresentation()
        {
            return color.ToString() + " " + fullName;
        }
    }

    class Pawn : Piece
    {
        public bool justLeaped;
        public Pawn(Color _color, int _id) : base(_color, _id)
        {
            justLeaped = false;
            representation = 'p';
            fullName = "Pawn";
            kind = Pieces.Pawn;
        }

        public override List<Move> moves(Board b)
        {
            if (tile == null)
            {
                return new List<Move>();
            }
            var result = new List<Move>();
            int direction = -1;
            int startingRow = 6;
            if (color == Color.Black)
            {
                direction = 1;
                startingRow = 1;
            }
            var coords = new List<Coord>();
            coords.Add(new Coord(1 * direction, 0));
            coords.Add(new Coord(2 * direction, 0));
            coords.Add(new Coord(1 * direction, 1));
            coords.Add(new Coord(1 * direction, -1));

            foreach (Coord c in coords)
            {
                var target = b.getTile(tile.pos.row + c.row, tile.pos.col + c.col);
                if (target == null)
                {
                    continue;
                }
                // starting leap
                // - Must not have moved yet (be on the starting row)
                if (Math.Abs(c.row) == 2)
                {
                    if (tile.pos.row == startingRow && target.piece == null && b.getTile(tile.pos.row + 1 * direction, tile.pos.col) != null && b.getTile(tile.pos.row + 1 * direction, tile.pos.col).piece == null)
                    {
                        result.Add(new Move(this, this.tile, target, SpecialMove.None));
                    }
                }
                // regular move
                // - move one square straight forwards
                else if (c.col == 0)
                {
                    if (target.piece == null)
                    {
                        if ((color == Color.Black && target.pos.row == 7) || (color == Color.White && target.pos.row == 0))
                        {
                            result.Add(new Move(this, this.tile, target, SpecialMove.PromoteToQueen));
                            result.Add(new Move(this, this.tile, target, SpecialMove.PromoteToRook));
                            result.Add(new Move(this, this.tile, target, SpecialMove.PromoteToBishop));
                            result.Add(new Move(this, this.tile, target, SpecialMove.PromoteToKnight));
                        }
                        else
                        {
                            result.Add(new Move(this, this.tile, target, SpecialMove.None));
                        }
                    }
                }
                // standard capture
                // - opposite colored piece to the ahead-diagonal.
                else if (target.piece != null && target.piece.color != color)
                {
                    if ((color == Color.Black && target.pos.row == 7) || (color == Color.White && target.pos.row == 0))
                    {
                        result.Add(new Move(this, this.tile, target, SpecialMove.PromoteToQueen, target.piece));
                        result.Add(new Move(this, this.tile, target, SpecialMove.PromoteToRook, target.piece));
                        result.Add(new Move(this, this.tile, target, SpecialMove.PromoteToBishop, target.piece));
                        result.Add(new Move(this, this.tile, target, SpecialMove.PromoteToKnight, target.piece));
                    }
                    else
                    {
                        result.Add(new Move(this, this.tile, target, SpecialMove.None, target.piece));
                    }
                }
                // en passant
                else
                {
                    int horizDirection = (target.pos.col - tile.pos.col);
                    var enPTile = b.getTile(tile.pos.row, tile.pos.col + 1 * horizDirection);
                    // - the destination diagonal square must be free
                    // - the square touching both this pawn horizontally and the target square vertically must be a pawn
                    // - that pawn must be the opposite color
                    // - and that pawn must have just done a double jump
                    if (target.piece == null && enPTile != null && enPTile.piece != null &&
                        enPTile.piece.color != color &&
                        enPTile.piece.GetType() == typeof(Pawn) 
                        && ((Pawn)enPTile.piece).justLeaped == true)
                    {
                        result.Add(new Move(this, this.tile, target, SpecialMove.None, enPTile.piece));
                    }
                }
            }
            return result;
        }

    }

    class Rook : Piece
    {
        public bool hasMoved;
        public Rook(Color _color, int _id) : base(_color, _id)
        {
            representation = 'r';
            fullName = "Rook";
            hasMoved = false;
            kind = Pieces.Rook;
        }
        public override List<Move> moves(Board b)
        {
            if (tile == null)
            {
                return new List<Move>();
            }
            var result = new List<Move>();
            // Determine direction of movement
            var coords = new List<Coord>();
            coords.Add(new Coord(-1, 0));
            coords.Add(new Coord(1, 0));
            coords.Add(new Coord(0, -1));
            coords.Add(new Coord(0, 1));
            foreach (Coord c in coords)
            {
                bool stop = false;
                while (!stop)
                {
                    if (tile.pos.row + c.row >= 0 && tile.pos.row + c.row <= 7 && tile.pos.col + c.col >= 0 && tile.pos.col + c.col <= 7)
                    {
                        var destTile = b.getTile(tile.pos.row + c.row, tile.pos.col + c.col);
                        if (destTile.piece != null)
                        {
                            stop = true;
                            if (destTile.piece.color != color)
                            {
                                result.Add(new Move(this, this.tile, destTile, SpecialMove.None, destTile.piece));
                            }
                        }
                        else
                        {
                            result.Add(new Move(this, this.tile, destTile, SpecialMove.None));
                        }
                    }
                    else
                    {
                        stop = true;
                    }
                    // bad
                    if (c.row < 0)
                    {
                        c.row--;
                    }
                    else if (c.row > 0)
                    {
                        c.row++;
                    }
                    if (c.col < 0)
                    {
                        c.col--;
                    }
                    else if (c.col > 0)
                    {
                        c.col++;
                    }
                }
            }
            return result;
        }

        public override bool commitMove(Move m, Board b)
        {
            hasMoved = true;
            return base.commitMove(m, b);
        }
    }

    class Bishop : Piece
    {
        public Bishop(Color _color, int _id) : base(_color, _id)
        {
            representation = 'b';
            fullName = "Bishop";
            kind = Pieces.Bishop;
        }

        public override List<Move> moves(Board b)
        {
            if (tile == null)
            {
                return new List<Move>();
            }
            var result = new List<Move>();
            // Determine direction of movement
            var coords = new List<Coord>();
            coords.Add(new Coord(-1, -1));
            coords.Add(new Coord(1, 1));
            coords.Add(new Coord(1, -1));
            coords.Add(new Coord(-1, 1));
            foreach (Coord c in coords)
            {
                bool stop = false;
                while (!stop)
                {
                    if (tile.pos.row + c.row >= 0 && tile.pos.row + c.row <= 7 && tile.pos.col + c.col >= 0 && tile.pos.col + c.col <= 7)
                    {
                        var destTile = b.getTile(tile.pos.row + c.row, tile.pos.col + c.col);
                        if (destTile.piece != null)
                        {
                            stop = true;
                            if (destTile.piece.color != color)
                            {
                                result.Add(new Move(this, this.tile, destTile, SpecialMove.None, destTile.piece));
                            }
                        }
                        else
                        {
                            result.Add(new Move(this, this.tile, destTile, SpecialMove.None));
                        }
                    }
                    else
                    {
                        stop = true;
                    }
                    // bad
                    if (c.row < 0)
                    {
                        c.row--;
                    }
                    else if (c.row > 0)
                    {
                        c.row++;
                    }
                    if (c.col < 0)
                    {
                        c.col--;
                    }
                    else if (c.col > 0)
                    {
                        c.col++;
                    }
                }
            }
            return result;
        }
    }

    class Queen : Piece
    {
        public Queen(Color _color, int _id) : base(_color, _id)
        {
            representation = 'q';
            fullName = "Queen";
            kind = Pieces.Queen;
        }

        public override List<Move> moves(Board b)
        {
            if (tile == null)
            {
                return new List<Move>();
            }
            var result = new List<Move>();
            // Determine direction of movement
            var coords = new List<Coord>();
            coords.Add(new Coord(-1, 0));
            coords.Add(new Coord(1, 0));
            coords.Add(new Coord(0, -1));
            coords.Add(new Coord(0, 1));
            coords.Add(new Coord(-1, -1));
            coords.Add(new Coord(1, 1));
            coords.Add(new Coord(1, -1));
            coords.Add(new Coord(-1, 1));
            foreach (Coord c in coords)
            {
                bool stop = false;
                while (!stop)
                {
                    if (tile.pos.row + c.row >= 0 && tile.pos.row + c.row <= 7 && tile.pos.col + c.col >= 0 && tile.pos.col + c.col <= 7)
                    {
                        var destTile = b.getTile(tile.pos.row + c.row, tile.pos.col + c.col);
                        if (destTile.piece != null)
                        {
                            stop = true;
                            if (destTile.piece.color != color)
                            {
                                result.Add(new Move(this, this.tile, destTile, SpecialMove.None, destTile.piece));
                            }
                        }
                        else
                        {
                            result.Add(new Move(this, this.tile, destTile, SpecialMove.None));
                        }
                    }
                    else
                    {
                        stop = true;
                    }
                    // bad
                    if (c.row < 0)
                    {
                        c.row--;
                    }
                    else if (c.row > 0)
                    {
                        c.row++;
                    }
                    if (c.col < 0)
                    {
                        c.col--;
                    }
                    else if (c.col > 0)
                    {
                        c.col++;
                    }
                }
            }
            return result;
        }
    }

    class Knight : Piece
    {
        public Knight(Color _color, int _id) : base(_color, _id)
        {
            representation = 'n';
            fullName = "Knight";
            kind = Pieces.Knight;
        }

        public override List<Move> moves(Board b)
        {
            var result = new List<Move>();
            if (tile == null)
            {
                return new List<Move>();
            }
            var coords = new List<Coord>();
            coords.Add(new Coord(2, 1));
            coords.Add(new Coord(2, -1));
            coords.Add(new Coord(-2, 1));
            coords.Add(new Coord(-2, -1));
            coords.Add(new Coord(1, 2));
            coords.Add(new Coord(1, -2));
            coords.Add(new Coord(-1, 2));
            coords.Add(new Coord(-1, -2));
            foreach (Coord c in coords)
            {
                var target = b.getTile(tile.pos.row + c.row, tile.pos.col + c.col);
                if (target != null && ((target.piece != null && target.piece.color != color) || target.piece == null))
                {
                    result.Add(new Move(this, this.tile, target, SpecialMove.None, target.piece));
                }
            }
            return result;
        }
    }

    class King : Piece
    {
        public bool hasMoved;
        public King(Color _color, int _id) : base(_color, _id)
        {
            representation = 'k';
            fullName = "King";
            hasMoved = false;
            kind = Pieces.King;
        }

        public override List<Move> moves(Board b)
        {
            if (tile == null)
            {
                return new List<Move>();
            }
            var result = new List<Move>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 0 || y != 0)
                    {
                        var target = b.getTile(tile.pos.row + x, tile.pos.col + y);
                        if (target != null && (target.piece == null || target.piece.color != color))
                        {
                            bool check = false;
                            // kings can't attack each other (this prevents stack overflow as they check each other for attacks back and forth)
                            foreach (Piece p in b.pieces.Where(a => a.tile != null && a.color != color && a.GetType() != typeof(King)))
                            {
                                foreach (Move m in p.moves(b))
                                {
                                    if (m.to == target)
                                    {
                                        check = true;
                                    }
                                }
                            }
                            if (!check)
                            {
                                result.Add(new Move(this, this.tile, target, SpecialMove.None, target.piece));
                            }
                        }
                    }
                }
            }

            // if castling is available, add it to the list
            // this logic is very bulky and a candidate for refactoring
            // this next line is especially temporary
             if (!b.checkingMoves)
            {
                if (!hasMoved)
                {
                    if (canCastle(b, b.getTile(tile.pos.row, 7)))
                    {
                        result.Add(new Move(this, this.tile, b.getTile(tile.pos.row, 6)));
                    }
                    if (canCastle(b, b.getTile(tile.pos.row, 0)))
                    {
                        result.Add(new Move(this, this.tile, b.getTile(tile.pos.row, 2)));
                    }
                }
            }
            
            return result;
        }

        private bool canCastle(Board b, Tile rookSquare)
        {
            if (tile.pos.col != 4 || (tile.pos.row != 0 && tile.pos.row != 7))
            {
                return false;
            }
            // First check tiles between king and rook and make sure they're not occupied
            // there's surely a way to one liner this but it's late & i'm tired
            if (rookSquare.pos.col > tile.pos.col)
            {
                if (b.checkTile(new Coord(tile.pos.row, tile.pos.col + 1)) != "")
                {
                    return false;
                }
                if (b.checkTile(new Coord(tile.pos.row, tile.pos.col + 2)) != "")
                {
                    return false;
                }
            } else
            {
                if (b.checkTile(new Coord(tile.pos.row, tile.pos.col - 1)) != "")
                {
                    return false;
                }
                if (b.checkTile(new Coord(tile.pos.row, tile.pos.col - 2)) != "")
                {
                    return false;
                }
                if (b.checkTile(new Coord(tile.pos.row, tile.pos.col - 3)) != "")
                {
                    return false;
                }
            }

            Piece rook = rookSquare.piece;
            if (rook != null && rook.GetType() == typeof(Rook))
            {
                if (!((Rook)rook).hasMoved)
                {
                    var moveThroughTile = b.getTile(tile.pos.row, tile.pos.col + Math.Sign(rookSquare.pos.col - tile.pos.col) * 1);
                    // make sure we're not moving through check (as this isn't the final square, the checkFilter won't pick it up)
                    if (!b.wouldThisMoveResultInMeBeingInCheck(color, new Move(this, this.tile, b.getTile(tile.pos.row, 5), SpecialMove.None)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool commitMove(Move m, Board b)
        {
            hasMoved = true;
            return base.commitMove(m, b);
        }
    }

    class Tile
    {
        public Piece piece;
        public Coord pos;
        public Tile(Coord c)
        {
            pos = c;
            piece = null;
        }

        public override string ToString()
        {
            string output = pos.ToString();
            if (piece != null)
            {
                output += " - " + piece.ToString();
            }
            return output;
        }
    }
    class Board
    {
        public bool debugInterestingAction;
        public List<List<Tile>> board;
        public List<Piece> pieces;
        public int fiftyTurnCounter;
        public List<Move> history;
        public List<string> threefold;
        public Color nextToMove;
        public GameState state;
        public bool enforceTurns;
        // please take this out eventually
        public bool checkingMoves;
        public Board(bool _enforceTurns = true)
        {
            debugInterestingAction = false;
            nextToMove = Color.White;
            fiftyTurnCounter = 0;
            history = new List<Move>();
            threefold = new List<string>();
            state = GameState.Active;
            enforceTurns = _enforceTurns;
            board = new List<List<Tile>>();
            pieces = new List<Piece>();
            checkingMoves = false;

            int id = 0;

            for (int row = 0; row < 8; row++)
            {
                var tileRow = new List<Tile>();
                for (int col = 0; col < 8; col++)
                {
                    var tile = new Tile(new Coord(row,col));
                    Piece p = null;
                    Color c;
                    if (row < 2 || row > 5)
                    {
                        if (row < 2)
                        {
                            c = Color.Black;
                        } else
                        {
                            c = Color.White;
                        }
                        if (row == 1 || row == 6)
                        {
                            p = new Pawn(c, id);
                        }
                        else
                        {
                            if (col == 0 || col == 7)
                            {
                                p = new Rook(c, id);
                            }
                            else if (col == 1 || col == 6)
                            {
                                p = new Knight(c, id);
                            }
                            else if (col == 2 || col == 5)
                            {
                                p = new Bishop(c, id);
                            }
                            else if (col == 3)
                            {
                                p = new Queen(c, id);
                            }
                            else
                            {
                                p = new King(c, id);
                            }
                        }
                        p.tile = tile;
                        tile.piece = p;
                        pieces.Add(p);
                        id++;
                    }

                    tileRow.Add(tile);
                }
                board.Add(tileRow);
            }
        }

        public Color thisColor()
        {
            return Operations.oppositeColor(nextToMove);
        }

        public bool move(Coord from, Coord to, SpecialMove special = SpecialMove.None)
        {
            bool success = false;
            if (from.row < 0 || from.row > 7 || from.col < 0 || from.col > 7 || to.row < 0 || to.row > 7 || to.col < 0 || to.col > 7)
            {
                // cannot move out of bounds
                return false;
            }

            
            var source = board[from.row][from.col];

            // can't move a non-piece
            if (source.piece == null)
            {
                return false;
            }
            // can only move your own piece (only if turns are being enforced)
            if (enforceTurns && source.piece.color != nextToMove)
            {
                return false;
            }

            // if the proposed move matches an available move
            foreach (Move m in source.piece.moves(this))
            {
                if (m.to.pos.row == to.row && m.to.pos.col == to.col && m.special == special)
                {
                    // perform the move
                    m.agent.commitMove(m, this);

                    // check if we need to move a rook to complete a castle
                    if (m.agent.GetType() == typeof(King) && Math.Abs(m.to.pos.col - m.from.pos.col) == 2)
                    {
                        Piece rook = getTile(m.from.pos.row, Math.Min(7, m.from.pos.col + Math.Sign(m.to.pos.col - m.from.pos.col) * 4)).piece;
                        // we don't need to check if the king and rook are castle-legal here as that check happens in king move generation
                        if (rook != null && rook.GetType() == typeof(Rook))
                        {
                            rook.commitMove(new Move(rook, rook.tile, getTile(m.to.pos.row, m.to.pos.col - (Math.Sign(rook.tile.pos.col - m.to.pos.col) * 1))), this);
                        }
                    }

                    // check if we need to promote a piece
                    switch (m.special)
                    {
                        case SpecialMove.PromoteToQueen:
                            promote(m.agent, new Queen(m.agent.color, m.agent.id));
                            break;
                        case SpecialMove.PromoteToRook:
                            promote(m.agent, new Rook(m.agent.color, m.agent.id));
                            break;
                        case SpecialMove.PromoteToBishop:
                            promote(m.agent, new Bishop(m.agent.color, m.agent.id));
                            break;
                        case SpecialMove.PromoteToKnight:
                            promote(m.agent, new Knight(m.agent.color, m.agent.id));
                            break;
                        default:
                            break;
                    }
                    success = true;
                    // don't need to keep checking moves once we've found the one
                    break;
                }
                
            }
            nextToMove = Operations.oppositeColor(nextToMove);
            afterTurn();
            return success;
        }

        private void promote(Piece _old, Piece _new)
        {
            _new.tile = _old.tile;
            _new.tile.piece = _new;
            // replace it in the pieces list
            pieces.Remove(pieces.Where(x => x.id == _new.id).First());
            pieces.Add(_new);
            // is the old one dead now?
        }

        public Piece get(string namedCoord)
        {
            Coord c = Coord.fromString(namedCoord);
            if (c == null)
            {
                return null;
            }
            Tile t = getTile(c.row, c.col);
            if (t.piece != null)
            {
                return t.piece;
            }
            return null;
        }

        public void clear()
        {
            foreach (Piece p in pieces)
            {
                if (p.tile != null)
                {
                    p.tile.piece = null;
                    p.tile = null;
                }
            }
        }

        public bool set (string command)
        {
            bool success = false;
            command = command.ToUpper();
            List<string> commandParts = command.Split().ToList();
            Piece p = null;
            Color c = Color.Black;
            Type t = null;
            switch (commandParts[0])
            {
                case "WHITE":
                    c = Color.White;
                    break;
                case "BLACK":
                    c = Color.Black;
                    break;
                default:
                    return false;
            }
            switch (commandParts[1])
            {
                case "PAWN":
                    t = typeof(Pawn);
                    break;
                case "BISHOP":
                    t = typeof(Bishop);
                    break;
                case "KNIGHT":
                    t = typeof(Knight);
                    break;
                case "ROOK":
                    t = typeof(Rook);
                    break;
                case "QUEEN":
                    t = typeof(Queen);
                    break;
                case "KING":
                    t = typeof(King);
                    break;
                default:
                    return false;
            }
            Coord pos = Coord.fromString(commandParts[2]);
            if (pos == null)
            {
                return false;
            }
            Tile tile = getTile(pos.row, pos.col);
            if (tile == null || tile.piece != null)
            {
                return false;
            }
            p = pieces.Where(x => x.tile == null).First(x => x.color == c && x.GetType() == t);
            p.tile = tile;
            p.tile.piece = p;
            return success;
        }

        public List<Move> availableMoves(Color playerColor)
        {
            var result = new List<Move>();
            foreach (Piece p in pieces)
            {
                if (p.color == playerColor && p.tile != null)
                {
                    foreach (Move m in (p.filterOutCheckMoves(p.moves(this), this, p.color)))
                    {
                        result.Add(m);
                    }
                    
                }
            }
            return result;
        }

        public Board clone()
        {
            var b = new Board();

            // wipe all the pieces
            b.pieces = new List<Piece>();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    b.getTile(row, col).piece = null;
                }
            }
            
            // replace them with p1 clones
            foreach (Piece p1 in pieces)
            {
                Piece p2 = null;
                if (p1.GetType() == typeof(Pawn))
                {
                    p2 = new Pawn(p1.color, p1.id);
                    if (((Pawn)p1).justLeaped)
                    {
                        ((Pawn)p2).justLeaped = true;
                    }
                } else if (p1.GetType() == typeof(Queen))
                {
                    p2 = new Queen(p1.color, p1.id);
                } else if (p1.GetType() == typeof(Knight))
                {
                    p2 = new Knight(p1.color, p1.id);
                } else if (p1.GetType() == typeof(Bishop))
                {
                    p2 = new Bishop(p1.color, p1.id);
                } else if (p1.GetType() == typeof(Rook))
                {
                    p2 = new Rook(p1.color, p1.id);
                    if (((Rook)p1).hasMoved)
                    {
                        ((Rook)p2).hasMoved = true;
                    }
                } else
                {
                    p2 = new King(p1.color, p1.id);
                    if (((King)p1).hasMoved)
                    {
                        ((King)p2).hasMoved = true;
                    }
                }
                if (p1.tile == null)
                {
                    p2.tile = null;
                } else
                {
                    p2.tile = b.getTile(p1.tile.pos.row, p1.tile.pos.col);
                    p2.tile.piece = p2;
                }
                b.pieces.Add(p2);
            }
            b.nextToMove = nextToMove;
            // is this safe?
            //b.threefold = threefold;
            // turns out it was not.....!
            foreach (string s in threefold)
            {
                b.threefold.Add(s);
            }
            b.enforceTurns = enforceTurns;
            return b;
        }

        public bool wouldThisMoveResultInMeBeingInCheck(Color playerColor, Move m)
        {
            Board b = clone();
            int moveAgentId = m.agent.id;
            int moveDestX = m.to.pos.row;
            int moveDestY = m.to.pos.col;
            int? moveRemoveMe = m.removeMe?.id;
            Piece newAgent = null;
            Piece newRemoveMe = null;
            Tile newTile = b.getTile(moveDestX, moveDestY);
            foreach (Piece p in b.pieces)
            {
                if (p.id == moveAgentId)
                {
                    newAgent = p;
                }
                else if (moveRemoveMe != null && moveRemoveMe == p.id)
                {
                    newRemoveMe = p;
                }
            }
            var newMove = new Move(newAgent, newAgent.tile, newTile, m.special, newRemoveMe);
            // let b handle the move (for specials & whatnot)?
            // would that put us in a recursive loop?
            newAgent.commitMove(newMove, b);
            return b.amIInCheck(playerColor);
        }

        public string checkTile(Coord tile)
        {
            if (getTile(tile.row, tile.col).piece == null)
            {
                return "";
            }
            return getTile(tile.row, tile.col).piece.longRepresentation();
        }

        public string checkTile(string input)
        {
            return checkTile(Coord.fromString(input));
        }

        public void removePiece(Coord pos)
        {
            Tile t = getTile(pos.row, pos.col);
            Piece p = t.piece;
            if (p != null)
            {
                p.tile = null;
                t.piece = null;
            }
        }

        public bool amIInCheck(Color playerColor)
        {
            checkingMoves = true;
            Piece king = null;
            foreach (Piece p in pieces)
            {
                if (p.color == playerColor && p.GetType() == typeof(King))
                {
                    king = p;
                    break;
                }
            }
            bool kingAttacked = false;
            foreach (Piece p in pieces.Where(x => x.tile != null && x.color != playerColor))
            {
                foreach (Move m in p.moves(this))
                {
                    if (m.removeMe == king)
                    {
                        kingAttacked = true;
                        break; // one is enough
                    }
                }
            }
            checkingMoves = false;
            return kingAttacked;
        }

        public Tile getTile(int row, int col)
        {
            if (row >= 0 && row <= 7 && col >= 0 && col <= 7)
            {
                return board[row][col];
            }
            return null;
        }

        public Tile getTile(Coord c)
        {
            if (c.row >= 0 && c.row <= 7 && c.col >= 0 && c.col <= 7)
            {
                return board[c.row][c.col];
            }
            return null;
        }
        public void display()
        {
            string output = "";
            int newMoveToIndex = -1;
            int newMoveFromIndex = -1;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Tile t = board[row][col];
                    if (t.piece == null)
                    {
                        output += ".";
                        if (history.Count > 0)
                        {
                            var mostRecent = history[history.Count - 1];
                            if (mostRecent.from.pos.row == row && mostRecent.from.pos.col == col)
                            {
                                newMoveFromIndex = output.Length - 1;
                            }
                        }
                    }
                    else
                    {
                        if (history.Count > 0)
                        {
                            var mostRecent = history[history.Count - 1];
                            if (mostRecent.agent.id == t.piece.id)
                            {
                                newMoveToIndex = output.Length;
                            }
                            // is there a case where the from block is still occupied? like, castling or something? not sure
                            if (mostRecent.from.pos.row == row && mostRecent.from.pos.col == col)
                            {
                                newMoveFromIndex = output.Length - 1;
                            }
                        }
                        output += (t.piece.color == Color.Black) ? t.piece.representation.ToString().ToUpper() : t.piece.representation.ToString();
                    }
                }
                output += "\n";
            }
            debugInterestingAction = false;
            if (history.Count > 0)
            {
                var mostRecent = history[history.Count - 1];
                output += mostRecent.ToString() + "\n";
            }            


            // this all can be DRYed I expect
            // Display output
            if (newMoveToIndex == -1 || newMoveFromIndex == -1)
            {
                Console.WriteLine(output);
            } else
            {
                if (newMoveFromIndex < newMoveToIndex)
                {
                    Console.Write(output.Substring(0, newMoveFromIndex));
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.Write(output[newMoveFromIndex]);
                    Console.ResetColor();
                    Console.Write(output.Substring(newMoveFromIndex + 1, (newMoveToIndex - newMoveFromIndex) - 1));
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(output[newMoveToIndex]);
                    Console.ResetColor();
                    Console.WriteLine(output.Substring(newMoveToIndex + 1));
                } else
                {
                    Console.Write(output.Substring(0, newMoveToIndex));
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(output[newMoveToIndex]);
                    Console.ResetColor();
                    Console.Write(output.Substring(newMoveToIndex + 1, (newMoveFromIndex - newMoveToIndex) - 1));
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.Write(output[newMoveFromIndex]);
                    Console.ResetColor();
                    Console.WriteLine(output.Substring(newMoveFromIndex + 1));
                }                
            }            
        }

        public bool checkmated(Color playerColor)
        {
            if (amIInCheck(playerColor) && availableMoves(playerColor).Empty())
            {
                return true;
            }
            return false;
        }

        public string formatThreefold()
        {
            string output = "";
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Tile t = getTile(x, y);
                    if (t.piece == null)
                    {
                        output += "-";
                    }
                    else
                    {
                        // i can optimize this with ascii math later
                        output += (t.piece.color == Color.Black) ? (char)t.piece.representation.ToString().ToUpper()[0] : t.piece.representation;                        
                    } 
                    
                }
            }
            return output;
        }

        public bool isThreefoldRepetition()
        {
            string current = formatThreefold();
            int already = 0;
            foreach (string s in threefold)
            {
                if (s == current)
                {
                    already += 1;
                }
            }
            if (already >= 2)
            {
                return true;
            }
            threefold.Add(current);
            return false;
        }

        public void afterTurn()
        {
            // checkmate, stalemate checks
            if (nextToMove == Color.Black)
            {
                if (checkmated(Color.Black))
                {
                    state = GameState.WhiteVictory;
                }
                else if (availableMoves(Color.Black).Empty())
                {
                    state = GameState.DrawStalemate;
                }
            }
            else
            {
                if (checkmated(Color.White))
                {
                    state = GameState.BlackVictory;
                }
                else if (availableMoves(Color.White).Empty())
                {
                    state = GameState.DrawStalemate;
                }
            }

            // see if draw by insufficient material
            // king v king, king v king+bishop, king v king + knight, king + bishop v king + bishop with both bishops on same color square
            var activePieces = pieces.Where(x => x.tile != null);
            int numPieces = activePieces.Count();
            if (numPieces < 5) // king, bishop vs king, bishop (same color) is the smallest insufficient material draw
            {
                if (numPieces == 2) // king v king
                {
                    state = GameState.DrawInsufficientMaterial;
                } else if (numPieces == 3)
                {
                    var nonKing = activePieces.Where(x => x.GetType() != typeof(King)).First();
                    if (nonKing.GetType() == typeof(Bishop) || nonKing.GetType() == typeof(Knight))
                    {
                        state = GameState.DrawInsufficientMaterial;
                    }
                } else if (numPieces == 4)
                {
                    var nonKings = activePieces.Where(x => x.GetType() != typeof(King)).ToArray();
                    var nonKing1 = nonKings[0];
                    var nonKing2 = nonKings[1];
                    if (nonKing1.color != nonKing2.color &&
                        nonKing1.GetType() == typeof(Bishop) &&
                        nonKing2.GetType() == typeof(Bishop) &&
                        (nonKing1.tile.pos.row * 8 + nonKing1.tile.pos.col) % 2 == (nonKing2.tile.pos.row * 8 + nonKing2.tile.pos.col) % 2)
                    {
                        state = GameState.DrawInsufficientMaterial;
                    }
                }
            }
            

            // see if draw by threefold repetition
            if (isThreefoldRepetition())
            {
                state = GameState.DrawThreefold;
            }

            // see if draw by fifty turn rule
            if (history.Count > 0 && history[history.Count - 1].removeMe == null && history[history.Count - 1].agent.GetType() != typeof(Pawn))
            {
                fiftyTurnCounter += 1;
            }
            else
            {
                // Should this be set to 0?
                fiftyTurnCounter = 1;
            }
            if (fiftyTurnCounter == 50)
            {
                state = GameState.DrawFiftyTurns;
            }

            // Expire justLeaped on applicable pawns
            if (history.Count > 1 && history[history.Count - 2].agent.GetType() == typeof(Pawn))
            {
                ((Pawn)history[history.Count - 2].agent).justLeaped = false;
            }
        }
    }

    enum ObserveStyle
    {
        Step,
        ScrubToInteresting
    }
    class Engine
    {
        public Agents.Player one, two, nextToMove;
        public Board b;
        public Random rng;
        public bool observeDisplay;
        public bool observeComments;
        public ObserveStyle style;
        public Engine(Agents.Player _one, Agents.Player _two, Random _rng = null)
        {
            observeDisplay = false;
            observeComments = true;
            style = ObserveStyle.ScrubToInteresting;
            one = _one;
            two = _two;
            // Prefer to pass in persistent random
            if (_rng != null)
            {
                rng = _rng;
            } else
            {
                rng = new Random();
            }
        }

        private void randomizeColors()
        {
            one.playerColor = Color.White;
            two.playerColor = Color.Black;
            nextToMove = one;
            if (rng.Next(0, 2) == 0)
            {
                one.playerColor = Color.Black;
                two.playerColor = Color.White;
                nextToMove = two;
            }
        }

        private void changeNextToMove()
        {
            if (nextToMove == one)
            {
                nextToMove = two;
            } else
            {
                nextToMove = one;
            }
        }

        private string finish()
        {
            one.finish();
            two.finish();
            return b.state.ToString();
        }

        private void considerPausing()
        {
            if (style == ObserveStyle.Step)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Spacebar)
                {
                    style = ObserveStyle.ScrubToInteresting;
                }
            }
            else if (style == ObserveStyle.ScrubToInteresting && b.debugInterestingAction == true)
            {
                style = ObserveStyle.Step;
                Console.ReadKey();
            }
        }

        private void display()
        {
            if (observeDisplay)
            {
                Console.Clear();
                b.display();
                considerPausing();
            }
        }

        public string play()
        {
            b = new Board();
            one.startup();
            two.startup();
            one.setBoard(b);
            two.setBoard(b);
            randomizeColors();

            while (b.state == GameState.Active)
            {
                var moveChoice = nextToMove.chooseMove(b.availableMoves(nextToMove.playerColor));
                if (b.move(moveChoice.Item1, moveChoice.Item2, moveChoice.Item3) == false)
                {
                    return "Move failed!";
                }
                display();
                changeNextToMove();
            }
            return finish();
        }
    }
}