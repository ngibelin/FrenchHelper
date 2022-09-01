using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using ExtendedVariants.Entities.ForMappers;
using FrenchHelper.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace FrenchHelper.Entities
{
    [RegisterStrawberry(false, false)]
    [CustomEntity("FrenchHelper/TheoBerry")]
    public class TheoBerry : Entity, IStrawberry
    {
        public EntityID ID;
        public Follower Follower;

        private Sprite sprite;
        private float wobble = 0f;
        private Wiggler wiggler;
        private float collectTimer = 0f;
        private BloomPoint bloom;
        private VertexLight light;
        private Tween lightTween;
        private Vector2 start;
        private bool isOwned;
        private bool collected = false;
        private Level level;

        public TheoBerry(EntityData data, Vector2 offset, EntityID gid)
        {
            ID = gid;
            Position = (start = data.Position + offset);
            isOwned = SaveData.Instance.CheckStrawberry(ID);
            Depth = -100;
            base.Collider = new Hitbox(14f, 14f, -7f, -7f);
            Add(new PlayerCollider(OnPlayer));
            Add(Follower = new Follower(ID, null, OnLoseLeader));
            Follower.FollowDelay = 0.3f;
        }

        public override void Awake(Scene scene)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            
            if (level.Tracker.CountEntities<ExtendedVariantTheoCrystal>() > 0)
            {
                if (!(level.Tracker.GetEntity<ExtendedVariantTheoCrystal>() as TheoCrystal).Hold.IsHeld || !SceneAs<Level>().Session.StartedFromBeginning)
                {
                    RemoveSelf();
                }
                else
                {
                    sprite = FrenchHelperModule.Instance._spriteBank.Create("FrenchHelperTheoBerry");
                    Add(sprite);
                    sprite.Play("idle");
                    sprite.OnFrameChange = OnAnimate;
                    wiggler = Wiggler.Create(0.4f, 4f, delegate (float v)
                    {
                        sprite.Scale = Vector2.One * (1f + v * 0.35f);
                    });
                    Add(wiggler);
                    bloom = new BloomPoint(isOwned ? 0.25f : 0.5f, 12f);
                    Add(bloom);
                    light = new VertexLight(Color.White, 1f, 16, 24);
                    lightTween = light.CreatePulseTween();
                    Add(light);
                    Add(lightTween);
                    if (SceneAs<Level>().Session.BloomBaseAdd > 0.1f)
                    {
                        bloom.Alpha *= 0.5f;
                    }
                }
            }
            else
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            if (!collected)
            {
                wobble += Engine.DeltaTime * 4f;
                Sprite obj = sprite;
                BloomPoint bloomPoint = bloom;
                float num2 = (light.Y = (float)Math.Sin(wobble) * 2f);
                float num5 = (obj.Y = (bloomPoint.Y = num2));
                if (Follower.Leader != null)
                {
                    Player player = Follower.Leader.Entity as Player;
                    if (Follower.DelayTimer <= 0f && StrawberryRegistry.IsFirstStrawberry(this))
                    {
                        if (player != null && player.Scene != null && !player.StrawberriesBlocked && player.OnSafeGround && player.StateMachine.State != 13)
                        {
                            collectTimer += Engine.DeltaTime;
                            if (collectTimer > 0.15f)
                            {
                                OnCollect();
                            }
                        }
                        else
                        {
                            collectTimer = Math.Min(collectTimer, 0f);
                        }
                    }
                    else if (Follower.FollowIndex > 0)
                    {
                        collectTimer = -0.15f;
                    }
                }
            }
            if (Follower.Leader != null && base.Scene.OnInterval(0.08f))
            {
                ParticleType type = (isOwned ? Strawberry.P_GhostGlow : Strawberry.P_Glow);
                level.ParticlesFG.Emit(type, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
            }
            base.Update();
        }

        private void OnAnimate(string id)
        {
            int num = 35;
            if (sprite.CurrentAnimationFrame == num - 4)
            {
                bool flag = CollideCheck<FakeWall>() || CollideCheck<Solid>();
                if (!collected && flag)
                {
                    Audio.Play("event:/game/general/strawberry_pulse", Position);
                    level.Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f);
                }
                else
                {
                    Audio.Play("event:/game/general/strawberry_pulse", Position);
                    level.Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
                }
            }
        }

        public void OnPlayer(Player player)
        {
            if (Follower.Leader == null && !collected)
            {
                Audio.Play(isOwned ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch", Position);
                player.Leader.GainFollower(Follower);
                wiggler.Start();
                base.Depth = -1000000;
            }
        }

        public void OnCollect()
        {
            if (!collected)
            {
                collected = true;
                int collectIndex = 0;
                if (Follower.Leader != null)
                {
                    Player player = Follower.Leader.Entity as Player;
                    collectIndex = player.StrawberryCollectIndex;
                    player.StrawberryCollectIndex++;
                    player.StrawberryCollectResetTimer = 2.5f;
                    Follower.Leader.LoseFollower(Follower);
                }
                SaveData.Instance.AddStrawberry(ID, golden: false);
                Session session = SceneAs<Level>().Session;
                session.DoNotLoad.Add(ID);
                session.Strawberries.Add(ID);
                session.UpdateLevelStartDashes();
                Add(new Coroutine(CollectRoutine(collectIndex)));
            }
        }

        private IEnumerator CollectRoutine(int collectIndex)
        {
            SceneAs<Level>();
            base.Tag = Tags.TransitionUpdate;
            base.Depth = -2000010;
            Audio.Play(value: isOwned ? 1 : 0, path: "event:/char/theo/yolo_fist", position: Position, param: "colour", param2: "count", value2: collectIndex);
            Audio.Play(value: isOwned ? 1 : 0, path: "event:/french_helper/char/theo/yolo", position: Position, param: "colour", param2: "count", value2: collectIndex);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            sprite.Play("collect");
            while (sprite.Animating)
            {
                yield return null;
            }
            RemoveSelf();
        }

        private void OnLoseLeader()
        {
            if (collected)
            {
                return;
            }

        }

        // Initialize the spritebank.
        public static void LoadContent()
        {
            
        }

        public static void Load()
        {
            if(!FrenchHelperModule.ExtendedVariantLoaded)
            {
                throw new Exception("Theo Berry attempted to load without Extended Variant Mode as a dependency.");
            }
        }
        public static void Unload() { }

    }
}
