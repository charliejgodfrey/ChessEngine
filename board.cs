//this is the file where all the code for the board representation is going to be managed from
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace ChessEngine 
{
    public class Board
    { 
        public const string DefaultFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"; //this is the default chess starting position

        public int ColourToMove = 0; // 0 for white 1 for black
        public int EnPassantSquare;
        public int PreviousEnPassantSquare;
        public int MoveNumber;
        public bool WhiteShortCastle = false;
        public bool WhiteLongCastle = false;
        public bool BlackShortCastle = false;
        public bool BlackLongCastle = false;
        public int[] PieceCount = new int[12];
        public float Eval = 0;
        public ZobristHasher Hasher = new ZobristHasher();
        public ulong Zobrist;
        public float GamePhase = 7800;
        public Stack<Move> History = new Stack<Move>();



        //bitboards

        public ulong BlackPieces = 0UL;
        public ulong WhitePieces = 0UL;
        public ulong OccupiedSquares = 0UL;
        public ulong[] Pieces = new ulong[12];

        public Board(string Fen = DefaultFEN, bool isCopy = false) 
        {
            if (!isCopy) 
            {
            this.UploadFEN(Fen);
            BlackPieces = Pieces[6] | Pieces[7] | Pieces[8] | Pieces[9] | Pieces[10] | Pieces[11];
            WhitePieces = Pieces[0] | Pieces[1] | Pieces[2] | Pieces[3] | Pieces[4]| Pieces[5];
            OccupiedSquares = BlackPieces | WhitePieces;
            //Pieces = [WhitePawns, WhiteKnights, WhiteBishops, WhiteRooks, WhiteQueens, WhiteKing, BlackPawns, BlackKnights, BlackBishops, BlackRooks, BlackQueens, BlackKing];
            Zobrist = Hasher.Hash(this);
            Eval = Evaluation.WeightedMaterial(this);
            }
        }

        public int GetPiece(int square, int Colour) //7 indicates that it isn't a capture
        {
            ulong mask = (1UL << square);
            if (Colour == 0) //white piece on the location
            {
                if ((Pieces[0] & mask) != 0) return 0;
                if ((Pieces[1] & mask) != 0) return 1;
                if ((Pieces[2] & mask) != 0) return 2;
                if ((Pieces[3] & mask) != 0) return 3;
                if ((Pieces[4] & mask) != 0) return 4;
                if ((Pieces[5] & mask) != 0) return 5;
                else return 7;
            }
            if (Colour == 1) //black piece on the location
            {
                if ((Pieces[6] & mask) != 0) return 0;
                if ((Pieces[7] & mask) != 0) return 1;
                if ((Pieces[8] & mask) != 0) return 2;
                if ((Pieces[9] & mask) != 0) return 3;
                if ((Pieces[10] & mask) != 0) return 4;
                if ((Pieces[11] & mask) != 0) return 5;
                else return 7;
            }
            return 7;
        }

        public void MakeEmpty()
        {
            MoveNumber++;
            ColourToMove = (ColourToMove == 0 ? 1 : 0);
            PreviousEnPassantSquare = EnPassantSquare;
            EnPassantSquare = -1;
            Zobrist ^= Hasher.SideToMove;
        }

        public void UnmakeEmpty()
        {
            MoveNumber--;
            ColourToMove = (ColourToMove == 0 ? 1 : 0);
            EnPassantSquare = PreviousEnPassantSquare;
            PreviousEnPassantSquare = -1;
            Zobrist ^= Hasher.SideToMove;
        }

        public void MakeMove(Move move) 
        {
            MoveNumber++;
            //History.Push(move);
            if (move.GetCapture() == 5)
            {
                ColourToMove = (ColourToMove == 0 ? 1 : 0);
                Console.WriteLine(MoveGenerator.InCheck(this, ColourToMove));
                ColourToMove = (ColourToMove == 0 ? 1 : 0);

                move.PrintMove();
                PrintBoard();
                //OccupiedSquares.PrintData();
                for (int i = 0; i < History.Count(); i++) {
                    //History.Pop().PrintMove();
                }
            }

            // if ((move.GetFlag() & 0b1110) == 0b0010) //a castling move
            // {
            //     this.Castle((move.GetFlag() == 0b0010) ? 1 : 2);
            //     UpdateEval(move);
            //     UpdateZobrist(move);    
            //     ColourToMove = (ColourToMove == 0 ? 1 : 0);
            //     return;
            // }
            int start = move.GetStart();
            int target = move.GetTarget();
            int piece = move.GetPiece();
            int flag = move.GetFlag();
            int capture = move.GetCapture();
            OccupiedSquares &= ~(1UL << start); //adjusts the occupied bitboard
            OccupiedSquares |= 1UL << target;

            //adjusted colour specific occupancy bitboards
            if (ColourToMove == 0) //white moving
            {
                WhitePieces &= ~(1UL << start); //clear the start square
                WhitePieces |= (1UL << target); //target square is now occupied for white
                //for piece specific bitboards
                Pieces[piece] &= ~(1UL << start);
                Pieces[piece] |= (1UL << target);
                //taking into account captures
                if (capture != 0b111) 
                {
                    Pieces[capture + 6] &= ~(1UL << target);
                    BlackPieces &= ~(1UL << target); //any piece on the target square is captured
                }
                //for (int i = 6; i<12;i++) Pieces[i].ClearBit(target);
            } else { //black moving
                BlackPieces &= ~(1UL << start);
                BlackPieces |= (1UL << target);
                //for the piece specific bitboards
                Pieces[piece + 6] &= ~(1UL << start); //the +6 is for offsetting the piece to the index for the black pieces
                Pieces[piece + 6] |= (1UL << target);
                //taking into account captures
                if (capture != 0b111)
                { 
                    Pieces[capture] &= ~(1UL << target);
                    WhitePieces &= ~(1UL << target);
                }
                //for (int i = 0; i<6;i++) Pieces[i].ClearBit(target);
            }

            //other checks:
            if (flag == 0b0001) //double pawn push
            {
                //PreviousEnPassantSquare = EnPassantSquare;
                EnPassantSquare = target + (ColourToMove == 0 ? -8 : 8); //the offset means that the square is referring to where an enpassanting pawn would move to - not where the piece is being captured
            } 
            else EnPassantSquare = -1;
            if (flag == 0b0101) //en passant capture
            {
                if (ColourToMove == 0) {
                    Pieces[6] &= ~(1UL << (target - 8));
                    BlackPieces &= ~(1UL << (target - 8));
                    OccupiedSquares &= ~(1UL << (target + 8));
                }
                else {
                    Pieces[0] &= ~(1UL << (target + 8));
                    WhitePieces &= ~(1UL << (target + 8));
                    OccupiedSquares &= ~(1UL << (target + 8));
                }
                EnPassantSquare = -1;
            }
            if (flag >= 0b1000) //promotion
            {
                Pieces[(flag & 0b0011) + (ColourToMove == 0 ? 1 : 7)] |= 1UL << target; //promotion piece bitboard
                Pieces[(ColourToMove == 0 ? 0 : 6)] &= ~(1UL << target); //pawn bitboard
                Pieces[(ColourToMove == 0 ? 0 : 6)] &= ~(1UL << start);
            }
            //UpdateEval(move);
            UpdateZobrist(move);
            ColourToMove = (ColourToMove == 0 ? 1 : 0);
        }

        public void UnmakeMove(Move move) //add castling, en passant
        {
            MoveNumber--;
            //History.Pop();
            int start = move.GetStart();
            int target = move.GetTarget();
            int piece = move.GetPiece();
            int flag = move.GetFlag();
            int capture = move.GetCapture();
            // if ((move.GetFlag() & 0b1110) == 0b0010) //a castling move
            // {
            //     this.Uncastle((move.GetFlag() == 0b0010) ? 1 : 2);
            //     UpdateEval(move);
            //     UpdateZobrist(move);
            //     ColourToMove = (ColourToMove == 0 ? 1 : 0); //because the zobrist is made from scratch including a move change    
            //     return;
            // }
            if (ColourToMove == 1) //white unmaking the unmove
            {
                WhitePieces |= (1UL << start);
                WhitePieces &= ~(1UL << target);
                OccupiedSquares |= (1UL << start);
                Pieces[piece] &= ~(1UL << target); //empty where the piece just moved back from
                if (capture != 0b111) // if it was a capturing move
                {
                    Pieces[capture + 6] |= (1UL << target);
                    BlackPieces |= (1UL << target);
                    OccupiedSquares |= (1UL << target);
                } else{
                    OccupiedSquares &= ~(1UL << target);
                }
                if (flag >= 0b1000) //promotion
                {
                    Pieces[0] |= (1UL << start); //move the piece back but into a pawn
                    Pieces[(flag & 0b0011) + 1] &= ~(1UL << target);
                } else {
                    Pieces[piece] |= (1UL << start);
                }
                if (flag == 0b0101) { //en passant
                    Pieces[6] |= (1UL << (target - 8));
                    BlackPieces |= (1UL << (target - 8));
                    OccupiedSquares |= (1UL << (target - 8));
                    EnPassantSquare = target - 8;
                }
            } else {
                BlackPieces |= (1UL << start);
                BlackPieces &= ~(1UL << target);
                OccupiedSquares |= (1UL << start);
                Pieces[piece + 6] &= ~(1UL << target); //empty where the piece just moved back from
                if (capture != 0b111) // if it was a capturing move
                {
                    Pieces[capture] |= (1UL << target);
                    WhitePieces |= (1UL << target);
                    OccupiedSquares |= (1UL << target);
                } else{
                    OccupiedSquares &= ~(1UL << target);
                }
                if (flag >= 0b1000) //promotion
                {
                    Pieces[6] |= (1UL << start); //move the piece back
                    Pieces[(flag & 0b0011) + 7] &= ~(1UL << target);
                } else {
                    Pieces[piece+6] |= (1UL << start);
                }
                if (flag == 0b0101) { //en passant
                    Pieces[0] |= (1UL << (target + 8));
                    WhitePieces |= (1UL << (target + 8));
                    OccupiedSquares |= (1UL << (target + 8));
                    EnPassantSquare = (target + 8);
                }
            }
            if (flag == 0b0001) { //double pawn push
                EnPassantSquare = -1;
            }
            //UnupdateEval(move);
            if (flag == 0 || flag == 0b0100 || flag == 0b0001) {
                ColourToMove = (ColourToMove == 0 ? 1 : 0);
                UpdateZobrist(move);
            } else {
                UpdateZobrist(move);
                ColourToMove = (ColourToMove == 0 ? 1 : 0);
            }
        }

        public void UpdateEval(Move move)
        {
            if (move.GetFlag() == 0b0101 || move.GetFlag() >= 0b1000 || move.GetFlag() == 0b0010 || move.GetFlag() == 0b0011)
            {
                Eval = Evaluation.WeightedMaterial(this);
                return;
            }
            int piece = move.GetPiece();
            int target = move.GetTarget();
            int start = move.GetStart();
            int capture = move.GetCapture();
            GamePhase -= Evaluation.MaterialValues[capture];
            if (ColourToMove == 0)
            {
                Eval += Evaluation.PieceSquareTable[piece][Evaluation.Flip[target]];
                Eval -= Evaluation.PieceSquareTable[piece][Evaluation.Flip[start]];
                if (capture != 0b111) //it is a capture
                {
                    Eval += Evaluation.MaterialValues[capture];
                    Eval += Evaluation.PieceSquareTable[capture][target];
                }
            }
            else if (ColourToMove == 1)
            {
                Eval -= Evaluation.PieceSquareTable[piece][target]; //the funky stuff is for reversing the piece square table to be from the black perspective
                Eval += Evaluation.PieceSquareTable[piece][start];
                if (capture != 0b111)
                {
                    Eval -= Evaluation.MaterialValues[capture];
                    Eval -= Evaluation.PieceSquareTable[capture][Evaluation.Flip[target]];
                }
            }
        }

        public void UnupdateEval(Move move)
        {
            if (move.GetFlag() == 0b0101 || move.GetFlag() >= 0b1000)
            {
                Eval = Evaluation.WeightedMaterial(this);
                return;
            }
            int piece = move.GetPiece();
            int target = move.GetTarget();
            int start = move.GetStart();
            int capture = move.GetCapture();
            GamePhase += Evaluation.MaterialValues[capture];


            if (ColourToMove == 1) //white is unmaking the move
            {
                Eval -= Evaluation.PieceSquareTable[piece][Evaluation.Flip[target]];
                Eval += Evaluation.PieceSquareTable[piece][Evaluation.Flip[start]];
                if (capture != 0b111) //is an uncapturing unmove
                {
                    Eval -= Evaluation.MaterialValues[capture];
                    Eval -= Evaluation.PieceSquareTable[capture][target];
                }
            } else { //black unmaking the move
                Eval += Evaluation.PieceSquareTable[piece][target];
                Eval -= Evaluation.PieceSquareTable[piece][start];
                if (capture != 0b111) //is an uncapturing unmove
                {
                    Eval += Evaluation.MaterialValues[capture];
                    Eval += Evaluation.PieceSquareTable[capture][Evaluation.Flip[target]];
                }
            }
        }

        public void UpdateZobrist(Move move) //this works for making and unmaking moves 
        {
            //Console.WriteLine("prior zobby: " + Zobrist);
            int colourAdd = (ColourToMove == 0 ? 0 : 6); // determines adding constant if it is black moving
            //if (unmove) colourAdd = (colourAdd == 0 ? 6 : 0);
            if (move.GetFlag() == 0b0101 || move.GetFlag() >= 0b1000 || move.GetFlag() == 0b0010 || move.GetFlag() == 0b0011)
            {
                Zobrist = Hasher.Hash(this);
                Zobrist ^= Hasher.SideToMove; //this is because UpdateZobrist is called before ColourToMove is updated
                // Console.WriteLine("manual update zobrist: " + Zobrist);
                // Console.WriteLine("correct zobrist: " + Hasher.Hash(this));
                return;
            }
            Zobrist ^= Hasher.ZobristTable[move.GetPiece()+colourAdd][move.GetStart()];
            Zobrist ^= Hasher.ZobristTable[move.GetPiece()+colourAdd][move.GetTarget()];
            if (move.GetCapture() != 0b111) //it is a capture
            {
                Zobrist ^= Hasher.ZobristTable[move.GetCapture() + (ColourToMove == 0 ? 6 : 0)][move.GetTarget()];
            }
            Zobrist ^= Hasher.SideToMove;
        }

        // public void Uncastle(int type)
        // {
        //     if (type == 1 && this.ColourToMove == 1) //short castle for white
        //     {
        //         this.WhiteKing.SetData(0x10UL); //king back in position
        //         this.OccupiedSquares.AND(~0x60UL); //clear the king and rook
        //         this.OccupiedSquares.OR(0x90UL); //puts king and rook back
        //         this.WhitePieces.AND(~0x60UL); //clear the king and rook
        //         this.WhitePieces.OR(0x90UL);
        //         this.WhiteRooks.ClearBit(5); //clear rook
        //         this.WhiteRooks.SetBit(7); //replace rook
        //         this.WhiteLongCastle = true;
        //         this.WhiteShortCastle = true;
        //         return;
        //     }
        //     if (type == 2 && this.ColourToMove == 1) //long castle for white
        //     {
        //         this.WhiteKing.SetData(0x10UL); //resets king position
        //         this.OccupiedSquares.AND(~0x6UL); //clears the king and rook
        //         this.OccupiedSquares.OR(0x11UL);
        //         this.WhitePieces.AND(~0x6UL); //clears the king and rook
        //         this.WhitePieces.OR(0x11UL);
        //         this.WhiteRooks.ClearBit(3); //clear rook
        //         this.WhiteRooks.SetBit(0);// replace rook
        //         this.WhiteLongCastle = true;
        //         this.WhiteShortCastle = true;
        //         return;
        //     }
        //     if (type == 1 && this.ColourToMove == 0) //short castle for black
        //     {
        //         this.BlackKing.SetData(0x1000000000000000UL); //resets king position
        //         this.OccupiedSquares.AND(~0x6000000000000000UL); //clears rook and king
        //         this.OccupiedSquares.OR(0x9000000000000000UL);
        //         this.BlackPieces.AND(~0x6000000000000000UL); //clears rook and king
        //         this.BlackPieces.OR(0x9000000000000000UL);
        //         this.BlackRooks.ClearBit(61);
        //         this.BlackRooks.SetBit(63);
        //         this.BlackShortCastle = true;
        //         this.BlackLongCastle = true;
        //         return;
        //     }
        //     if (type == 2 && this.ColourToMove == 0) //long castle for black
        //     {
        //         //Console.WriteLine("un long castled for black");
        //         this.BlackKing.SetData(0x1000000000000000UL); //resets king position
        //         this.OccupiedSquares.AND(~0x0C00000000000000UL); //clears rook and king
        //         this.OccupiedSquares.OR(0x1100000000000000UL);
        //         this.BlackPieces.AND(~0x0C00000000000000UL); //clears rook and king
        //         this.BlackPieces.OR(0x1100000000000000UL);
        //         this.BlackRooks.ClearBit(59); //clears rook from castled position
        //         this.BlackRooks.SetBit(56); //puts rook back in corner
        //         this.BlackShortCastle = true;
        //         this.BlackLongCastle = true;
        //         return;
        //     }
        // }

        // public void Castle(int type)
        // {
        //     if (type == 1 && this.ColourToMove == 0) //short castle for white
        //     {
        //         this.WhiteKing.SetData(0x40UL);
        //         this.OccupiedSquares.AND(~0x90UL); 
        //         this.OccupiedSquares.OR(0x60UL);
        //         this.WhitePieces.AND(~0x90UL);
        //         this.WhitePieces.OR(0x60UL);
        //         this.WhiteRooks.ClearBit(7);
        //         this.WhiteRooks.SetBit(5);
        //         this.WhiteLongCastle = false;
        //         this.WhiteShortCastle = false;
        //         return;
        //     }
        //     if (type == 2 && this.ColourToMove == 0) //long castle for white
        //     {
        //         this.WhiteKing.SetData(0x4UL);
        //         this.OccupiedSquares.AND(~0x11UL);
        //         this.OccupiedSquares.OR(0xCUL);
        //         this.WhitePieces.AND(~0x11UL);
        //         this.WhitePieces.OR(0xCUL);
        //         this.WhiteRooks.ClearBit(0);
        //         this.WhiteRooks.SetBit(3);
        //         this.WhiteLongCastle = false;
        //         this.WhiteShortCastle = false;
        //         return;
        //     }
        //     if (type == 1 && this.ColourToMove == 1) //short castle for black
        //     {
        //         this.BlackKing.SetData(0x4000000000000000UL);
        //         this.OccupiedSquares.AND(~0x9000000000000000UL);
        //         this.OccupiedSquares.OR(0x6000000000000000UL);
        //         this.BlackPieces.AND(~0x9000000000000000UL);
        //         this.BlackPieces.OR(0x6000000000000000UL);
        //         this.BlackRooks.ClearBit(63);
        //         this.BlackRooks.SetBit(61);
        //         this.BlackShortCastle = false;
        //         this.BlackLongCastle = false;
        //         return;
        //     }
        //     if (type == 2 && this.ColourToMove == 1) //long castle for black
        //     {
        //         this.BlackKing.SetData(0x400000000000000UL);
        //         this.OccupiedSquares.AND(~0x1100000000000000UL);
        //         this.OccupiedSquares.OR(0x0C00000000000000UL);
        //         this.BlackPieces.AND(~0x1100000000000000UL);
        //         this.BlackPieces.OR(0x0C00000000000000UL);
        //         this.BlackRooks.ClearBit(56);
        //         this.BlackRooks.SetBit(59);
        //         this.BlackShortCastle = false;
        //         this.BlackLongCastle = false;
        //         return;
        //     }
        // }

        public void RefreshBitboardConfiguration()
        {
            BlackPieces = Pieces[6] | Pieces[7] | Pieces[8] | Pieces[9] | Pieces[10] | Pieces[11];
            WhitePieces = Pieces[0] | Pieces[1] | Pieces[2] | Pieces[3] | Pieces[4]| Pieces[5];
            OccupiedSquares = BlackPieces | WhitePieces;
            //Pieces = [WhitePawns, WhiteKnights, WhiteBishops, WhiteRooks, WhiteQueens, WhiteKing, BlackPawns, BlackKnights, BlackBishops, BlackRooks, BlackQueens, BlackKing];
        }

        public Board Copy()
        {
            Board board = new Board(DefaultFEN, true);
            board.Hasher = this.Hasher;
            board.Zobrist = this.Zobrist;
            board.ColourToMove = this.ColourToMove;
            board.EnPassantSquare = this.EnPassantSquare;
            board.MoveNumber = this.MoveNumber;
            board.WhiteShortCastle = this.WhiteShortCastle;
            board.WhiteLongCastle = this.WhiteLongCastle;
            board.BlackShortCastle = this.BlackShortCastle;
            board.BlackLongCastle = this.BlackLongCastle;
            board.Eval = this.Eval;
            board.GamePhase = this.GamePhase;
            board.History = this.History;

            for (int i = 0; i < 12; i++)
            {
                board.Pieces[i] = this.Pieces[i];
            }
            
            board.RefreshBitboardConfiguration();

            return board;
        }

        public void UploadFEN(string FEN)
        {
            int currentSquare = 56;
            foreach (char character in FEN)
            {
                if (character == '/') //new rank
                {
                    currentSquare -= 17;
                }
                if (char.IsDigit(character)) //is it a number?
                {
                    currentSquare += character - '0'; // turns character into a number
                }
                else 
                {
                    switch (character) //lowercase for black, uppercase for white
                    {
                        case 'p':
                            PieceCount[6]++;
                            Pieces[6] |= (1UL << currentSquare);
                            break;
                        case 'n':
                            PieceCount[7]++;
                            Pieces[7] |= (1UL << currentSquare);
                            break;
                        case 'b':
                            PieceCount[8]++;
                            Pieces[8] |= (1UL << currentSquare);
                            break;
                        case 'r':
                            PieceCount[9]++;
                            Pieces[9] |= (1UL << currentSquare);
                            break;
                        case 'q':
                            PieceCount[10]++;
                            Pieces[10] |= (1UL << currentSquare);
                            break;
                        case 'k':
                            Pieces[11] |= (1UL << currentSquare);
                            break;
                        case 'P':
                            PieceCount[0]++;
                            Pieces[0] |= (1UL << currentSquare);
                            break;
                        case 'N':
                            PieceCount[1]++;
                            Pieces[1] |= (1UL << currentSquare);
                            break;
                        case 'B':
                            PieceCount[2]++;
                            Pieces[2] |= (1UL << currentSquare);
                            break;
                        case 'R':
                            PieceCount[3]++;
                            Pieces[3] |= (1UL << currentSquare);
                            break;
                        case 'Q':
                            PieceCount[4]++;
                            Pieces[4] |= (1UL << currentSquare);
                            break;
                        case 'K':
                            Pieces[5] |= (1UL << currentSquare);
                            break;
                        // case ' ':
                        //     ColourToMove = (FEN[currentSquare+1] == 'w' ? 0 : 1);
                        //     break;
                        default:
                            break;
                    }
                    currentSquare++;
                }
            }
            // if (!WhiteKing.IsBitSet(3) || !WhiteRooks.IsBitSet(0)) WhiteShortCastle = false;
            // if (!WhiteKing.IsBitSet(3) || !WhiteRooks.IsBitSet(7)) WhiteLongCastle = false;
            // if (!WhiteKing.IsBitSet(59) || !WhiteRooks.IsBitSet(56)) BlackShortCastle = false;
            // if (!WhiteKing.IsBitSet(59) || !WhiteRooks.IsBitSet(63)) BlackLongCastle = false;
        }

        public void PrintBoard()
        {
            Console.WriteLine("================");
            string BoardRepresentation = ""; //initial state to print
            for (int i = 0; i < 64; i++)
            {
                if ((Pieces[0] & (1UL << i))!=0) {BoardRepresentation += "P ";}
                else if ((Pieces[1] & (1UL << i))!=0) {BoardRepresentation += "N ";}
                else if ((Pieces[2] & (1UL << i))!=0) {BoardRepresentation += "B ";}
                else if ((Pieces[3] & (1UL << i))!=0) {BoardRepresentation += "R ";}
                else if ((Pieces[4] & (1UL << i))!=0) {BoardRepresentation += "Q ";}
                else if ((Pieces[5] & (1UL << i))!=0) {BoardRepresentation += "K ";}
                else if ((Pieces[6] & (1UL << i))!=0) {BoardRepresentation += "p ";}
                else if ((Pieces[7] & (1UL << i))!=0) {BoardRepresentation += "n ";}
                else if ((Pieces[8] & (1UL << i))!=0) {BoardRepresentation += "b ";}
                else if ((Pieces[9] & (1UL << i))!=0) {BoardRepresentation += "r ";}
                else if ((Pieces[10] & (1UL << i))!=0) {BoardRepresentation += "q ";}
                else if ((Pieces[11] & (1UL << i))!=0) {BoardRepresentation += "k ";}
                else {BoardRepresentation += "- ";}
            }
            Console.WriteLine(BoardRepresentation.Substring(112,16));
            Console.WriteLine(BoardRepresentation.Substring(96,16));
            Console.WriteLine(BoardRepresentation.Substring(80,16));
            Console.WriteLine(BoardRepresentation.Substring(64,16));
            Console.WriteLine(BoardRepresentation.Substring(48,16));
            Console.WriteLine(BoardRepresentation.Substring(32,16));
            Console.WriteLine(BoardRepresentation.Substring(16, 16));
            Console.WriteLine(BoardRepresentation.Substring(0,16));
            Console.WriteLine("================");
        }

        public void PrintBitboards()
        {
            Console.WriteLine("White Pawns:");
            (new Bitboard(Pieces[0])).PrintData();
            
            Console.WriteLine("White Knights:");
            (new Bitboard(Pieces[1])).PrintData();
            
            Console.WriteLine("White Bishops:");
            (new Bitboard(Pieces[2])).PrintData();
            
            Console.WriteLine("White Rooks:");
            (new Bitboard(Pieces[3])).PrintData();
            
            Console.WriteLine("White Queens:");
            (new Bitboard(Pieces[4])).PrintData();
            
            Console.WriteLine("White King:");
            (new Bitboard(Pieces[5])).PrintData();
            
            Console.WriteLine("Black Pawns:");
            (new Bitboard(Pieces[6])).PrintData();
            
            Console.WriteLine("Black Knights:");
            (new Bitboard(Pieces[7])).PrintData();
            
            Console.WriteLine("Black Bishops:");
            (new Bitboard(Pieces[8])).PrintData();
            
            Console.WriteLine("Black Rooks:");
            (new Bitboard(Pieces[9])).PrintData();
            
            Console.WriteLine("Black Queens:");
            (new Bitboard(Pieces[10])).PrintData();
            
            Console.WriteLine("Black King:");
            (new Bitboard(Pieces[11])).PrintData();
            
            Console.WriteLine("White Pieces:");
            (new Bitboard(WhitePieces)).PrintData();
            
            Console.WriteLine("Black Pieces:");
            (new Bitboard(BlackPieces)).PrintData();
            
            Console.WriteLine("Occupied Squares:");
            (new Bitboard(OccupiedSquares)).PrintData();
        }
    }
}