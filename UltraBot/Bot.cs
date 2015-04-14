﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WindowsInput;
using CSScriptLibrary;
namespace UltraBot
{
    /// <summary>
    /// This is a dummy interface used for the CS-Script integration
    /// </summary>
    public interface IBot
    {
         void Init(int index);
         void StateCheck();
         List<BotAIState> peekStateStack();
         List<Combo> getComboList();
         void Run();
    }

    public class Bot : IBot
    {
        /// <summary>
        /// This function sets up the dynamic bot loader with search paths!
        /// </summary>
        /// <param name="Dir"></param>
        public static void AddSearchPath(string Dir)
        {
            CSScript.GlobalSettings.AddSearchDir(Dir);
        }
        /// <summary>
        /// This function loads, compiles, and instantiates a bot on the fly.
        /// </summary>
        /// <param name="BotName">The name of the bot. Should be in the a folder that has been added to the search path
        /// via AddSearchPath, and named BotName.cs</param>
        /// <returns></returns>
        public static IBot LoadBotFromFile(string BotName)
        {
            
            var tmp = CSScript.Load(BotName + ".cs");
            
            var tmp2 = tmp.CreateInstance(BotName);
            IBot bot = tmp2.AlignToInterface<IBot>();
            foreach(var dir in CSScript.GlobalSettings.SearchDirs.Split(';'))
            {
                var fname = System.IO.Path.Combine(dir,BotName+".xml");
                if (System.IO.File.Exists(fname))
                {
                    LoadCombos((Bot)bot, fname);
                }
            }
            return bot;

        }
        private static void LoadCombos(Bot bot, string XMLFilename)
        {
            var xmldoc = new XmlDocument();
            xmldoc.Load(XMLFilename);
            foreach (XmlNode comboXml in xmldoc.DocumentElement.SelectNodes("//Combo"))
            {
                var combo = new Combo();

                combo.Type = (ComboType)Enum.Parse(typeof(ComboType), comboXml.Attributes["Type"].Value);
                combo.Startup = Int32.Parse(comboXml.Attributes["Startup"].Value);
                combo.XMin = float.Parse(comboXml.Attributes["XMin"].Value);
                combo.XMax = float.Parse(comboXml.Attributes["XMax"].Value);
                combo.YMin = float.Parse(comboXml.Attributes["YMin"].Value);
                combo.YMax = float.Parse(comboXml.Attributes["YMax"].Value);
                combo.EXMeter = Int32.Parse(comboXml.Attributes["EXMeter"].Value);
                combo.Ultra = bool.Parse(comboXml.Attributes["Ultra"].Value);
                combo.Input = comboXml.Attributes["Input"].Value;
                bot.comboList.Add(combo);
            }
        }

        public void Init(int index)
        {
            myState = FighterState.getFighter(index);
            enemyState = FighterState.getFighter(index == 0 ? 1 : 0);
        }

        public FighterState myState;
        public FighterState enemyState;

         
        
        public List<BotAIState> peekStateStack()
        {
            return stateStack.AsEnumerable().ToList();
        }
        /// <summary>
        /// This function does the magic, and makes the bot actually work. It only handles input when the window is focused.
        /// TODO: MatchState also has some of this current window logic, feels redundant
        /// </summary>
        public virtual void Run()
        {
            //Setup some derived variables.
            myState.XDistance = myState.X - enemyState.X;
            myState.YDistance = myState.Y - enemyState.Y;

            StateCheck();
            stateStack[0].Run(this);
            if (Util.GetActiveWindowTitle() == "SSFIVAE")
            {
                foreach (var key in pressed)
                {                                   
                    WindowsInput.InputSimulator.SimulateKeyDown(map(key));
                }
                foreach(var key in last_pressed)
                    if(!pressed.Contains(key))
                        WindowsInput.InputSimulator.SimulateKeyUp(map(key));
                last_pressed.Clear();
                last_pressed.AddRange(pressed);
                pressed.Clear();
            }
        }


        #region State Management
        private BotAIState previousState;
        private List<BotAIState> stateStack = new List<BotAIState>();
        /// <summary>
        /// This function runs before any state.
        /// By overriding this function, you can have checks that force the bot into an arbitrary state based on triggers.
        /// Eventually I want to actually use a event/listener pattern here, and allow states to be registered with autotriggers that automatically enter them.
        /// I.E. ThrowTechState would have a Statecheck that selftriggers when the bot is thrown. TODO
        /// </summary>
        public virtual void StateCheck()
        {

        }
        /// <summary>
        /// This ends the current state permanantly and changes to a new state.
        /// 
        /// </summary>
        /// <param name="nextState"></param>
        public void changeState(BotAIState nextState)
        {
            if (stateStack.Count > 0)
            {
                previousState = stateStack[0];
                stateStack.RemoveAt(0);
            }
            stateStack.Insert(0, nextState);
        }
        /// <summary>
        /// This ends the current state, removes it from the stack, and changes back to the previous state, 
        /// the one that last called pushStack().
        /// </summary>
        public void popState()
        {
            previousState = stateStack[0];
            stateStack.RemoveAt(0);
        }
        /// <summary>
        /// This switches to a new state, leaving the previous one on the stack, 
        /// allowing the new state to call popState() and return execution to the current stack 
        /// </summary>
        /// <param name="nextState"></param>
        public void pushState(BotAIState nextState)
        {
            previousState = stateStack[0];
            stateStack.Insert(0, nextState);
        }
                /// <summary>
        /// These keycodes exist so that we can map them to keyboard or vJoy or whatever.
        /// </summary>
        #endregion
        #region Input Management
        private List<VirtualKeyCode> pressed = new List<VirtualKeyCode>();
        private List<VirtualKeyCode> last_pressed = new List<VirtualKeyCode>();
        private List<VirtualKeyCode> held = new List<VirtualKeyCode>();

