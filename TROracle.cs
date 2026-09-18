using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using TestIterator;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TestIterator;

public class TROracle : Oracle {
    public TROracle(AbstractPhysicalObject apo, Room room) : base(apo, room) {
        room.AddObject(myScreen = new OracleProjectionScreen(room, oracleBehavior = new TROracleBehavior(this)));
        marbles = [];
        SetUpMarbles();
        room.gravity = 0.0f;
        var updateList = room.updateList;
        foreach (var bodyChunk in bodyChunks) {
            bodyChunk.pos = new Vector2(500, 300);
        }
        foreach (var t in updateList) {
            if (t is not AntiGravity ag) continue;
            ag.active = false;
            break;
        }
        arm = new TROracleArm(this);
        behavior.SetNewDestination(new Vector2(480, 350));
    }

    public TROracleBehavior behavior => (TROracleBehavior)oracleBehavior;

    private new void SetUpMarbles() {
        PhysicalObject orbitObj = this;
        System.Random rnd = new(872324);
        for (int index = 0; index < 4; ++index)
            CreateMarble(orbitObj, new Vector2(500f, 300f) + Custom.RNV() * 20f, 0, 35f, rnd.Next(3));
        for (int index = 0; index < 2; ++index)
            CreateMarble(orbitObj, new Vector2(500f, 300f) + Custom.RNV() * 20f, 1, 100f, index == 1 ? 2 : 0);
        for (int i = 0; i < 21; i++) {
            int x = i % 7;
            int y = i / 7;
            CreateMarble(null, new Vector2(240 + x*20, 130 - x*5 + y*20), 0, 0.0f, rnd.Next(3));
        }
        CreateMarble(null, new Vector2(680, 350), 0, 0.0f, 2);
        var orbitPearl = marbles.Last();
        for (int i = 0; i < 3; i++) {
            CreateMarble(orbitPearl, new Vector2(680, 350) + Custom.RNV() * 20f, 10, 30f, rnd.Next(3));
        }
        for (int i = 0; i < 5; i++) {
            CreateMarble(null, new Vector2(260, 590 - i * 50), 0, 0f, rnd.Next(3));
            CreateMarble(marbles.Last(), new Vector2(260, 590 - i * 50) + Custom.RNV()*20, 0, 3f, rnd.Next(3));
        }
        CreateMarble(null, new Vector2(380, 510), 0, 0.0f, 2);
        for (int i = 0; i < 2; i++)
            CreateMarble(marbles.Last(), new Vector2(380, 510) + Custom.RNV() * 20f, 1, 15f, rnd.Next(3));
        for (int i = 0; i < 10; i++)
            CreateMarble(rnd.Next(3) == 0 ? marbles.Last() : null, new Vector2(640, 510) + Custom.DegToVec(rnd.Next(360))*((float)rnd.NextDouble()*90f + 30f), 0, rnd.Next(2, 9), rnd.Next(3));
    }
    
    public override void InitiateGraphicsModule() {
        if (graphicsModule != null)
            return;
        graphicsModule = new TROracleGraphics(this);
    }

    public override void HitByWeapon(Weapon weapon) {
        if (!Consious)
            return;
        behavior.ReactToHitWeapon();
    }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk) {
        base.Collide(otherObject, myChunk, otherChunk);
        if (otherObject is Player) behavior.gotHitByPlayer = true;
    }
}



public class TROracleBehavior : SSOracleBehavior {
    private GlyphLabel testLabel;
    private GlyphLabel errorLabel;
    private GlyphLabel debugLabel_currentGetTo;
    private GlyphLabel debugLabel_nextPos;
    private GlyphLabel debugLabel_lastPos;
    internal bool gotHitByPlayer;
    public TROracleBehavior(TROracle oracle) : base(oracle) {
        try {
            currSubBehavior.Deactivate();
            allSubBehaviors.RemoveAt(allSubBehaviors.Count - 1);
            allSubBehaviors.Add(currSubBehavior = new TRNoSubBehavior(this));
            movementBehavior = MovementBehavior.Idle;
            if (debug) {
                oracle.room.AddObject(testLabel = new(new Vector2(240f, 600f),
                    [ 12, 11 ]));
                oracle.room.AddObject(errorLabel = new(new Vector2(240f, 580f), [9, 13]));
                oracle.room.AddObject(debugLabel_nextPos = new(new Vector2(240f, 580f), [2]));
                debugLabel_nextPos.color = Color.green;
                oracle.room.AddObject(debugLabel_lastPos = new(new Vector2(240f, 580f), [9]));
                debugLabel_lastPos.color = Color.yellow;
                oracle.room.AddObject(debugLabel_currentGetTo = new(new Vector2(240f, 580f), [2]));
                debugLabel_currentGetTo.color = Color.magenta;
            }
            SwitchState(TROracleState.Idle);
        }
        catch (Exception e) {
            Plugin.Logger.LogError("istg");
            Plugin.Logger.LogError(e);
            cantFuckingWork = true;
        }
    }

