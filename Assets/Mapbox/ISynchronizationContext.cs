using System;
using UnityEngine;


namespace Assets.Mapbox
{
    
    public interface ISynchronizationContext
    {
        event Action<Alignment> OnAlignmentAvailable;
    }

    public struct Alignment
    {
        public Vector3 Position;
        public float Rotation;
    }
}
