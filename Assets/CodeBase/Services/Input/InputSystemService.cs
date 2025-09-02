using System;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace CodeBase.Services.Input
{
    /// <summary>
    /// A service responsible for creating and managing the InputSystem_Actions instance and controlling Action Maps.
    /// </summary>
    public class InputSystemService : IStartable, IDisposable
    {
        private const string EventSystemObjectName = "EventSystem";

        private static InputSystemUIInputModule _uiInputModule;
        private static EventSystem _eventSystem;

        public InputSystem_Actions InputActions { get; private set; }
        
        // Events for switching between input modes
        public event Action OnSwitchToUI;
        public event Action OnSwitchToPlayer;
        
        public void Start()
        {
            CreateInputSystemActions();
            InitializeEventSystem();
            InputActions.UI.Enable(); // Enable UI Action Map by default
        }
        
        /// <summary>
        /// Enables only the UI Action Map, keeping it always enabled.
        /// </summary>
        public void SwitchToUI()
        {
            // InputActions.Player.Disable(); // Commented out - Player ActionMap not implemented yet
            InputActions.UI.Enable(); // Ensure UI is always enabled
            Debug.Log("Switched to UI mode.");
            OnSwitchToUI?.Invoke();
        }

        /// <summary>
        /// Enables Player Action Map, keeping UI enabled.
        /// </summary>
        public void SwitchToPlayer()
        {
            // InputActions.Player.Enable(); // Commented out - Player ActionMap not implemented yet
            InputActions.UI.Enable(); // UI remains enabled
            Debug.Log("Switched to Player mode.");
            OnSwitchToPlayer?.Invoke();
        }

        /// <summary>
        /// Enables the UI Action Map without affecting other Action Maps (always enabled by default).
        /// </summary>
        public void EnableUI()
        {
            InputActions.UI.Enable();
            Debug.Log("UI Action Map ensured to be enabled.");
        }

        /// <summary>
        /// Disables the UI Action Map (now overridden to do nothing to keep UI always enabled).
        /// </summary>
        public void DisableUI()
        {
            Debug.LogWarning("UI Action Map cannot be disabled as per design.");
            InputActions.UI.Enable(); // Ignore disable attempt
        }
        
        /// <summary>
        /// Checks if the Player Action Map is enabled.
        /// </summary>
        public bool IsPlayerInputEnabled() 
        {
            // return InputActions.Player.enabled; // Commented out - Player ActionMap not implemented yet
            return false; // Temporary return until Player ActionMap is implemented
        }
        
        /// <summary>
        /// Sets the first selected object for UI navigation.
        /// </summary>
        /// <param name="selectedObject">The object to be set as the first selected.</param>
        public void SetFirstSelectedObject(Selectable selectedObject)
        {
            if (_eventSystem == null)
            {
                Debug.LogWarning("EventSystem is not initialized. Cannot set first selected object.");
                return;
            }

            if (selectedObject == null)
            {
                Debug.LogWarning("Selected object is null.");
                _eventSystem.SetSelectedGameObject(null);
                return;
            }

            _eventSystem.SetSelectedGameObject(selectedObject.gameObject);
        }
        
        /// <summary>
        /// Gets the full action path for debugging purposes.
        /// </summary>
        /// <param name="action">The input action to get path for.</param>
        /// <returns>Full path in format "MapName/ActionName".</returns>
        public string GetFullActionPath(InputAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("InputAction is null. Cannot get full path.");
                return string.Empty;
            }

            string mapName = action.actionMap?.name ?? "UnknownMap";
            string actionName = action.name;
            return $"{mapName}/{actionName}";
        }
        
        /// <summary>
        /// Creates an Observable for input action performed events.
        /// </summary>
        /// <param name="action">The input action to observe.</param>
        /// <returns>Observable that emits Unit when action is performed.</returns>
        public Observable<Unit> GetPerformedObservable(InputAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("InputAction is null. Cannot create Observable.");
                return Observable.Empty<Unit>(); // Return empty Observable on error
            }

            return Observable.FromEvent(
                (Action<InputAction.CallbackContext> h) => action.performed += h,
                h => action.performed -= h
            ).Select(_ => Unit.Default); // Convert to Unit for unification
        }

        // TODO: Watch UnityEvent to Observable conversion (Unity built-in script)
        // public static Observable<Unit> AsObservable(this UnityEngine.Events.UnityEvent unityEvent, CancellationToken cancellationToken = default)
        // {
        //     return Observable.FromEvent(h => new UnityAction(h), h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h), cancellationToken);
        // }
        //
        // public static Observable<T> AsObservable<T>(this UnityEngine.Events.UnityEvent<T> unityEvent, CancellationToken cancellationToken = default)
        // {
        //     return Observable.FromEvent<UnityAction<T>, T>(h => new UnityAction<T>(h), h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h), cancellationToken);
        // }
        
        public void Dispose()
        {
            if (InputActions == null) return;

            // InputActions.Player.Disable(); // Commented out - Player ActionMap not implemented yet
            InputActions.UI.Disable();
            
            InputActions.Disable();
            InputActions.Dispose();
        }

        /// <summary>
        /// Initializes the EventSystem, creating a new one if it doesn't exist.
        /// </summary>
        private void InitializeEventSystem()    
        {
            _eventSystem = Object.FindObjectOfType<EventSystem>();
            if (_eventSystem == null)
            {
                _eventSystem = CreateEventSystem();
                _uiInputModule.actionsAsset = InputActions.asset;
                Object.DontDestroyOnLoad(_eventSystem.gameObject);
            }
            else
            {
                Debug.Log("Found existing EventSystem.");
            }
        }

        /// <summary>
        /// Initializes the InputSystem_Actions.
        /// </summary>
        private void CreateInputSystemActions() => InputActions = new InputSystem_Actions();

        /// <summary>
        /// Creates a new EventSystem with an InputSystemUIInputModule.
        /// </summary>
        /// <returns>The created EventSystem.</returns>
        private static EventSystem CreateEventSystem()
        {
            var eventSystem = new GameObject(EventSystemObjectName).AddComponent<EventSystem>();
            _uiInputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            return eventSystem;
        }
    }
}