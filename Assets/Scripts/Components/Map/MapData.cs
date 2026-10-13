using System.Collections.Generic;
using UnityEngine;

namespace Components.Map
{
    [CreateAssetMenu(fileName = "MapData", menuName = "Rooms/MapData")]
    public class MapData : ScriptableObject
    {
        [SerializeField] private List<MapRoomEntry> _rooms = new();

        public IReadOnlyList<MapRoomEntry> Rooms => _rooms;
    }
}