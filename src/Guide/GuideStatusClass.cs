
using UnityEngine;
using System.Runtime.CompilerServices;
using RWCustom;
using SlugBase.DataTypes;


namespace Guide.Guide
{
    public static class GuideStatusClass
    {
        public class GuideStatus
        {
            // Define your variables to store here!
            //state slippery and countdown
            public int slipperyTime;
            public bool slippery;
            public bool artiSpawn;
            public bool SpearKey;

            public bool IsHarvested;
            public int harvestCounter;

            public readonly bool IsGuide;
            public readonly Player player;

            public bool SpritesReady;
            public int BodySpotsSprite;
            public int HipsSpotsSprite;
            public int LegsSpotsSprite;
            //public int HeadGillsSprite;
            public UpperHavenGills topGills;
            public int FaceBlushSprite;
            public int[] TasselSpriteA = new int[5];
            public int[] TasselSpriteB = new int[5];
            public int[] TailSpots = new int[5];


            public Color BodyColor;
            public Color EyesColor;
            public Color GillsColor;
            public Color SpotsColor;
            public Color TasselAColor;
            public Color TasselBColor;


            public GuideStatus(Player player)
            {
                // Initialize your variables here! (Anything not added here will be null or false or 0 (default values))
                IsGuide = player.slugcatStats.name.value == "Guide";
                if (!IsGuide) return;

                this.player = player;
                artiSpawn = false;
                SpearKey = false;
                harvestCounter = 0;

            }

            public void SetupColors()
            {
                var pg = (PlayerGraphics)player.graphicsModule;

                BodyColor = new PlayerColor("Body").GetColor(pg) ?? Custom.hexToColor("e8f5ca");
                EyesColor = new PlayerColor("Eyes").GetColor(pg) ?? Custom.hexToColor("00271f");
                GillsColor = new PlayerColor("Gills").GetColor(pg) ?? Custom.hexToColor("26593c");
                SpotsColor = new PlayerColor("Spots").GetColor(pg) ?? Custom.hexToColor("60c0bb");
                TasselAColor = new PlayerColor("Tassels").GetColor(pg) ?? Custom.hexToColor("12a23e");
                TasselBColor = new Color(TasselAColor.r - 5, TasselAColor.g - 5, TasselAColor.b - 5, 0.5f);


            }
        }

        // This part lets you access the stored stuff by simply doing "self.GetCat()" in Plugin.cs or everywhere else!
        private static readonly ConditionalWeakTable<Player, GuideStatus> CWT = new();
        public static GuideStatus GetCat(this Player player) => CWT.GetValue(player, _ => new(player));
        public static bool IsGuide(this Player player, out GuideStatus guide) => (guide = player.GetCat()).IsGuide;
        public static bool IsGuide(this PlayerGraphics pg, out GuideStatus guide) => IsGuide(pg.player, out guide);


        public static void Follow(this FSprite sprite, FSprite originalSprite)
        {
            sprite.SetPosition(originalSprite.GetPosition());
            sprite.rotation = originalSprite.rotation;
            sprite.scaleX = originalSprite.scaleX;
            sprite.scaleY = originalSprite.scaleY;
            sprite.isVisible = originalSprite.isVisible;
            sprite.alpha = originalSprite.alpha;
            sprite.anchorX = originalSprite.anchorX;
            sprite.anchorY = originalSprite.anchorY;
        }
    }
}
