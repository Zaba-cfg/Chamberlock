using UnityEngine;

namespace Components.Rooms
{
    [RequireComponent(typeof(RoomController))]

    public class RoomDebugger : MonoBehaviour
    {
        private RoomController _room;

        private void Awake()
        {
            _room = GetComponent<RoomController>();
        }

        private void OnEnable()
        {
            _room.RoomActivated += OnRoomActivated;
            _room.RoomCleared += OnRoomCleared;
        }

        private void OnDisable()
        {
            _room.RoomActivated -= OnRoomActivated;
            _room.RoomCleared -= OnRoomCleared;
        }

        private void OnRoomActivated()
        {
            UnityEngine.Debug.Log("Room activated: " + gameObject.name);
        }

        private void OnRoomCleared()
        {
            UnityEngine.Debug.Log("Room cleared: " + gameObject.name);
        }
    }
}