using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.ML;
using Microsoft.ML.Data;
using ChessLogic;
using System.Reflection;

namespace ChessLogic
{

    public class ChessBot
    {
        public readonly ChessNeuralNetwork neuralNetwork;
        public ChessBot()
        {
            neuralNetwork = new ChessNeuralNetwork();
        }

        public float[] PredictMove(string stateString)
        {
            return neuralNetwork.PredictMove(stateString);
        }


        // Define the input schema for the data
        public class GameState
        {
            [ColumnName("BoardState"), Column("0")]
            public float[] BoardState;
        }

        // Define the output schema for the data
        public class MoveBot
        {
            [ColumnName("Move"), Column("0")]
            public float[] Move;
        }

        public NormalMove ConvertPredictedMoveToMove(float[] move)
        {
            // Extract the start and end positions from the predicted move
            Position startPosition = new Position((int)move[0], (int)move[1]);
            Position endPosition = new Position((int)move[2], (int)move[3]);

            // Create a new NormalMove object with the extracted start and end positions
            NormalMove predictedMove = new NormalMove(startPosition, endPosition);

            // Return the new NormalMove object
            return predictedMove;
        }

        public class ChessNeuralNetwork
        {
            private MLContext context;
            private ITransformer model;
            private int index; // Added index field

            public ChessNeuralNetwork()
            {
                // Create a new MLContext
                context = new MLContext();
            }

            public void Train(IEnumerable<GameState> gameStates, IEnumerable<Move> moves)
            {
                // Define the pipeline
                var pipeline = context.Transforms.Conversion.MapValueToKey("Label")
                    .Append(context.Transforms.NormalizeMinMax("Features"))
                    .Append(context.MulticlassClassification.Trainers.SdcaNonCalibrated())
                    .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                var dataView = context.Data.LoadFromEnumerable(gameStates);

                // Train the model
                model = pipeline.Fit(dataView);
            }
            public float[] PredictMove(string stateString)
            {
                // Convert the stateString into a board state
                float[] boardState = ConvertPosStringToBoardState(stateString);

                // Create a prediction engine
                var predictionEngine = context.Model.CreatePredictionEngine<GameState, MoveBot>(model);

                // Make a prediction
                var prediction = predictionEngine.Predict(new GameState { BoardState = boardState });

                // Return the predicted move
                return prediction.Move;
            }


            public float[] ConvertPosStringToBoardState(string posString)
            {
                float[] boardState = new float[65]; // Changed from -1 to 65
                index = 0; // Reset index for each conversion

                // Split the PosString into parts
                string[] parts = posString.Split(' ');

                // Extract board placement data
                string boardPart = parts[0];
                AddBoardPlacementToBoardState(boardPart, boardState);

                // Extract current player
                string currentPlayer = parts[1];
                boardState[64] = currentPlayer == "w" ? 1 : 2;

                // Extract castling rights
                string castlingRights = parts[2];
                AddCastlingRightsToBoardState(castlingRights, boardState);

                // Extract en passant data
                string enPassantData = parts[3];
                AddEnPassantDataToBoardState(enPassantData, boardState);

                return boardState;
            }

            private void AddBoardPlacementToBoardState(string boardPart, float[] boardState)
            {
                // Initialize the board state index
                int index = 0;

                // Split the board part into ranks
                string[] ranks = boardPart.Split('/');

                foreach (var rank in ranks)
                {
                    foreach (char square in rank)
                    {
                        if (char.IsDigit(square))
                        {
                            // If the character is a digit, it represents empty squares
                            int emptySquares = int.Parse(square.ToString());
                            for (int i = 0; i < emptySquares; i++)
                            {
                                boardState[index++] = 0;
                            }
                        }
                        else
                        {
                            // Otherwise, map the piece to its corresponding number
                            switch (square)
                            {
                                case 'p':
                                    boardState[index++] = 1;
                                    break;
                                case 'P':
                                    boardState[index++] = 2;
                                    break;
                                case 'r':
                                    boardState[index++] = 3;
                                    break;
                                case 'R':
                                    boardState[index++] = 4;
                                    break;
                                case 'n':
                                    boardState[index++] = 5;
                                    break;
                                case 'N':
                                    boardState[index++] = 6;
                                    break;
                                case 'b':
                                    boardState[index++] = 7;
                                    break;
                                case 'B':
                                    boardState[index++] = 8;
                                    break;
                                case 'q':
                                    boardState[index++] = 9;
                                    break;
                                case 'Q':
                                    boardState[index++] = 10;
                                    break;
                                case 'k':
                                    boardState[index++] = 11;
                                    break;
                                case 'K':
                                    boardState[index++] = 12;
                                    break;
                                default:
                                    throw new Exception("Invalid board placement string");
                            }
                        }
                    }
                }
            }

            private void AddCastlingRightsToBoardState(string castlingRights, float[] boardState)
            {
                // Here we assume that the castling rights are represented as individual characters
                foreach (char c in castlingRights)
                {
                    switch (c)
                    {
                        case 'K':
                            boardState[index++] = 1;
                            break;
                        case 'Q':
                            boardState[index++] = 2;
                            break;
                        case 'k':
                            boardState[index++] = 4;
                            break;
                        case 'q':
                            boardState[index++] = 8;
                            break;
                        default:
                            throw new Exception("Invalid castling rights string");
                    }
                }
            }

            private void AddEnPassantDataToBoardState(string enPassantData, float[] boardState)
            {
                // Here we assume that the en passant data is represented as a 2-digit number
                if (enPassantData != "-")
                {
                    int file = enPassantData[0] - 'a';
                    int rank = int.Parse(enPassantData[1].ToString()) - 1;
                    boardState[index++] = rank * 8 + file;
                }
            }
        }
    }
}
