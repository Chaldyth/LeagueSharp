using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace SFMasterYi
{
    class Program
    {
        public static string ChampName = "MasterYi";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player; // Instead of typing ObjectManager.Player you can just type Player
        public static Spell Q,  E, R;
        public static Items.Item DFG;

        public static Menu SF;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            
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
            //Farm
            SF.AddSubMenu(new Menu("Farm", "Farm"));
            SF.SubMenu("Farm").AddItem(new MenuItem("WaveClear", "WaveClear").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            SF.SubMenu("Farm").AddItem(new MenuItem("useQW", "Use Q?").SetValue(true));
            //Jung clear
            SF.AddSubMenu(new Menu("JFarm", "JFarm"));
            SF.SubMenu("JFarm").AddItem(new MenuItem("JungClear", "Jungle clear").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            //Exploits
            SF.AddItem(new MenuItem("NFE", "No-Face Exploit").SetValue(true));
            //Make the menu visible
            SF.AddToMainMenu();
            Q = new Spell(SpellSlot.Q, 600);
            E = new Spell(SpellSlot.E, Orbwalking.GetRealAutoAttackRange(Player));
            R = new Spell(SpellSlot.R, Orbwalking.GetRealAutoAttackRange(Player));
            Drawing.OnDraw += Drawing_OnDraw; // Add onDraw
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)

            Game.PrintChat("SF" + ChampName + " loaded! By iSnorflake");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (SF.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (SF.Item("WaveClear").GetValue<KeyBind>().Active)
            {
                Waveclear();
            }
            
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            Utility.DrawCircle(Player.Position, Q.Range, Color.Crimson);
        }
        public static void JungleClear()
        { // leeched off flapperdoodle ty bby
            dynamic jungleMobs = MinionManager.GetMinions(Player.ServerPosition, 700, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (jungleMobs.Count > 0)
            {
                if (Q.IsReady())
                {
                    Q.Cast(jungleMobs);
                }
                if (E.IsReady())
                {
                    E.Cast();
                }
            }
        }

        public static void Waveclear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var useQ = SF.Item("UseQF").GetValue<bool>(); // From katarina script

            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion != null)
                    {
                        if (minion.IsValidTarget(Q.Range))
                        {
                            Q.Cast(minion, true); // Packet cast because fk u rito
                        }
                    }
                }
            }
        }
        public static void Combo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            if (target.IsValidTarget(DFG.Range) && DFG.IsReady())
                DFG.Cast(target);
            if (target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                if (SF.Item("NFE").GetValue<bool>())
                {
                    Q.Cast(target, true);
                }
                else
                {
                    Q.Cast(target, false);
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
        }
    }
}
