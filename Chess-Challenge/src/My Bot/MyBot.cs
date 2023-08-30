using ChessChallenge.API;
using ChessChallenge.Application;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

public class MyBot : IChessBot
{

    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    bool myColor;
    public Move Think(Board board, Timer timer)
    {
        
        myColor = board.IsWhiteToMove;
        int bestMoveFoundValue = -30000;
        Move[] allMoves = board.GetLegalMoves();


        
        Move moveToPlay = allMoves[0];


        foreach (Move move in allMoves){

            board.MakeMove(move);


            
            if (ValueOfThe(board) > bestMoveFoundValue){
                moveToPlay = move;
                bestMoveFoundValue = ValueOfThe(board);
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
            
            if (!checkmatePreventionFound && bestMoveFoundValue<20000){
                moveToPlay = move;
                bestMoveFoundValue = 19999;
            }


            board.UndoMove(move);

        }

        return moveToPlay;
    }
    private int ValueOfThe(Board inputBoard){
        bool isMyTurn = inputBoard.IsWhiteToMove == myColor;
        if (inputBoard.IsDraw()){
            return 0;
        }
        if (inputBoard.IsInCheckmate()){
            if (!isMyTurn){
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
            if (pieces.IsWhitePieceList != myColor){
                modifier = -1;
            }
            returnValue+= modifier * pieces.Count * pieceValues[(int)pieces.TypeOfPieceInList];
        }
        if (inputBoard.IsInCheck()){
            if (isMyTurn){
                returnValue-=1;
            }
            else {
                returnValue+=1;
            }
        }
        Square enemyKingSquare = inputBoard.GetKingSquare(!myColor);
        return returnValue;
    }
}