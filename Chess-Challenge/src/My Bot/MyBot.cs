using ChessChallenge.API;
using ChessChallenge.Application;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

public class MyBot : IChessBot
{

    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    bool mySideIsWhite;
    public Move Think(Board board, Timer timer)
    {
        mySideIsWhite = board.IsWhiteToMove;

        Move[] allMoves = board.GetLegalMoves();


        // Pick a random move to play if nothing better is found
        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        int highestValueCapture = 0;


        foreach (Move move in allMoves){

            board.MakeMove(move);
            
            // Always play checkmate in one
            if (board.IsInCheckmate())
            {
                board.UndoMove(move);
                moveToPlay = move;
                break;
            }

            Move[] allOpponentMoves = board.GetLegalMoves();
            bool checkmatePreventionFound = true;
            foreach (Move opponentMove in allOpponentMoves){
                checkmatePreventionFound = true;
                board.MakeMove(opponentMove);
                Move[] allResponses = board.GetLegalMoves();
                foreach (Move response in allResponses){
                    board.MakeMove(response);
                    if (board.IsInCheckmate()){
                        checkmatePreventionFound = false;
                    }
                    board.UndoMove(response);
                }
                board.UndoMove(opponentMove);
                if (checkmatePreventionFound){
                    break;
                }  
            }
            
            if (!checkmatePreventionFound){
                board.UndoMove(move);
                moveToPlay = move;
                break;
            }

            board.UndoMove(move);



            // Find highest value capture
            Piece capturedPiece = board.GetPiece(move.TargetSquare);
            int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

            if (capturedPieceValue > highestValueCapture)
            {
                moveToPlay = move;
                highestValueCapture = capturedPieceValue;
            }
        }

        return moveToPlay;
    }
    private int ValueOfThe(Board inputBoard){
        if (inputBoard.IsDraw()){
            return 0;
        }
        if (inputBoard.IsInCheckmate()){
            if (inputBoard.IsWhiteToMove == mySideIsWhite){
                return 20000;
            }
            else {
                return -20000;
            }
        }
        int returnValue = 0;
        PieceList[] allPieces = inputBoard.GetAllPieceLists();
        foreach (PieceList pieces in allPieces){
            int modifier = 1;
            if (pieces.IsWhitePieceList != mySideIsWhite){
                modifier = -1;
            }
            returnValue+= modifier * pieces.Count * pieceValues[(int)pieces.TypeOfPieceInList];
        }
        return returnValue;
    }
}