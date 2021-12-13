using MelonLoader;
using UnityEngine;
using System.IO;


namespace GestureIndicator
{
    class LoadAssets
    {
        public static Sprite FingerGun, Fist, OpenHand, RockAndRoll, ThumbsUp, Victory, Point;
        public static void loadAssets()
        {
            FingerGun = LoadEmbeddedImages("FingerGun.png");
            Fist = LoadEmbeddedImages("Fist.png");
            OpenHand = LoadEmbeddedImages("OpenHand.png");
            RockAndRoll = LoadEmbeddedImages("RockAndRoll.png");
            ThumbsUp = LoadEmbeddedImages("ThumbsUp.png");
            Victory = LoadEmbeddedImages("Victory.png");
            Point = LoadEmbeddedImages("Point.png");
        }

        private static Sprite LoadEmbeddedImages(string imageName)
        {
            try
            {
                //Load image into Texture
                using var assetStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("GestureIndicator.Images." + imageName);
                using var tempStream = new MemoryStream((int)assetStream.Length);
                assetStream.CopyTo(tempStream);
                var Texture2 = new Texture2D(2, 2);
                ImageConversion.LoadImage(Texture2, tempStream.ToArray());
                Texture2.name = imageName.Replace(".png", "") + "-Tex";
                Texture2.wrapMode = TextureWrapMode.Clamp;
                Texture2.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                //Texture to Sprite
                var rec = new Rect(0.0f, 0.0f, Texture2.width, Texture2.height);
                var piv = new Vector2(.5f, 5f);
                var border = Vector4.zero;
                var s = Sprite.CreateSprite_Injected(Texture2, ref rec, ref piv, 100.0f, 0, SpriteMeshType.Tight, ref border, false);
                s.name = imageName.Replace(".png","") + "-Sprite";
                s.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                return s;
            }
            catch (System.Exception ex) { GestureIndicator.Logger.Error("Failed to load image: " + imageName + "\n" + ex.ToString()); return null; }
        }
    }
}
