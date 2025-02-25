using Godot;

namespace Minecraft.scripts.utils;

public static class MathUtil
{
    public struct AABB
    {
        public Vector3 Min, Max;

        public AABB(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public static AABB CenterSize(Vector3 center, Vector3 size)
        {
            return new AABB(center - size / 2f, center + size / 2f);
        }
    }
    
    public static bool AABBBoxTest(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax)
    {
        return bMax < aMin || aMax > bMin;
    }
}