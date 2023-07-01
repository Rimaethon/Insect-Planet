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
        private readonly Dictionary<object, List<GameEvents>> _eventSubscriptions = new();
        private readonly Dictionary<GameEvents, int> _eventPriorities = new();
        private readonly Queue<Action> _eventQueue = new();
        private bool _isProcessingEvent;

        #endregion


        #region Singleton

        private static EventManager _instance;

        public static EventManager Instance
        {
            get
            {
                if (_instance == null) _instance = new EventManager();
                return _instance;
            }
        }

        private EventManager()
        {
        }

        #endregion

        #region Event Handlers

        public void AddHandler(GameEvents gameEvent, Action handler)
        {
            // Add handler to the event handlers dictionary
            if (!_eventHandlers.ContainsKey(gameEvent)) _eventHandlers[gameEvent] = new List<Delegate>();

            _eventHandlers[gameEvent].Add(handler);
            Debug.Log($"Added handler {handler.Method.Name} for game event {gameEvent}");

            // Store the handler and game event in a dictionary
            if (!_eventSubscriptions.ContainsKey(handler)) _eventSubscriptions[handler] = new List<GameEvents>();

            _eventSubscriptions[handler].Add(gameEvent);
            Debug.Log($"Subscribed handler {handler.Method.Name} to game event {gameEvent}");
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

            // Remove the handler from the subscriptions dictionary
            if (_eventSubscriptions.TryGetValue(handler, out var subscriptions))
            {
                subscriptions.Remove(gameEvent);
                Debug.Log($"Unsubscribed handler {handler.Method.Name} from game event {gameEvent}");

                if (subscriptions.Count == 0)
                {
                    _eventSubscriptions.Remove(handler);
                    Debug.Log($"No more subscriptions for handler {handler.Method.Name}");
                }
            }
        }

        // RemoveHandler overloads for different parameter types

        #endregion

        #region Event Subscriptions

        public void Subscribe(object subscriber, GameEvents gameEvents)
        {
            // Add subscription to the subscriptions dictionary
            if (!_eventSubscriptions.ContainsKey(subscriber)) _eventSubscriptions[subscriber] = new List<GameEvents>();

            _eventSubscriptions[subscriber].Add(gameEvents);
            Debug.Log($"Subscribed {subscriber.GetType().Name} to game event {gameEvents}");
        }

        public void Unsubscribe(object subscriber, GameEvents gameEvents)
        {
            if (_eventSubscriptions.ContainsKey(subscriber))
            {
                _eventSubscriptions[subscriber].Remove(gameEvents);
                Debug.Log($"Unsubscribed {subscriber.GetType().Name} from game event {gameEvents}");

                if (_eventSubscriptions[subscriber].Count == 0)
                {
                    _eventSubscriptions.Remove(subscriber);
                    Debug.Log($"No more subscriptions for {subscriber.GetType().Name}");
                }
            }
        }

        #endregion

        #region Event Broadcasting

        public void Broadcast(GameEvents gameEvents)
        {
            // Enqueue the event for processing
            _eventQueue.Enqueue(() => ProcessEvent(gameEvents));
            ProcessEventQueueWithPriority();
        }

        // Broadcast overloads for different parameter types

        private void ProcessEventQueueWithPriority()
        {
            // If an event is already being processed, wait for the next frame to continue
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

            // Notify subscribers of the event
            foreach (var subscriber in _eventSubscriptions)
                if (subscriber.Value.Contains(gameEvents))
                    Debug.Log(
                        $"Broadcasted event {gameEvents.ToString()} with arguments {string.Join(", ", args.Select(arg => arg.ToString()))} to subscriber {subscriber.Key.GetType().Name}");

            _isProcessingEvent = false;
        }

        #endregion
    }
}