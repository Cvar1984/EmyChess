﻿
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using Emychess.Misc;
using Emychess.Interactions;
using JetBrains.Annotations;

namespace Emychess
{
    /// <summary>
    /// Manages game flow and UI
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ChessManager : UdonSharpBehaviour
    {
        public Board board;
        public Timer timer;
        public PromotionPicker picker;

        /// <summary>
        /// An object of which its ownership represents the white player
        /// </summary>
        public PlayerHolder whitePlayerHolder;
        [UdonSynced][HideInInspector]
        public bool isWhiteRegistered;
        /// <summary>
        /// An object of which its ownership represents the black player
        /// </summary>
        public PlayerHolder blackPlayerHolder;
        [UdonSynced][HideInInspector]
        public bool isBlackRegistered;
        [UdonSynced][HideInInspector]
        public bool inProgress;
        [UdonSynced][HideInInspector]
        public bool currentSide;
        [UdonSynced][HideInInspector]
        public bool anarchy;
        [UdonSynced] [HideInInspector]
        public byte whiteScore;
        [UdonSynced] [HideInInspector]
        public byte blackScore;
        /// <summary>
        /// Whether to have the timer automatically switch side at the end of a turn, as well as having time running out be a game over state, toggle with <see cref="_ToggleAutoTimer"/>
        /// </summary>
        [UdonSynced][HideInInspector]
        public bool automatedTimer;



        
        public Text whitePlayerLabel;
        public Text blackPlayerLabel;

        public GameObject mainMenu;
        public Text gameModeLabel;
        public Text logText;
        public Button startButton;
        public Text whiteJoinLabel;
        public Text blackJoinLabel;
        public GameObject autoTimerCheckMark;

        public GameObject gameMenu;
        public Text currentPlayerLabel;
        public Text currentSideLabel;
        public Text whiteScoreLabel;
        public Text blackScoreLabel;
        public Button endButton;
        public GameOverMessage gameOverMessage;

        public GameObject anarchyControls_black;
        public GameObject anarchyControl_white;
        public UdonBehaviour endTurnButton_black;
        public UdonBehaviour endTurnButton_white;
        public PiecePlacer blackPiecePlacer;
        public PiecePlacer whitePiecePlacer;
        



        

        public void _RefreshUI()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            VRCPlayerApi whitePlayer = GetPlayer(true);
            VRCPlayerApi blackPlayer = GetPlayer(false);
            VRCPlayerApi currentPlayer = GetPlayer(currentSide);
            Piece currentKing = currentSide ? board.whiteKing : board.blackKing;
            bool isPlayerWhite = (localPlayer == whitePlayer);
            bool isPlayerBlack = (localPlayer == blackPlayer);
            bool isPlayerTurn = (localPlayer == currentPlayer);

            whitePlayerLabel.text = whitePlayer == null ? "-" : whitePlayer.displayName;
            blackPlayerLabel.text = blackPlayer == null ? "-" : blackPlayer.displayName;

            mainMenu.SetActive(!inProgress);
            gameModeLabel.text = anarchy ? "Anarchy" : "Standard";
            startButton.interactable = (whitePlayer != null && blackPlayer != null)&&isRegistered(Networking.LocalPlayer);
            whiteJoinLabel.text = (isPlayerWhite) ? "Leave White" : "Join White";
            blackJoinLabel.text = (isPlayerBlack) ? "Leave Black" : "Join Black";
            autoTimerCheckMark.SetActive(automatedTimer);

            gameMenu.SetActive(inProgress);
            currentPlayerLabel.text = currentPlayer==null?"-":currentPlayer.displayName;
            currentSideLabel.text = currentSide ? "(white)" : "(black)";
            whiteScoreLabel.text = whiteScore.ToString();
            blackScoreLabel.text = blackScore.ToString();
            endButton.interactable = isRegistered(localPlayer);

            anarchyControls_black.SetActive(anarchy);
            anarchyControl_white.SetActive(anarchy);
            endTurnButton_black.DisableInteractive = !(isPlayerBlack&&(!currentSide)&&inProgress);
            endTurnButton_white.DisableInteractive = !(isPlayerWhite&&currentSide&&inProgress);
            blackPiecePlacer.SetEnabled(isPlayerBlack&&(!currentSide)&&inProgress);
            blackPiecePlacer._Refresh();
            whitePiecePlacer.SetEnabled(isPlayerWhite&&currentSide&&inProgress);
            whitePiecePlacer._Refresh();

            board._ClearIndicators();

            board._SetPiecesGrabbable(currentPlayer, currentSide);

        }

        public void RefreshPiecePlacerPieceCount()
        {
            blackPiecePlacer._Refresh();
            whitePiecePlacer._Refresh();
        }


        public void Start()
        {
             _RefreshUI();
            SetLogText("");
        }
        public override void OnDeserialization()
        {
            _RefreshUI();
        }

        /// <summary>
        /// Get player registered for the specified side
        /// </summary>
        /// <param name="white"></param>
        /// <returns></returns>
        public VRCPlayerApi GetPlayer(bool white)
        {
            if (white)
            {
                return isWhiteRegistered ? whitePlayerHolder.GetOwner() : null;
            }
            else
            {
                return isBlackRegistered ? blackPlayerHolder.GetOwner() : null;
            }
        }

