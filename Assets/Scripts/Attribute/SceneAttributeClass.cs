using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SceneAttribute : PropertyAttribute { }