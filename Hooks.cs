#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EffExt;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using TestIterator;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TestIterator;
public static class NewOracleID {
    public static Oracle.OracleID TR = new (nameof (TR), true);
    internal static void UnregisterValues() {
        if (TR == null)
            return;
        TR.Unregister();
        TR = null;
    }
}

public static class Hooks {
    public static void Apply() {
        On.Room.ReadyForAI += On_Room_ReadyForAI;
        On.OracleGraphics.Gown.Color += On_Gown_Color;
        On.OracleGraphics.SkinColor += On_OracleGraphics_SkinColor;
        On.OracleGraphics.ArmJointGraphics.ctor += On_ArmJointGraphics_ctor;
        IL.Oracle.ctor += IL_Oracle_ctor;
        IL.Oracle.OracleArm.Joint.Update += IL_Joint_Update;
        IL.Oracle.OracleArm.Update += IL_OracleArm_Update;
        On.Oracle.SetUpMarbles += On_Oracle_SetUpMables;
        On.DataPearl.ApplyPalette += On_DataPearl_ApplyPalette;
        On.SuperStructureFuses.ctor += On_SuperStructureFuses_ctor;
        On.DebugMouse.Update += On_DebugMouse_Update;
        //On.Room.WaterFluxController.waterFluxState += On_Room_WaterFluxController_WaterFluxState;
    }

    public static void Unapply() {
        On.Room.ReadyForAI -= On_Room_ReadyForAI;
        On.OracleGraphics.Gown.Color -= On_Gown_Color;
        On.OracleGraphics.SkinColor -= On_OracleGraphics_SkinColor;
        On.OracleGraphics.ArmJointGraphics.ctor -= On_ArmJointGraphics_ctor;
        IL.Oracle.ctor -= IL_Oracle_ctor;
        IL.Oracle.OracleArm.Joint.Update -= IL_Joint_Update;
        IL.Oracle.OracleArm.Update -= IL_OracleArm_Update;
        NewOracleID.UnregisterValues();
        On.Oracle.SetUpMarbles -= On_Oracle_SetUpMables;
        On.DataPearl.ApplyPalette -= On_DataPearl_ApplyPalette;
        On.SuperStructureFuses.ctor -= On_SuperStructureFuses_ctor;
        On.DebugMouse.Update -= On_DebugMouse_Update;
    }
    private static void On_DebugMouse_Update(On.DebugMouse.orig_Update orig, DebugMouse self, bool eu) {
        orig(self, eu);
        if (!self.room.readyForAI || !self.room.BeingViewed) return;
        string text = self.label.text;
        TROracle? oracle = null;
        foreach (var i in self.room.physicalObjects) {
            if (oracle is not null) break;
            foreach (var j in i) {
                if (j is TROracle o) {
                    oracle = o;
                    break;
                }
            }
        }

        if (oracle != null) text += 
            $"\n== Oracle state ==\n{oracle.behavior.state}\ntime: {oracle.behavior.stateTime} ({oracle.behavior.stateSwitchTime})\nprogress: {oracle.behavior.stateProgress}";


        text += $"\nCycleProgression: {self.room.world.rainCycle.CycleProgression}";
        self.label.text = text;
        self.label2.text = text;
    }
    
    private static void On_SuperStructureFuses_ctor(On.SuperStructureFuses.orig_ctor orig, SuperStructureFuses self, PlacedObject placedObject, IntRect rect, Room room) {
        orig(self, placedObject, rect, room);
        if (room.world.region is { name: "TR" }) {
            self.broken = 0f;
        }
    }
    private static void On_DataPearl_ApplyPalette(On.DataPearl.orig_ApplyPalette orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
        orig(self, sLeaser, rCam, palette);
        if (self is PebblesPearl { oracle: TROracle } pp) {
            self.color = Math.Abs((pp.abstractPhysicalObject as PebblesPearl.AbstractPebblesPearl)?.color ?? 4) switch {
                0 => new Color(0.7f, 0.7f, 0.7f),
                1 => new Color(0.1764705882f, 0.5882352941f, 0.262745098f),
                2 => new Color(0.1f, 0.01f, 0.01f),
                _ => new Color(0.9f, 0.01f, 0.01f)
            };
            /*self.highlightColor = Math.Abs((pp.abstractPhysicalObject as PebblesPearl.AbstractPebblesPearl)?.color ?? 4) switch {
                0 => new Color(0.7f, 0.7f, 0.7f),
                1 => new Color(0.3215686275f, 1f, 0.4705882353f),
                2 => new Color(0.1f, 0.01f, 0.01f),
                _ => new Color(0.9f, 0.01f, 0.01f)
            };*/
        }
    }

    private static void On_Oracle_SetUpMables(On.Oracle.orig_SetUpMarbles orig, Oracle self) {
        Plugin.Logger.LogInfo("Making poarls.,,..,.,");
        if (self.ID == NewOracleID.TR) return; 
        orig(self);
    }

    private static void On_SSOracleBehavior_SpecialEvent(On.SSOracleBehavior.orig_SpecialEvent orig,
        SSOracleBehavior self, string eventName) {
        if (self is TROracleBehavior) {
            Plugin.Logger.LogInfo($"Special event: {eventName}");
        }
        else orig(self, eventName);
    }