    private bool cantFuckingWork;
    private const bool debug = false;
    PebblesPearl MakePearl(PhysicalObject orbitObj, Vector2 pos, int circle, float dist, int color, int? label = null) {
        oracle.CreateMarble(orbitObj, pos, circle, dist, color);
        PebblesPearl pearl = oracle.marbles.Last();
        //if (label.HasValue) pearl.label = new GlyphLabel(pearl.abstractPhysicalObject.pos.Vec2(), [label.Value] );
        if (debug) Plugin.Logger.LogDebug("New custom pearl:"+pearl.abstractPhysicalObject.ID);
        return pearl;
    }

    private void SetLabel(params int[] glyphs) {
        if (!debug) return;
        testLabel.glyphs = glyphs;
        testLabel.visibleGlyphs = 0;
    }

    private void Bang(BodyChunk chunk) {
        oracle.room.AddObject(new ShockWave(chunk.pos, 60f, 0.1f, 8));
        oracle.room.AddObject(new Spark(chunk.pos, Custom.RNV(), Color.white, null, 16, 24));
        oracle.room.AddObject(new Explosion.ExplosionLight(chunk.pos, 150f, 1f, 8, Color.white));
        oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, chunk);
    }

    private int timeSinceLastError = 999;
    private int tikk = 0;
    private PebblesPearl errorTestPearl; // trigger a test exception
    private bool errorPearlWasHeld;

    private void BlinkPearl(PebblesPearl pearl, bool condition = true) {
        if (pearl.label == null) return;
        if (condition)
            pearl.label.visibleGlyphs = tikk%10 / 5 % 2;
        else
            pearl.label.visibleGlyphs = 1;
    }

    public void ReactToHitByWeapon() {
        
    }

    public class TROracleState(string value, bool register = false) : ExtEnum<TROracleState>(value, register) {
        public static readonly TROracleState Idle = new("Idle", true);
        public static readonly TROracleState FirstEncounter_Spear = new("FirstEncounter_Spear", true);
        public static readonly TROracleState FirstEncounter_Arti = new("FirstEncounter_Arti", true);
        public static readonly TROracleState FirstEncounter_Red = new("FirstEncounter_Red", true);
        public static readonly TROracleState FirstEncounter_Gourm = new("FirstEncounter_Gourm", true);
        public static readonly TROracleState FirstEncounter_White = new("FirstEncounter_White", true);
        public static readonly TROracleState FirstEncounter_Yellow = new("FirstEncounter_Yellow", true);
        public static readonly TROracleState FirstEncounter_Riv = new("FirstEncounter_Riv", true);
        public static readonly TROracleState FirstEncounter_Saint = new("FirstEncounter_Saint", true);
        public static readonly TROracleState FirstEncounter_Unknown = new("FirstEncounter_Unknown", true);
        public static readonly TROracleState FirstEncounter_Inv = new("FirstEncounter_Inv", true);
        public static readonly TROracleState InspectObject = new("InspectObject", true);
        public static readonly TROracleState SMThrowOut = new("SMThrowOut", true);
        public static readonly TROracleState Welcome = new("Welcome", true);
    }

    public TROracleState state;
    public int stateProgress;
    public int stateSwitchTime;
    public int stateTime => oracle.room.game.timeInRegionThisCycle - stateSwitchTime;

    [CanBeNull]
    public TROracleState GetFirstEncounterState(SlugcatStats.Name name) {
        if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
            return TROracleState.FirstEncounter_Spear;
        if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            return TROracleState.FirstEncounter_Arti;
        if (player.SlugCatClass == SlugcatStats.Name.Red)
            return TROracleState.FirstEncounter_Red;
        if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            return TROracleState.FirstEncounter_Gourm;
        if (player.SlugCatClass == SlugcatStats.Name.White)
            return TROracleState.FirstEncounter_White;
        if (player.SlugCatClass == SlugcatStats.Name.Yellow)
            return TROracleState.FirstEncounter_Yellow;
        if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            return TROracleState.FirstEncounter_Riv;
        if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            return TROracleState.FirstEncounter_Saint;
        if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
            return TROracleState.FirstEncounter_Inv;
        return null;
    }

    public void SwitchState(TROracleState newState) {
        var oldState = state;
        stateProgress = 0;
        stateSwitchTime = oracle.room.game.timeInRegionThisCycle;
        // filtering
        if (newState == TROracleState.Welcome && !oracle.room.game.GetStorySession.saveState.unrecognizedSaveStrings.Contains("TR_MetPlayer")) newState = GetFirstEncounterState(player.SlugCatClass) ?? TROracleState.FirstEncounter_Unknown;
        
        state = newState;
        Plugin.Logger.LogInfo("New state: " + state);
        if (state.value.StartsWith("FirstEncounter")) {
            awareOfPlayer = true;
            awareOfPlayerTime = 0f;
            movementBehavior = MovementBehavior.Investigate;
            oracle.room.gravity = 0.8f;
            oracle.room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_Off);
            LockShortcuts();
            TurnOffSSMusic(true);
        }

        if (state == TROracleState.Idle) {
            UnlockShortcuts();
        }
    }

    private bool awareOfPlayer;
    private float awareOfPlayerTime;

    private void SetPalette(int palA, int palB, float blend) {
        foreach (var t in oracle.room.game.cameras) {
            if (t.room == oracle.room && !t.AboutToSwitchRoom)
                t.ChangeBothPalettes(palA, palB, blend);
        }
    }

    private int idleProgress;
    void FiniteStateMachine(Room room) {
        if (state == TROracleState.Idle) {
            SetPalette(23, 26, Math.Min(1f, stateProgress/20f));
            if (movementBehavior == MovementBehavior.Idle) {
                invstAngSpeed = 1f;
                if (investigateMarble == null && oracle.marbles.Count > 0) {
                    if (idleProgress > (room.world.rainCycle.AmountLeft > 0 ? 2400 : 120) && Random.Range(0, room.world.rainCycle.AmountLeft > 0 ? 150 : 10) == 0) {
                        movementBehavior = MovementBehavior.Meditate;
                        SetNewDestination(new Vector2(480, 350));
                        idleProgress = 0;
                    }
                    var selectable = (from x in oracle.marbles where x.orbitObj == null select x).ToArray();
                    investigateMarble = selectable[Random.Range(0, selectable.Length)];
                    //Bang(investigateMarble.firstChunk);
                    SetLabel(GlyphLabel.RandomString(1, 10, investigateMarble.marbleIndex, false));
                    if (debug) Plugin.Logger.LogDebug("Picked new pearl to look at: " + investigateMarble.abstractPhysicalObject.ID);
                    investigateAngle =
                        Custom.VecToDeg(investigateMarble.firstChunk.pos -
                                        oracle.firstChunk.pos); //Random.value * 360f;
                    SetNewDestination(investigateMarble.firstChunk.pos - Custom.DegToVec(investigateAngle) * 100f);
                }

                if (player != null && player.room == oracle.room && player.DangerPos.y < 640f && !awareOfPlayer) {
                    SwitchState(TROracleState.Welcome);
                    return;
                }
                if (player != null && player.room != oracle.room)
                    awareOfPlayer = false;
                if (investigateMarble != null) {
                    lookPoint = investigateMarble.firstChunk.pos;
                    floatyMovement = true;
                    if (Mathf.Approximately(pathProgression, 1f) && Random.value < 0.05) investigateMarble = null;
                }

                
                if (debug) {
                    debugLabel_currentGetTo.pos = currentGetTo;
                    debugLabel_lastPos.pos = lastPos;
                    debugLabel_nextPos.pos = nextPos;
                    //testLabel.pos = oracle.firstChunk.pos + new Vector2(100f, 100f);
                    if (tikk % 5 == 0 && testLabel.visibleGlyphs < testLabel.glyphs.Length) {
                        testLabel.visibleGlyphs++;
                        oracle.room.PlaySound(SoundID.SS_AI_Text);
                    }
                } 
            } else if (movementBehavior == MovementBehavior.Meditate) {
                lookPoint = oracle.bodyChunks[0].pos;
                currentGetTo = new Vector2(480, 350);
                if (idleProgress > (room.world.rainCycle.AmountLeft > 0 ? 800 : 1600) && Random.Range(0, room.world.rainCycle.AmountLeft > 0 ? 150 : 1200) == 0) {
                    movementBehavior = MovementBehavior.Idle;
                    idleProgress = 0;
                }
                if (gotHitByPlayer) {
                    gotHitByPlayer = false;
                    SwitchState(TROracleState.Welcome);
                    return;
                }
            }
            stateProgress++;
            idleProgress++;
        } else if (state == TROracleState.FirstEncounter_White) {
            if (stateProgress == 0) {
                SetPalette(26, 23, Math.Min(1f, stateTime/20f));
                movementBehavior = MovementBehavior.Investigate;
                if (stateTime > 640) {
                    stateProgress = 1;
                }
            } else if (stateProgress == 1) {
                movementBehavior = MovementBehavior.KeepDistance;
                if (stateTime > 800) {
                    if (stateTime == 830) {
                        room.PlaySound(SoundID.SS_AI_Give_The_Mark_Telekenisis, 0.0f, 1f, 1f);
                    }
                    if (stateTime > 830) {
                        Vector2 vector2 = Vector2.ClampMagnitude(oracle.room.MiddleOfTile(24, 14) - player.mainBodyChunk.pos, 40f) / 40f * (2.8f * Mathf.InverseLerp(30f, 160f, stateTime-800));
                        player.mainBodyChunk.vel += vector2;
                    }
                    if (stateTime < 1100) {
                        player.Stun(20);    
                    }
                    if (stateTime == 1100) {
                        player.mainBodyChunk.vel += Custom.RNV() * 10f;
                        player.bodyChunks[1].vel += Custom.RNV() * 10f;
                        ((StoryGameSession)oracle.room.game.session).saveState.deathPersistentSaveData.karmaCap = 9;
                        Bang(player.mainBodyChunk);
                        oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0.0f, 1f, 1f);
                        ((StoryGameSession)oracle.room.game.session).saveState.deathPersistentSaveData.karma = ((StoryGameSession)oracle.room.game.session).saveState.deathPersistentSaveData.karmaCap;
                        ((StoryGameSession)oracle.room.game.session).saveState.deathPersistentSaveData.theMark = true;
                        player.AddFood(player.MaxFoodInStomach - player.FoodInStomach);
                    }
                    if (stateTime > 1100) {
                        ((PlayerGraphics)player.graphicsModule).markAlpha = Mathf.Max(((PlayerGraphics)player.graphicsModule).markAlpha, Mathf.InverseLerp(500f, 300f, stateTime-800));
                    }

                    if (stateTime == 1200) stateProgress = 2;
                }
            } else if (stateProgress == 2) {
                
            }
        }
    }

    private float investigateAngle2;
    public override void Update(bool eu) {
        if (cantFuckingWork) return;
        try {
            Room room = oracle.room;
            if (room?.game.session is not StoryGameSession session) return;
            player = oracle.room.game.AlivePlayers.Count > 0
                ? oracle.room.game.FirstAlivePlayer.realizedCreature as Player
                : null;
            if (tikk == 0) {
                if (debug) {
                    errorTestPearl = MakePearl(null, new Vector2(240f, 550f), 0, 0f, 2, 13);
                    oracle.room.AddObject(errorTestPearl);
                }
            }
            FiniteStateMachine(room);
            if (movementBehavior == MovementBehavior.Investigate) {
                if (player != null) {
                    lookPoint = player.DangerPos;
                    investigateAngle = 180f;
                    bool newPos = false;
                    if (investigateAngle2 < -90.0 || investigateAngle2 > 90.0 || oracle.room.aimap.getTerrainProximity(nextPos) < 2.0 || Random.value < 0.01) {
                        investigateAngle2 = Mathf.Lerp(-70f, 70f, Random.value);
                        invstAngSpeed = Mathf.Lerp(0.4f, 0.8f, Random.value) * (Random.value < 0.5 ? -1f : 1f);
                        newPos = true;
                    }

                    if (Custom.Dist(player.DangerPos, oracle.bodyChunks[0].pos) >= 210f || Custom.Dist(player.DangerPos, oracle.bodyChunks[0].pos) < 100f || newPos) {
                        Vector2 vector2 = player.DangerPos + Custom.DegToVec(investigateAngle2) * Random.Range(110f, 200f);
                        if (oracle.room.aimap.getTerrainProximity(vector2) >= 2.0) {
                            if (pathProgression > 0.9) {
                                if (Custom.DistLess(oracle.firstChunk.pos, vector2, 30f))
                                    floatyMovement = false;
                                else if (!Custom.DistLess(nextPos, vector2, 30f))
                                    SetNewDestination(vector2);
                            }
                            nextPos = vector2;
                        }
                    }
                }
            } else if (movementBehavior == MovementBehavior.KeepDistance) {
                if (player != null) {
                    lookPoint = player.DangerPos;
                    investigateAngle = 180f;
                    if (investigateAngle2 < -90.0 || investigateAngle2 > 90.0 || oracle.room.aimap.getTerrainProximity(nextPos) < 2.0) {
                        investigateAngle2 = Mathf.Lerp(-70f, 70f, Random.value);
                        invstAngSpeed = Mathf.Lerp(0.4f, 0.8f, Random.value) * (Random.value < 0.5 ? -1f : 1f);
                    }

                    if (Custom.Dist(player.DangerPos, oracle.bodyChunks[0].pos) < 400f) {
                        Vector2 vector2 = player.DangerPos + Custom.DegToVec(investigateAngle2) * Random.Range(410f, 500f);
                        if (oracle.room.aimap.getTerrainProximity(vector2) >= 2.0) {
                            if (pathProgression > 0.9) {
                                if (Custom.DistLess(oracle.firstChunk.pos, vector2, 30f))
                                    floatyMovement = false;
                                else if (!Custom.DistLess(nextPos, vector2, 30f))
                                    SetNewDestination(vector2);
                            }
                            nextPos = vector2;
                        }
                    }
                }
            }
            if (awareOfPlayer)
                awareOfPlayerTime += 0.025f;
            currentGetTo = Custom.Bezier(lastPos, ClampVectorInRoom(lastPos + lastPosHandle), nextPos, ClampVectorInRoom(nextPos + nextPosHandle), pathProgression);
            pathProgression = Mathf.Min(1f, pathProgression + 1f / Mathf.Lerp((float)(40.0 + pathProgression * 80.0),
                Vector2.Distance(lastPos, nextPos) / 5f, 0.5f));
            consistentBasePosCounter++;
            if (oracle.room.readyForAI) { 
                Vector2 vector2 = new Vector2(Random.value * oracle.room.PixelWidth, Random.value * oracle.room.PixelHeight);
                if (!(oracle.room.GetTile(vector2).Solid || BasePosScore(vector2) + 40.0 >= BasePosScore(baseIdeal))) {
                    baseIdeal = vector2;
                    consistentBasePosCounter = 0;
                }
            }
            else
                baseIdeal = nextPos;
            ((TROracleArm)oracle.arm).Update();
            

            if (debug) {
                if (timeSinceLastError < 20) {
                    if (tikk % 10 == 0) {
                        errorLabel.visibleGlyphs = 2;
                        oracle.room.PlaySound(SoundID.SS_AI_Text_Blink);
                    }
                    else if (tikk % 10 == 5) errorLabel.visibleGlyphs = 0;
                }
                else errorLabel.visibleGlyphs = 0;
                if (IsHeld(errorTestPearl) && !errorPearlWasHeld) throw new Exception("THIS IS A TEST EXCEPTION");
                errorPearlWasHeld = IsHeld(errorTestPearl);
            }
        } catch (Exception e) {
            timeSinceLastError = 0;
            Plugin.Logger.LogError("ERROR");
            Plugin.Logger.LogError(e);
        }

        tikk++;
        timeSinceLastError++;
    }

    bool IsHeld(PhysicalObject obj) {
        if (player == null || obj == null) return false;
        foreach (var grasp in player.grasps) {
            if (grasp is not { grabbed: PebblesPearl }) continue;
            if (grasp.grabbed.abstractPhysicalObject.ID == obj.abstractPhysicalObject.ID) return true;
        }
        return false;
    }

    public new void Move() {
        
    }

    public class TRNoSubBehavior(SSOracleBehavior ow) : NoSubBehavior(ow)
    {
        public float PartialGravity;
        public bool SeenPlayer;
        public bool LockPaths;
        public bool GravOn;

        public override void Update() {
            if (LockPaths)
                owner.LockShortcuts();
            else
                owner.UnlockShortcuts();
        }

        public override float LowGravity => !GravOn ? -1f : PartialGravity;

        public override bool Gravity => GravOn;
    }
}

