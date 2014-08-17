using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SFAhri
{
    class Program
    {
        public static string Name = "Ahri";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Spell Q, W, E;
        public static Items.Item DFG;

        public static Menu SF;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != Name) return;

            Q = new Spell(SpellSlot.Q, 880);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 975);

            Q.SetSkillshot(0.50f, 100f, 1100f, false, Prediction.SkillshotType.SkillshotLine);
            E.SetSkillshot(0.50f, 60f, 1200f, true, Prediction.SkillshotType.SkillshotLine);
            //Base menu
            SF = new Menu("SFAhri", Name, true);
            //Orbwalker and menu
            SF.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(SF.SubMenu("Orbwalker"));
            //Target selector and menu
            var ts = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(ts);
            SF.AddSubMenu(ts);
            //Combo menu
            SF.AddSubMenu(new Menu("Combo", "Combo"));
            SF.SubMenu("Combo").AddItem(new MenuItem("useW", "Use W?").SetValue(true));
            SF.AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            //Exploits
            SF.AddItem(new MenuItem("NFE", "No-face exploit").SetValue(true));
            //Make the menu visible
            SF.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw; // Add onDraw
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)

            Game.PrintChat("SFAhri loaded! By iSnorflake");


        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (SF.Item("ComboActive").GetValue<KeyBind>().Active) {
                Combo();
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            
        }
        public static void Combo()
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (target != null) return;
            
                if (target.IsValidTarget(DFG.Range) && DFG.IsReady())
                    DFG.Cast(target);
                if (target.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    if (SF.Item("NFE").GetValue<bool>())
                    {
                        var pTarget = Prediction.GetBestPosition(target, 0.5f, 100f, 1100f, Player.ServerPosition, 880f, false, Prediction.SkillshotType.SkillshotLine).Position;
                        Q.Cast(pTarget, true);
                    }

                    else
                    {
                        var pTarget = Prediction.GetBestPosition(target, 0.5f, 100f, 1100f, Player.ServerPosition, 880f, false, Prediction.SkillshotType.SkillshotLine).Position;
                        Q.Cast(pTarget, false);
                    }
                    }
                if (target.IsValidTarget( W.Range) && W.IsReady())
                {
                    W.Cast();
                }
                if (target.IsValidTarget(E.Range) & E.IsReady())
                {
                    if (SF.Item("NFE").GetValue<bool>())
                    {
                        var pTarget = Prediction.GetBestPosition(target, 0.5f, 60f, 1200f, Player.ServerPosition, 880f, true, Prediction.SkillshotType.SkillshotLine).Position;
                        E.Cast(pTarget, true);
                    }
                    else
                    {
                        var pTarget = Prediction.GetBestPosition(target, 0.5f, 60f, 1200f, Player.ServerPosition, 880f, true, Prediction.SkillshotType.SkillshotLine).Position;
                        E.Cast(pTarget, false);
                    }
                }
                
            }
        private static double GetDamage(Obj_AI_Base unit) // Credit to TC-Crew and PQMailer for the base of this 
        {
            double damage = 0;
            if (Q.IsReady()) damage += DamageLib.getDmg(unit, DamageLib.SpellType.Q);
            if (W.IsReady()) damage += DamageLib.getDmg(unit, DamageLib.SpellType.W);
            if (E.IsReady()) damage += DamageLib.getDmg(unit, DamageLib.SpellType.E);
            return damage * (DFG.IsReady() ? 1.2f : 1);

        }
    }
       
}
    

