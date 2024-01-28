using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace ChessLogic
{
    public class NormalMove : Move
    {

        public override MoveType Type => MoveType.Normal;
        public override Position FromPos { get; }
        public override Position ToPos { get; }

        [JsonConstructor]
        public NormalMove(Position from, Position to)
        {
            FromPos = from;
            ToPos = to;
        }

        public override bool Execute(Board board)
        {
            Piece piece = board[FromPos];
            
            bool capture = !board.IsEmpty(ToPos);
            board[ToPos] = piece;
            board[FromPos] = null;

            piece.HasMoved = true;
            return capture || piece.Type == PieceType.Pawn;
        }

        [JsonConstructor]
        public NormalMove(JsonElement json)
        {
            // Validate the JSON structure and extract values
            if (json.TryGetProperty("FromPos", out var fromPosJson) &&
                json.TryGetProperty("ToPos", out var toPosJson))
            {
                // Parse integers from JSON and pass them to the Position constructor
                int fromRow = fromPosJson.GetProperty("Row").GetInt32();
                int fromColumn = fromPosJson.GetProperty("Column").GetInt32();
                FromPos = new Position(fromRow, fromColumn);

                int toRow = toPosJson.GetProperty("Row").GetInt32();
                int toColumn = toPosJson.GetProperty("Column").GetInt32();
                ToPos = new Position(toRow, toColumn);
            }
            else
            {
                // Handle the case where the expected properties are not found in the JSON
                throw new JsonException("Invalid JSON structure for NormalMove.");
            }
        }

        [JsonConstructor]
        public NormalMove()
        {
            // Parameterless constructor for deserialization
        }
    }


}
