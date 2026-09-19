using System.Linq;
using UnityEngine;

namespace TestIterator;
using EffExt;

public static class Effects {
    public static void Register() {
        EffectDefinitionBuilder cycleDrainDefBuilder = new("CycleDrain");
        cycleDrainDefBuilder.SetUADFactory(CycleDrain.UADFactory).SetCategory("Test Iterator").Register();
        Pom.Pom.RegisterEmptyObjectType<CycleDrain.CycleDrainMax, Pom.Pom.ManagedRepresentation>("CycleDrainMax", "Test Iterator");
    }

    public class CycleDrain : UpdatableAndDeletable {
        public static UpdatableAndDeletable UADFactory(Room _room, EffectExtraData _data, bool firstTimeRealized) {
            return new CycleDrain(_room, _data);
        }
        public class CycleDrainMax(PlacedObject owner) : Pom.Pom.ManagedData(owner, null);
        private EffectExtraData data;
        public CycleDrain(Room room, EffectExtraData data) {
            this.data = data;
            endLevel = room.waterObject.originalWaterLevel;
            var limits = (from x in room.roomSettings.placedObjects where x.type.value == "CycleDrainMax" select x).ToArray();
            if (limits.Length == 0) {
                Plugin.Logger.LogInfo($"No limit points found, start level same as end");
                startLevel = endLevel;
            } else {
                Plugin.Logger.LogInfo($"CycleDrain: found {limits.Length} limit points");
                startLevel = limits[Random.Range(0, limits.Length)].pos.y;
                Plugin.Logger.LogInfo($"CycleDrain: start level: {startLevel}");
            }
        }
        public float startLevel;
        public float endLevel;
        
        public override void Update(bool eu) {
            if (room?.waterObject is null) return;
            room.waterObject.originalWaterLevel = Mathf.Lerp(startLevel, endLevel, room.world.rainCycle.CycleProgression);
            Plugin.Logger.LogInfo(room.waterObject.originalWaterLevel);
        }
    }
}