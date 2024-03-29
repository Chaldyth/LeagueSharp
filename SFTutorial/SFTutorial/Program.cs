﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace SFTutorial
{
    class Program
    {
        public static string ChampName = "Ezreal";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player; // Instead of typing ObjectManager.Player you can just type Player
        public static Spell Q, W, E, R;
        public static Items.Item DFG;

        public static Menu SF;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 1150);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, float.MaxValue);

            Q.SetSkillshot(0.5f, 80f, 1200f, true, Prediction.SkillshotType.SkillshotLine);-
            W.SetSkillshot(0.5f, 80f, 1200f, false, Prediction.SkillshotType.SkillshotLine);
            //Base menu
            SF = new Menu("SF" + ChampName, ChampName, true);
            //Orbwalker and menu
            SF.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(SF.SubMenu("Orbwalker"));
            //Target selector and menu
            var ts = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(ts);
            SF.AddSubMenu(ts);
            //Combo menu
            SF.AddSubMenu(new Menu("Combo", "Combo"));
            SF.SubMenu("Combo").AddItem(new MenuItem("useQ", "Use Q?").SetValue(true));
            SF.SubMenu("Combo").AddItem(new MenuItem("useW", "Use W?").SetValue(true));
            SF.SubMenu("Combo").AddItem(new MenuItem("useE", "Use E?").SetValue(true));
            SF.SubMenu("Combo").AddItem(new MenuItem("useR", "Use R?").SetValue(true));
            SF.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            //Farming menu
            SF.AddSubMenu(new Menu("Farming", "Farming"));
            SF.SubMenu("Farming").AddItem(new MenuItem("useQF", "Use Q?").SetValue(true));
            SF.SubMenu("Farming").AddItem(new MenuItem("FreezeActive", "Freeze lane").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            //Exploits
            SF.AddItem(new MenuItem("NFE", "No-Face Exploit").SetValue(true));
            //Make the menu visible
            SF.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw; // Add onDraw
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)

            Game.PrintChat("SF"+ChampName+" loaded! By iSnorflake");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (SF.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (SF.Item("FreezeActive").GetValue<KeyBind>().Active)
            {
                Farm();
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            Utility.DrawCircle(Player.Position, Q.Range, Color.Crimson);
            Utility.DrawCircle(Player.Position, W.Range, Color.Ivory);
            Utility.DrawCircle(Player.Position, E.Range, Color.Khaki);
            Utility.DrawCircle(Player.Position, R.Range, Color.HotPink);
        }
        public static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            var useQ = SF.Item("useQF").GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 1400)) < 0.75 * DamageLib.getDmg(minion, DamageLib.SpellType.Q, DamageLib.StageType.FirstDamage))){
                    if (Vector3.Distance(minion.ServerPosition, Player.ServerPosition) > Orbwalking.GetRealAutoAttackRange(Player))
                    {
                        Q.CastIfHitchanceEquals(minion, Prediction.HitChance.HighHitchance, true);
                    }
                }
            }
        }
        public static void Combo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            if (target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                if (SF.Item("NFE").GetValue<bool>())
                {
                    Q.Cast(target, true); // Packetcasting = true
                }
                else
                {
                    Q.Cast(target, false); // Packetcasting = false
                }
            }
            if (target.IsValidTarget(W.Range) && W.IsReady())
            {
                if (SF.Item("NFE").GetValue<bool>())
                {
                    W.Cast(target, true);
                }
                else
                {
                    W.Cast(target, false);
                }
            }
            if (target.IsValidTarget(E.Range) && E.IsReady())
            {
                if (SF.Item("NFE").GetValue<bool>())
                {
                    E.Cast(target, true);
                }
                else
                {
                    E.Cast(target, false);
                }
            }
            if (target.IsValidTarget(R.Range) && R.IsReady())
            {
                if (SF.Item("NFE").GetValue<bool>())
                {
                    R.Cast(target, true);
                }
                else
                {
                    R.Cast(target, false);
                }
            }

            if (target.IsValidTarget(DFG.Range) && DFG.IsReady())
                DFG.Cast(target);
        }
    }
}