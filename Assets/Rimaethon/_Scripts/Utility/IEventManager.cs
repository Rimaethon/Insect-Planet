using System;

namespace Rimaethon._Scripts.Managers
{
    public interface IEventManager
    {
        void AddHandler(GameStates gameStates, Action handler);
        void AddHandler<T>(GameStates gameStates, Action<T> handler);
        void AddHandler<T1, T2>(GameStates gameStates, Action<T1, T2> handler);
        void RemoveHandler(GameStates gameStates, Action handler);
        void RemoveHandler<T>(GameStates gameStates, Action<T> handler);
        void RemoveHandler<T1, T2>(GameStates gameStates, Action<T1, T2> handler);
        void Broadcast(GameStates gameStates);
        void Broadcast<T>(GameStates gameStates, T arg);
        void Broadcast<T1, T2>(GameStates gameStates, T1 arg1, T2 arg2);
    }
}