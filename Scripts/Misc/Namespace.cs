using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class MathExtension
    {
        public static float GetInBetween(float a, float b)
        {
            if (a == b)
            {
                return a;
            }

            float big = Mathf.Max(a, b);
            float small = Mathf.Min(a, b);

            return small + (big - small) / 2;
        }

        public static Vector2 Vector2Clamp(Vector2 value, Vector2 min, Vector2 max)
        {
            return new Vector2(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));
        }

        public static Vector3 Vector3Clamp(Vector3 value, Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y), Mathf.Clamp(value.z, min.z, max.z));
        }

        public static float PointToward(Vector2 me, Vector2 target)
        {
            return (float)Mathf.Atan2(target.y - me.y, target.x - me.x) * Mathf.Rad2Deg;
        }
    }

    public class Get
    {
        public static PlayerController Player()
        {
            return GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }

        public static Dialog Dialog()
        {
            return GameObject.FindWithTag("Dialog").GetComponent<Dialog>();
        }

        public static SoundManager SoundManager()
        {
            return GameObject.FindWithTag("Sound Manager").GetComponent<SoundManager>();
        }

        public static DialogOptions DialogOptions()
        {
            return GameObject.FindWithTag("Dialog Options").GetComponent<DialogOptions>();
        }
    }
}
