using Components.Rooms;
using UnityEngine;

namespace Components.Map
{
    [RequireComponent(typeof(Camera))]
    public class RoomCamera : MonoBehaviour
    {
        public void Frame(RoomController room)
        {
            if (room == null)
                return;

            transform.position = new Vector3(
                room.transform.position.x,
                room.transform.position.y,
                transform.position.z);
        }
    }
}