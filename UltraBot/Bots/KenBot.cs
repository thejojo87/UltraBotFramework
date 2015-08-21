﻿using System;
using UltraBot;

public class KenBot : Bot
{
    public KenBot()
    {
        RegisterState(typeof(ThrowTechState));
		RegisterState(typeof(DefendState));
    }
    public override BotAIState DefaultState()
    {
        return new IdleState();
    }
    public class TestState : BotAIState
    {
        protected override System.Collections.Generic.IEnumerator<string> Run(Bot bot)
        {
            while(Math.Abs(bot.myState.XDistance) > 1)
            {
                bot.pressButton("6");
                yield return "Getting in range";
            }
            bot.pressButton("2LP");
            yield return "CR JAB";
            
        }
    }
}
