using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using FrenchHelper.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Procedurline;

namespace FrenchHelper.Entities
{
    [CustomEntity("FrenchHelper/StrongJelly")]
    internal class StrongJelly : Actor
    {
        public ParticleType P_Glide;

        public ParticleType P_GlideUp;

        public ParticleType P_Platform;

        public ParticleType P_Glow;

        public ParticleType P_Expand;

        private const float HighFrictionTime = 0.5f;

        public Vector2 Speed;

        public Holdable Hold;

        private Level level;

        private Collision onCollideH;

        private Collision onCollideV;

        private Vector2 prevLiftSpeed;

        private Vector2 startPos;

        private float noGravityTimer;

        private float highFrictionTimer;

        private bool bubble;

        private bool tutorial;

        private bool destroyed;

        private Sprite sprite;

        //private MixerSprite mSprite;

        private Wiggler wiggler;

        private SineWave platformSine;

        private SoundSource fallingSfx;

        private BirdTutorialGui tutorialGui;

        private bool customColor;

        private string GlidePath;

        private string barrierBehavior;

        private Color jellyColor;

        private string sColor;

        private Dictionary<string, Color> jellyToBarrierColors;

        public StrongJelly(EntityData e, Vector2 offset)
            : base(e.Position + offset)
        {

            jellyToBarrierColors = new Dictionary<string, Color>()
            {
                {"glider", Color.White},
                {"deep_blue",FrenchHelper.ColorFix("5555FF")},
                {"green",FrenchHelper.ColorFix("71C837")},
                {"grey",FrenchHelper.ColorFix("808080")},
                {"orange",FrenchHelper.ColorFix("FF7F2A")},
                {"pink",FrenchHelper.ColorFix("D42AFF")},
                {"purple",FrenchHelper.ColorFix("7F2AFF")},
                {"red",FrenchHelper.ColorFix("FF2A2A")},
                {"yellow",FrenchHelper.ColorFix("FFD42A")}
            };

            bubble = e.Bool("bubble");
            tutorial = e.Bool("tutorial");
            startPos = Position;
            base.Collider = new Hitbox(8f, 10f, -4f, -10f);
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;

            this.SpriteManager(e);

        }

        /// <summary>
        /// Create or update the jellyfich sprite
        /// </summary>
        /// <param name="e"></param>
        private void SpriteManager(EntityData e)
        {
            string jellySpritePath = e.Attr("jellyColor", "objects/glider").Trim('/');

            this.SpriteManager(
                jellySpritePath
                , e.Bool("customColor")
                , e.Attr("barrierBehavior", "AllThrough")
                );
        }