public class TROracleGraphics : OracleGraphics {
    public TROracleGraphics(TROracle ow) : base(ow) {
        totalSprites -= armBase.totalSprites;
        killSprite = totalSprites;
        totalSprites++;
        armBase.firstSprite = firstArmBaseSprite = totalSprites;
        totalSprites += armBase.totalSprites;
    }
    
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        FSprite[] sprites = sLeaser.sprites;
        sprites[neckSprite].scaleX = 3f;
        sprites[fadeSprite].color = Color.black;
        sprites[fadeSprite].alpha = 0.5f;
        for (int e = 0; e < 2; ++e)
            sprites[EyeSprite(e)].color = new Color(1f, 1f, 1f);
    }
    
    public override void AddToContainer(
        RoomCamera.SpriteLeaser sLeaser,
        RoomCamera rCam,
        FContainer newContatiner)
    {
        FSprite[] sprites = sLeaser.sprites;
        int killSprite = this.killSprite;
        if (sprites[killSprite] == null)
        {
            FSprite[] fspriteArray = sprites;
            int index = killSprite;
            FSprite fsprite = new FSprite("Futile_White");
            fsprite.shader = Custom.rainWorld.Shaders["FlatLight"];
            fspriteArray[index] = fsprite;
        }
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
    public override void Update() {
        base.Update();
        Oracle oracle = this.oracle;
        Room room1 = oracle.room;
        if (room1 == null || room1.game.cameras[0].AboutToSwitchRoom && lightsource != null)
            return;
        if (lightsource == null)
        {
            Room room2 = room1;
            LightSource lightSource1 = new LightSource(oracle.firstChunk.pos, false, Custom.HSL2RGB(0.5555556f, 1f, 0.5f), oracle);
            lightSource1.affectedByPaletteDarkness = 0.0f;
            LightSource lightSource2 = lightSource1;
            lightsource = lightSource1;
            LightSource lightSource3 = lightSource2;
            room2.AddObject(lightSource3);
        }
        else
        {
            lightsource.setAlpha = Mathf.Max(1f - room1.gravity, 0.01f);
            lightsource.setRad = 300f;
            lightsource.setPos = oracle.firstChunk.pos;
        }
    }
    public override void DrawSprites(
        RoomCamera.SpriteLeaser sLeaser,
        RoomCamera rCam,
        float timeStacker,
        Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (this.oracle is not TROracle oracle)
            return;
        Room room = oracle.room;
        if (room == null || oracle.slatedForDeletetion || room != rCam.room || dispose || !(oracle.oracleBehavior is TROracleBehavior oracleBehavior))
            return;
        FSprite[] sprites = sLeaser.sprites;
        BodyChunk bodyChunk = oracle.bodyChunks[1];
        BodyChunk firstChunk = oracle.firstChunk;
        Vector2 vector2_1 = Vector2.Lerp(bodyChunk.lastPos, bodyChunk.pos, timeStacker);
        Vector2 p2 = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector2 v1 = Custom.DirVec(vector2_1, p2);
        Vector2 vector2_2 = Custom.PerpendicularVector(v1);
        FSprite fsprite = sprites[killSprite];
        if (oracleBehavior.killFac > 0.0)
        {
            fsprite.isVisible = true;
            Player player = oracleBehavior.player;
            if (player != null)
            {
                BodyChunk mainBodyChunk = player.mainBodyChunk;
                fsprite.SetPosition(Vector2.Lerp(mainBodyChunk.lastPos, mainBodyChunk.pos, timeStacker) - camPos);
            }
            float f = Mathf.Lerp(oracleBehavior.lastKillFac, oracleBehavior.killFac, timeStacker);
            fsprite.scale = Mathf.Lerp(200f, 2f, Mathf.Pow(f, 0.5f));
            fsprite.alpha = Mathf.Pow(f, 3f);
        }
        else
            fsprite.isVisible = false;
        float t = Mathf.Lerp(lastEyesOpen, eyesOpen, timeStacker);
        GenericBodyPart[] hands = this.hands;
        for (int index1 = 0; index1 < hands.Length; ++index1)
        {
            sprites[EyeSprite(index1)].scaleY = Mathf.Lerp(1f, 2.5f, t);
            float num1 = index1 == 1 ? -1f : 1f;
            GenericBodyPart genericBodyPart = hands[index1];
            Vector2 vector2_3 = Vector2.Lerp(genericBodyPart.lastPos, genericBodyPart.pos, timeStacker);
            Vector2 vector2_4 = p2 + vector2_2 * 4f * num1;
            Vector2 cB1 = vector2_3 + Custom.DirVec(vector2_3, vector2_4) * 3f + v1;
            Vector2 cA1 = vector2_4 + vector2_2 * 5f * num1;
            Vector2 vector2_5 = vector2_4 - vector2_2 * 2f * num1;
            for (int index2 = 0; index2 < 7; ++index2)
            {
                Vector2 vector2_6 = Custom.Bezier(vector2_4, cA1, vector2_3, cB1, (float) index2 / 6f);
                Vector2 v2 = Custom.DirVec(vector2_5, vector2_6);
                Vector2 vector2_7 = Custom.PerpendicularVector(v2) * (index1 == 0 ? -1f : 1f);
                float num2 = Vector2.Distance(vector2_5, vector2_6);
                TriangleMesh triangleMesh = sprites[HandSprite(index1, 1)] as TriangleMesh;
                triangleMesh.MoveVertice(index2 * 4, vector2_6 - v2 * num2 * 0.3f - vector2_7 * 4f - camPos);
                triangleMesh.MoveVertice(index2 * 4 + 1, vector2_6 - v2 * num2 * 0.3f + vector2_7 * 4f - camPos);
                triangleMesh.MoveVertice(index2 * 4 + 2, vector2_6 - vector2_7 * 4f - camPos);
                triangleMesh.MoveVertice(index2 * 4 + 3, vector2_6 + vector2_7 * 4f - camPos);
                vector2_5 = vector2_6;
            }
            GenericBodyPart foot = feet[index1];
            Vector2 vector2_8 = Vector2.Lerp(foot.lastPos, foot.pos, timeStacker);
            Vector2 b = Vector2.Lerp(knees[index1, 1], knees[index1, 0], timeStacker);
            Vector2 cB2 = Vector2.Lerp(vector2_8, b, 0.9f);
            Vector2 cA2 = Vector2.Lerp(vector2_1, b, 0.9f);
            Vector2 vector2_9 = vector2_1 - vector2_2 * 2f * num1;
            float num3 = 4f;
            for (int index3 = 0; index3 < 7; ++index3)
            {
                Vector2 vector2_10 = Custom.Bezier(vector2_1, cA2, vector2_8, cB2, (float) index3 / 6f);
                Vector2 v3 = Custom.DirVec(vector2_9, vector2_10);
                Vector2 vector2_11 = Custom.PerpendicularVector(v3) * (index1 == 0 ? -1f : 1f);
                float num4 = Vector2.Distance(vector2_9, vector2_10);
                TriangleMesh triangleMesh = sprites[FootSprite(index1, 1)] as TriangleMesh;
                triangleMesh.MoveVertice(index3 * 4, vector2_10 - v3 * num4 * 0.3f - vector2_11 * (num3 + 2f) * 0.5f - camPos);
                triangleMesh.MoveVertice(index3 * 4 + 1, vector2_10 - v3 * num4 * 0.3f + vector2_11 * (num3 + 2f) * 0.5f - camPos);
                triangleMesh.MoveVertice(index3 * 4 + 2, vector2_10 - vector2_11 * 2f - camPos);
                triangleMesh.MoveVertice(index3 * 4 + 3, vector2_10 + vector2_11 * 2f - camPos);
                vector2_9 = vector2_10;
                num3 = 2f;
            }
        }
    }
}

