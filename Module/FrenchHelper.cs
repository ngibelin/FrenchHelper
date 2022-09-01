using Microsoft.Xna.Framework;
using Celeste.Mod;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Monocle;
using System.Threading.Tasks;
using Celeste.Mod.Procedurline;

namespace FrenchHelper
{
    public static class FrenchHelper
    {
        public static Dictionary<string, Color> ColorHelper;

        /// <summary>
        /// Returns a Color instance from an RGB string
        /// Taken with authorisation from Viv's Helper
        /// </summary>
        /// <param name="s">hexadecimal string</param>
        /// <returns>Color instance</returns>
        public static Color ColorFix(string s)
        {
            try
            {
                if (ColorHelper.ContainsKey(s.ToLower()))
                {
                    return ColorHelper[s.ToLower()];
                }
            }catch(Exception ex)
            {
                Logger.Log(LogLevel.Error, "FrenchHelper.ColorFix", ex.Message + " detail : s= " + s);
            }

            return AdvHexToColor(s);
        }

        /// <summary>
        /// Returns a Color instance from an RGB string
        /// Taken with authorisation from Viv's Helper
        /// </summary>
        /// <param name="hex">hexadecimal string</param>
        /// <returns>Color instance</returns>
        public static Color AdvHexToColor(string hex)
        {
            string text = hex.Trim('#');
            if (text.StartsWith("0x"))
            {
                text = text.Substring(2);
            }

            if (text.Length == 6 && int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
            {
                return Calc.HexToColor(result);
            }

            if (text.Length == 8 && text.Substring(0, 2) == "00" && Regex.IsMatch(text.Substring(2), "[^0-9a-f]"))
            {
                return Color.Transparent;
            }

            if (int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
            {
                return AdvHexToColor(result);
            }

            return Color.Transparent;
        }

        /// <summary>
        /// Returns a Color instance from an RGB int
        /// Taken with authorisation from Viv's Helper
        /// </summary>
        /// <param name="hex"></param>
        /// <returns>Color instance</returns>
        public static Color AdvHexToColor(int hex)
        {
            Color result = default(Color);
            result.A = (byte)(hex >> 24);
            result.R = (byte)(hex >> 16);
            result.G = (byte)(hex >> 8);
            result.B = (byte)hex;
            return result;
        }

        public static bool TryGetModule(EverestModuleMetadata meta, out EverestModule module)
        {
            foreach (EverestModule other in Everest.Modules)
            {
                if (SatisfiesDependency(meta, other.Metadata))
                {
                    module = other;
                    return true;
                }
            }
            module = null;
            return false;
        }

        public static bool SatisfiesDependency(EverestModuleMetadata meta, EverestModuleMetadata dependency)
        {
            return meta.Name == dependency.Name && Everest.Loader.VersionSatisfiesDependency(meta.Version, dependency.Version);
        }

        /// <summary>
        /// bubble state jellyfish white platform drawing
        /// </summary>
        /// <param name="num"></param>
        /// <param name="timeActive"></param>
        /// <returns></returns>
        public static Vector2 PlatformAdd(int num, float timeActive, int size=24)
        {
            return new Vector2(-12 + num, -5 + (int)Math.Round(Math.Sin(timeActive + (float)num * 0.2f) * 1.7999999523162842));
        }

        /// <summary>
        /// bubble state jellyfish white platform drawing
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static Color PlatformColor(int num)
        {
            if (num <= 1 || num >= 22)
            {
                return Color.White * 0.4f;
            }
            return Color.White * 0.8f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="animId"></param>
        public static void ReloadAnimation(Sprite sprite, string animId = null)
        {
            // retrieve the current entity animation
            string current_animation = sprite.CurrentAnimationID;

            if(!sprite.Animating || string.IsNullOrEmpty(current_animation))
            {
                current_animation = sprite.LastAnimationID;
            }
            if(!string.IsNullOrEmpty(current_animation)
                && (animId is null || current_animation.Equals(animId,StringComparison.OrdinalIgnoreCase)))
            {
                sprite.Texture = null;
            }
        }
    }


}