        private void SpriteManager(String jellySpritePath, bool customColor, String barrierBehavior)
        {
            Remove(sprite);

            sprite = new Sprite(GFX.Game, jellySpritePath + "/");
            sprite.Justify = new Vector2(0.5f, 0.58f);
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.AddLoop("held", "held", 0.1f);
            sprite.AddLoop("fallLoop", "fallLoop", 0.06f);
            sprite.Add("fall", "fall", 0.06f, "fallLoop");
            sprite.Add("death", "death", 0.06f);
            Add(sprite);

            sprite.Play("idle");

            Remove(wiggler);
            Add(wiggler = Wiggler.Create(0.25f, 4f));

            Remove(Hold);
            Add(Hold = new Holdable(0.3f));
            Hold.PickupCollider = new Hitbox(20f, 22f, -10f, -16f);
            Hold.SlowFall = true;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.SpeedGetter = () => Speed;
            Hold.OnHitSpring = HitSpring;
            platformSine = new SineWave(0.3f, 0f);
            Remove(platformSine);
            Add(platformSine);
            fallingSfx = new SoundSource();
            Remove(fallingSfx);
            Add(fallingSfx);
            Add(new WindMover(WindMode));
            GlidePath = "particles/rect";
            this.customColor = customColor;
            this.barrierBehavior = barrierBehavior;
            this.sColor = jellySpritePath.Split('/')[jellySpritePath.Split('/').Length - 1];
            jellyColor = jellyToBarrierColors[this.sColor];

            P_Glide = new ParticleType
            {
                Acceleration = Vector2.UnitY * 60f,
                SpeedMin = 30f,
                SpeedMax = 40f,
                Direction = -(float)Math.PI / 2f,
                DirectionRange = (float)Math.PI / 2f,
                LifeMin = 0.6f,
                LifeMax = 1.2f,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Color = customColor ? jellyColor : FrenchHelper.ColorFix("4FFFF3"),
                Color2 = customColor ? jellyColor : FrenchHelper.ColorFix("FFF899"),
                Size = 0.5f,
                SizeRange = 0.2f,
                RotationMode = ParticleType.RotationModes.SameAsDirection
            };
            if (!string.IsNullOrWhiteSpace(GlidePath))
            {
                P_Glide.Source = GFX.Game[GlidePath];
            }
            P_GlideUp = new ParticleType(Glider.P_Glide)
            {
                Acceleration = Vector2.UnitY * -10f,
                SpeedMin = 50f,
                SpeedMax = 60f
            };
            P_Glow = new ParticleType
            {
                SpeedMin = 8f,
                SpeedMax = 16f,
                DirectionRange = (float)Math.PI * 2f,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                Size = 1f,
                FadeMode = ParticleType.FadeModes.Late,
                Color = customColor ? jellyColor : FrenchHelper.ColorFix("B7F3FF"),
                Color2 = customColor ? jellyColor : FrenchHelper.ColorFix("F4FDFF"),
                ColorMode = ParticleType.ColorModes.Blink
            };
            P_Expand = new ParticleType(Glider.P_Glow)
            {
                SpeedMin = 40f,
                SpeedMax = 80f,
                SpeedMultiplier = 0.2f,
                LifeMin = 0.6f,
                LifeMax = 1.2f,
                DirectionRange = (float)Math.PI * 3f / 4f
            };
            P_Platform = new ParticleType
            {
                Acceleration = Vector2.UnitY * 60f,
                SpeedMin = 5f,
                SpeedMax = 20f,
                Direction = -(float)Math.PI / 2f,
                LifeMin = 0.6f,
                LifeMax = 1.4f,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            if (tutorial)
            {
                tutorialGui = new BirdTutorialGui(this, new Vector2(0f, -24f), Dialog.Clean("tutorial_carry"), Dialog.Clean("tutorial_hold"), Input.Grab);
                tutorialGui.Open = true;
                base.Scene.Add(tutorialGui);
            }
        }

        public override void Update()
        {
            if (base.Scene.OnInterval(0.05f))
            {
                level.Particles.Emit(P_Glow, 1, base.Center + Vector2.UnitY * -9f, new Vector2(10f, 4f));
            }
            float target = ((!Hold.IsHeld) ? 0f : ((!Hold.Holder.OnGround()) ? Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, (float)Math.PI / 3f, -(float)Math.PI / 3f) : Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, 0.6981317f, -0.6981317f)));
            sprite.Rotation = Calc.Approach(sprite.Rotation, target, (float)Math.PI * Engine.DeltaTime);
            if (Hold.IsHeld && !Hold.Holder.OnGround() && (sprite.CurrentAnimationID == "fall" || sprite.CurrentAnimationID == "fallLoop"))
            {
                if (!fallingSfx.Playing)
                {
                    Audio.Play("event:/new_content/game/10_farewell/glider_engage", Position);
                    fallingSfx.Play("event:/new_content/game/10_farewell/glider_movement");
                }
                Vector2 speed = Hold.Holder.Speed;
                Vector2 vector = new Vector2(speed.X * 0.5f, (speed.Y < 0f) ? (speed.Y * 2f) : speed.Y);
                float value = Calc.Map(vector.Length(), 0f, 120f, 0f, 0.7f);
                fallingSfx.Param("glider_speed", value);
            }
            else
            {
                fallingSfx.Stop();
            }
            base.Update();
            if (!destroyed)
            {
                foreach (SeekerBarrier entity in base.Scene.Tracker.GetEntities<SeekerBarrier>())
                {
                    entity.Collidable = true;
                    bool flag = CollideCheck(entity);
                    entity.Collidable = false;
                    if (flag)
                    {
                        FieldInfo fi = typeof(CustomSeekerBarrier).GetField("renderer", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fi != null && entity is CustomSeekerBarrier fsb)
                        {

                            byte csbR = SeekerBarrierColorController.GetCurrentBarrierColor(fi.GetValue(fsb) as SeekerBarrierRenderer).R;
                            byte csbG = SeekerBarrierColorController.GetCurrentBarrierColor(fi.GetValue(fsb) as SeekerBarrierRenderer).G;
                            byte csbB = SeekerBarrierColorController.GetCurrentBarrierColor(fi.GetValue(fsb) as SeekerBarrierRenderer).B;

                            if ((csbR != jellyColor.R || csbG != jellyColor.G || csbB != jellyColor.B) && (barrierBehavior == "ColorCoded" || barrierBehavior == "ColorCollide") || barrierBehavior == "Vanilla")
                            {

                                destroyed = true;
                                Collidable = false;
                                if (Hold.IsHeld)
                                {
                                    Vector2 speed2 = Hold.Holder.Speed;
                                    Hold.Holder.Drop();
                                    Speed = speed2 * 0.333f;
                                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                                }
                                Add(new Coroutine(DestroyAnimationRoutine()));
                                return;

                            }
                            else if(csbR == jellyColor.R && csbG == jellyColor.G && csbB == jellyColor.B && barrierBehavior == "ColorCollide")
                            {
                                entity.Collidable = true;
                            }
                        }
                        else
                        {
                            destroyed = true;
                            Collidable = false;
                            if (Hold.IsHeld)
                            {
                                Vector2 speed2 = Hold.Holder.Speed;
                                Hold.Holder.Drop();
                                Speed = speed2 * 0.333f;
                                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                            }
                            Add(new Coroutine(DestroyAnimationRoutine()));
                            return;
                        }
                    }
                }
                
                if (Hold.IsHeld)
                {
                    prevLiftSpeed = Vector2.Zero;
                }
                else if (!bubble)
                {
                    if (highFrictionTimer > 0f)
                    {
                        highFrictionTimer -= Engine.DeltaTime;
                    }
                    if (OnGround())
                    {
                        float target2 = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
                        Speed.X = Calc.Approach(Speed.X, target2, 800f * Engine.DeltaTime);
                        Vector2 liftSpeed = base.LiftSpeed;
                        if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                        {
                            Speed = prevLiftSpeed;
                            prevLiftSpeed = Vector2.Zero;
                            Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                            if (Speed.X != 0f && Speed.Y == 0f)
                            {
                                Speed.Y = -60f;
                            }
                            if (Speed.Y < 0f)
                            {
                                noGravityTimer = 0.15f;
                            }
                        }
                        else
                        {
                            prevLiftSpeed = liftSpeed;
                            if (liftSpeed.Y < 0f && Speed.Y < 0f)
                            {
                                Speed.Y = 0f;
                            }
                        }
                    }
                    else if (Hold.ShouldHaveGravity)
                    {
                        float num = 200f;
                        if (Speed.Y >= -30f)
                        {
                            num *= 0.5f;
                        }
                        float num2 = ((Speed.Y < 0f) ? 40f : ((!(highFrictionTimer <= 0f)) ? 10f : 40f));
                        Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                        if (noGravityTimer > 0f)
                        {
                            noGravityTimer -= Engine.DeltaTime;
                        }
                        else if (level.Wind.Y < 0f)
                        {
                            Speed.Y = Calc.Approach(Speed.Y, 0f, num * Engine.DeltaTime);
                        }
                        else
                        {
                            Speed.Y = Calc.Approach(Speed.Y, 30f, num * Engine.DeltaTime);
                        }
                    }
                    MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                    MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                    if (base.Left < (float)level.Bounds.Left)
                    {
                        base.Left = level.Bounds.Left;
                        CollisionData collisionData = default(CollisionData);
                        collisionData.Direction = -Vector2.UnitX;
                        CollisionData data = collisionData;
                        OnCollideH(data);
                    }
                    else if (base.Right > (float)level.Bounds.Right)
                    {
                        base.Right = level.Bounds.Right;
                        CollisionData collisionData = default(CollisionData);
                        collisionData.Direction = Vector2.UnitX;
                        CollisionData data = collisionData;
                        OnCollideH(data);
                    }
                    if (base.Top < (float)level.Bounds.Top)
                    {
                        base.Top = level.Bounds.Top;
                        CollisionData collisionData = default(CollisionData);
                        collisionData.Direction = -Vector2.UnitY;
                        CollisionData data = collisionData;
                        OnCollideV(data);
                    }
                    else if (base.Top > (float)(level.Bounds.Bottom + 16))
                    {
                        RemoveSelf();
                        return;
                    }
                    Hold.CheckAgainstColliders();
                }
                else
                {
                    Position = startPos + Vector2.UnitY * platformSine.Value * 1f;
                }
                Vector2 one = Vector2.One;
                if (!Hold.IsHeld)
                {
                    if (level.Wind.Y < 0f)
                    {
                        PlayOpen();
                    }
                    else
                    {
                        sprite.Play("idle");
                    }
                }
                else if (Hold.Holder.Speed.Y > 20f || level.Wind.Y < 0f)
                {
                    if (level.OnInterval(0.04f))
                    {
                        if (level.Wind.Y < 0f)
                        {
                            level.ParticlesBG.Emit(P_GlideUp, 1, Position - Vector2.UnitY * 20f, new Vector2(6f, 4f));
                        }
                        else
                        {
                            level.ParticlesBG.Emit(P_Glide, 1, Position - Vector2.UnitY * 10f, new Vector2(6f, 4f));
                        }
                    }
                    PlayOpen();
                    if (Input.GliderMoveY.Value > 0)
                    {
                        one.X = 0.7f;
                        one.Y = 1.4f;
                    }
                    else if (Input.GliderMoveY.Value < 0)
                    {
                        one.X = 1.2f;
                        one.Y = 0.8f;
                    }
                    Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
                }
                else
                {
                    sprite.Play("held");
                }
                sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, one.Y, Engine.DeltaTime * 2f);
                sprite.Scale.X = Calc.Approach(sprite.Scale.X, (float)Math.Sign(sprite.Scale.X) * one.X, Engine.DeltaTime * 2f);

            }
            else
            {
                Position += Speed * Engine.DeltaTime;
            }


