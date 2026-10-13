using System;
using Components.Rooms;
using UnityEngine;

namespace Components.Map
{
    [Serializable]
    public class RoomDoorLink
    {
        [SerializeField] private DoorDirection _from;
        [SerializeField, Min(0)] private int _toRoomIndex;
        [SerializeField] private DoorDirection _to;

        public DoorDirection From => _from;
        public int ToRoomIndex => _toRoomIndex;
        public DoorDirection To => _to;
    }
}