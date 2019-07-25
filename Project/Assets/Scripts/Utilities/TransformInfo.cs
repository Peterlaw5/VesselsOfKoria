using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VoK
{
    public class TransformInfo
    {
        public Vector3 position;
        public Quaternion rotation;

        public override string ToString()
        {
            return NetBehaviour.SerializeMessage(position.x.ToString("F2"), position.y.ToString("F2"), position.z.ToString("F2"), rotation.eulerAngles.x.ToString("F2"), rotation.eulerAngles.y.ToString("F2"), rotation.eulerAngles.z.ToString("F2"));
        }

        public TransformInfo(string serialized)
        {
            string[] strArray = NetBehaviour.DeserializeMessage(serialized);
            position = new Vector3(System.Convert.ToSingle(strArray[0]), System.Convert.ToSingle(strArray[1]), System.Convert.ToSingle(strArray[2]));
            rotation = Quaternion.Euler(System.Convert.ToSingle(strArray[3]), System.Convert.ToSingle(strArray[4]), System.Convert.ToSingle(strArray[5]));
        }
        public TransformInfo(Vector3 pos, Vector3 rotAngles)
        {
            position = pos;
            rotation = Quaternion.Euler(rotAngles.x, rotAngles.y, rotAngles.z);
        }
        public TransformInfo(Transform t)
        {
            position = t.position;
            rotation = Quaternion.Euler(t.rotation.eulerAngles.x, t.rotation.eulerAngles.y, t.rotation.eulerAngles.z);
        }
    }
}
