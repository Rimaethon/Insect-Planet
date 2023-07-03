using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rimaethon._Scripts.Utility
{
    public class EventManager
    {
        #region Event Priorities

        public void SetEventPriority(GameEvents gameEvents, int priority)
        {
            _eventPriorities[gameEvents] = priority;
        }

        #endregion

        #region Fields

        private readonly Dictionary<GameEvents, List<Delegate>> _eventHandlers = new();
        private readonly Dictionary<GameEvents, int> _eventPriorities = new();
        private readonly Queue<Action> _eventQueue = new();
        private bool _isProcessingEvent;

        #endregion


        #region Singleton

        private static EventManager _instance;

        public static EventManager Instance
        {
            get { return _instance ??= new EventManager(); }
        }

        private EventManager()
        {
        }

        #endregion

        #region Event Handlers

        public void AddHandler(GameEvents gameEvent, Action handler)
        {
            if (!_eventHandlers.ContainsKey(gameEvent)) _eventHandlers[gameEvent] = new List<Delegate>();

            _eventHandlers[gameEvent].Add(handler);
            Debug.Log($"Added handler {handler.Method.Name} for game event {gameEvent}");


            Debug.Log($"Subscribed handler {handler.Method.Name} to game event {gameEvent}");
        }
        
        public void AddHandler<T>(GameEvents gameEvent, Action<T> handler)
        {
            // Add handler to the event handlers dictionary
            if (!_eventHandlers.ContainsKey(gameEvent))
                _eventHandlers[gameEvent] = new List<Delegate>();

            _eventHandlers[gameEvent].Add(handler);
            Debug.Log($"Added handler {handler.Method.Name} for game event {gameEvent}");


        }

        public void AddHandler<T1, T2>(GameEvents gameEvent, Action<T1, T2> handler)
        {
            // Add handler to the event handlers dictionary
            if (!_eventHandlers.ContainsKey(gameEvent))
                _eventHandlers[gameEvent] = new List<Delegate>();

            _eventHandlers[gameEvent].Add(handler);
            Debug.Log($"Added handler {handler.Method.Name} for game event {gameEvent}");

        }

        // AddHandler overloads for different parameter types

        public void RemoveHandler(GameEvents gameEvent, Action handler)
        {
            // Remove the handler from the event handlers dictionary
            if (_eventHandlers.TryGetValue(gameEvent, out var handlers))
            {
                handlers.Remove(handler);
                Debug.Log($"Removed handler {handler.Method.Name} for game event {gameEvent}");

                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(gameEvent);
                    Debug.Log($"No more handlers for game event {gameEvent}");
                }
            }

            
        }
          public void RemoveHandler<T>(GameEvents gameEvent, Action<T> handler)
        {
            // Remove the handler from the event handlers dictionary
            if (_eventHandlers.TryGetValue(gameEvent, out var handlers))
            {
                handlers.Remove(handler);
                Debug.Log($"Removed handler {handler.Method.Name} for game event {gameEvent}");

                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(gameEvent);
                    Debug.Log($"No more handlers for game event {gameEvent}");
                }
            }

         
        }

        public void RemoveHandler<T1, T2>(GameEvents gameEvent, Action<T1, T2> handler)
        {
            // Remove the handler from the event handlers dictionary
            if (_eventHandlers.TryGetValue(gameEvent, out var handlers))
            {
                handlers.Remove(handler);
                Debug.Log($"Removed handler {handler.Method.Name} for game event {gameEvent}");

                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(gameEvent);
                    Debug.Log($"No more handlers for game event {gameEvent}");
                }
            }

           
        }

        // RemoveHandler overloads for different parameter types

        #endregion

       

        #region Event Broadcasting

        public void Broadcast(GameEvents gameEvents)
        {
            _eventQueue.Enqueue(() => ProcessEvent(gameEvents));
            ProcessEventQueueWithPriority();
        }
        
        public void Broadcast<T>(GameEvents gameEvent, T arg)
        {
            _eventQueue.Enqueue(() => ProcessEvent(gameEvent, arg));
            ProcessEventQueueWithPriority();
        }

        public void Broadcast<T1, T2>(GameEvents gameEvent, T1 arg1, T2 arg2)
        {
            _eventQueue.Enqueue(() => ProcessEvent(gameEvent, arg1, arg2));
            ProcessEventQueueWithPriority();
        }


        private void ProcessEventQueueWithPriority()
        {
            if (_isProcessingEvent) return;

            // Sort the event queue based on priority
            var sortedEvents = new List<Action>(_eventQueue);
            sortedEvents.Sort((a, b) => GetEventPriority(b) - GetEventPriority(a));

            // Process events in the sorted queue
            while (sortedEvents.Count > 0)
            {
                var eventAction = sortedEvents[0];
                sortedEvents.RemoveAt(0);
                eventAction.DynamicInvoke();
            }
        }

        private int GetEventPriority(Action eventAction)
        {
            foreach (var eventHandlers in _eventHandlers)
                if (eventHandlers.Value.Contains(eventAction))
                {
                    if (_eventPriorities.TryGetValue(eventHandlers.Key, out var priority)) return priority;
                    break;
                }

            return 0; // Default priority if not found
        }

        private void ProcessEvent(GameEvents gameEvents, params object[] args)
        {
            _isProcessingEvent = true;

            // Invoke handlers for the event
            if (_eventHandlers.TryGetValue(gameEvents, out var eventHandler))
                foreach (var handler in eventHandler)
                {
                    handler.DynamicInvoke(args);
                    Debug.Log(
                        $"Broadcasted event {gameEvents.ToString()} with arguments {string.Join(", ", args.Select(arg => arg.ToString()))} to handler {handler.Method.Name}");
                }
            
            _isProcessingEvent = false;
        }

        #endregion
    }
}