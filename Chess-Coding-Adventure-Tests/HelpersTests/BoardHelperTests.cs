using Chess.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Coding_Adventure_Tests.HelpersTests
{
    public class BoardHelperTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(7, 0)]
        [InlineData(8, 1)]
        [InlineData(15, 1)]
        [InlineData(16, 2)]
        [InlineData(23, 2)]
        [InlineData(24, 3)]
        [InlineData(31, 3)]
        [InlineData(32, 4)]
        [InlineData(39, 4)]
        [InlineData(40, 5)]
        [InlineData(47, 5)]
        [InlineData(48, 6)]
        [InlineData(55, 6)]
        [InlineData(56, 7)]
        [InlineData(63, 7)]
        public void RankIndexEvaluation(int value, int expectedResult)
        {
            //Expected input between 0 and 63 inclusive
            //Expected to return values between 0 and 7 inclusive
            var result = BoardHelper.RankIndex(value);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(7, 7)]
        [InlineData(8, 0)]
        [InlineData(15, 7)]
        [InlineData(16, 0)]
        [InlineData(23, 7)]
        [InlineData(24, 0)]
        [InlineData(31, 7)]
        [InlineData(32, 0)]
        [InlineData(39, 7)]
        [InlineData(40, 0)]
        [InlineData(47, 7)]
        [InlineData(48, 0)]
        [InlineData(55, 7)]
        [InlineData(56, 0)]
        [InlineData(63, 7)]
        public void FileIndexEvaluation(int value, int expectedResult)
        {
            //Expected input between 0 and 63 inclusive
            //Expected to return values between 0 and 7 inclusive
            var result = BoardHelper.FileIndex(value);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(7, 7, 63)]
        [InlineData(0, 7, 56)]
        [InlineData(7, 0, 7)]
        public void IndexFromCoordEvaluationFromValue(int fileIndex, int rankIndex, int expectedResult)
        {
            //Expected input between 0 and 7 inclusive
            //Expected to return values between 0 and 63 inclusive
            var result = BoardHelper.IndexFromCoord(fileIndex, rankIndex);
            Assert.Equal(expectedResult, result);
        }


        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(7, 7, 63)]
        [InlineData(0, 7, 56)]
        [InlineData(7, 0, 7)]
        public void IndexFromCoordEvaluationFromCoord(int fileIndex, int rankIndex, int expectedResult)
        {
            //Expected input from coord.fileIndex and coord.rankIndex between 0 and 7 inclusive
            //Expected to return values between 0 and 63 inclusive

            //arrange
            Coord coord = new Coord(fileIndex, rankIndex);
            //act
            var result = BoardHelper.IndexFromCoord(coord);
            //assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(7, 7, 0)]
        [InlineData(63, 7, 7)]

        public void CoordFromIndexEvaluation(int squareIndex, int expectedCoordFileIndex, int expectedCoordRankIndex)
        {
            //Expected input from squareIndex between 0 and 63 inclusive
            //Expected to return Coord object with fileIndex and rankIndex between 0 and 7 inclusive

            //act
            var result = BoardHelper.CoordFromIndex(squareIndex);
            //assert
            Assert.Equal(expectedCoordFileIndex, result.fileIndex);
            Assert.Equal(expectedCoordRankIndex, result.rankIndex);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(10, true)] // index = 1 , rank = 2
        [InlineData(63, false)]

        public void LightSquareEvaluation(int squareIndex, bool expectedResult)
        {
            //Expected input from squareIndex between 0 and 63 inclusive
            //Expected to return True / False if sum of file and rank index is not even

            //arrance
            int fileIndex = BoardHelper.FileIndex(squareIndex),
                rankIndex = BoardHelper.RankIndex(squareIndex);

            //act
            var resultFromSquare = BoardHelper.LightSquare(squareIndex);
            var resultFromIndex = BoardHelper.LightSquare(fileIndex, rankIndex);

            //assert
            Assert.Equal(expectedResult, resultFromSquare);
            Assert.Equal(expectedResult, resultFromIndex);
        }

        [Theory]
        [InlineData(0, "a1")]
        [InlineData(7, "h1")]
        [InlineData(56, "a8")]
        [InlineData(63, "h8")]
        public void SquareNameEvaluation(int squareIndex, string expectedResult)
        {
            //arrange
            int fileIndex = BoardHelper.FileIndex(squareIndex),
                rankIndex = BoardHelper.RankIndex(squareIndex);
            Coord coord = new Coord(squareIndex);

            //act
            string resultFromSquare = BoardHelper.SquareNameFromIndex(squareIndex),
                   resultFromIndex = BoardHelper.SquareNameFromCoordinate(fileIndex, rankIndex),
                   resultFromCoord = BoardHelper.SquareNameFromCoordinate(coord);

            //assert
            Assert.Equal(resultFromSquare, expectedResult);
            Assert.Equal(resultFromIndex, expectedResult);
            Assert.Equal(resultFromCoord, expectedResult);
        }

        [Theory]
        [InlineData("a1", 0)]
        [InlineData("h1" ,7)]
        [InlineData("a8", 56)]
        [InlineData("h8", 63)]
        public void SquareIndexFromNameEvaluation(string squareName, int expectedResult)
        {
            //act
            var result = BoardHelper.SquareIndexFromName(squareName);

            //assert
            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(7, 7, true)]
        [InlineData(8, 8, false)]
        [InlineData(8, 0, false)]
        [InlineData(0, 8, false)]
        public void IsValidCoordinateEvaluation(int x, int y, bool expectedResult)
        {
            //act
            var result = BoardHelper.IsValidCoordinate(x,y);
            //assert
            Assert.Equal(result, expectedResult);
        }
    }
}
