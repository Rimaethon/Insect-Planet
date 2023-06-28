using System;
using System.Collections.Generic;
using Rimaethon._Scripts.Managers;
using UnityEngine;

namespace Rimaethon._Scripts.Utility
{
    public class EventManager : IEventManager
    {
        
        private static EventManager _instance;

        private readonly Dictionary<GameStates, List<Delegate>> _eventHandlers =
            new Dictionary<GameStates, List<Delegate>>();

        // Private constructor to prevent outside instantiation
        private EventManager() {}

        // Public static property to access the instance
        public static EventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventManager();
                }
                return _instance;
            }
        }

        public void AddHandler(GameStates gameStates, Action handler)
        {
            if (!_eventHandlers.ContainsKey(gameStates))
            {
                _eventHandlers[gameStates] = new List<Delegate>();
            }

            _eventHandlers[gameStates].Add(handler);
            Debug.Log($"Added handler {handler.Method.Name} for game state {gameStates}");
        }

        public void AddHandler<T>(GameStates gameStates, Action<T> handler)
        {
            if (!_eventHandlers.ContainsKey(gameStates))
            {
                _eventHandlers[gameStates] = new List<Delegate>();
            }

            _eventHandlers[gameStates].Add(handler);
            Debug.Log($"Added handler {handler.Method.Name} for game state {gameStates}");
        }

        public void AddHandler<T1, T2>(GameStates gameStates, Action<T1, T2> handler)
        {
            if (!_eventHandlers.ContainsKey(gameStates))
            {
                _eventHandlers[gameStates] = new List<Delegate>();
            }

            _eventHandlers[gameStates].Add(handler);
            Debug.Log($"Added handler {handler.Method.Name} for game state {gameStates}");
        }

        public void RemoveHandler(GameStates gameStates, Action handler)
        {
            if (_eventHandlers.ContainsKey(gameStates))
            {
                _eventHandlers[gameStates].Remove(handler);
                Debug.Log($"Removed handler {handler.Method.Name} for game state {gameStates}");

                if (_eventHandlers[gameStates].Count == 0)
                {
                    _eventHandlers.Remove(gameStates);
                    Debug.Log($"No more handlers for game state {gameStates}");
                }
            }
        }

        public void RemoveHandler<T>(GameStates gameStates, Action<T> handler)
        {
            if (_eventHandlers.ContainsKey(gameStates))
            {
                _eventHandlers[gameStates].Remove(handler);
                Debug.Log($"Removed handler {handler.Method.Name} for game state {gameStates}");

                if (_eventHandlers[gameStates].Count == 0)
                {
                    _eventHandlers.Remove(gameStates);
                    Debug.Log($"No more handlers for game state {gameStates}");
                }
            }
        }

        public void RemoveHandler<T1, T2>(GameStates gameStates, Action<T1, T2> handler)
        {
            if (_eventHandlers.ContainsKey(gameStates))
            {
                _eventHandlers[gameStates].Remove(handler);
                Debug.Log($"Removed handler {handler.Method.Name} for game state {gameStates}");

                if (_eventHandlers[gameStates].Count == 0)
                {
                    _eventHandlers.Remove(gameStates);
                    Debug.Log($"No more handlers for game state {gameStates}");
                }
            }
        }

        public void Broadcast(GameStates gameStates)
        {
            if (_eventHandlers.ContainsKey(gameStates))
            {
                foreach (Delegate handler in _eventHandlers[gameStates])
                {
                    if (handler is Action action)
                    {
                        action();
                        Debug.Log($"Broadcasted event for game state {gameStates} to handler {handler.Method.Name}");
                    }
                }
            }
        }

        public void Broadcast<T>(GameStates gameStates, T arg)
        {
            if (_eventHandlers.ContainsKey(gameStates))
            {
                foreach (Delegate handler in _eventHandlers[gameStates])
                {
                    if (handler is Action<T> action)
                    {
                        action(arg);
                        Debug.Log(
                            $"Broadcasted event for game state {gameStates} with argument {arg} to handler {handler.Method.Name}");
                    }
                }
            }
        }

        public void Broadcast<T1, T2>(GameStates gameStates, T1 arg1, T2 arg2)
        {
            if (_eventHandlers.ContainsKey(gameStates))
            {
                foreach (Delegate handler in _eventHandlers[gameStates])
                {
                    if (handler is Action<T1, T2> action)
                    {
                        Debug.Log(
                            $"Broadcasting event {gameStates.ToString()} with arguments {arg1.ToString()} and {arg2.ToString()}");
                        action(arg1, arg2);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"No event handlers found for {gameStates.ToString()}");
            }
        }
    }
}