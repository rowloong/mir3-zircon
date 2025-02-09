﻿using Library;
using Server.DBModels;

namespace Server.Models.Magics
{
    [MagicType(MagicType.Shocked)]
    public class Shocked : MagicObject
    {
        protected override Element Element => Element.Lightning;
        public override bool AugmentedSkill => true;

        public Shocked(PlayerObject player, UserMagic magic) : base(player, magic)
        {

        }
    }
}
