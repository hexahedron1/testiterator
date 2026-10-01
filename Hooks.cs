#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CustomRegions.Collectables;
using CustomRegions.Mod;
using EffExt;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using TestIterator;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TestIterator;
public static class NewExtEnums {
    public static Oracle.OracleID TR = new(nameof (TR), true);
    public static SSOracleBehavior.MovementBehavior ReadPearl = new(nameof(ReadPearl), true);
    
    

    static void Unregister<T>(ExtEnum<T> id) where T : class {
        if (id == null)
            return;
        id.Unregister();
        id = null;
    }
    internal static void UnregisterValues() {
        Unregister(TR);
    }
}

public static class Conversations {
    public static Conversation.ID TR_MeetWhite = new(nameof(TR_MeetWhite), true);
    public static Conversation.ID TR_WelcomeBack = new(nameof(TR_WelcomeBack), true);
    public static Conversation.ID TR_Object = new(nameof(TR_Object), true);

    static void Unregister(Conversation.ID id) {
        if (id == null)
            return;
        id.Unregister();
        id = null;
    }
    internal static void UnregisterValues() {
        Unregister(TR_MeetWhite);
        Unregister(TR_WelcomeBack);
        Unregister(TR_Object);
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
        On.RoomCamera.ModifyEffectColorA += On_RoomCamera_ModifyEffectColorA;
        On.SSOracleBehavior.PebblesConversation.AddEvents += On_SSOracleBehavior_PebblesConversation_AddEvents;
    }

    public static void Unapply() {
        On.Room.ReadyForAI -= On_Room_ReadyForAI;
        On.OracleGraphics.Gown.Color -= On_Gown_Color;
        On.OracleGraphics.SkinColor -= On_OracleGraphics_SkinColor;
        On.OracleGraphics.ArmJointGraphics.ctor -= On_ArmJointGraphics_ctor;
        IL.Oracle.ctor -= IL_Oracle_ctor;
        IL.Oracle.OracleArm.Joint.Update -= IL_Joint_Update;
        IL.Oracle.OracleArm.Update -= IL_OracleArm_Update;
        NewExtEnums.UnregisterValues();
        Conversations.UnregisterValues();
        On.Oracle.SetUpMarbles -= On_Oracle_SetUpMables;
        On.DataPearl.ApplyPalette -= On_DataPearl_ApplyPalette;
        On.SuperStructureFuses.ctor -= On_SuperStructureFuses_ctor;
        On.DebugMouse.Update -= On_DebugMouse_Update;
        On.RoomCamera.ModifyEffectColorA -= On_RoomCamera_ModifyEffectColorA;
        On.SSOracleBehavior.PebblesConversation.AddEvents -= On_SSOracleBehavior_PebblesConversation_AddEvents;
    }

    private static Color[] On_RoomCamera_ModifyEffectColorA(On.RoomCamera.orig_ModifyEffectColorA orig, RoomCamera self, Color[] colors) {
        Color[] col = orig(self, colors);
        // todo actually do the thing lol
        return col;
    }

