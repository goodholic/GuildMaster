using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GuildMaster.Core
{
    public enum EventType
    {
        GuildLevelUp,
        NewAdventurer,
        SpecialMission,
        ResourceBonus,
        NPCVisit,
        BattleVictory,
        ExplorationComplete
    }

    [System.Serializable]
    public class GameEvent
    {
        public EventType eventType;
        public string title;
        public string description;
        public float duration;
        public bool isActive;
        public DateTime startTime;
    }

    public class EventManager : MonoBehaviour
    {
        // Active events
        private List<GameEvent> activeEvents = new List<GameEvent>();
        private Queue<GameEvent> pendingEvents = new Queue<GameEvent>();

        // Event settings
        private float eventCheckInterval = 10f; // 10초마다 이벤트 체크
        private float lastEventCheck;

        // Events
        public event Action<GameEvent> OnEventTriggered;
        public event Action<GameEvent> OnEventCompleted;

        public IEnumerator Initialize()
        {
            Debug.Log("이벤트 시스템 초기화 중...");
            
            lastEventCheck = Time.time;
            
            Debug.Log("이벤트 시스템 초기화 완료");
            yield break;
        }

        void Update()
        {
            UpdateActiveEvents();
            CheckForNewEvents();
        }

        void UpdateActiveEvents()
        {
            for (int i = activeEvents.Count - 1; i >= 0; i--)
            {
                var gameEvent = activeEvents[i];
                
                if (Time.time - lastEventCheck >= gameEvent.duration)
                {
                    CompleteEvent(gameEvent);
                }
            }
        }

        void CheckForNewEvents()
        {
            if (Time.time - lastEventCheck >= eventCheckInterval)
            {
                lastEventCheck = Time.time;
                ProcessPendingEvents();
            }
        }

        void ProcessPendingEvents()
        {
            if (pendingEvents.Count > 0)
            {
                var nextEvent = pendingEvents.Dequeue();
                TriggerEvent(nextEvent);
            }
        }

        public void TriggerEvent(GameEvent gameEvent)
        {
            if (gameEvent == null) return;

            gameEvent.isActive = true;
            gameEvent.startTime = DateTime.Now;
            activeEvents.Add(gameEvent);

            OnEventTriggered?.Invoke(gameEvent);
            
            Debug.Log($"이벤트 발생: {gameEvent.title}");
        }

        public void CompleteEvent(GameEvent gameEvent)
        {
            if (gameEvent == null) return;

            gameEvent.isActive = false;
            activeEvents.Remove(gameEvent);

            OnEventCompleted?.Invoke(gameEvent);
            
            Debug.Log($"이벤트 완료: {gameEvent.title}");
        }

        public void QueueEvent(EventType eventType, string title, string description, float duration = 30f)
        {
            var gameEvent = new GameEvent
            {
                eventType = eventType,
                title = title,
                description = description,
                duration = duration,
                isActive = false
            };

            pendingEvents.Enqueue(gameEvent);
        }

        public List<GameEvent> GetActiveEvents()
        {
            return new List<GameEvent>(activeEvents);
        }

        public bool HasActiveEvent(EventType eventType)
        {
            return activeEvents.Exists(e => e.eventType == eventType);
        }
    }
} 