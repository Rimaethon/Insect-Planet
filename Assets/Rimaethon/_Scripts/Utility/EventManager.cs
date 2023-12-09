using System;
using System.Collections.Generic;
using System.Linq;
using Rimaethon._Scripts.Utility;
using Rimaethon.Scripts.Utility;
using Rimaethon.Utility;
using UnityEngine;

namespace Rimaethon.Scripts.Managers
{
    public class EventManager : PersistentSingleton<EventManager>
    {
        #region Fields And Properties

        //public SerializableDictionary<GameEvents, List<Delegate>> _eventHandlers = new SerializableDictionary<GameEvents, List<Delegate>> ();
        public Dictionary<GameEvents, List<Delegate>> _eventHandlers = new();
        [SerializeField] protected LogSystem logSystem;

        #endregion


        #region Unity Methods

        protected override void Awake()
        {
            if (logSystem != null)
            {
                logSystem.sender = this;
            }
            base.Awake();
        }
 
        [SerializeField] private List<string> _eventNames = new();

        private void Start()
        {
            foreach (var events in _eventHandlers.Keys)
            {
                _eventNames.Add(events.ToString());
                foreach (var value in _eventHandlers[events].ToList())
                {
                    Type type = value.Method.DeclaringType;
                    string className = type.Name;
                    _eventNames.Add(className+ "." + value.Method.Name);
                }
               
            }
        }

        protected override void OnApplicationQuit()
        {
            _eventHandlers.Clear();
            Debug.LogWarning("Event Manager is cleared");
            base.OnApplicationQuit();
        }

        #endregion

        #region Event Handlers

        public void AddHandler(GameEvents playerInput, Action handler)
        {
            if (!_eventHandlers.ContainsKey(playerInput)) _eventHandlers[playerInput] = new List<Delegate>();

            _eventHandlers[playerInput].Add(handler);
            logSystem.PromptLog($"Added handler {handler.Method.Name} for game event {playerInput}");
        }

        public void AddHandler<T>(GameEvents playerInput, Action<T> handler)
        {
            if (!_eventHandlers.ContainsKey(playerInput)) _eventHandlers[playerInput] = new List<Delegate>();

            _eventHandlers[playerInput].Add(handler);
            logSystem.PromptLog($"Added handler {handler.Method.Name} for game event {playerInput}");
        }

        public void RemoveHandler(GameEvents playerInput, Action handler)
        {
            if (_eventHandlers.TryGetValue(playerInput, out var handlers))
            {
                handlers.Remove(handler);
                logSystem.PromptLog($"Removed handler {handler.Method.Name} for game event {playerInput}");

                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(playerInput);
                    logSystem.PromptLog($"No more handlers for game event {playerInput}");
                }
            }
        }

        public void RemoveHandler<T>(GameEvents playerInput, Action<T> handler)
        {
            if (_eventHandlers.TryGetValue(playerInput, out var handlers))
            {
                handlers.Remove(handler);
                logSystem.PromptLog($"Removed handler {handler.Method.Name} for game event {playerInput}");

                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(playerInput);
                    logSystem.PromptLog($"No more handlers for game event {playerInput}");
                }
            }
        }

        #endregion

        #region Event Broadcasting

        public void Broadcast(GameEvents playerInputs)
        {
            ProcessEvent(playerInputs);
        }

        public void Broadcast<T>(GameEvents playerInput, T arg)
        {
            ProcessEvent(playerInput, arg);
        }

        private void ProcessEvent(GameEvents playerInputs, params object[] args)
        {
            if (_eventHandlers.TryGetValue(playerInputs, out var eventHandler))
                foreach (var handler in eventHandler)
                {
                    handler.DynamicInvoke(args);
                    logSystem.PromptLog(
                        $"Broadcasted event {playerInputs} with arguments {string.Join(", ", args.Select(arg => arg.ToString()))} to handler {handler.Method.Name}");
                }
        }

        #endregion
    }
}