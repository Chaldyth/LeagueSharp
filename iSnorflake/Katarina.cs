using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Katarina
{
    internal class Program
    {
        public static string ChampionName = "Katarina";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //Menu
        public static Menu Config;
        private static Obj_AI_Hero Player;

       private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

      private static void Game_OnGameLoad(EventArgs args)
       {
           
           Player = ObjectManager.Player;
           if (Player.BaseSkinName != ChampionName) return;
           Q = new Spell(SpellSlot.Q, 675);
           W = new Spell(SpellSlot.W, 375);
           E = new Spell(SpellSlot.E, 700);
           R = new Spell(SpellSlot.R, 550);
           Game.PrintChat(ChampionName + " Loaded! By iSnorflake");
           SpellList.Add(Q);
           SpellList.Add(W);
           SpellList.Add(E);
           SpellList.Add(R);
           //Create the menu
           Config = new Menu(ChampionName, ChampionName, true);

           //Orbwalker submenu
           Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

           //Add the targer selector to the menu.
           var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
           SimpleTs.AddToMenu(targetSelectorMenu);
           Config.AddSubMenu(targetSelectorMenu);

           Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

           //Combo menu
           Config.AddSubMenu(new Menu("Combo", "Combo"));
           Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
           Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
           Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
           Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
           Config.SubMenu("Combo")
               .AddItem(
               new MenuItem("ComboActive", "Combo!").SetValue(
                    new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

           // Misc
           Config.AddSubMenu(new Menu("Misc", "Misc"));
           Config.SubMenu("Misc").AddItem(new MenuItem("KillstealQ", "Killsteal with Q").SetValue(true));

           // Drawings
           Config.AddSubMenu(new Menu("Drawings", "Drawings"));
           Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
           Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.Red))));

           //Add the events we are going to use
           Game.OnGameUpdate += Game_OnGameUpdate;
           Drawing.OnDraw += Drawing_OnDraw;
        

          
       }
        private static void Combo()
       {
           Orbwalker.SetAttacks(true);
           var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
           var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
           var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
           var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
           bool useQ = Config.Item("UseQCombo").GetValue<bool>();
           bool useW = Config.Item("UseWCombo").GetValue<bool>();
           bool useE = Config.Item("UseECombo").GetValue<bool>();
           bool useR = Config.Item("UseRCombo").GetValue<bool>();

           if (qTarget != null && useQ && Q.IsReady())
           {
               Q.Cast(qTarget);
           }
           if (wTarget != null && useW && W.IsReady())
           {
               W.Cast(wTarget);
           }
           if (eTarget != null && useE && E.IsReady())
           {
               E.Cast(eTarget);
           }
           if (rTarget != null && useR && R.IsReady())
           {
               R.Cast(rTarget);
           }
       }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            Orbwalker.SetAttacks(true);
            Orbwalker.SetMovement(true);
            var useQKS = Config.Item("KillstealQ").GetValue<bool>() && Q.IsReady();
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (useQKS)
                Killsteal();
        }
       private static void Drawing_OnDraw(EventArgs args)
       {
           foreach (var spell in SpellList)
           {
               var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
               if (menuItem.Active)
                   Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
           }
       }
       private static void Killsteal()
       {
           foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(Q.Range)))
           {
               if(Q.IsReady() && hero.Distance(ObjectManager.Player) <= Q.Range && DamageLib.getDmg(hero, DamageLib.SpellType.Q) >= hero.Health)
               Q.Cast();
           }
       }
    
    }
}
