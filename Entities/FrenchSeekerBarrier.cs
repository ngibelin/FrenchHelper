using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using FrenchHelper.Module;
using System;

namespace FrenchHelper.Entities
{
    [CustomEntity(new string[] { "FrenchHelper/FrenchSeekerBarrier" })]
    [TrackedAs(typeof(SeekerBarrier))]
    internal class FrenchSeekerBarrier : CustomSeekerBarrier
    {
        public FrenchSeekerBarrier(EntityData data, Vector2 offset): base(data, offset)
        {
            
        }

        public static void Load()
        {
            if (!FrenchHelperModule.MaxHelpingHandLoaded)
            {
                throw new Exception("French Seeker Barrier attempted to load without Max's Helping Hand as a dependency.");
            }
        }

        public static void Unload()
        { }

        public static void LoadContent()
        { }
    }
}