            try
            {
                //let's look for all JellyBlock entities in the chapter
                foreach(JellyBlock entity in base.Scene.Tracker.GetEntities<JellyBlock>())
                {
                    if (this.Intersects(entity.Collider as Hitbox))
                    {
                        if (this.sColor != entity.color)
                        {
                            this.SpriteManager("objects/FrenchHelper/StrongJelly/" + entity.color, customColor, barrierBehavior);
                        }

                    }
                }                
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Log(LogLevel.Error, "StrongJelly.Update", knfe.Message);
            }
        }

        /// <summary>
        /// Check if the jellyfish has hit the jellyBlock from any left, right or top side
        /// </summary>
        /// <param name="hb">the hitbox of the jellyblock</param>
        /// <returns>true if hit, false otherwise</returns>
        private bool Intersects(Hitbox hb)
        {
            // check interaction from left or right
            bool sideHit = (this.Collider.AbsoluteLeft == hb.AbsoluteRight || this.Collider.AbsoluteRight == hb.AbsoluteLeft)
                && this.Collider.AbsoluteBottom >= hb.AbsoluteTop
                && this.Collider.AbsoluteTop <= hb.AbsoluteBottom;
            // if it didn't take a side hit...
            if(!sideHit)
            {
                // let's check a top hit
                bool topHit = this.Collider.AbsoluteBottom == hb.AbsoluteTop
                && this.Collider.AbsoluteLeft <= hb.AbsoluteRight
                && this.Collider.AbsoluteRight >= hb.AbsoluteLeft;

                return topHit;
            }
            return true;
        }

        private void PlayOpen()
        {
            if (sprite.CurrentAnimationID != "fall" && sprite.CurrentAnimationID != "fallLoop")
            {
                sprite.Play("fall");
                sprite.Scale = new Vector2(1.5f, 0.6f);
                level.Particles.Emit(P_Expand, 16, base.Center + (Vector2.UnitY * -12f).Rotate(sprite.Rotation), new Vector2(8f, 3f), -(float)Math.PI / 2f + sprite.Rotation);
                if (Hold.IsHeld)
                {
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                }
            }
        }

        public override void Render()
        {
            if (!destroyed)
            {
                sprite.DrawSimpleOutline();
            }
            base.Render();
            if (bubble)
            {
                for (int i = 0; i < 24; i++)
                {
                    Draw.Point(Position + FrenchHelper.PlatformAdd(i, base.Scene.TimeActive), FrenchHelper.PlatformColor(i));
                }
            }
        }

        private void WindMode(Vector2 wind)
        {
            if (!Hold.IsHeld)
            {
                if (wind.X != 0f)
                {
                    MoveH(wind.X * 0.5f);
                }
                if (wind.Y != 0f)
                {
                    MoveV(wind.Y);
                }
            }
        }

        

        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            if (Speed.X < 0f)
            {
                Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_left", Position);
            }
            else
            {
                Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_right", Position);
            }
            Speed.X *= -1f;
            sprite.Scale = new Vector2(0.8f, 1.2f);
        }

        private void OnCollideV(CollisionData data)
        {
            if (Math.Abs(Speed.Y) > 8f)
            {
                sprite.Scale = new Vector2(1.2f, 0.8f);
                Audio.Play("event:/new_content/game/10_farewell/glider_land", Position);
            }
            if (Speed.Y < 0f)
            {
                Speed.Y *= -0.5f;
            }
            else
            {
                Speed.Y = 0f;
            }
        }

        private void OnPickup()
        {
            if (bubble)
            {
                for (int i = 0; i < 24; i++)
                {
                    level.Particles.Emit(P_Platform, Position + FrenchHelper.PlatformAdd(i, base.Scene.TimeActive), FrenchHelper.PlatformColor(i));
                }
            }
            AllowPushing = false;
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            highFrictionTimer = 0.5f;
            bubble = false;
            wiggler.Start();
            tutorial = false;
        }

        private void OnRelease(Vector2 force)
        {
            if (force.X == 0f)
            {
                Audio.Play("event:/new_content/char/madeline/glider_drop", Position);
            }
            AllowPushing = true;
            RemoveTag(Tags.Persistent);
            force.Y *= 0.5f;
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            Speed = force * 100f;
            wiggler.Start();
        }

        protected override void OnSquish(CollisionData data)
        {
            if (!TrySquishWiggle(data))
            {
                RemoveSelf();
            }
        }

        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    wiggler.Start();
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 160f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    wiggler.Start();
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -160f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    wiggler.Start();
                    return true;
                }
            }
            return false;
        }

        private IEnumerator DestroyAnimationRoutine()
        {
            Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", Position);
            sprite.Play("death");
            yield return 1f;
            RemoveSelf();
        }

        public static void Load()
        {
            if (!FrenchHelperModule.MaxHelpingHandLoaded)
            {
                throw new Exception("Strong Jelly attempted to load without Max's Helping Hand as a dependency.");
            }
        }

        public static void Unload()
        { }

        public static void LoadContent()
        { }

    }
}
