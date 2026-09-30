using MoreSlugcats;
using RWCustom;
using Smoke;
using UnityEngine;
using Watcher;
using static Pom.Pom;

namespace TestIterator;

public class Objects {
    public static void Register() {
        RegisterManagedObject<AreaSteamer, AreaSteamer.Data, ManagedRepresentation>("AreaSteamer", "Test Iterator");
    }
}

public class AreaSteamer : UpdatableAndDeletable, IProvideWarmth {
    private readonly PlacedObject PlacedObject;
    private SteamSmoke steam;
    private const float hazardRange = 0.9f;
    private RectangularDynamicSoundLoop SoundLoop;
    public Room loadedRoom => room;
    private Vector2 center;
    public Vector2 Position() => center;
    public float range => 400f*power;
    public float warmth => RainWorldGame.DefaultHeatSourceWarmth*2*power;

    public AreaSteamer(PlacedObject pobj, Room room) {
        PlacedObject = pobj;
        this.room = room;
        steam = new SteamSmoke(room);
        stateSwitchTime = 0;
        var data = (Data)PlacedObject.data;
        center = new Vector2((pobj.pos.x + data.Dir.x) / 2f, (pobj.pos.y + data.Dir.y) / 2f);
        float mag = Vector2.Distance(center, pobj.pos);
        SoundLoop = new RectangularDynamicSoundLoop(this,
            new FloatRect(center.x - mag, center.y - mag, center.x + mag, center.y + mag), room) {
            sound = SoundID.Gate_Water_Steam_LOOP
        };
        Plugin.Logger.LogDebug($"areasteamer area: {data.Area.x}, {data.Area.y}");
        Plugin.Logger.LogDebug($"areasteamer dir: {data.Dir.x}, {data.Dir.y}");
    }
    // another finite state machine lmao
    private int t => room.game.timeInRegionThisCycle - stateSwitchTime;
    private int stateSwitchTime;
    /* 0 = idle
     * 1 = windup
     * 2 = steaming
     * 3 = winddown
     */
    private float power = 0f;
    private int state = 0;

    private void SwitchState(int _state) {
        state = _state;
        stateSwitchTime = room.game.timeInRegionThisCycle;
    }
    public override void Update(bool eu) {
        base.Update(eu);
        var data = (Data)PlacedObject.data;
        switch (state) {
            case 0: 
                power = 0f;
                if (t >= data.Interval)
                    SwitchState(1);
                break;
            case 1:
                if (t >= data.Windup) {
                    SwitchState(2);
                    break;
                }
                power = t / (float)data.Windup;
                break;
            case 2: 
                power = 1f;
                if (t >= data.Duration)
                    SwitchState(3);
                break;
            case 3:
                if (t >= data.Windup) {
                    SwitchState(0);
                    break;
                }
                power = 1f - t / (float)data.Windup;
                break;
        }

        if (power > 0f && Random.value <= power) {
            for (int i = 0; i < data.Mult; i++)
                steam.EmitSmoke(PlacedObject.pos + data.Area*Random.value, data.Dir*Mathf.Pow(power, 2), room.RoomRect, data.Lifetime);
        }
        if (SoundLoop != null) {
            center = new Vector2((PlacedObject.pos.x + data.Dir.x) / 2f, (PlacedObject.pos.y + data.Dir.y) / 2f);
            float mag = Vector2.Distance(center, PlacedObject.pos);
            SoundLoop.rect = new FloatRect(center.x - mag, center.y - mag, center.x + mag, center.y + mag);
            SoundLoop.Volume = Mathf.Max(power, 0f);
            if (SoundLoop.Volume > 0f) SoundLoop.Update();
        }
        if (!data.Hazard || state != 2) return;
        // *borrowed* from SteamHazard
        foreach (var partikl in steam.particles) {
            
            if (partikl.life <= range) continue;
            foreach (var objectObject in room.physicalObjects) {
                foreach (var obj in objectObject) {
                    foreach (var chunk in obj.bodyChunks) {
                        
                        //var v = chunk.pos + chunk.ContactPoint.ToVector2() * (chunk.rad + 30f);
                        if (!(Vector2.Distance(partikl.pos, chunk.pos) < chunk.rad+20f) || obj is not Creature c) continue;
                        c.stun = 100;
                        room.AddObject(new CreatureSpasmer(c, false, c.stun));
                        room.PlaySound(SoundID.Gate_Water_Steam_Puff, c.mainBodyChunk, false, 0.8f, 1f);
                    }
                }
            }
        }
    }

    public class Data(PlacedObject obj) : ManagedData(obj, null) {
        [Vector2Field("dir", 0f, 100f, Vector2Field.VectorReprType.line, "Direction")]
        internal Vector2 Dir;
        [Vector2Field("area", 20f, 0, Vector2Field.VectorReprType.circle, "Steam line")]
        internal Vector2 Area;
        [IntegerField("int", 0, 100, 100, ManagedFieldWithPanel.ControlType.slider, "Interval")]
        internal int Interval;
        [IntegerField("duration", 1, 100, 100, ManagedFieldWithPanel.ControlType.slider, "Duration")]
        internal int Duration;
        [FloatField("life", 0, 2, 1, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Lifetime")]
        internal float Lifetime;
        [IntegerField("windup", 0, 40, 20,  ManagedFieldWithPanel.ControlType.slider, "Windup")]
        internal int Windup;
        [IntegerField("mult", 1, 10, 1, ManagedFieldWithPanel.ControlType.slider, "Multiplier")]
        internal int Mult;
        [BooleanField("hazard", true, ManagedFieldWithPanel.ControlType.button, "Hazardous")]
        internal bool Hazard;
    }
}
