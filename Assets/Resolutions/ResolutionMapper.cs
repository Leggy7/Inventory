using System.Collections.Generic;
using UnityEngine;

namespace Resolutions
{
    public static class ResolutionMapper
    {
        public static readonly Dictionary<ResolutionType, Vector2> Map = new Dictionary<ResolutionType, Vector2>()
        {
            {ResolutionType.Hd, new Vector2(1280, 720)},
            {ResolutionType.Fhd, new Vector2(1920, 1080)},
            {ResolutionType.Uhd, new Vector2(3860, 2160)}
        };
        
        public static ResolutionType Current { get; private set; } = ResolutionType.Fhd;

        private static readonly List<ResolutionType> ResolutionTypes = new List<ResolutionType>()
        {
            ResolutionType.Hd,
            ResolutionType.Fhd,
            ResolutionType.Uhd
        };

        public static void NextResolution()
        {
            var index = ResolutionTypes.FindIndex(x => x == Current) + 1;
            
            if (index >= ResolutionTypes.Count)
                index = 0;
            
            SetResolution(ResolutionTypes[index]);
        }

        public static void PreviousResolution()
        {
            var index = ResolutionTypes.FindIndex(x => x == Current) - 1;
            
            if (index < 0)
                index = ResolutionTypes.Count - 1;
            
            SetResolution(ResolutionTypes[index]);
        }

        public static void SetResolution(ResolutionType res)
        {
            var value = NameToValue(res);
            Screen.SetResolution(
                (int)value.x, 
                (int)value.y, 
                value.x >= Screen.currentResolution.width );
            
            Current = res;
        }
        
        private static Vector2 NameToValue(ResolutionType name)
        {
            return Map[name];
        }
    }
}
