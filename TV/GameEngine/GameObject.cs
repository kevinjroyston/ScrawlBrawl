using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameEngine
{
    public abstract class GameObject
    {
        public GameObject()
        {
            GameManager.RegisterGameObject(this);
        }

        //Current implementation will result in this being called several times in some cases. Working under the assumption that content loading of the same asset is memoized.
        public abstract void LoadContent(ContentManager content, GraphicsDevice graphics);
        public abstract void UnloadContent();
        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract Rectangle BoundingBox { get; set; }
    }
}
