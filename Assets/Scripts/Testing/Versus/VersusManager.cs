using System.Collections;
using System.Collections.Generic;
using System.IO;
using Chess.Game;
using TMPro;
using UnityEngine;

namespace Chess.Testing {
	public class VersusManager : MonoBehaviour {

		/*
				public TMP_Text log;

				PlayerInfo playerA;
				PlayerInfo playerB;

				string playerAFilePath;
				string playerBFilePath;
				VersusInfo versusInfo;

				BoardUI boardUI;

				void Start () {
					if (Application.isEditor) {
						Debug.Log ("Please run in build");
					} else {
						ClearLog ();
						boardUI = FindObjectOfType<BoardUI> ();

						versusInfo = new VersusInfo ();
						VersusCommunication.CreateManagerFile ();
						StartCoroutine (WaitForPlayers ());
						FindObjectOfType<VersusCommunication> ().onPlayerUpdated += OnPlayerUpdated;
					}
				}

				void OnPlayerUpdated (PlayerInfo playerInfo) {
					if (playerInfo.lastMovePly == versusInfo.numPly && versusInfo.gameInProgress) {
						Move move = new Move (playerInfo.lastMove);
						if (!move.IsInvalid) {
							Board.MakeMove (move);
							boardUI.OnMoveMade (move);

							MoveGenerator moveGenerator = new MoveGenerator ();
							if (moveGenerator.GenerateMoves ().Count == 0) {
								Debug.LogError ("Game Over");
								Log ("Game Over");
								versusInfo.gameInProgress = false;
							} else {
								versusInfo.numPly++;
							}
							versusInfo.lastMove = playerInfo.lastMove;

							VersusCommunication.WriteManagerFile (versusInfo);
							Log ("Move Received: " + playerInfo.lastMove + " ply = " + playerInfo.lastMovePly);
						}
					}

				}

				void OnBothPlayersRegistered () {
					Board.LoadStartPosition ();
					boardUI.UpdatePosition ();

					playerA = VersusCommunication.GetPlayerInfo (playerAFilePath);
					playerB = VersusCommunication.GetPlayerInfo (playerBFilePath);

					versusInfo.whiteID = playerA.id;
					versusInfo.blackID = playerB.id;
					versusInfo.gameInProgress = true;
					versusInfo.gameNumber = 1;

					Log ("Both players registered");
					VersusCommunication.WriteManagerFile (versusInfo);
				}

				void ClearLog () {
					log.text = "";
				}

				void Log (string message) {
					log.text += message + "\n";
				}

				IEnumerator WaitForPlayers () {
					while (true) {
						string[] playerFiles = VersusCommunication.GetPlayerFiles ();
						if (playerFiles.Length >= 2) {
							playerAFilePath = playerFiles[0];
							playerBFilePath = playerFiles[1];
							break;
						}
						yield return new WaitForSeconds (0.5f);
					}

					OnBothPlayersRegistered ();
				}
					*/
	}

	[System.Serializable]
	public class VersusInfo {
		public int whiteID;
		public int blackID;

		public bool gameInProgress;
		public int gameNumber;
		public int numPly;
		public ushort lastMove;

	}

}