public class TROracleArm : Oracle.OracleArm {
    public TROracleArm(TROracle oracle) : base(oracle) {
        var room = oracle.room; baseMoveSoundLoop = new StaticSoundLoop(SoundID.SS_AI_Base_Move_LOOP, oracle.firstChunk.pos, room, 1f, 1f);
        string a = room.game?.StoryCharacter?.value;
        cornerPositions[0] = room.MiddleOfTile(10, 31);
        cornerPositions[1] = room.MiddleOfTile(38, 31);
        cornerPositions[2] = room.MiddleOfTile(38, 3);
        cornerPositions[3] = room.MiddleOfTile(10, 3);
    }

    public new void Update() {
        if (oracle.Consious) {
            float num = 1f;
            if (ModManager.MSC)
                num = oracle.dazed <= 240.0 ? (float) (1.0 - oracle.dazed / 240.0) : 0.0f; 
            foreach (var t in oracle.bodyChunks)
                t.vel *= 0.4f;
            oracle.bodyChunks[0].vel += Vector2.ClampMagnitude(oracle.oracleBehavior.OracleGetToPos - oracle.bodyChunks[0].pos, 100f) / 100f * (6.2f * num);
            for (int index = 1; index < oracle.bodyChunks.Length; ++index)
                oracle.bodyChunks[index].vel += Vector2.ClampMagnitude(oracle.oracleBehavior.OracleGetToPos - oracle.oracleBehavior.GetToDir * oracle.bodyChunkConnections[0].distance - oracle.bodyChunks[0].pos, 100f) / 100f * (3.2f * num);
        }
        Vector2 baseGetToPos = oracle.oracleBehavior.BaseGetToPos;
        Vector2 vector2 = new Vector2(Mathf.Clamp(baseGetToPos.x, cornerPositions[0].x, cornerPositions[1].x), cornerPositions[0].y);
        float num1 = Vector2.Distance(vector2, baseGetToPos);
        float num2 = Mathf.InverseLerp(cornerPositions[0].x, cornerPositions[1].x, baseGetToPos.x);
        for (int index = 1; index < 4; ++index) {
            Vector2 a = index % 2 != 0 
                ? new Vector2(cornerPositions[index].x, Mathf.Clamp(baseGetToPos.y, cornerPositions[2].y, cornerPositions[0].y))
                : new Vector2(Mathf.Clamp(baseGetToPos.x, cornerPositions[0].x, cornerPositions[1].x), cornerPositions[index].y);
            float num3 = Vector2.Distance(a, baseGetToPos);
            if (num3 < num1) {
                vector2 = a;
                num1 = num3;
                switch (index) {
                    case 1:
                        num2 = index + Mathf.InverseLerp(cornerPositions[0].y, cornerPositions[2].y, baseGetToPos.y);
                        continue;
                    case 2:
                        num2 = index + Mathf.InverseLerp(cornerPositions[1].x, cornerPositions[0].x, baseGetToPos.x);
                        continue;
                    case 3:
                        num2 = index + Mathf.InverseLerp(cornerPositions[2].y, cornerPositions[0].y, baseGetToPos.y);
                        continue;
                    default:
                        continue;
                }
            }
        }
        baseMoving = Vector2.Distance(BasePos(1f), vector2) > (baseMoving ? 50.0 : 350.0) && oracle.oracleBehavior.consistentBasePosCounter > 30;
        lastFramePos = framePos;
        if (baseMoving) {
            framePos = Mathf.MoveTowardsAngle(framePos * 90f, num2 * 90f, 0.3f) / 90f;
            if (baseMoveSoundLoop != null) {
                baseMoveSoundLoop.volume = Mathf.Min(baseMoveSoundLoop.volume + 0.1f, 1f);
                baseMoveSoundLoop.pitch = Mathf.Min(baseMoveSoundLoop.pitch + 0.025f, 1f);
            }
        }
        else if (baseMoveSoundLoop != null) {
            baseMoveSoundLoop.volume = Mathf.Max(baseMoveSoundLoop.volume - 0.1f, 0.0f);
            baseMoveSoundLoop.pitch = Mathf.Max(baseMoveSoundLoop.pitch - 0.025f, 0.5f);
        }
        if (baseMoveSoundLoop != null) {
            baseMoveSoundLoop.pos = BasePos(1f);
            baseMoveSoundLoop.Update();
            if (ModManager.MSC)
                baseMoveSoundLoop.volume *= 1f - oracle.noiseSuppress;
        }
    }
}