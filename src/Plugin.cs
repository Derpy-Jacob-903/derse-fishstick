using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;

using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Runtime.Remoting.Contexts;
using MoreSlugcats;
using RWCustom;

namespace SlugTemplate
{
    [BepInPlugin(MOD_ID, "derse-fishstick", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "derpyjacob903.derse-fishstick";

        //public static readonly PlayerFeature<float> SuperJump = PlayerFloat("slugtemplate/super_jump");
        //public static readonly PlayerFeature<bool> ExplodeOnDeath = PlayerBool("slugtemplate/explode_on_death");
        //public static readonly GameFeature<float> MeanLizards = GameFloat("slugtemplate/mean_lizards");

        //Patch Player.SlugSlamConditions to include Amalgam and Aurora
        //Patch Spear.Spear_NeedleCanFeed to include Amalgam
        public bool explodeScuglat = false;
        public bool scuglatCampaign = false;

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // Put your custom hooks here!
            On.RainCycle.Update += RainCycle_Update;
            On.Player.Update += Player_Update;

            On.Player.SlugSlamConditions += Player_SlugSlamConditions;
            On.RoomRain.Update += RoomRain_Update;
            On.GlobalRain.Update += GlobalRain_Update;

            On.RainWorldGame.ctor += RainWorldGame_ctor;
        }

        private void GlobalRain_Update(On.GlobalRain.orig_Update orig, GlobalRain self)
        {
            scuglatCampaign = self.game.world.name == "Scuglat";
            orig(self);
        }

        private void RoomRain_Update(On.RoomRain.orig_Update orig, RoomRain self, bool eu)
        {
            //if() { }
            orig(self, eu);
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if ((self.SlugCatClass.value == "Scuglat" || scuglatCampaign) && explodeScuglat == true && !self.dead)
            {
                self.PyroDeath();
                //self.

                //if (scuglatCampaign) { GoToDeathScreen()}
            }
        }

        private void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);

            if (scuglatCampaign && explodeScuglat && self.GameOverModeActive)
            {
                self.GoToDeathScreen(); 
            }
        }


        private bool Player_SlugSlamConditions(On.Player.orig_SlugSlamConditions orig, Player self, PhysicalObject otherObject)
        {
            return orig(self, otherObject);
            //if (self.SlugCatClass.value == "Gourmand" || self.SlugCatClass.value == "Cloudtail") //Aka The Forager
            //{
                //return orig(self, otherObject);
            //}
            if (true)
            {
                //return false;
            }
            if (self.SlugCatClass.value != "Amalgam" || self.SlugCatClass.value != "Aurora" || self.SlugCatClass.value != "Aurora")
            {
                return false;
            }
            if ((otherObject as Creature).abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            {
                return false;
            }
            if (self.gourmandAttackNegateTime > 0)
            {
                return false;
            }
            if (self.gravity == 0f)
            {
                return false;
            }
            if (self.cantBeGrabbedCounter > 0)
            {
                return false;
            }
            if (self.forceSleepCounter > 0)
            {
                return false;
            }
            if (self.timeSinceInCorridorMode < 5)
            {
                return false;
            }
            if (self.submerged)
            {
                return false;
            }
            if (self.enteringShortCut != null || (self.animation != Player.AnimationIndex.BellySlide && self.canJump >= 5))
            {
                return false;
            }
            if (self.animation == Player.AnimationIndex.CorridorTurn || self.animation == Player.AnimationIndex.CrawlTurn || self.animation == Player.AnimationIndex.ZeroGSwim || self.animation == Player.AnimationIndex.ZeroGPoleGrab || self.animation == Player.AnimationIndex.GetUpOnBeam || self.animation == Player.AnimationIndex.ClimbOnBeam || self.animation == Player.AnimationIndex.AntlerClimb || self.animation == Player.AnimationIndex.BeamTip)
            {
                return false;
            }
            Vector2 vel = self.bodyChunks[0].vel;
            if (self.bodyChunks[1].vel.magnitude < vel.magnitude)
            {
                vel = self.bodyChunks[1].vel;
            }
            if (self.animation != Player.AnimationIndex.BellySlide && vel.y >= -10f && vel.magnitude <= 25f)
            {
                return false;
            }
            Creature creature = otherObject as Creature;
            foreach (Creature.Grasp grasp in self.grabbedBy)
            {
                if (grasp.pacifying || grasp.grabber == creature)
                {
                    return false;
                }
            }
            return !ModManager.CoopAvailable || !(otherObject is Player) || Custom.rainWorld.options.friendlyFire;
        }

        private void RainCycle_Update(On.RainCycle.orig_Update orig, RainCycle self)
        {
            orig(self);
            explodeScuglat = self.deathRainHasHit;
        }


        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }
    }
}