        private static WindowsInput.VirtualKeyCode map(VirtualKeyCode key)
        {
            WindowsInput.VirtualKeyCode rawKey;
            switch (key)
            {
                case VirtualKeyCode.DOWN:
                default:
                    rawKey = WindowsInput.VirtualKeyCode.DOWN;
                    break;
                case VirtualKeyCode.LEFT:
                    rawKey = WindowsInput.VirtualKeyCode.LEFT;
                    break;
                case VirtualKeyCode.RIGHT:
                    rawKey = WindowsInput.VirtualKeyCode.RIGHT;
                    break;
                case VirtualKeyCode.UP:
                    rawKey = WindowsInput.VirtualKeyCode.UP;
                    break;
                case VirtualKeyCode.LP:
                    rawKey = WindowsInput.VirtualKeyCode.VK_9;
                    break;
                case VirtualKeyCode.MP:
                    rawKey = WindowsInput.VirtualKeyCode.VK_0;
                    break;
                case VirtualKeyCode.HP:
                    rawKey = WindowsInput.VirtualKeyCode.SUBTRACT;
                    break;
                case VirtualKeyCode.PPP:
                    rawKey = WindowsInput.VirtualKeyCode.OEM_PLUS;
                    break;
                case VirtualKeyCode.LK:
                    rawKey = WindowsInput.VirtualKeyCode.VK_O;
                    break;
                case VirtualKeyCode.MK:
                    rawKey = WindowsInput.VirtualKeyCode.VK_P;
                    break;
                case VirtualKeyCode.HK:
                    rawKey = WindowsInput.VirtualKeyCode.OEM_4;
                    break;
                case VirtualKeyCode.KKK:
                    rawKey = WindowsInput.VirtualKeyCode.OEM_6;
                    break;

            }
            return rawKey;
        }
        public enum VirtualKeyCode
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            START,
            SELECT,
            LP,
            MP,
            HP,
            PPP,
            LK,
            MK,
            HK,
            KKK,
            THROW,
            FOCUS
        }
        public VirtualKeyCode Forward()
        {
            if (myState.XDistance > 0)
                return VirtualKeyCode.LEFT;
            return VirtualKeyCode.RIGHT;
        }
        public VirtualKeyCode Up()
        {
            return VirtualKeyCode.UP;
        }
        public VirtualKeyCode Down()
        {
            return VirtualKeyCode.DOWN;
        }
        public VirtualKeyCode Back()
        {
            if (myState.XDistance > 0)
                return VirtualKeyCode.RIGHT;
            return VirtualKeyCode.LEFT;
        }
        public void pressButton(string key)
        {
            if (key.Contains("2"))
                pressButton(VirtualKeyCode.DOWN);
            if (key.Contains("6"))
                pressButton(this.Forward());
            if (key.Contains("4"))
                pressButton(this.Back());
            if (key.Contains("8"))
                pressButton(VirtualKeyCode.UP);
			if (key.Contains("1"))	
			{
				pressButton(this.Back());
				pressButton(VirtualKeyCode.DOWN);
			}
			if (key.Contains("1"))	
			{
				pressButton(this.Forward());
				pressButton(VirtualKeyCode.DOWN);
			}
			if (key.Contains("7"))	
			{
				pressButton(this.Back());
				pressButton(VirtualKeyCode.UP);
			}
			if (key.Contains("9"))	
			{
				pressButton(this.Forward());
                pressButton(VirtualKeyCode.UP);
			}
			
            if (key.Contains("LP"))
                pressButton(VirtualKeyCode.LP);
            if (key.Contains("MP"))
                pressButton(VirtualKeyCode.MP);
            if (key.Contains("HP"))
                pressButton(VirtualKeyCode.HP);
            if (key.Contains("LK"))
                pressButton(VirtualKeyCode.LK);
            if (key.Contains("MK"))
                pressButton(VirtualKeyCode.MK);
            if (key.Contains("HK"))
                pressButton(VirtualKeyCode.HK);
        }
        public void pressButton(VirtualKeyCode key)
        {
            if (!pressed.Contains(key))
                pressed.Add(key);
        }
        public void holdButton(VirtualKeyCode key)
        {
            if (!held.Contains(key))
                held.Add(key);
            pressButton(key);
        }
        public void releaseButton(VirtualKeyCode key)
        {
            if (held.Contains(key))
                held.Remove(key);
            if (pressed.Contains(key))
                pressed.Remove(key);
        }
        #endregion

        private List<Combo> comboList = new List<Combo>();
        public List<Combo> getComboList()
        {

            return comboList;
        }
    }
}
