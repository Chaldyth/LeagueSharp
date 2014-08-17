#region References
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
#endregion
namespace SFAhri
{
    class Program
    {
        #region Declares
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
        #endregion

        #region OnGameLoad
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
            SF.SubMenu("Combo").AddItem(new MenuItem("useQ", "Use Q?").SetValue(true));
            SF.SubMenu("Combo").AddItem(new MenuItem("useW", "Use W?").SetValue(true));
            SF.SubMenu("Combo").AddItem(new MenuItem("useE", "Use E?").SetValue(true));
            SF.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            //Exploits
            SF.AddItem(new MenuItem("NFE", "No-Face (Normal cast not implemented)").SetValue(true));
            //Make the menu visible
            SF.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw; // Add onDraw
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)

            Game.PrintChat("SFAhri loaded! By iSnorflake");


        }
#endregion

        #region OnGameUpdate
        static void Game_OnGameUpdate(EventArgs args)
        {
            if (SF.Item("ComboActive").GetValue<KeyBind>().Active) {
                Combo();
            }
        }
        #endregion

        #region OnDraw
        static void Drawing_OnDraw(EventArgs args)
        {
            
        }
        #endregion

        #region Combo
        public static void Combo()
        {
           // Game.PrintChat("Got to COMBO function");
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;
            
                if (target.IsValidTarget(DFG.Range) && DFG.IsReady())
                    DFG.Cast(target);
                if (target.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    if (SF.Item("NFE").GetValue<bool>())
                    {
                        Game.PrintChat("Casting Q");
                        //Spell_Cast_LineSkillshot("Combo", "useQ", Q, SimpleTs.DamageType.Magical);
                        Q.Cast(target, true);
                        Game.PrintChat("Q Casted");
                    }

                    else
                    {
                        var pTarget = Prediction.GetBestPosition(target, 0.5f, 100f, 1100f, Player.ServerPosition, 880f, false, Prediction.SkillshotType.SkillshotLine).Position;
                        //Spell_Cast_LineSkillshot("Combo", "useQ", Q, SimpleTs.DamageType.Magical);
                        Q.Cast(target, true);
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
                        //Spell_Cast_LineSkillshot("Combo", "useE", E, SimpleTs.DamageType.Magical,"Enemy",true);
                        E.Cast(target, true);
                    }
                    else
                    {
                        var pTarget = Prediction.GetBestPosition(target, 0.5f, 60f, 1200f, Player.ServerPosition, 880f, true, Prediction.SkillshotType.SkillshotLine).Position;
                        E.Cast(target, true);
                    }
                }
                
            }
        #endregion

        #region GetDamage
        private static double GetDamage(Obj_AI_Base unit) // Credit to TC-Crew and PQMailer for the base of this 
        {
            double damage = 0;
            if (Q.IsReady()) damage += DamageLib.getDmg(unit, DamageLib.SpellType.Q);
            if (W.IsReady()) damage += DamageLib.getDmg(unit, DamageLib.SpellType.W);
            if (E.IsReady()) damage += DamageLib.getDmg(unit, DamageLib.SpellType.E);
            return damage * (DFG.IsReady() ? 1.2f : 1);

        }
        #endregion

        #region Spellcast
        //Credits to Lexxes gave me this function to use
        public static bool Spell_Cast_LineSkillshot(string MainMenu, string Menu, Spell Spell, SimpleTs.DamageType DmgType, string Objekt = "Enemy", bool Condition = true, bool Lasthit = false, DamageLib.StageType Stage = DamageLib.StageType.Default)
        {
            
                if (Objekt == "Enemy")
                {
                    var Target = SimpleTs.GetTarget(Spell.Range, DmgType);
                    if (Target != null)
                    {
                        if (Target.IsValidTarget(Spell.Range) && Spell.IsReady())
                        {
                            if (Spell.GetPrediction(Target).HitChance >= Prediction.HitChance.HighHitchance)
                            {
                                Spell.Cast(Target, true);
                                
                                return true;
                            }
                        }
                    }
                }
                if (Objekt == "Minion")
                {
                    var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Spell.Range, MinionTypes.All, MinionTeam.NotAlly);
                    foreach (var Target in allMinions)
                    {
                        if (Target != null)
                        {
                            var spelltype = DamageLib.SpellType.AD;

                            if (Spell.Slot.ToString() == "Q")
                                spelltype = DamageLib.SpellType.Q;
                            if (Spell.Slot.ToString() == "W")
                                spelltype = DamageLib.SpellType.W;
                            if (Spell.Slot.ToString() == "E")
                                spelltype = DamageLib.SpellType.E;
                            if (Spell.Slot.ToString() == "R")
                                spelltype = DamageLib.SpellType.R;

                            if (Target.IsValidTarget(Spell.Range) && Spell.IsReady())
                            {
                                if ((Lasthit && (DamageLib.getDmg(Target, spelltype, Stage) > Target.Health) || (DamageLib.getDmg(Target, spelltype, Stage) + 100 < Target.Health) && !Lasthit))
                                {
                                    Spell.Cast(Target.Position, true);
                                    return true;
                                }
                            }
                        }
                    }
                }
                if (Objekt == "KS")
                {

                }

            
            return true;
        }
        public static bool Menu_IsMenuActive(string Menu)
  {
   return (SF.Item(Menu).GetValue<bool>());

  }
        #endregion
    }
       
}
    

