﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.GameModes.Play.Osu
{
    /// <summary>
    /// Uses the 'x' symbol and has a pop-out effect while rolling over. Used in osu! standard.
    /// </summary>
    public class OsuComboCounter : ComboCounter
    {
        protected uint ScheduledPopOutCurrentId = 0;

        protected virtual float PopOutSmallScale => 1.1f;
        protected virtual bool CanPopOutWhileRolling => false;
        
        public Vector2 InnerCountPosition
        {
            get
            {
                return DisplayedCountSpriteText.Position;
            }
            set
            {
                DisplayedCountSpriteText.Position = value;
            }
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            PopOutSpriteText.Origin = this.Origin;
            PopOutSpriteText.Anchor = this.Anchor;

            Add(PopOutSpriteText);
        }

        protected override string FormatCount(ulong count)
        {
            return $@"{count}x";
        }

        protected virtual void transformPopOut(ulong newValue)
        {
            PopOutSpriteText.Text = FormatCount(newValue);

            PopOutSpriteText.ScaleTo(PopOutScale);
            PopOutSpriteText.FadeTo(PopOutInitialAlpha);
            PopOutSpriteText.MoveTo(Vector2.Zero);

            PopOutSpriteText.ScaleTo(1, PopOutDuration, PopOutEasing);
            PopOutSpriteText.FadeOut(PopOutDuration, PopOutEasing);
            PopOutSpriteText.MoveTo(DisplayedCountSpriteText.Position, PopOutDuration, PopOutEasing);
        }

        protected virtual void transformPopOutRolling(ulong newValue)
        {
            transformPopOut(newValue);
            transformPopOutSmall(newValue);
        }

        protected virtual void transformNoPopOut(ulong newValue)
        {
            DisplayedCountSpriteText.Text = FormatCount(newValue);
            DisplayedCountSpriteText.ScaleTo(1);
        }

        protected virtual void transformPopOutSmall(ulong newValue)
        {
            DisplayedCountSpriteText.Text = FormatCount(newValue);
            DisplayedCountSpriteText.ScaleTo(PopOutSmallScale);
            DisplayedCountSpriteText.ScaleTo(1, PopOutDuration, PopOutEasing);
        }

        protected virtual void scheduledPopOutSmall(uint id)
        {
            // Too late; scheduled task invalidated
            if (id != ScheduledPopOutCurrentId)
                return;

            DisplayedCount++;
        }

        protected override void OnCountRolling(ulong currentValue, ulong newValue)
        {
            ScheduledPopOutCurrentId++;
            base.OnCountRolling(currentValue, newValue);
        }

        protected override void OnCountIncrement(ulong currentValue, ulong newValue)
        {
            while (DisplayedCount != currentValue)
                DisplayedCount++;

            DisplayedCountSpriteText.Show();

            transformPopOut(newValue);

            ScheduledPopOutCurrentId++;
            uint newTaskId = ScheduledPopOutCurrentId;
            Scheduler.AddDelayed(delegate
            {
                scheduledPopOutSmall(newTaskId);
            }, PopOutDuration);
        }

        protected override void OnCountChange(ulong currentValue, ulong newValue)
        {
            ScheduledPopOutCurrentId++;
            base.OnCountChange(currentValue, newValue);
        }

        protected override void OnDisplayedCountRolling(ulong currentValue, ulong newValue)
        {
            if (newValue == 0)
                DisplayedCountSpriteText.FadeOut(PopOutDuration);
            else
                DisplayedCountSpriteText.Show();

            if (CanPopOutWhileRolling)
                transformPopOutRolling(newValue);
            else
                transformNoPopOut(newValue);
        }

        protected override void OnDisplayedCountChange(ulong newValue)
        {
            DisplayedCountSpriteText.FadeTo(newValue == 0 ? 0 : 1);

            transformNoPopOut(newValue);
        }

        protected override void OnDisplayedCountIncrement(ulong newValue)
        {
            DisplayedCountSpriteText.Show();

            transformPopOutSmall(newValue);
        }
    }
}
