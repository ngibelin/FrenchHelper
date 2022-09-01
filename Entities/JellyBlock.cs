using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace FrenchHelper.Entities
{
    [CustomEntity(new string[] { "FrenchHelper/JellyBlock" })]
    [Tracked]
    public class JellyBlock : Solid
    {
        private MTexture[,] nineSlice;

        private float startY;

        public string color;

        private float width;

        private float height;

        private Vector2 position;

        public JellyBlock(Vector2 position, float width, float height, string color)
        : base(position, width, height, safe: false)
        {
            startY = base.Y;

            this.width = width;
            this.height = height;
            this.position = position;

            this.color = color == "normal" ? "green" : color;

            MTexture mTexture = GFX.Game[this.GetBlockSpritePath(color)];
            nineSlice = new MTexture[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    nineSlice[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
            base.Depth = -10000;
            Add(new LightOcclude());
            Add(new MirrorSurface());
            SurfaceSoundIndex = 32;
        }

        public JellyBlock(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Attr("color", "normal"))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            DisableStaticMovers();
            Visible = true;
            Collidable = true;
        }

        public override void Update()
        {

        }

        private void DrawBlock(Vector2 offset, Color color)
        {
            float num = base.Collider.Width / 8f - 1f;
            float num2 = base.Collider.Height / 8f - 1f;
            for (int i = 0; (float)i <= num; i++)
            {
                for (int j = 0; (float)j <= num2; j++)
                {
                    int num3 = (((float)i < num) ? Math.Min(i, 1) : 2);
                    int num4 = (((float)j < num2) ? Math.Min(j, 1) : 2);
                    nineSlice[num3, num4].Draw(Position + offset + base.Shake + new Vector2(i * 8, j * 8), Vector2.Zero, color);
                }
            }
            
        }

        public override void Render()
        {
            Level level = base.Scene as Level;
            Vector2 vector = new Vector2(0f, ((float)level.Bounds.Bottom - startY + 32f) * Ease.CubeIn(0));
            Vector2 position = Position;
            Position += vector;
            DrawBlock(new Vector2(-1f, 0f), Color.Black);
            DrawBlock(new Vector2(1f, 0f), Color.Black);
            DrawBlock(new Vector2(0f, -1f), Color.Black);
            DrawBlock(new Vector2(0f, 1f), Color.Black);
            DrawBlock(Vector2.Zero, Color.White);
            
            Position = position;
        }

        public void orig_Update()
        {
            
        }

        /// <summary>
        /// Returns the sprite path based on the color picked
        /// </summary>
        /// <param name="color">color selected in Ahorn</param>
        /// <returns>Path to the jelly sprite</returns>
        private string GetJellySpritePath(string color)
        {
            if (color == "normal")
                color = "green";
                return "objects/FrenchHelper/StrongJelly/" + color + "/idle0";
        }

        ///
        private string GetBlockSpritePath(string color)
        {
            if (color == "normal")
                color = "green";
            return "objects/FrenchHelper/JellyBlock/" + color;
        }

        public static void Load()
        {

        }

        public static void Unload()
        {

        }

        public static void LoadContent()
        { }
    }
}
