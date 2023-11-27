using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quiz
{
    /// <summary>
    /// A SequenceManager controls the overall flow of the application using a state machine.
    /// 
    /// Use this class to define how each State will transition to the next. Each state can
    /// transition to the next state when receiving an event or reaching a specific condition.
    ///
    /// Note: this class currently is only used for demonstration/diagnostic purposes. You can use
    ///       the start and end of each state to instantiate GameObjects/play effects. Another simple
    ///       state machine for UI screens (UIManager) actually drives most of the quiz gameplay.
    /// 
    /// </summary>
    
    public class SequenceManager : MonoBehaviour
    {
        // Inspector fields
        [Header("Preload (Splash Screen)")]
        [Tooltip("Prefab assets that load first. These can include level management Prefabs or textures, sounds, etc.")]
        [SerializeField] GameObject[] m_PreloadedAssets;

        [Tooltip("Time in seconds to show Splash Screen")]
        [SerializeField] float m_LoadScreenTime = 5f;

        StateMachine m_StateMachine = new StateMachine();

        // Define all States here
        IState m_SplashScreenState;     // Startup and load assets, show a splash screen
        IState m_StartScreenState;      // Empty screen with a start button

        IState m_MainMenuState;         // Show the main menu screens
        IState m_LevelSelectionState;   // Show a UI Screen to choose a game mode/level
        IState m_MenuSettingsState;     // Show the Settings Screen while in the Main Menu
        IState m_GamePlayState;         // Play the game
        IState m_GameSettingsState;     // Go to the Settings Screen while in Game play mode
        IState m_PauseState;            // Pause the game during gameplay

        IState m_GameWinState;          // Show the win screen
        IState m_GameLoseState;         // Show the lose screen

        // Access to the StateMachine's CurrentState
        public IState CurrentState => m_StateMachine.CurrentState;

        #region MonoBehaviour event messages
        private void Start()
        {
            // Set this MonoBehaviour to control the coroutines - unused in this demo
            Coroutines.Initialize(this);

            // Checks for required fields in the Inspector
            NullRefChecker.Validate(this);

            // Instantiates any assets needed before gameplay
            InstantiatePreloadedAssets();

            // Sets up States and transitions, runs initial State
            Initialize();
        }

        // Subscribe to event channels
        private void OnEnable()
        {
            SceneEvents.ExitApplication += SceneEvents_ExitApplication;
        }

        // Unsubscribe from event channels to prevent errors
        private void OnDisable()
        {
            SceneEvents.ExitApplication -= SceneEvents_ExitApplication;
        }
        #endregion

        #region Methods

        public void Initialize()
        {
            // Define the Game States
            SetStates();
            AddLinks();

            // Run first state/loading screen
            m_StateMachine.Run(m_SplashScreenState);
            UIEvents.SplashScreenShown?.Invoke();
        }

        // Define the state machine's states
        private void SetStates()
        {
            // Create States for the game. Pass in an Action to execute or null to do nothing

            // Optional names added for debugging
            // Executes GameEvents.LoadProgressUpdated every frame and GameEvents.PreloadCompleted on exit
            m_SplashScreenState = new DelayState(m_LoadScreenTime, SceneEvents.LoadProgressUpdated,
                SceneEvents.PreloadCompleted, "LoadScreenState");

            m_StartScreenState = new State(null, "StartScreenState");
            m_MainMenuState = new State(null, "MainMenuState");
            m_LevelSelectionState = new State(null, "LevelSelectionState");
            m_MenuSettingsState = new State(null, "SettingsState");
            m_GamePlayState = new State(null, "GamePlayState");
            m_PauseState = new State(null, "PauseState");
            m_GameWinState = new State(null, "GameWinState");
            m_GameLoseState = new State(null, "GameLoseState");
            m_GameSettingsState = new State(null, "GameSettingsState");

        }

        // Define links between the states
        private void AddLinks()
        {
            // Transition automatically to the StartScreen once the loading time completes
            m_SplashScreenState.AddLink(new Link(m_StartScreenState));

            m_StartScreenState.AddLink(new EventLink(UIEvents.MainMenuShown, m_MainMenuState));

            m_MainMenuState.AddLink(new EventLink(UIEvents.SettingsShown, m_MenuSettingsState));

            m_LevelSelectionState.AddLink(new EventLink(UIEvents.ScreenClosed, m_MainMenuState));

            m_MenuSettingsState.AddLink(new EventLink(UIEvents.ScreenClosed, m_MainMenuState));

            m_LevelSelectionState.AddLink(new EventLink(GameEvents.GameStarted, m_GamePlayState));
            m_LevelSelectionState.AddLink(new EventLink(UIEvents.ScreenClosed, m_MainMenuState));

            m_GamePlayState.AddLink(new EventLink(GameEvents.GameLost, m_GameLoseState));
            m_GamePlayState.AddLink(new EventLink(GameEvents.GameWon, m_GameWinState));
            m_GamePlayState.AddLink(new EventLink(UIEvents.SettingsShown, m_GameSettingsState));
            m_GamePlayState.AddLink(new EventLink(GameEvents.GamePaused, m_PauseState));

            m_PauseState.AddLink(new EventLink(GameEvents.GameUnpaused, m_GamePlayState));
            m_PauseState.AddLink(new EventLink(GameEvents.GameAborted, m_MainMenuState));
        }

        // Use this to preload any assets. The QuizU sample only loads a few prefabs, but this is an
        // opportunity to load any textures, models, etc. in advance to avoid loading during gameplay 
        private void InstantiatePreloadedAssets()
        {
            foreach (var asset in m_PreloadedAssets)
            {
                if (asset != null)
                    Instantiate(asset);
            }
        }
        #endregion

        // Event-handling methods
        private void SceneEvents_ExitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
