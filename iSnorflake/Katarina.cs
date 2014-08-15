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
        public static Items.Item DFG;

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
            DFG = Utility.Map.GetMap() == Utility.Map.MapType.TwistedTreeline || Utility.Map.GetMap() == Utility.Map.MapType.CrystalScar ? new Items.Item(3188, 750) : new Items.Item(3128, 750);
            Game.PrintChat(ChampionName + " Loaded! By iSnorflake V2");
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            //Create the menu
            Config = new Menu(ChampionName, ChampionName, true);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            //Add the targer selector to the menu.
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);



            //Combo menu
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Misc
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealQ", "Killsteal with Q").SetValue(true));

            // Drawings
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
           // Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.AddToMainMenu();
            //Add the events we are going to use
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;



        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            if(isEnemyInRange()) // If an enemy is in range and im ultimating - dont cancel the ult before their dead
                if (Interrupter.IsChannelingImportantSpell(ObjectManager.Player)) return;
            if (!isEnemyInRange() && Interrupter.IsChannelingImportantSpell(ObjectManager.Player)) // If the ult isnt hitting anyone
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, ObjectManager.Player); // Cancels ult
            }
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
        private static void Combo()
        {
            Orbwalker.SetAttacks(true);
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            if ((GetDamage(target) > target.Health))
            {
                if (ObjectManager.Player.Distance(target) < E.Range && DFG.IsReady())
                    DFG.Cast(target);

                if (ObjectManager.Player.Distance(target) < Q.Range && Q.IsReady())
                    Q.CastOnUnit(target, true);

                if (ObjectManager.Player.Distance(target) < E.Range && E.IsReady() && !Q.IsReady())
                    E.CastOnUnit(target, true);

                if (ObjectManager.Player.Distance(target) < W.Range && W.IsReady() && !Q.IsReady())
                    W.Cast();

                if (ObjectManager.Player.Distance(target) < R.Range && R.IsReady() && !Q.IsReady())
                    R.Cast();


            }
            else if (!(GetDamage(target) > target.Health))
            {
                if (ObjectManager.Player.Distance(target) < Q.Range && Q.IsReady())
                    Q.CastOnUnit(target, true);

                if (Config.Item("ComboActive").GetValue<KeyBind>().Active &&
                    ObjectManager.Player.Distance(target) < E.Range && E.IsReady())
                    E.CastOnUnit(target, true);

                if (ObjectManager.Player.Distance(target) < W.Range && W.IsReady())
                    W.Cast();
            }
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
                if (Q.IsReady() && hero.Distance(ObjectManager.Player) <= Q.Range && DamageLib.getDmg(hero, DamageLib.SpellType.Q) >= hero.Health)
                    Q.Cast();
            }
        }
        private static double GetDamage(Obj_AI_Base unit)
        {
            double damage = 0;
            if (Q.IsReady()) damage += DamageLib.getDmg(unit, DamageLib.SpellType.Q);
            if (W.IsReady()) damage += DamageLib.getDmg(unit, DamageLib.SpellType.W);
            if (E.IsReady()) damage += DamageLib.getDmg(unit, DamageLib.SpellType.E);
            if (R.IsReady()) damage += DamageLib.getDmg(unit, DamageLib.SpellType.R, DamageLib.StageType.FirstDamage) * 7;
            return damage * (DFG.IsReady() ? 1.2f : 1);
        }
        private static bool isEnemyInRange() // Checks if an enemy is in range of my ultimate.
        {
            var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            if (target == null) return false;
            return true;
        }
    }
}