    private static void IL_Oracle_ctor(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(MoveType.After, x => x.MatchLdsfld<Oracle.OracleID>("SS"))) {
            if (cursor.TryGotoNext(MoveType.After, x => x.MatchStfld<Oracle>("ID"))) {
                cursor.Emit(OpCodes.Ldarg_0).Emit(OpCodes.Ldarg_2).EmitDelegate<Action<Oracle, Room>>((self, room) => {
                    if (self is not TROracle) return;
                    self.ID = NewOracleID.TR;
                });
                goto skip;
            }
        }
        Plugin.Logger.LogError("Couldn't ILHook Oracle.ctor");
        skip:
        if (cursor.TryGotoNext(MoveType.After, x => x.MatchLdsfld<Oracle.OracleID>("SS"),
                x => x.MatchCall(out MethodReference _)))
            cursor.Emit(OpCodes.Ldarg_0)
                .EmitDelegate<Func<bool, Oracle, bool>>((flag, self) => flag || self is TROracle);
        else
            Plugin.Logger.LogError("Couldn't ILHook Oracle.ctor");
    }
    private static void IL_Joint_Update(ILContext il)
    {
        ILCursor ilCursor = new ILCursor(il);
        if (ilCursor.TryGotoNext(MoveType.After, x => x.MatchLdsfld<Oracle.OracleID>("SS"), x => x.MatchCall(out MethodReference _)))
            ilCursor.Emit(OpCodes.Ldarg_0).EmitDelegate((Func<bool, Oracle.OracleArm.Joint, bool>) ((flag, self) => flag || self.arm is TROracleArm));
        else
            Plugin.Logger.LogError("Couldn't ILHook Oracle.OracleArm.Joint.Update!");
    }
    private static void IL_OracleArm_Update(ILContext il)
    {
        ILCursor ilCursor = new ILCursor(il);
        for (int index = 1; index <= 3; ++index)
        {
            if (ilCursor.TryGotoNext(MoveType.After, x => x.MatchLdsfld<Oracle.OracleID>("SS"), x => x.MatchCall(out MethodReference _)))
                ilCursor.Emit(OpCodes.Ldarg_0).EmitDelegate((Func<bool, Oracle.OracleArm, bool>) ((flag, self) => flag || self is TROracleArm));
            else
                Plugin.Logger.LogError($"Couldn't ILHook Oracle.OracleArm.Update (part {index})!");
        }
    }
    private static Color On_OracleGraphics_SkinColor(On.OracleGraphics.orig_SkinColor orig, OracleGraphics self) {
        return self is TROracleGraphics ? new Color(0.9058f, 0, 0.3568f) : orig(self);
    }

    private static Color On_Gown_Color(On.OracleGraphics.Gown.orig_Color orig, OracleGraphics.Gown self, float f) {
        return self.owner is TROracleGraphics ? new Color(Mathf.Lerp(0.2745f, 0.3176f, f), Mathf.Lerp(0.1137f, 0.145f, f), Mathf.Lerp(0.2627f, 0.3058f, f)) : orig(self, f);
    }
    
    private static void On_ArmJointGraphics_ctor(
        On.OracleGraphics.ArmJointGraphics.orig_ctor orig,
        OracleGraphics.ArmJointGraphics self,
        OracleGraphics owner,
        Oracle.OracleArm.Joint myJoint,
        int firstSprite)
    {
        orig(self, owner, myJoint, firstSprite);
        if (owner is not TROracleGraphics)
            return;
        self.armJointSound.soundID = SoundID.SS_AI_Arm_Joint_LOOP;
    }
    
    private static void On_Room_ReadyForAI(On.Room.orig_ReadyForAI orig, Room self)
    {
        orig(self);
        if (self.game?.session is not StoryGameSession || !string.Equals(self.abstractRoom.name, "TR_AI", StringComparison.OrdinalIgnoreCase))
            return;
        var pos = self.MiddleOfTile(24, 17);
        self.AddObject(new TROracle(new AbstractPhysicalObject(self.world, AbstractPhysicalObject.AbstractObjectType.Oracle, null, new WorldCoordinate(self.abstractRoom.index, (int)pos.x, (int)pos.y, -1), self.game.GetNewID()), self));
        self.waitToEnterAfterFullyLoaded = Math.Max(self.waitToEnterAfterFullyLoaded, 80 /*0x50*/);
    }
    
    private static void On_OracleChatLabel_DrawSprites(
        On.OracleChatLabel.orig_DrawSprites orig,
        OracleChatLabel self,
        RoomCamera.SpriteLeaser sLeaser,
        RoomCamera rCam,
        float timeStacker,
        Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.slatedForDeletetion || self.room != rCam.room || !self.visible || self.oracleBehav is not TROracleBehavior)
            return;
        foreach (FSprite sprite in sLeaser.sprites)
            sprite.color = self.color;
    }

    private static void On_OracleChatLabel_AddToContainer(
        On.OracleChatLabel.orig_AddToContainer orig,
        OracleChatLabel self,
        RoomCamera.SpriteLeaser sLeaser,
        RoomCamera rCam,
        FContainer newContainer)
    {
        if (self.oracleBehav is TROracleBehavior)
            newContainer = rCam.ReturnFContainer("BackgroundShortcuts");
        orig(self, sLeaser, rCam, newContainer);
    }
}