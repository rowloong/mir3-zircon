﻿using Library;
using Server.DBModels;
using Server.Envir;
using System;
using S = Library.Network.ServerPackets;

namespace Server.Models.Magics
{
    [MagicType(MagicType.FlamingSword)]
    public class FlamingSword : MagicObject
    {
        protected override Element Element => Element.None;
        public override bool AttackSkill => true;

        public bool CanFlamingSword { get; private set; }
        public DateTime FlamingSwordTime { get; private set; }

        public FlamingSword(PlayerObject player, UserMagic magic) : base(player, magic)
        {

        }

        public override void Process()
        {
            if (CanFlamingSword && SEnvir.Now >= FlamingSwordTime)
            {
                CanFlamingSword = false;
                Player.Enqueue(new S.MagicToggle { Magic = Type, CanUse = CanFlamingSword });

                Player.Connection.ReceiveChat(string.Format(Player.Connection.Language.ChargeExpire, Magic.Info.Name), MessageType.System);
                foreach (SConnection con in Player.Connection.Observers)
                    con.ReceiveChat(string.Format(con.Language.ChargeExpire, Magic.Info.Name), MessageType.System);
            }
        }

        public override void Toggle(bool canUse)
        {
            if (Magic.Cost > Player.CurrentMP || SEnvir.Now < Magic.Cooldown || Player.Dead || (Player.Poison & PoisonType.Paralysis) == PoisonType.Paralysis || (Player.Poison & PoisonType.Silenced) == PoisonType.Silenced) return;

            Player.ChangeMP(-Magic.Cost);
            Magic.Cooldown = SEnvir.Now.AddMilliseconds(Magic.Info.Delay);
            Player.Enqueue(new S.MagicCooldown { InfoIndex = Magic.Info.Index, Delay = Magic.Info.Delay });

            if (CanFlamingSword)
            {
                Player.Connection.ReceiveChat(string.Format(Player.Connection.Language.ChargeFail, Magic.Info.Name), MessageType.System);

                foreach (SConnection con in Player.Connection.Observers)
                    con.ReceiveChat(string.Format(con.Language.ChargeFail, Magic.Info.Name), MessageType.System);
            }
            else
            {
                FlamingSwordTime = SEnvir.Now.AddSeconds(12);
                CanFlamingSword = true;
                Player.Enqueue(new S.MagicToggle { Magic = Type, CanUse = CanFlamingSword });
            }

            //Delay DragonRise
            if (Player.Magics.TryGetValue(MagicType.DragonRise, out UserMagic magic) && SEnvir.Now.AddSeconds(2) > magic.Cooldown)
            {
                magic.Cooldown = SEnvir.Now.AddSeconds(2);
                Player.Enqueue(new S.MagicCooldown { InfoIndex = magic.Info.Index, Delay = 2000 });
            }

            //Delay BladeStorm
            if (Player.Magics.TryGetValue(MagicType.BladeStorm, out magic) && SEnvir.Now.AddSeconds(2) > magic.Cooldown)
            {
                magic.Cooldown = SEnvir.Now.AddSeconds(2);
                Player.Enqueue(new S.MagicCooldown { InfoIndex = magic.Info.Index, Delay = 2000 });
            }
        }

        public override AttackCast AttackCast(MagicType attackType)
        {
            var response = new AttackCast();

            if (attackType != Type || !CanFlamingSword)
                return response;

            if (Player.Level < Magic.Info.NeedLevel1)
                return response;

            CanFlamingSword = false;
            Player.Enqueue(new S.MagicToggle { Magic = Type, CanUse = false });

            response.Cast = true;
            response.Magics.Add(Type);

            return response;
        }

        public override int ModifyPowerAdditionner(bool primary, int power, MapObject ob, Stats stats = null, int extra = 0)
        {
            power = power * Magic.GetPower() / 100;

            return power;
        }
    }
}
