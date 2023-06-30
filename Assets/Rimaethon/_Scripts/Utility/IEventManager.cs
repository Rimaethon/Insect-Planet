using System;
using Rimaethon._Scripts.Utility;

namespace Rimaethon._Scripts.Managers
{
    public interface IEventManager
    {
        void AddHandler(GameEvents gameEvents, Action handler);
        void AddHandler<T>(GameEvents gameEvents, Action<T> handler);
        void AddHandler<T1, T2>(GameEvents gameEvents, Action<T1, T2> handler);
        void RemoveHandler(GameEvents gameEvents, Action handler);
        void RemoveHandler<T>(GameEvents gameEvents, Action<T> handler);
        void RemoveHandler<T1, T2>(GameEvents gameEvents, Action<T1, T2> handler);
        void Broadcast(GameEvents gameEvents);
        void Broadcast<T>(GameEvents gameEvents, T arg);
        void Broadcast<T1, T2>(GameEvents gameEvents, T1 arg1, T2 arg2);
    }
}