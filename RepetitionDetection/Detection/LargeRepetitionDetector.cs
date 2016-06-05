﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RepetitionDetection.Catching;
using RepetitionDetection.Commons;

namespace RepetitionDetection.Detection
{
    public class LargeRepetitionDetector : Detector
    {
        public LargeRepetitionDetector([NotNull] StringBuilder text, RationalNumber e, bool detectEqual) : base(text, e, detectEqual)
        {
            catchers = new Dictionary<CatcherInterval, Catcher>();
        }

        public override bool TryDetect(out Repetition repetition)
        {
            repetition = new Repetition(0, 0);

            UpdateCatchers(reverse:false);
            DeleteCatchers();

            var result = false;
            foreach (var pair in catchers)
            {
                if (pair.Value.IsActive())
                {
                    Repetition rep;
                    if (pair.Value.TryCatch(out rep))
                    {
                        result = true;
                        repetition = rep;
                    }
                }
            }
            return result;
        }

        private void UpdateCatchers(bool reverse)
        {
            var n = Text.Length;
            for (var deg2 = 1; deg2*S <= n && n%deg2 == 0; deg2 *= 2)
            {
                UpdateCatcher(new CatcherInterval(n - 1 - deg2 * S, n - 1 - deg2 * (S - 1)), reverse);
                if (n%(deg2*2) == 0)
                {
                    var l = n - 1 - deg2*2*S;
                    var m = n - 1 - deg2*2*S + deg2;
                    var r = n - 1 - deg2*2*(S - 1);
                    UpdateCatcher(new CatcherInterval(l, m), !reverse);
                    UpdateCatcher(new CatcherInterval(m, r), !reverse);
                }
            }
        }

        private void UpdateCatcher(CatcherInterval interval, bool reverse)
        {
            if (!reverse)
            {
                CreateCatcher(interval);
            }
            else
            {
                RemoveCatcher(interval);
            }
        }

        public override void BackTrack()
        {
            foreach (var catcher in catchers.Values)
            {
                if (catcher.IsActive())
                    catcher.Backtrack();
            }

            UpdateCatchers(reverse:true);
            DeleteCatchers();
        }

        public override void Reset()
        {
            catchers.Clear();
            Text.Clear();
        }

        private void DeleteCatchers()
        {
            var catchersToDelete = catchers
                .Where(pair => pair.Value.ShouldBeDeleted())
                .Select(pair => pair.Key)
                .ToArray();

            foreach (var catcher in catchersToDelete)
            {
                catchers.Remove(catcher);
            }
        }

        private void RemoveCatcher(CatcherInterval interval)
        {
            if (interval.L < -1)
                return;
            Catcher catcher;
            if (!catchers.TryGetValue(interval, out catcher))
            {
                throw new InvalidProgramStateException(string.Format("Can't find catcher for interval: {0}", interval));
            }
            catcher.RemoveTime = Text.Length;
        }

        private void CreateCatcher(CatcherInterval interval)
        {
            if (interval.L < -1)
                return;
            var n = interval.L + 1 + S * interval.Length;
            Catcher catcher;
            var i = interval.R;
            var j = Math.Max(i, n - 1 - (new RationalNumber(S) / E * interval.Length).Ceil());
            if (!catchers.TryGetValue(interval, out catcher))
            {
                catcher = new Catcher(Text, i, j, E, DetectEqual, interval.Length);
                catcher.WarmUp(j + 2, Text.Length);
                catchers[interval] = catcher;
            }
            catcher.RemoveTime = -1;
        }

        private readonly Dictionary<CatcherInterval, Catcher> catchers;
    }
}
