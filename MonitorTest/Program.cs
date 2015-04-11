﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using UltraBot;
using System.Threading;
using DX9OverlayAPIWrapper;
namespace MonitorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DX9Overlay.SetParam("process", "SSFIV.exe");
            DX9Overlay.DestroyAllVisual();
            TextLabel roundTimer = new TextLabel("Consolas", 10, TypeFace.NONE, new Point(390, 0), Color.White, "", true, true);
            TextLabel player1 = new TextLabel("Consolas", 10, TypeFace.NONE, new Point(90, 0), Color.White, "", true, true);
            TextLabel player2 = new TextLabel("Consolas", 10, TypeFace.NONE, new Point(480, 0), Color.White, "", true, true);
            
            //Stopwatch sw = new Stopwatch();
            

            // Do something you want to time

            
            var ms = new MatchState();
            var f1 = FighterState.getFighter(0);
            var f2 = FighterState.getFighter(1);
            var KenBot = new KenBot(0);
            Util.Init();
            while (true)
            {
                ms.Update();
                roundTimer.Text = String.Format("Frame:{0}", ms.FrameCounter);
                UpdateOverlay(player1, f1);
                UpdateOverlay(player2, f2);
                KenBot.Run();
            }
        }

        private static void UpdateOverlay(TextLabel label, FighterState f)
        {
            f.UpdatePlayerState();
            label.Text = String.Format("X={0,-7} Y={1,-7} XVel={12,-7} YVel={13,-7}\n{2,-15} F:{3,-3}\nACT:{4,-3} ENDACT:{5,-3} IASA:{6,-3} TOT:{7,-3}\n{8,-10} {9,-10} {10,-10} {11:X}\n{14}\n{15}",
                f.X, f.Y, f.ScriptName, f.ScriptFrame, f.ScriptFrameHitboxStart, f.ScriptFrameHitboxEnd, f.ScriptFrameIASA, f.ScriptFrameTotal, f.State, f.AState, f.StateTimer, f.RawState, f.XVelocity, f.YVelocity, String.Join(", ", f.ActiveCancelLists), String.Join("\n", f.InputBuffer));
        }
    }
}