        /// <summary>
        /// Register the local player as the specified side
        /// </summary>
        /// <param name="white"></param>
        public void _RegisterPlayer(bool white)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            PlayerHolder targetHolder = white ? whitePlayerHolder : blackPlayerHolder;
            targetHolder._SetOwner();
            if (white) 
            { 
                isWhiteRegistered = true; 
                Debug.Log("[ChessManager] "+GetPlayer(true).displayName+" joined White");
            } else 
            { 
                isBlackRegistered = true; 
                Debug.Log("[ChessManager] "+GetPlayer(false).displayName+" joined Black");
            }
            _RefreshUI();
            RequestSerialization();
        }
        /// <summary>
        /// Registers the local player as white, unregisters if they're already registered
        /// </summary>
        [PublicAPI] public void _RegisterWhite()
        {
            if (isWhiteRegistered && GetPlayer(true) == Networking.LocalPlayer) 
            { 
                _UnRegisterPlayer(true); 
            } else 
            { 
                _RegisterPlayer(true); 
            }
        }
        /// <summary>
        /// Registers the local player as black, unregisters if they're already registered
        /// </summary>
        [PublicAPI] public void _RegisterBlack()
        {
            if (isBlackRegistered && GetPlayer(false) == Networking.LocalPlayer) 
            { 
                _UnRegisterPlayer(false); 
            } 
            else 
            { 
                _RegisterPlayer(false); 
            }
        }
        public bool isRegistered(VRCPlayerApi player)
        {
            return (isWhiteRegistered && (GetPlayer(true) == player)) || (isBlackRegistered && (GetPlayer(false) == player));
        }
        /// <summary>
        /// Only Standard and Anarchy are available game modes currently
        /// </summary>
        /// <param name="anarchy"></param>
        public void SetGameMode(bool anarchy)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            board.currentRules.SetAnarchy(anarchy);
            this.anarchy = anarchy;
            _RefreshUI();
            RequestSerialization();
        }
        [PublicAPI] public void _SetStandardGameMode()
        {
            SetGameMode(false);
        }
        [PublicAPI] public void _SetAnarchyGameMode()
        {
            SetGameMode(true);
        }

        /// <summary>
        /// Unregister the local player from the specified side
        /// </summary>
        /// <param name="white"></param>
        public void _UnRegisterPlayer(bool white)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            if (white) 
            { 
                Debug.Log($"[ChessManager] {GetPlayer(true).displayName} left White");
                isWhiteRegistered = false; 
            } 
            else 
            { 
                Debug.Log($"[ChessManager] {GetPlayer(false).displayName} left black");
                isBlackRegistered = false; 
            }
            _RefreshUI();
            RequestSerialization();
        }
        /// <summary>
        /// There's a text UI component in the interface that can be used for debugging, but as of right now it's unused
        /// </summary>
        /// <param name="text"></param>
        public void SetLogText(string text)
        {
            Debug.Log($"[ChessManager] {text}");
            logText.text = text;
        }
        [PublicAPI] public void _EndTurn()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            currentSide = !currentSide;
            if (GetPlayer(currentSide) == null)
            {
                _EndGame();
            }
            if (automatedTimer)
            {
                timer._SwitchSide();
            }
            if (!board.currentRules.anarchy)
            {
                Piece currentKing = currentSide ? board.whiteKing : board.blackKing;
                if (currentKing!=null)
                {
                    bool isKingInCheck = board.currentRules.isKingInCheck(currentKing.GetVec(), board.grid, board, board.PawnThatDidADoublePushLastRound, currentSide);
                    
                    if (isKingInCheck) { board.SetIndicator(currentKing.x, currentKing.y, 2); 
                    Debug.Log("[ChessManager] "+(currentSide ? "White" : "Black")+" King is in check");
                    } //TODO king in check should be moved to the refreshUI

                    int endState = board._CheckIfGameOver(currentSide, isKingInCheck);
                    if (endState != 0)
                    {
                        bool winningSide = !currentSide;
                        gameOverMessage._SetGameOverMessage(endState, winningSide);
                    }
                    else
                    {
                        gameOverMessage._Reset();
                    }
                }
            }

            _RefreshUI();
            RequestSerialization();
        }
        public void AddScore(int score,bool white)
        {
            
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            if (white) { 
                whiteScore += (byte)score; 
                Debug.Log($"[ChessManager] White Scored: {score}");
                }
            else { 
                blackScore += (byte)score;
                Debug.Log($"[ChessManager] Black Scored: {score}");
                }

            //RequestSerialization();
        }

        [PublicAPI] public void _StartGame()
        {

            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            inProgress = true;
            whiteScore = blackScore = 0;
            currentSide=true;
            board._ResetBoard();
            gameOverMessage._Reset();
            if (automatedTimer)
            {
                if (timer.isStarted) { timer._ResetTimer(0); }
                timer._StartCountDown(true);
            }
            _RefreshUI();
            RequestSerialization();

            Debug.Log("[ChessManager] Game Started");
        }
        [PublicAPI] public void _EndGame()
        {
            Debug.Log("[ChessManager] Game Stopped");

            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            inProgress = false;
            board._ClearBoard();
            if (automatedTimer)
            {
                timer._ResetTimer(0);
            }
            gameOverMessage._Reset();
            picker._Hide();
            _RefreshUI();
            RequestSerialization();
        }

        /// <summary>
        /// Toggles <see cref="automatedTimer"/>
        /// </summary>
        [PublicAPI] public void _ToggleAutoTimer()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            automatedTimer = !automatedTimer;
            _RefreshUI();
            RequestSerialization();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (isRegistered(player) && isRegistered(Networking.LocalPlayer))
            {
                if (inProgress) { _EndGame(); } else
                {

                    bool white = GetPlayer(true) == player;
                    _UnRegisterPlayer(white);

                }
            }
        }
    }
}
