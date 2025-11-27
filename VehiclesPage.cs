using GTA;
using GTA.Native;
using GTA.Math;

namespace NinnyTrainer.Pages
{
    public static class VehiclePage
    {
        public static void SpawnSupercar()
        {
            Model m = new Model(VehicleHash.Adder);
            m.Request(500);

            Ped p = Game.Player.Character;
            Vector3 pos = p.Position + p.ForwardVector * 5f;

            Vehicle v = World.CreateVehicle(m, pos);
            v.PlaceOnGround();
            p.SetIntoVehicle(v, VehicleSeat.Driver);
        }

        public static void Repair()
        {
            Vehicle v = Game.Player.Character.CurrentVehicle;
            if (v != null) v.Repair();
        }

        public static void Flip()
        {
            Vehicle v = Game.Player.Character.CurrentVehicle;
            if (v != null)
            {
                Vector3 rot = v.Rotation;
                v.Rotation = new Vector3(0, rot.Y, rot.Z);
            }
        }

        public static void SetInvincible(bool state)
        {
            Vehicle v = Game.Player.Character.CurrentVehicle;
            if (v != null)
                v.IsInvincible = state;
        }

        public static void SetTorque(float value)
        {
            Function.Call(Hash.MODIFY_VEHICLE_TOP_SPEED, Game.Player.Character.CurrentVehicle, value);
        }

        public static void SetBrakes(float value)
        {
            Vehicle v = Game.Player.Character.CurrentVehicle;
            if (v != null)
            {
                v.EngineTorqueMultiplier = value;
            }
        }
    }
}
