using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;

namespace Chess
{
	// Place in scene to ensure that static constructors are called at start up.
	// This prevents them from potentially being called at a bad moment, like during search
	public class StaticInitializer : MonoBehaviour
	{
		void Awake()
		{
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(PrecomputedEvaluationData).TypeHandle);
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(PrecomputedMoveData).TypeHandle);
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(BitBoardUtility).TypeHandle);
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Magic).TypeHandle);
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Bits).TypeHandle);
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(PieceSquareTable).TypeHandle);
		}

	}
}