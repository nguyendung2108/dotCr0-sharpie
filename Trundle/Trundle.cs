﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;

namespace Trundle
{
    internal class Trundle
    {
        public const string CharName = "Trundle";

        public static Obj_AI_Hero Target;

        public Trundle()
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            try
            {
                Game.PrintChat("<font color=\"#00BFFF\">Trundle# -</font> Loaded!");
                TMenu.AddMenu();

                T.SetSkillShots();
                T.GetSmiteSlot();

                Drawing.OnDraw += OnDraw;
                Game.OnGameUpdate += OnGameUpdate;
                Orbwalking.AfterAttack += AfterAttack;
                Interrupter.OnPossibleToInterrupt += OnPossibleInterrupt;

                AntiGapcloser.OnEnemyGapcloser += OnGapCloser;
            }

            catch
            {
                Game.PrintChat("Oops. Something went wrong with Trundle");
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || MenuGUI.IsChatOpen || ObjectManager.Player.IsChannelingImportantSpell() ||
                ObjectManager.Player.IsRecalling())
            {
                return;
            }

            if (T.Orbwalker.ActiveMode.ToString() == "Combo")
            {
                Target = TargetSelector.GetTarget(T.E.Range, TargetSelector.DamageType.Physical);
                if (Target.IsValidTarget())
                {
                    T.Combo(Target);
                }
            }
            
            if (T.Orbwalker.ActiveMode.ToString() == "Mixed")
            {
                Target = TargetSelector.GetTarget(420f, TargetSelector.DamageType.Physical);
                if (Target.IsValidTarget())
                {
                    T.Mixed(Target);
                }
            } 
            
            if (T.Orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                T.LaneClear();
            }

        }

        private static void AfterAttack(AttackableUnit sender, AttackableUnit target)
        {
            if (sender.IsMe)
            {
                switch (T.Orbwalker.ActiveMode.ToString())
                {
                    case "Combo":
                        if (T.Q.IsReady() && ObjectManager.Player.Distance(target) <= Orbwalking.GetRealAutoAttackRange(target))
                        {
                            T.Q.Cast();
                            Orbwalking.ResetAutoAttackTimer();
                        }
                        break;
                    case "Mixed":
                        if (T.Q.IsReady() && ObjectManager.Player.Distance(target) <= Orbwalking.GetRealAutoAttackRange(target) && target.Type == GameObjectType.obj_AI_Hero)
                        {
                            T.Q.Cast();
                            Orbwalking.ResetAutoAttackTimer();
                        }
                        break;
                    case "LaneClear":
                        if (T.Q.IsReady() && ObjectManager.Player.Distance(target) <= Orbwalking.GetRealAutoAttackRange(target) && TMenu.Config.Item("useQLC").GetValue<bool>())
                        {
                            T.Q.Cast();
                            Orbwalking.ResetAutoAttackTimer();
                        }
                        break;


                }
            }
        }

        private static void OnPossibleInterrupt(Obj_AI_Base sender, InterruptableSpell spell)
        {
            if (sender.IsAlly ||
                !TMenu.Config.Item("EInterrupt").GetValue<bool>() ||
                !TMenu.Config.Item("Interr" + spell.Slot).GetValue<bool>())
            {
                return;
            }
            if (T.E.IsReady() && sender.Distance(ObjectManager.Player) < T.E.Range)
            {
                T.E.Cast(
                    ObjectManager.Player.Position.Extend(sender.Position, sender.Distance(ObjectManager.Player) + T.E.Width));
            }
        }

        private static void OnGapCloser(ActiveGapcloser gapcloser)
        {
            if (TMenu.Config.Item("EGap").GetValue<bool>() && 
                TMenu.Config.Item("Gap" + gapcloser.Slot).GetValue<bool>() &&
                gapcloser.Sender.IsValidTarget() &&
                ObjectManager.Player.Distance(gapcloser.Sender.Position) <= T.E.Range)
            {
                if (gapcloser.End.Distance(ObjectManager.Player.Position) > gapcloser.Start.Distance(ObjectManager.Player.Position))
                {
                    T.E.Cast(gapcloser.Start.Extend(gapcloser.End, T.E.Width + gapcloser.Sender.BoundingRadius));
                }
                else
                {
                    T.E.Cast(ObjectManager.Player.Position.Extend(gapcloser.End, T.E.Width/2));
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (TMenu.Config.Item("drawW").GetValue<bool>())
            {
                Render.Circle.DrawCircle(T.Player.Position, T.W.Range, Color.DeepSkyBlue);
            }
            if (TMenu.Config.Item("drawE").GetValue<bool>())
            {
                Render.Circle.DrawCircle(T.Player.Position, T.E.Range, Color.DeepPink);
            }
            if (TMenu.Config.Item("drawR").GetValue<bool>())
            {
                Render.Circle.DrawCircle(T.Player.Position, T.R.Range, Color.GreenYellow);
            }


        }

    }
}