    private static void On_SSOracleBehavior_PebblesConversation_AddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, SSOracleBehavior.PebblesConversation self) {
        if (self.owner.oracle.ID == NewExtEnums.TR) {
            if (self.id == Conversations.TR_MeetWhite) {
                if (!self.owner.playerEnteredWithMark)
                    self.events.Add(new Conversation.TextEvent(self, 0, "Can you understand me?", 0));
                self.events.Add(new Conversation.TextEvent(self, 40, "Hello there, little thing.", 0));
                self.events.Add(new Conversation.TextEvent(self, 0,
                    "You must be native to the surface jungle near Five Pebbles.", 0));
                self.events.Add(new Conversation.TextEvent(self, 0,
                    "What brings you here, then? I can't offer much to you that you would find useful.", 0));
                self.events.Add(new Conversation.TextEvent(self, 0, "Do you perhaps, seek salvation?", 0));
                self.events.Add(new Conversation.TextEvent(self, 20, self.owner.playerEnteredWithMark
                        ? "In that case, you already have what you need."
                        : "In that case, i have given you what is required.",
                    0));
                self.events.Add(new Conversation.TextEvent(self, 0,
                    "However, to properly utilize such gift, you must find the appropriate location to do so.<LINE>And to my knowledge, there are none nearby.",
                    0));
                self.events.Add(new Conversation.TextEvent(self, 0,
                    "So, I can't direct you to a specific goal, but only give a general description of such place:",
                    0));
                self.events.Add(new Conversation.TextEvent(self, 0,
                    "Find a place which goes deep down, far below the ground level, where the rock gives way and begins the void sea.<LINE>The mark you have will let you pass through.",
                    0));
                self.events.Add(new Conversation.TextEvent(self, 0,
                    "So, unless you have something interesting to show me, it is time for you to go.", 0));
                self.events.Add(new Conversation.TextEvent(self, 0,
                    "Don't hesitate to return with something, through. I would like some company.", 0));
                return;
            }
            if (self.id == Conversations.TR_WelcomeBack) {
                self.events.Add(new Conversation.TextEvent(self, 0, Random.Range(0, 3) switch {
                    0 => "Welcome back.",
                    1 => "Hello again.",
                    _ => "Hello again, little visitor.",
                }, 0));
                return;
            }
            if (self.id == Conversations.TR_Object) {
                Plugin.Logger.LogDebug($"Initiating dialogue {((TROracleBehavior)self.owner).interestingShit?.GetType().Name}");
                switch (((TROracleBehavior)self.owner).interestingShit?.GetType().Name) {
                    case "DataPearl":
                        self.events.Add(new Conversation.TextEvent(self, 40, "This is a data pearl.", 0));
                        if (!self.owner.oracle.room.game.GetStorySession.saveState.unrecognizedSaveStrings.Contains("TR_DescribedPearl")) { 
                            self.events.Add(new Conversation.TextEvent(self, 0, "It is a special crystal memory complex made of diamond that acted as our creators' primary storage medium,<LINE>and now as ours.", 0));
                            self.events.Add(new Conversation.TextEvent(self, 0, "These pearls can contains a multitude of data types, from plain text to multimedia to raw internal language.", 0));
                            self.events.Add(new Conversation.TextEvent(self, 0, "Do you want me to read it?", 0));
                        } else {
                            self.events.Add(new Conversation.TextEvent(self, 30, Random.Range(0, 5) switch {
                                0 => "You want me to read this one as well?",
                                1 => "I suppose you would like me to read it.",
                                2 => "I'll read it for you.",
                                3 => "I'll read it.",
                                4 => "You want me to read it, don't you?",
                                _ => "this line should be impossible to see"
                            }, 0));
                            self.events.Add(new Conversation.TextEvent(self, 30, "Let's see...", 0));
                        }
                        break;
                    case "SSOracleSwarmer":
                        self.events.Add(new Conversation.TextEvent(self, 40, "This is a neuron fly. It's an organic device intended for local temporary storage and transport of data.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "While i do have millions of these and can replace them, I still request you to not take them.<LINE>Each one of them is there for a reason, and replacing them is not a fun thing to do...", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Please return it to where you found it.", 0));
                        break;
                    case "EnergyCell":
                        self.events.Add(new Conversation.TextEvent(self, 40, "Oh...", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Where did you get this?", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Actually, do you have any clue what this is?", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "This is a mass rarefaction cell, a backup energy source for our structures, originally used temporarily.<LINE>After our creators left this world and couldn't service us anymore, though, they became the primary one.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "The fact that this cell is in my chamber means that someone is not having a good time.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Please, return this.", 0));
                        break;
                    case "Rock":
                        self.events.Add(new Conversation.TextEvent(self, 0, "It's a rock.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Not very useful, aside from perhaps as a weapon. Thank you, but I don't need it.", 0));
                        break;
                    case "Spear":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This is just a piece of sharpened rebar. It was likely detached from a wall by the elements.", 0));
                        if (self.owner.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
                            self.events.Add(new Conversation.TextEvent(self, 0, "You seem skilled enough at using it, what is it that you would like to know more?", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "It's just that - a pointy metal stick. You could also use it to bribe a scavenger, i suppose.", 0));
                        break;
                    case "ExplosiveSpear":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This appears to be a spear with a pouch of fire powder strapped to its tip. Hitting something with probably will cause it to explode.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.owner.player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer
                            ? "I don't have records of this. Did you make this? Or did you steal it from an unfortunate scavenger who happened to stumble upon you?"
                            : "i don't have records of this. Did you get this from the scavengers?", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "In any case, I don't need to defend myself from anything, so you can take it.", 0));
                        break;
                    case "WaterNut":
                    case "SwollenWaterNut":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This is a plant that usually inhabits areas near water.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "While dormant, its fruit has a hard shell, and it's as good as a rock.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Upon contact with water, though, the shell pops and it attains a bubble-like buoyant form, which it uses to travel across<LINE>bodies of water to spread itself.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "After popping, it becomes soft and can be eaten. People said it tastes good but watery,<LINE>but i can't tell for sure as i can't taste it.", 0));
                        break;
                    case "KarmaFlower":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This is a psychoactive plant, colloquially called a 'Wheel Flower'", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "It lets beings momentarily shed their carnal self and reach the selves of other planes of existence.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Because of that, it became the symbol of enlightenment.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "While it would be useful in research, I already have it thoroughly documented, so you can enjoy it yourself.", 0));
                        break;
                    case "DangleFruit":
                        self.events.Add(new Conversation.TextEvent(self, 0, "It's a bug pupa.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "These things are extremely widespread - you can find one nearly everywhere. Which makes it a good source of food.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "I don't need it, so you can have it back.", 0));
                        break;
                    case "FlareBomb":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This is a bioluminescent plant. A quite bright one, at that.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "It usually inhabits dark areas, which are also frequented by arachnids.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "To prevent being preyed on, it evolved to release an extremely powerful flash of light on impact with the ground.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Because most inhabitants of shaded areas are very sensitive to light, this sudden luminance spike<LINE>is extremely painful to them, often being lethal.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "For others, it simply causes temorary retina damage. It's not lethal and recovers quickly, but not pleasant,", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Close your eyes and look away if you need to use this.", 0));
                        break;
                    case "VultureMask":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This is a mask made of a composide material bonded with bone.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "They're primarily worn by vultures, though I've seen some crafty scavengers make ornate ones.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Since your species is frequently preyed on by lizards, here's a survival tip:", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Lizards are naturally scared of vultures, and covering your face with this mask will let you<LINE>pose as one and exploit their self-preservation instincts, for a short while at least. Eventually they'll realize you're just a meal to them,<LINE>by which time you should do your best to escape.", 0));
                        if (self.owner.player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer || self.owner.player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear || self.owner.player.SlugCatClass == SlugcatStats.Name.Red)
                            self.events.Add(new Conversation.TextEvent(self, 0, "Or you could make them your own meal, i suppose. I'm sure you wouldn't hesitate.", 0));
                        break;
                    case "PuffBall":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This is a fungus. Its inside is at a high pressure - rupturing it will release lots of spores<LINE>in all directions.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Arachnids and the like don't handle inhaling them well. That doesn't mean you should, either.", 0));
                        break;
                    case "Jellyfish":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This is a jellyfish. They are often seen in aquatic areas.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "While usually passive, their stingers can deliver a painful electric shock, so be careful.", 0));
                        break;
                    case "Lantern":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This looks to be a small translucent shell with a glowing substance inside. Likely distilled slime mold.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "I wasn't provided with data for it, so it's likely a scavenger invention.", 0));
                        break;
                    case "Mushroom":
                        self.events.Add(new Conversation.TextEvent(self, 0, "It's a bioluminescent mushroom. These usually inhabit small, poorly lit crannys like caves or underground tunnels.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "It contains a non-toxic psychoactive drug that briefly increases adrenaline release and improves reaction time.", 0));
                        break;
                    case "FirecrackerPlant":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This is a fire bush. Each nut has a volatile substance inside, which is prone to exploding.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "It's not lethal, but rather unpleasant", 0));
                        break;
                    case "SlimeMold":
                        self.events.Add(new Conversation.TextEvent(self, 0, "It's a piece of slime mold plasmodium. It often lives in dark areas and eats whatever<LINE>decaying organic matter it can find.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Aside from that it's just a food source - you can have it.", 0));
                        break;
                    case "ScavengerBomb":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This appears to be a small shell with a lot of explosive powder in it. An impact would set it off.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Handle with care.", 0));
                        break;
                    case "BubbleGrass":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This is a plant with lots of tiny air reservoirs.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "They often grow in caves near water, and use this stored air to survive submerged in the periodic floods due to rain.", 0));
                        break;
                    case "OverseerCarcass":
                        self.events.Add(new Conversation.TextEvent(self, 0, "This is the eye of an overseer.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "They act as our eyes to the outside world, since our actual eyes can only reach within the puppet chamber.<LINE>You've probably seen a couple of them on the way here.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "The scavengers like to hunt them - it's their way of expressing defiance towards us. For what though, i'm not so sure.", 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, "Be careful if both of them are nearby.", 0));
                        break;
                    default:
                        Plugin.Logger.LogDebug("Unknown object");
                        switch (Random.Range(0, 3)) {
                            case 0:
                                self.events.Add(new Conversation.TextEvent(self, 40, "Interesting, I don't have this object on my record.", 0));
                                self.events.Add(new Conversation.TextEvent(self, 0, "Which is especially strange since it contains data for thousands of things.", 0));
                                self.events.Add(new Conversation.TextEvent(self, 0, "Sorry, i can't tell you anything about this because of that.", 0));
                                break;
                            case 1:
                                self.events.Add(new Conversation.TextEvent(self, 0, "I don't know what this is.", 0));
                                self.events.Add(new Conversation.TextEvent(self, 0, "By some miracle I do not have this kind of object documented in my database.", 0));
                                self.events.Add(new Conversation.TextEvent(self, 0, "Please forgive me, I can't provide any data for it.", 0));
                                break;
                            case 2:
                                self.events.Add(new Conversation.TextEvent(self, 0, "What is this?", 0));
                                self.events.Add(new Conversation.TextEvent(self, 0, "No specimen of this kind was recorded in my archives.", 0));
                                self.events.Add(new Conversation.TextEvent(self, 0, "I can't tell you anything about this thing, sorry.", 0));
                                break;
                        }
                        break;
                }
                return;
            }

            if (PearlData.CustomDataPearlsList.Any(x => x.Value.conversationID == self.id)) {
                Plugin.Logger.LogInfo("Found");
                var pearl = PearlData.CustomDataPearlsList.First(x => x.Value.conversationID == self.id);
                CustomConvo.LoadEventsFromFile(self, pearl.Value.filePath, NewExtEnums.TR);
                return;
            }
        }
        orig(self);
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
        if (self.ID == NewExtEnums.TR) return; 
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
                    self.ID = NewExtEnums.TR;
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