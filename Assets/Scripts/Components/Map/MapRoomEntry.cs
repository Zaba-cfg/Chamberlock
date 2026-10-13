using System;
using System.Collections.Generic;
using Components.Rooms;
using UnityEngine;

namespace Components.Map
{
    [Serializable]
    public class MapRoomEntry
    {
        [SerializeField] private RoomData _data;
        [SerializeField] private GameObject _roomPrefab;
        [SerializeField] private Vector2 _worldPosition;
        [SerializeField] private List<RoomDoorLink> _links = new();

        public RoomData Data => _data;
        public GameObject RoomPrefab => _roomPrefab;
        public Vector2 WorldPosition => _worldPosition;
        public IReadOnlyList<RoomDoorLink> Links => _links;
    }
}