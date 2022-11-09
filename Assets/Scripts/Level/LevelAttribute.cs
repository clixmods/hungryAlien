using System;
using UnityEngine;

namespace Level
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class LevelAttribute : PropertyAttribute { }
}