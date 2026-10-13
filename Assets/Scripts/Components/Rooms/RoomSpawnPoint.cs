using UnityEngine;

namespace Components.Rooms
{
    public class RoomSpawnPoint : MonoBehaviour
    {
        [SerializeField] private SpawnPointKind _kind = SpawnPointKind.Enemy;

        public SpawnPointKind Kind => _kind;
    }
}
