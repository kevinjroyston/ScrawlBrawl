using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoystonGame.TV.GameEngine.Rendering;

namespace RoystonGame.TV.GameEngine.Rendering
{
    public class TextObject : GameObject
    {
        public string SpriteFontName { get; set; } = "DefaultFont";
        public Color FontColor { get; set; } = Color.Black;
        public string Content { get; set; }

        // TODO: override / add extension to rectangle to define based on screen size / anchors. allow dynamic resizing!
        public override Rectangle BoundingBox { get; set; } = new Rectangle(50, 50, 1700, 900);

        private SpriteFont MyFont { get; set; }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(this.MyFont, this.Content, this.BoundingBox, this.FontColor);
        }

        public override void LoadContent(ContentManager content, GraphicsDevice graphics)
        {
            this.MyFont = content.Load<SpriteFont>(this.SpriteFontName);
        }

        public override void UnloadContent()
        {
            // Empty
        }

        public override void Update(GameTime gameTime)
        {
            // Empty
        }
    }
}
