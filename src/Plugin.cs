using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;

using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Runtime.Remoting.Contexts;

namespace SlugTemplate
{
    [BepInPlugin(MOD_ID, "derse-fishstick", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "derpyjacob903.derse-fishstick";

        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("slugtemplate/super_jump");
        public static readonly PlayerFeature<bool> ExplodeOnDeath = PlayerBool("slugtemplate/explode_on_death");
        public static readonly GameFeature<float> MeanLizards = GameFloat("slugtemplate/mean_lizards");

        //Patch Player.SlugSlamConditions to include Amalgam and Aurora
        //Patch Spear.Spear_NeedleCanFeed to include Amalgam

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // Put your custom hooks here!
            On.Player.Jump += Player_Jump;
            On.Player.Collide += Player_Collide;
            On.Lizard.ctor += Lizard_ctor;

            IL.Player.SlugSlamConditions += Player_SlugSlamConditions;
            IL.Player.SlugSlamConditions += Player_Collide;
        }

        private void Player_SlugSlamConditions(ILContext il)
        {
            try
            {
                ILCursor c = new(il);
                c.GotoNext(
                    MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld(typeof(Player).GetField(nameof(Player.SlugCatClass))),
                    x => x.MatchLdsfld(typeof(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName).GetField(nameof(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Gourmand))),
                    x => x.MatchCallOrCallvirt(typeof(ExtEnum<SlugcatStats.Name>).GetMethod("op_Inequality"))
                );

                c.MoveAfterLabels();
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<bool, Player, bool>>((bool isNotGourmand, Player self) =>
                {
                    return isNotGourmand && self.SlugCatClass.value != "Aurora";
                });
                // UnityEngine.Debug.Log(il);
            }
            catch (Exception e) { UnityEngine.Debug.Log(e); }
        }

        private void Player_Collide(ILContext il)
        {
            ILCursor c = new(il);
            try
            {
                c.GotoNext(
                    MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchCallOrCallvirt(typeof(Player).GetProperty(nameof(Player.isGourmand)).GetGetMethod())
                    );

                c.MoveAfterLabels();
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<bool, Player, bool>>((bool isGourmand, Player self) =>
                {
                    return isGourmand || self.SlugCatClass.value == "Aurora";
                });

                c.GotoNext(
                    MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld(typeof(Player).GetField(nameof(Player.SlugCatClass))),
                    x => x.MatchLdsfld(typeof(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName).GetField(nameof(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Gourmand))),
                    x => x.MatchCallOrCallvirt(typeof(ExtEnum<SlugcatStats.Name>).GetMethod("op_Equality"))
                    );

                c.MoveAfterLabels();
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<bool, Player, bool>>((bool isGourmand, Player self) =>
                {
                    return isGourmand || self.SlugCatClass.value == "Aurora";
                });
            }
            catch (Exception e) { Log(e); }
        }

        private void Log(Exception e)
        {
            throw new NotImplementedException();
        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }

        // Implement MeanLizards
        private void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            if(MeanLizards.TryGet(world.game, out float meanness))
            {
                self.spawnDataEvil = Mathf.Min(self.spawnDataEvil, meanness);
            }
        }


        // Implement SuperJump
        private void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);

            if (SuperJump.TryGet(self, out var power))
            {
                self.jumpBoost *= 1f + power;
            }
        }

        // Implement ExlodeOnDeath
        private void Player_Collide(On.Player. orig, Player self)
        {
            bool wasDead = self.dead;

            orig(self);

            if(!wasDead && self.dead
                && ExplodeOnDeath.TryGet(self, out bool explode)
                && explode)
            {
                // Adapted from ScavengerBomb.Explode
                var room = self.room;
                var pos = self.mainBodyChunk.pos;
                var color = self.ShortCutColor();

                this.PyroDeath();
                //room.AddObject(new Explosion(room, self, pos, 7, 250f, 6.2f, 2f, 280f, 0.25f, self, 0.7f, 160f, 1f));
                //room.AddObject(new Explosion.ExplosionLight(pos, 280f, 1f, 7, color));
                //room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, new Color(1f, 1f, 1f)));
                //room.AddObject(new ExplosionSpikes(room, pos, 14, 30f, 9f, 7f, 170f, color));
                //room.AddObject(new ShockWave(pos, 330f, 0.045f, 5, false));

                //room.ScreenMovement(pos, default, 1.3f);
                //room.PlaySound(SoundID.Bomb_Explode, pos);
                //room.InGameNoise(new Noise.InGameNoise(pos, 9000f, self, 1f));
            }
        }
    }
}