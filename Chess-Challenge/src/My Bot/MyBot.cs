using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{

    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    public Move Think(Board board, Timer timer)
    {
        int player;
        if (board.IsWhiteToMove){
            player = 'W';
        }
        else {
            player = 'B';
        }

        Move[] allMoves = board.GetLegalMoves();


        // Pick a random move to play if nothing better is found
        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        int highestValueCapture = 0;


        foreach (Move move in allMoves){

            bool isMate = false;
            
            board.MakeMove(move);
            isMate = board.IsInCheckmate();
            
            // Always play checkmate in one
            if (isMate)
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
    private static int ValueOfThe(Board board){
        return 0;
    }
}