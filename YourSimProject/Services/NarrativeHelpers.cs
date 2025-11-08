using System;

namespace YourSimProject.Services
{
    public static class NarrativeHelpers
    {
        private static readonly string[] SoloHr = new[]
        {
            "Solo shot to left! Turns on it and watches it sail into the seats.",
            "Towering drive to deep center — gone! Solo home run.",
            "Laser over the right-field wall — a no-doubter solo blast.",
            "High and deep to left-center… see ya! Solo homer.",
            "Opposite-field carry to right — solo home run just clears the wall.",
            "Crushed to dead center — solo bomb into the batter’s eye.",
            "Pulled down the line — fair by a foot! Solo home run.",
            "Skied to left, wind helps… it’s out! Solo homer.",
            "Hooking drive to right, stays fair — solo shot!",
            "Barreled to the gap and over the wall — solo home run."
        };

        private static readonly string[] TwoRunHr = new[]
        {
            "Hammered to left — a two-run shot!",
            "Line drive clears the wall in right-center — two-run homer!",
            "Lifted to deep center and gone — two-run blast!",
            "Crushed to the alley — two-run homer!",
            "No-doubter to left — two-run shot!",
            "Pulled to right and out — two-run blast!",
            "Deep to center — two-run home run!",
            "High fly to left sneaks out — two-run homer!",
            "Rifled to right-center — two-run shot!",
            "Wall-scraper down the line — two-run homer!"
        };

        private static readonly string[] ThreeRunHr = new[]
        {
            "Absolutely crushed to left — three-run bomb!",
            "Towering fly sneaks over in right — three-run homer!",
            "No-doubter to dead center — three-run shot!",
            "Cranked to left-center — three-run homer!",
            "Line drive to right is gone — three-run blast!",
            "Up and out to left — three-run shot!",
            "Deep to right-center — three-run homer!",
            "Hooked inside the pole — three-run blast!",
            "Screamer to left is out — three-run shot!",
            "Moonshot to center — three-run homer!"
        };

        private static readonly string[] GrandSlam = new[]
        {
            "Grand slam! He unloads to left-center and clears the bases!",
            "Cranked to right, into the seats — grand slam!",
            "Deep to center and gone — GRAND SLAM!",
            "Over the wall in a hurry — grand slam!",
            "Towering blast to left — grand slam!",
            "Pulled to right and out — grand slam!",
            "Wall-scraper to center — grand slam!",
            "Opposite field and gone — grand slam!",
            "No-doubter into the night — grand slam!",
            "Hammered to dead center — grand slam!"
        };

        private static readonly string[] Walks = new[]
        {
            "Four-pitch walk — never gave him a chance.",
            "Misses inside — ball four, take your base.",
            "Low and away, doesn’t nibble — draws a walk.",
            "Close, but just off the corner — ball four.",
            "Battles back from 0–2 to work a walk.",
            "Checks swing, appeal — he held! Ball four.",
            "Slider misses down — another free pass.",
            "Fastball rides high — ball four.",
            "Paint attempt misses — he walks.",
            "Great plate discipline — earns a walk."
        };

        private static readonly string[] Hbps = new[]
        {
            "Hit on the elbow guard — takes first base.",
            "Back foot gets clipped by the slider — awarded first.",
            "Fastball tails in and grazes the jersey — HBP.",
            "Curve doesn’t break, plunks him — he’ll take first.",
            "Inside cutter catches the hip — batter trots to first.",
            "Spiked breaking ball bounces up and hits him — first base.",
            "Two-seamer runs in — nicks the hand, time is called; he takes first.",
            "Changeup yanks inside — grazes him; HBP.",
            "Up and in — gets him on the shoulder; he’s fine and heads to first.",
            "Inside pitch catches a piece — hit by pitch."
        };

        private static readonly Random _rnd = new Random();
        private static int D10() => _rnd.Next(0, 10);

        public static string GetHomeRunNarrative(int runnersOn)
        {
            int i = D10();
            return runnersOn switch
            {
                0 => SoloHr[i],
                1 => TwoRunHr[i],
                2 => ThreeRunHr[i],
                _ => GrandSlam[i]
            };
        }

        public static string GetWalkNarrative()
        {
            return Walks[D10()];
        }

        public static string GetHbpNarrative()
        {
            return Hbps[D10()];
        }

        // ==============================
        // Doubles: Narrative helpers
        // ==============================
        public static string GetDoubleNoRunnersNarrative(int roll, bool noDoubles) => roll switch
        {
            1 => noDoubles ? "LC gap cut off deep — long single." : "LC gap one-hops wall — stand-up double.",
            2 => noDoubles ? "RF line guarded — single only." : "Liner down RF line rattles corner — sliding double.",
            3 => noDoubles ? "LF back on track; carom held — single." : "Towering off LF wall; perfect carom, barely beats throw for two.",
            4 => noDoubles ? "CF deep; quick cut off — single." : "Shot over CF to track; cruises into second.",
            5 => noDoubles ? "LF plays safe, cuts drive — single." : "Slicing down LF line; dive miss, rolls to wall — two-bagger.",
            6 => noDoubles ? "LF hugging line; quick retrieval — long single." : "Inside 3B bag skipping corner — standing double.",
            7 => noDoubles ? "Gap shaded; RF/CF cut off — single." : "Rocket to RC gap; hustle slide just ahead of tag for double.",
            8 => noDoubles ? "RF back on track; clean field — single." : "High fly deep RF bounces fair; pulls in for two.",
            9 => noDoubles ? "Deep positioning keeps in front; misplay only single." : "Misplayed off glove in LF — hustles into second.",
            10 => noDoubles ? "RF hugging line; cut off — single." : "Bouncer over 1B bag; rolls down line for easy double.",
            _ => "Opposite-field drive; takes two."
        };

        public static string GetDoubleR1Narrative(int roll, bool safe, bool noDoubles)
        {
            if (noDoubles)
            {
                return roll switch
                {
                    1 => "Gap guarded deep; batter held to single; runner goes first to third.",
                    2 => "RF on line; cut off to single; runner advances to third.",
                    3 => "CF shaded deep; held to single; runner to third.",
                    4 => "LF positioned; dive avoided — single; runner to third.",
                    5 => "RC gap chased; deep alignment cuts angle — single; runner to third.",
                    6 => "Inside 1B bag but RF guarding line — single; runner to third.",
                    7 => "Wall carom in LF tracked early — single; runner to third.",
                    8 => "RF cuts line drive before corner — single; runner advances only to 2nd.",
                    9 => "CF deep on track; one-hopper limited to single; runner to third.",
                    10 => "LC gap cut off quickly by deep LF — single; runner advances only to 2nd.",
                    _ => "Prevent-double alignment neutralizes extra base; single advancement."
                };
            }
            return roll switch
            {
                1 => safe ? "LC gap; hold sign — runner to 3rd only; batter doubles." : "LC gap off wall; runner from 1st scores; stand-up double.",
                2 => safe ? "RF line rattles; hold at 3rd; batter doubles." : "RF corner rattles; runner from 1st scores; sliding double.",
                3 => safe ? "Drive over CF; no chance to hold runner — he scores; batter doubles." : "Rocket over CF; runner from 1st scores standing; easy double.",
                4 => safe ? "LF line slicing; conservative stop at 3rd; batter doubles." : "LF line slicing into corner; runner from 1st scores; batter doubles.",
                5 => safe ? "RC gap cut off; runner holds at 3rd; batter doubles." : "RC gap chased; runner from 1st scores on send; batter doubles.",
                6 => safe ? "Inside 1B bag to corner; cannot hold runner — he scores; batter doubles." : "Inside 1B bag rolling corner; runner from 1st scores easily; double.",
                7 => safe ? "High carom LF wall; strong arm prompts hold at 3rd; double." : "High carom LF wall; runner from 1st scores; close play safe.",
                8 => safe ? "RF line drive cut off; runner still reaches 3rd; batter doubles." : "RF line drive; runner from 1st first-to-third; batter into second.",
                9 => safe ? "CF wall one-hopper; coach holds runner at 3rd; double." : "CF wall one-hopper; runner scores; solid double.",
                10 => safe ? "LC gap cut off; runner advances to 3rd; batter doubles." : "LC gap; runner to 3rd, batter safe at second.",
                _ => "Deep drive; runner scores; double."
            };
        }

        public static string GetDoubleR2Narrative(int roll, bool safe, bool noDoubles) => noDoubles ? roll switch
        {
            1 => "LC gap cut off deep — single; runner to 3rd.",
            2 => "RF line guarded; single; runner to 3rd.",
            3 => "CF deep; wall drive held to single; runner to 3rd.",
            4 => "RC gap shaded; single; runner to 3rd.",
            5 => "LF line near; single; runner to 3rd.",
            6 => "LC liner cut off; single; runner to 3rd.",
            7 => "RF track start; single; runner to 3rd.",
            8 => "3B line guarded; single; runner to 3rd.",
            9 => "Shallow RC liner; single; runner holds at 3rd.",
            10 => "CF deep; over head cut off; single; runner to 3rd.",
            _ => "Prevent-double positioning yields single; runner to 3rd."
        } : roll switch
        {
            1 => "LC gap one-hop wall; runner from 2nd scores; stand-up double.",
            2 => "RF corner rocket; runner from 2nd scores standing; sliding double.",
            3 => safe ? "CF wall big bounce; runner holds at 3rd; double." : "CF wall big bounce; runner from 2nd scores; double.",
            4 => safe ? "RC gap cut off; runner holds at 3rd; double." : "RC gap chased; runner from 2nd scores on send; double.",
            5 => "LF line chase corner; runner from 2nd scores; double.",
            6 => safe ? "LC liner cut off quickly; runner holds at 3rd; double." : "LC liner cut off; runner scores on send; double.",
            7 => "RF wall one-hopper; runner from 2nd scores standing; double.",
            8 => "Inside 3B bag rolling corner; runner from 2nd scores; clean double.",
            9 => "Shallow RC drop; runner from 2nd holds at 3rd; batter doubles.",
            10 => "Straight over CF's head; runner from 2nd scores; double.",
            _ => "Deep drive; runner from 2nd scores; double."
        };

        public static string GetDoubleR3Narrative(int roll, bool safe, bool noDoubles, string pitcherOption) => noDoubles ? "Prevent-double deep fielding — single; runner scores." : roll switch
        {
            1 => "LC gap; runner from 3rd scores easily; batter doubles.",
            2 => safe ? "RF line rattles; batter stops at 2nd; run scores." : "RF line rattles; runner scores standing; double.",
            3 => pitcherOption == "Outfield In" ? "Shallow LF flare; OF in — runner holds at 3rd." : "Shallow LF flare; runner reads then scores.",
            4 => safe ? "CF wall drive; batter holds at 2nd; run scores." : "CF wall drive; runner scores; batter doubles.",
            5 => safe ? "3B line corner; batter holds; run scores." : "3B line into corner; runner scores easily; double.",
            6 => "Past 1B into RF corner; run scores; double.",
            7 => "RC gap slicing; run scores; batter doubles.",
            8 => pitcherOption == "Outfield In" ? "Line up middle; shallow CF throw — runner barely safe." : "Line up middle; run scores; batter slides into 2nd.",
            9 => safe ? "LF wall bounce; batter holds; run scores." : "LF wall bounce; runner scores easily; double.",
            10 => "3B-SS hole to LF; run scores; double chance taken successfully.",
            _ => "Deep drive; run scores; double."
        };

        public static string GetDoubleR1R2Narrative(int roll, bool safe, bool noDoubles) => noDoubles ? roll switch
        {
            1 => "LC gap cut off; single loads bases.",
            2 => "RF line guarded; single; R2 to 3rd, R1 to 2nd.",
            3 => "CF wall drive held; single; both advance one.",
            4 => "RC gap shaded; single; runners advance one.",
            5 => "3B line near; single; runners advance one.",
            6 => "Shallow RC flare; single; bases loaded.",
            _ => "Prevent-double alignment converts to single; runners advance one.",
        } : roll switch
        {
            1 => safe ? "LC gap off wall; lead scores? hold sign keeps at 3rd; double." : "LC gap wall; lead scores, trail to 3rd; double.",
            2 => safe ? "RF corner rocket; lead holds at 3rd; double." : "RF corner rocket; both runners score; double.",
            3 => safe ? "CF wall one-hopper; lead holds at 3rd; double." : "CF wall one-hopper; two runs score; double.",
            4 => "RC gap slicing; lead scores, trail to 3rd; double.",
            5 => safe ? "Inside 3B corner; lead scores; trail holds at 3rd; double." : "Inside 3B corner; both score; double.",
            6 => safe ? "Shallow RC flare; hold at 3rd; double loads bases." : "Shallow RC flare; lead scores, trail to 3rd; double.",
            _ => "Deep drive; aggressive send scores lead; double."
        };

        public static string GetDoubleR1R3Narrative(int roll, bool safe, bool noDoubles, string pitcherOption) => noDoubles ? roll switch
        {
            1 => "LC gap cut off; single; R3 scores; R1 to 3rd.",
            2 => "RF corner guarded; single; R3 scores; R1 holds at 2nd.",
            3 => "Shallow LF; OF in keeps to single; bases now loaded as R3 holds.",
            4 => "Up the middle cut off; single; R3 scores; R1 to 3rd.",
            5 => "RC gap shaded; single; R3 scores; R1 to 3rd.",
            6 => "Inside 1B but line guarded; single; R3 scores; R1 to 3rd.",
            7 => "High off LF wall cut quickly; single; R3 scores; R1 holds at 2nd.",
            8 => "RF liner cut off; single loads bases as R3 holds.",
            9 => "CF deep cut; single; R3 scores; R1 to 3rd.",
            10 => "LC gap kept in front; single; R3 scores; R1 to 3rd.",
            _ => "Prevent-double positioning forces single; R3 scores."
        } : roll switch
        {
            1 => "LC gap to wall; R3 scores; R1 to 3rd; double.",
            2 => safe ? "RF corner rattles; hold R1 at 3rd; run in; double." : "RF corner rattles; R3 scores; R1 scores on send; double.",
            3 => pitcherOption == "Outfield In" ? "Shallow LF toward line; OF in — R3 holds at 3rd; double puts R1 at 3rd." : "Shallow LF; late read — R3 scores; R1 to 3rd; double.",
            4 => "Up the middle to wall; R3 scores; R1 to 3rd; double.",
            5 => safe ? "RC gap cut; run scores; R1 holds at 3rd; double." : "RC gap splits; two runs score; double.",
            6 => "Inside 1B bag down line; R3 scores; R1 to 3rd; double.",
            7 => safe ? "High carom LF wall; hold trail at 3rd; one in; double." : "High carom LF wall; both runners score; double.",
            8 => "RF liner to corner; R3 scores; R1 to 3rd; double.",
            9 => "CF one-hopper; R3 scores standing; R1 to 3rd; double.",
            10 => safe ? "LC gap cut off; run scores; R1 to 3rd; double." : "LC gap; send R1 — close at plate, safe; two score; double.",
            _ => "Deep drive; R3 scores; R1 to 3rd; double."
        };

        public static string GetDoubleR2R3Narrative(int roll, bool safe, bool noDoubles, string pitcherOption) => noDoubles ? roll switch
        {
            1 => "LC gap cut to single; one run scores; R2 to 3rd.",
            2 => "RF corner guarded; single; one run; R2 to 3rd.",
            3 => "CF deep but cut; single; both hold except run from 3rd scores.",
            4 => "RC gap shaded; single; R3 scores; R2 to 3rd.",
            5 => "LF line near; single; R3 scores; R2 to 3rd.",
            6 => "LC liner cut; single; R3 scores; R2 to 3rd.",
            7 => "RF track start; single; R3 scores; R2 holds at 3rd.",
            8 => "3B line guarded; single; bases remain loaded as R3 holds.",
            9 => "Shallow RC; OF in — single; R3 holds; R2 to 3rd.",
            10 => "CF deep one-hopper cut; single; R3 scores; R2 to 3rd.",
            _ => "Prevent-double positioning converts to single; R3 scores; R2 to 3rd."
        } : roll switch
        {
            1 => "LC gap off wall; two runs score; double.",
            2 => safe ? "RF corner rocket; one in, R2 holds at 3rd; double." : "RF corner rocket; both runners score; double.",
            3 => pitcherOption == "Outfield In" ? "Shallow LF; OF in — R3 holds; double moves R2 to 3rd." : "Shallow LF drop; R3 scores; R2 to 3rd; double.",
            4 => "RC gap chased; two score; double.",
            5 => "LF line into corner; two score; double.",
            6 => safe ? "LC liner cut quickly; only R3 scores; R2 to 3rd; double." : "LC liner past cut; two score; double.",
            7 => "RF wall one-hopper; both score; double.",
            8 => "Inside 3B bag rolling; both score; double.",
            9 => safe ? "Shallow RC; hold R3; only one run; double puts R2 at 3rd." : "Shallow RC; R3 scores on send; R2 to 3rd; double.",
            10 => "Over CF's head; both runs score; double.",
            _ => "Deep drive; both runners score; double."
        };

        public static string GetDoubleBasesLoadedNarrative(int roll, bool safe, bool noDoubles, string pitcherOption) => noDoubles ? roll switch
        {
            1 => "LC gap cut; single; one run; bases remain loaded.",
            2 => "RF line guarded; single; one run; bases remain loaded.",
            3 => "CF deep but kept in front; single; one run; bases remain loaded.",
            4 => "RC gap shaded; single; two runs? hold sign — only one scores.",
            5 => "LF corner checked; single; two runs score; R1 to 3rd.",
            6 => "LC liner cut; single; one in; force holds to bases loaded.",
            7 => "RF wall cut; single; one in; bases stay loaded.",
            8 => "Inside 3B bag kept in front; single; two score; R1 to 3rd.",
            9 => "Shallow RC; OF in — single; run may hold; default one in.",
            10 => "Over CF cut quickly; single; one in; bases stay loaded.",
            _ => "Prevent-double alignment yields single; one run."
        } : roll switch
        {
            1 => safe ? "LC gap wall; two score? hold trail — only one scores; double." : "LC gap wall; two score; double.",
            2 => safe ? "RF corner rocket; hold trail — only one scores; double." : "RF corner rocket; bases clear two score; double.",
            3 => pitcherOption == "Outfield In" ? "Shallow LF; OF in — only one run; double." : "Shallow LF drop; two score; double.",
            4 => "RC gap splits; two score; double.",
            5 => "Inside 3B bag to corner; two score; double.",
            6 => safe ? "LC liner cut; only one scores; double keeps others advancing one." : "LC liner past cut; two score; double.",
            7 => "RF wall one-hopper; two score; double.",
            8 => "Down 3B line rolling; two score; double.",
            9 => safe ? "Shallow RC; hold runner at 3rd; only one in; double." : "Shallow RC; send — two score; double.",
            10 => "Over CF's head; two score; double.",
            _ => "Deep drive; two score; double."
        };

        // ==============================
        // Triples: Narrative helpers
        // ==============================
        public static string GetTripleNoRunnersNarrative(int roll) => roll switch
        {
            1 => "Line to RC rolls to wall — stand-up triple.",
            2 => "Shot in LC gap; caroms oddly — diving into third.",
            3 => "Over CF's head to track; slides in safely at third.",
            4 => "Down RF line rattling corner — triple.",
            5 => "CF misplays hop; batter motors to third.",
            6 => "Opposite-field drive to wall; into third with a triple.",
            7 => "Hard bounce past LF; all the way to third.",
            8 => "Deep to CF; throws come late — triple.",
            9 => "RC alley rocket; turns on jets to third.",
            10 => "Down LF line; perfect read — slides into third.",
            _ => "Crushed to the gap for a triple."
        };

        public static string GetTripleR1Narrative(int roll) => roll switch
        {
            1 => "RC gap to wall; R1 scores easily; batter to third.",
            2 => "LC gap rattles; aggressive send — R1 scores; batter slides into third.",
            3 => "Over CF; R1 scores standing; triple.",
            4 => "RF line; relay late — R1 scores; triple.",
            5 => "Misplayed hop in LF; R1 scores; triple.",
            6 => "CF track; long chase — R1 around to score; triple.",
            7 => "RC alley; R1 scores; triple.",
            8 => "Deep CF; both advance — R1 scores; triple.",
            9 => "Down LF line; send R1 — he scores; triple.",
            10 => "Gapper; R1 never stops — scores; triple.",
            _ => "Deep drive; runner scores; triple."
        };

        public static string GetTripleR2Narrative(int roll) => roll switch
        {
            1 => "Rocket to deep left-center, one-hops the wall, runner from 2nd rounds 3rd and scores standing.",
            2 => "High drive over CF’s head, ball bounces to the wall — runner from 2nd scores, batter into 3rd without a throw.",
            3 => "Liner inside the 1B line, rolls into the corner — runner from 2nd scores easily, batter dives into 3rd safely.",
            4 => "Sharp gapper to right-center, ball ricochets off wall — runner from 2nd crosses the plate, batter pulls into 3rd with a triple.",
            5 => "Deep fly to left-center, drops between outfielders — runner from 2nd scores, batter slides in safely at 3rd.",
            6 => "Grounder past diving 3B, hugging the line — LF chases it to the corner, runner scores, batter into 3rd.",
            7 => "Line shot over shortstop’s head, lands near wall — runner from 2nd scores easily, batter coasts into 3rd.",
            8 => "Fly ball to deep right, kicks off wall padding — runner from 2nd scores, batter with an RBI triple.",
            9 => "Towering fly to dead center, hits warning track and rolls away — runner from 2nd scores, batter stands up at 3rd.",
            10 => "Gapper to left-center; perfect angle — runner from 2nd scores; batter glides into 3rd.",
            _ => "Driven deep; runner from 2nd scores; triple."
        };

        public static string GetTripleR3Narrative(int roll) => roll switch
        {
            1 => "Line drive into the left-center gap, runner from 3rd jogs home easily, batter slides into 3rd with a triple.",
            2 => "Rocket off the wall in right, big carom toward center — runner from 3rd scores standing, batter coasts into 3rd.",
            3 => "High fly over CF’s head, one-hops the fence — runner from 3rd scores, batter slides safely into 3rd.",
            4 => "Liner inside 1B line, rolls to the corner — runner from 3rd crosses the plate easily, batter into 3rd.",
            5 => "Deep drive to center, ball bounces off the wall — run scores, batter pulls into 3rd with a triple.",
            6 => "Gapper to right-center, perfect placement — runner from 3rd scores, batter beats relay to 3rd.",
            7 => "Line shot down the LF line, LF dives and misses — runner scores easily, batter stands up at 3rd.",
            8 => "Fly ball off top of wall in left, bounces high, runner scores easily, batter slides into 3rd.",
            9 => "Hard liner over RF’s head, ball ricochets back toward center — run in, batter to 3rd.",
            10 => "Long drive to left-center, both outfielders give chase — runner from 3rd scores easily, batter arrives at 3rd without a play.",
            _ => "Deep drive; runner scores; triple."
        };

        public static string GetTripleR1R2Narrative(int roll) => roll switch
        {
            1 => "Crushed line drive into RC gap; both runners score — batter slides into 3rd.",
            2 => "Towering fly off LF wall; both score — batter coasts into 3rd.",
            3 => "Gap shot deep CF splits outfielders; both score easily — batter pulls into 3rd.",
            4 => "Line drive down RF line; ricochets; both runners score — headfirst into 3rd.",
            5 => "Rocket to LC one-hops wall; both score comfortably — batter glides into 3rd.",
            6 => "High drive over CF; both score — batter reaches 3rd no throw.",
            7 => "Screaming liner past diving RF; both score — batter dives into 3rd.",
            8 => "Line shot between LF/CF; both cross home — RBI triple.",
            9 => "Deep fly to RC off wall; both score — batter slides safely.",
            10 => "Gapper to LC; outfielders collide; both score — batter cruises into 3rd.",
            _ => "Gapper; both runners score; triple."
        };

        public static string GetTripleR1R3Narrative(int roll) => roll switch
        {
            1 => "Crushed RC gap; both runners score; batter slides into 3rd.",
            2 => "Wall in LF rattler; R3 & R1 score; batter into 3rd.",
            3 => "Over CF’s head; both score; easy triple.",
            4 => "Inside 1B line to corner; both score standing; triple.",
            5 => "Deep drive CF; both runners score; batter pulls into 3rd.",
            6 => "Gapper RC perfect angle; both score; relay late.",
            7 => "Line LF line past dive; both score; batter stands at 3rd.",
            8 => "Off top of LF wall; both score; batter slides into 3rd.",
            9 => "Hard liner over RF; both score; batter to 3rd.",
            10 => "Long drive LC; both score easily; batter arrives at 3rd.",
            _ => "Deep gapper; both runners score; triple."
        };

        public static string GetTripleR2R3Narrative(int roll) => roll switch
        {
            1 => "Line drive RC gap; both runners score; batter slides into 3rd.",
            2 => "Towering LF wall carom; both score; batter standing at 3rd.",
            3 => "Gap shot deep CF; both score easily; triple.",
            4 => "RF line ricochet corner; both score; headfirst into 3rd.",
            5 => "Rocket LC one-hop wall; both score; batter glides into 3rd.",
            6 => "High drive CF warning track; both score; no throw to 3rd.",
            7 => "Screaming liner past RF; both score; batter dives into 3rd.",
            8 => "Line between LF/CF; both cross plate; RBI triple.",
            9 => "Deep fly RC off wall away; both score; batter slides safely.",
            10 => "Gapper LC; outfielders collide; both score; batter cruises into 3rd.",
            _ => "Gapper; both runners score; triple."
        };

        public static string GetTripleBasesLoadedNarrative(int roll) => roll switch
        {
            1 => "Crushed RC gap; all baserunners score — batter slides safely into 3rd.",
            2 => "Towering fly off LF wall; all score — batter coasts into 3rd.",
            3 => "Gap shot deep CF splits fielders; all runners score; triple.",
            4 => "RF line ricochet; all cross home; batter headfirst into 3rd.",
            5 => "Rocket LC one-hop wall; all score comfortably; batter standing at 3rd.",
            6 => "High drive CF warning track; all score — no throw.",
            7 => "Screaming liner past RF; all score easily; batter dives into 3rd.",
            8 => "Line between LF/CF; all runners cross; RBI triple.",
            9 => "Deep fly RC off wall; all score; batter slides safely.",
            10 => "Gapper LC; outfielders collide; all score; batter cruises into 3rd.",
            _ => "Gapper clears bases; triple."
        };

        // ==============================
        // Singles: Narrative helpers
        // ==============================
        public static string GetSoloSingleNarrative()
        {
            // Use the same pool as DescribeSoloSingle()
            string[] pool = new[]
            {
                "Grounder through short/third; LF keeps to single.",
                "Shot past diving 2B into RF for a single.",
                "Line over SS into shallow left.",
                "Grounder up middle past pitcher; CF charges.",
                "Chopper over 3B; held to single down line.",
                "Soft liner to center in front of CF.",
                "Opposite-field flare lands fair near line.",
                "Dribbler off end of bat; throw late to first.",
                "Stung back through box nearly hits pitcher.",
                "Sharp grounder under 1B's glove into RF.",
            };
            return pool[D10()];
        }

        public static string GetSingleR1Narrative(int roll) => roll switch
        {
            1 => "Through right side; R1 aggressive to 3rd.",
            2 => "Lined to LF; R1 stops at 2nd.",
            3 => "Up the middle, off pitcher's glove; R1 to 2nd.",
            4 => "Opposite field to RF; R1 takes 3rd, cutoff missed.",
            5 => "L-CF cut off; R1 holds at 2nd.",
            6 => "Slow roller between 3B/SS; station-to-station — R1 to 2nd.",
            7 => "Past diving SS; R1 hesitates, then takes 3rd.",
            8 => "Just inside 1B into shallow RF; R1 to 3rd.",
            9 => "Flare to R-CF; both move up one base.",
            10 => "Sharp to RF, overplayed; R1 goes first-to-third.",
            _ => "Base hit; runners advance appropriately."
        };

        public static string GetSingleR2Narrative(int roll, bool safe, bool infieldIn, bool outfieldIn)
        {
            return roll switch
            {
                1 => (safe || infieldIn || outfieldIn) ? "Lined to CF; conservative hold at 3rd." : "Lined to CF; R2 scores easily.",
                2 => "Through left side; R2 to 3rd safely.",
                3 => safe ? "Shallow left; late break — hold at 3rd." : "Shallow left; aggressive send — R2 scores.",
                4 => (safe || outfieldIn) ? "Sharp to RF; hold at 3rd." : "Sharp to RF; close play — safe at plate.",
                5 => safe ? "Up the middle; safe call holds at 3rd." : "Up the middle; R2 scores easily.",
                6 => safe ? "Shallow CF charging — hold at 3rd." : "Shallow CF; R2 attempts and scores.",
                7 => safe ? "Down 3B line; conservative — hold at 3rd." : "Down 3B line; R2 scores.",
                8 => "Bouncer over mound; R2 to 3rd safely.",
                9 => (safe || outfieldIn) ? "L-CF gap; hold at 3rd." : "L-CF gap; R2 scores standing.",
                10 => safe ? "Through right side; safe call holds." : "Through right side; throw offline — R2 scores.",
                _ => "Base hit; runner advances per situation."
            };
        }

        public static string GetSingleR3Narrative(int roll, bool outfieldIn)
        {
            return roll switch
            {
                1 => "Through left; R3 scores easily.",
                2 => "Past 2B into RF; R3 scores standing.",
                3 => outfieldIn ? "Shallow LF; OF in — R3 holds." : "Shallow LF; close play — safe at plate.",
                4 => "Up the middle; R3 scores routinely.",
                5 => outfieldIn ? "Flare in front of LF; OF in — R3 holds." : "Flare over SS; R3 scores.",
                6 => "Inside 3B toward corner; R3 scores.",
                7 => "Slow roller to SS; throw to 1st as R3 scores.",
                8 => "Line up the middle; R3 scores standing.",
                9 => outfieldIn ? "Bouncer into shallow RF; OF in — R3 holds." : "Bouncer into shallow RF; close play — safe at plate.",
                10 => "Opposite-field to RF; R3 scores easily.",
                _ => "Single; runner from 3rd likely scores."
            };
        }

        public static string GetSingleR1R2Narrative(int roll, bool safe, bool infieldIn, bool outfieldIn)
        {
            return roll switch
            {
                1 => (safe || infieldIn || outfieldIn) ? "To LF; conservative — bases loaded." : "To LF; R2 scores, R1 to 2nd.",
                2 => (safe || infieldIn) ? "Up the middle; R2 holds at 3rd." : "Up the middle; R2 scores, R1 to 2nd.",
                3 => (safe || outfieldIn) ? "Sharp to RF; both advance one base." : "Sharp to RF; R2 to 3rd, R1 holds at 2nd.",
                4 => safe ? "Down 3B line; R2 holds at 3rd." : "Down 3B line; R2 scores, R1 to 3rd.",
                5 => (safe || outfieldIn) ? "Blooper CF; hold at 3rd." : "Blooper CF; aggressive send — R2 scores.",
                6 => (safe || infieldIn || outfieldIn) ? "Through right; R2 stops at 3rd." : "Through right; throw offline — R2 scores.",
                7 => (safe || outfieldIn) ? "Flare to LF; R2 holds at 2nd." : "Flare to LF; R2 to 3rd.",
                8 => safe ? "Inside 3B; hold — bases remain loaded scenario pending." : "Inside 3B; R2 to 3rd.",
                9 => (safe || outfieldIn) ? "RC gap; hold at 3rd." : "RC gap; R2 scores, R1 to 3rd.",
                10 => "Chopper high bounce; all safe — bases loaded.",
                _ => "Single; runners advance accordingly."
            };
        }

        public static string GetSingleR1R3Narrative(int roll, bool safe, bool infieldIn, bool dpDepth, bool outfieldIn)
        {
            return roll switch
            {
                1 => (infieldIn || dpDepth) ? "Left side knocked down; R3 holds, R1 to 2nd." : "Through left; R3 scores, R1 to 2nd.",
                2 => outfieldIn ? "Lined to CF shallow; R3 holds, R1 to 2nd." : "Lined to CF; R3 scores standing, R1 to 2nd.",
                3 => outfieldIn ? "Sharp to RF shallow; R3 holds, R1 to 2nd." : "Sharp to RF; R3 scores, R1 to 2nd.",
                4 => (infieldIn || dpDepth) ? "2B slows ball; R3 holds, R1 to 2nd." : "Between 1B/2B into RF; R3 scores.",
                5 => outfieldIn ? "Blooper to LF, OF in; R3 holds, bases loaded." : "Shallow LF; R3 safe at plate, bases 1st/2nd.",
                6 => (infieldIn || dpDepth) ? "Up middle deflected; R3 holds, R1 to 2nd." : "Up the middle; run scores, R1 to 2nd.",
                7 => outfieldIn ? "Flare in front of LF; OF in — R3 holds." : "Flare to LF; R3 scores; R1 holds at 2nd.",
                8 => safe ? "RC line drive cut off; conservative — R1 only to 2nd, R3 scores." : "RC line drive; R3 scores, R1 to 3rd.",
                9 => (infieldIn || dpDepth) ? "Chopper; 2B throws home — R3 out." : "Chopper past mound; no play at home — run scores.",
                10 => outfieldIn ? "Down 3B line; LF tight — R3 holds." : "Down 3B line; R3 scores, R1 to 2nd.",
                _ => "Single; R3 likely scores; trail runner advances."
            };
        }

        public static string GetSingleR2R3Narrative(int roll, bool safe, bool infieldIn, bool dpDepth, bool outfieldIn)
        {
            return roll switch
            {
                1 => outfieldIn ? "Line to LF; OF in — R3 holds; bases loaded." : "Line to LF; R3 scores, R2 holds at 3rd.",
                2 => (infieldIn || dpDepth) ? "Up the middle vs drawn in; SS knocks it down — only R3 advances." : "Up the middle through; both runners score.",
                3 => outfieldIn ? "Sharp to RF shallow; R3 holds — bases loaded." : "Sharp to RF; R3 scores, R2 to 3rd.",
                4 => outfieldIn ? "Flare over SS; LF fields quickly — runners hold." : "Flare over SS; both advance one safely.",
                5 => (infieldIn || dpDepth) ? "Inside 3B deflected; only R3 scores, R2 to 3rd." : "Inside 3B to corner; both runners score easily.",
                6 => outfieldIn ? "Shallow RF charging; R3 holds, R2 moves up one." : "Shallow RF; close play — R3 safe at plate, R2 to 3rd.",
                7 => (infieldIn || dpDepth) ? "Left side bouncer; SS blocks — R3 holds, bases loaded." : "Through left; R3 scores, R2 to 3rd.",
                8 => (infieldIn || dpDepth) ? "Deflected off pitcher; quick home throw — R3 out." : "Deflected off pitcher; both move up one.",
                9 => outfieldIn ? "RC gap cutoff shallow; R3 holds." : "RC gap cut off; both runners score.",
                10 => outfieldIn ? "Flare beyond 2B; RF charges — runners hold." : "Flare beyond 2B; both advance one base safely.",
                _ => "Single; lead likely scores; trail advances per depth."
            };
        }

        public static string GetSingleBasesLoadedNarrative(int roll, bool safe, bool infieldIn, bool dpDepth, bool outfieldIn)
        {
            return roll switch
            {
                1 => outfieldIn ? "Line to LF; OF in — possible hold, run may not score; default hold." : "Line to LF; one run, others up one.",
                2 => (infieldIn || dpDepth) ? "Through 1B-2B knocked down; only force moves, run uncertain — default bases advance one, no run." : "Through right side; one run, others advance one.",
                3 => outfieldIn ? "Up the middle; CF shallow — runner at 3rd holds; others freeze (bases stay loaded)." : "Up the middle; run scores; others move up one.",
                4 => outfieldIn ? "Flare to shallow LF; OF in — runner holds; bases stay loaded." : "Flare over SS; run scores; others up one.",
                5 => "Inside 3B rolling to corner; two runs score, R1 to 3rd.",
                6 => (infieldIn || dpDepth) ? "Slow roller; SS throws home — force out, bases still loaded." : "Slow roller; SS goes to 2nd for one, run scores.",
                7 => "High chopper; all advance one — run in, bases remain loaded.",
                8 => outfieldIn ? "Sharp to RF shallow; play at plate — default hold or out; using hold to avoid double count." : "Sharp to RF; run scores; others up one.",
                9 => "RC gap cut off; two runs score, R1 to 3rd.",
                10 => (infieldIn || dpDepth) ? "Bouncer left side tipped; only R3 scores, others advance one." : "Bouncer left side; one run in, others up one.",
                _ => "Single; at least one run scores."
            };
        }

        // ==============================
        // Groundout Narratives (by base state)
        // ==============================
        public static string GetGroundoutBasesEmptyNarrative(int roll) => roll switch
        {
            1 => "Ground ball to shortstop, routine play — throw to first in time for the out.",
            2 => "Bouncer to second base, easy pickup and throw across — one away.",
            3 => "Slow roller toward third, 3B charges and throws on the run — just gets the batter.",
            4 => "Chopper up the middle, second baseman ranges over and fires to first — out by a step.",
            5 => "Hard one-hopper to first base, cleanly fielded and taken to the bag unassisted.",
            6 => "Grounder to the pitcher, calmly fields and tosses to first for the out.",
            7 => "Two-hopper to shortstop, strong throw across — out number one.",
            8 => "Bouncer toward second, routine play — batter thrown out easily.",
            9 => "Slow roller toward first, pitcher covers the bag for the out.",
            10 => "Hard shot off the mound, deflects to shortstop — recovers and throws in time.",
            _ => "Routine grounder; out recorded at first."
        };

        public static string GetGroundoutR1Narrative(int roll, bool infieldIn, bool dpDepth, bool safe)
        {
            // Return only descriptive text; advancement handled in resolver
            return roll switch
            {
                1 => infieldIn ? "Ground ball to shortstop, infield in; SS goes to first for the out — runner to 2nd." : safe ? "Ground ball to short; runner beats the flip to 2nd — batter out at 1st." : dpDepth ? "6–4–3 double play — inning over." : "6–4–3 double play — two-for-one.",
                2 => infieldIn ? "Chopper to second, infield in; 2B takes out at first; runner advances to 2nd." : safe ? "Chopper to second; runner slides safely into 2nd — batter out at 1st." : dpDepth ? "Chopper to second — smooth 4–6–3 double play." : "Chopper to second — double play executed smoothly.",
                3 => infieldIn ? "Grounder up the middle, drawn in; only play at 1st — runner to 2nd." : "Grounder up the middle; only one — batter out, runner to 2nd.",
                4 => "Slow roller toward 3rd, throw across — batter out, runner advances.",
                5 => "Hard grounder to 1st; steps on bag, throw to 2nd late — runner safe.",
                6 => infieldIn ? "Bouncer to short, infield in; routine 6–3, runner advances." : "Bouncer to short; relay wide — no double play, runner to 2nd.",
                7 => infieldIn ? "Grounder to second, infield in — only play to 1st, runner advances." : safe ? "Grounder to second — runner safe at 2nd; batter out at 1st." : "Grounder to second — 4–6–3 double play.",
                8 => infieldIn ? "Sharp grounder to third, infield in; single out at 1st — runner to 2nd." : "Sharp grounder to third, start DP — relay late, runner moves to 2nd.",
                9 => "Bouncer back to pitcher, looks to 2nd then goes to 1st — runner advances safely.",
                10 => safe ? "Hard grounder up the middle; Safe call — take the sure out at 1st, runner to 2nd." : "Hard grounder up the middle; only play at 1st — runner advances to 2nd.",
                _ => "Routine grounder; runner advances per situation."
            };
        }

        // Additional groundout narrative helpers could be added similarly for other base states.
        public static string GetGroundoutR2Narrative(int roll, bool infieldIn, bool safe)
        {
            return roll switch
            {
                1 => infieldIn ? "Grounder to short, drawn in — throw to first; runner holds at 2nd." : "Grounder to short — routine out; runner holds at 2nd.",
                2 => infieldIn ? "Chopper to second, looks runner — still goes to 1st; runner takes 3rd late." : "Chopper to second — runner advances to 3rd.",
                3 => "Slow roller to third — 3B checks runner, throws to 1st; runner holds.",
                4 => "Grounder up the middle — out at first; runner moves to 3rd.",
                5 => infieldIn ? "Hard grounder to first — steps on bag; runner stays at 2nd." : "Hard grounder to first — unassisted; runner advances to 3rd.",
                6 => infieldIn ? "Bouncer off mound — pitcher checks runner, throws to 1st; runner retreats." : "Bouncer off mound — pitcher to first; runner advances to 3rd.",
                7 => infieldIn ? "Grounder toward second — looks runner back; out at first; runner holds." : "Grounder toward second — routine out; runner to 3rd.",
                8 => infieldIn ? "Sharp grounder to third — holds runner; throw across for out." : "Sharp grounder to third — out; runner holds at 2nd.",
                9 => infieldIn ? "Grounder through right side cut off — runner advances to 3rd safely." : "Grounder through right side — toss to first; runner takes 3rd.",
                10 => infieldIn ? "Soft roller to short charging — throw to first; runner holds at 2nd." : "Soft roller to short — out at first; runner advances.",
                _ => "Routine grounder; runner decisions per depth."
            };
        }

        public static string GetGroundoutR3Narrative(int roll, bool infieldIn, bool safe)
        {
            return roll switch
            {
                1 => infieldIn ? "Grounder to short — throw home; runner out." : "Grounder to short — out at first; run scores.",
                2 => infieldIn ? "Chopper to third — fires home; runner out." : "Chopper to third — looks then goes to first; run scores.",
                3 => infieldIn ? "Grounder to second — throw home; runner tagged out." : "Grounder to second — throw to first; run scores.",
                4 => infieldIn ? "Soft roller to pitcher — flips home; runner out." : "Soft roller to pitcher — only play at first; run in.",
                5 => infieldIn ? "Hard grounder to first — comes home; bang-bang, runner out." : "Hard grounder to first — unassisted; runner scores.",
                6 => infieldIn ? "Bobbled at short — throw home late; runner scores." : "Bouncer toward short bobbled — only play at first; run scores.",
                7 => infieldIn ? "Slow roller to second — quick throw home; runner out." : "Slow roller to second — flips to first; run scores.",
                8 => infieldIn ? "Grounder to third — fired home; runner out." : "Grounder to third — out at first; run scores.",
                9 => infieldIn ? "Chopper over pitcher — shortstop tries home; too late, run scores." : "Chopper over pitcher — to first; run scores.",
                10 => infieldIn ? "Hard shot back to mound — pitcher throws home; possible tag out (treated out)." : "Hard shot off mound — to first; runner scores.",
                _ => "Grounder; play dictates run attempt outcome."
            };
        }

        public static string GetGroundoutR1R2Narrative(int roll, bool infieldIn, bool safe)
        {
            return roll switch
            {
                1 => infieldIn ? "Grounder to short — force at 3rd on lead runner." : "Grounder to short — 6–4–3 double play; R2 to 3rd.",
                2 => infieldIn ? "Chopper to second — throw to 3rd gets lead." : "Chopper to second — relay late; both advance.",
                3 => infieldIn ? "Slow roller to third — holds lead runner; out at first." : "Slow roller to third — both runners advance on routine out.",
                4 => infieldIn ? "Grounder up middle — attempt at 3rd just late; runners safe." : "Grounder up middle — steps on 2nd, throw to 1st; double play; R2 to 3rd.",
                5 => infieldIn ? "Bouncer to first — throw to 3rd forces lead runner." : "Bouncer to first — tries 3–6–3; runners end at corners.",
                6 => infieldIn ? "Hard grounder to third — goes to 3rd for force; others safe." : "Hard grounder to third — 5–4–3 double play.",
                7 => infieldIn ? "Grounder to second — only safe play to first; runners hold." : "Grounder to second — relay wide; no DP; runners advance.",
                8 => infieldIn ? "Sharp to short — fires to 3rd for force." : "Sharp to short — flips to 2nd, strong to 1st; inning-ending DP.",
                9 => infieldIn ? "Bouncer to pitcher — goes to 3rd; lead safe on close play." : "Bouncer to pitcher — checks lead, to first; both advance.",
                10 => infieldIn ? "Through right side — 2B dives, throw to 3rd; all safe." : "Through right side — out at first; both advance.",
                _ => "Grounder with multiple runners; advances vary by depth."
            };
        }

        public static string GetGroundoutR1R3Narrative(int roll, bool infieldIn, bool safe)
        {
            return roll switch
            {
                1 => infieldIn ? "Grounder to short — throw home; runner out." : "Grounder to short — 6–4–3 double play; run scores.",
                2 => infieldIn ? "Slow roller to second — throw home; close out." : "Slow roller to second — only play at first; run scores.",
                3 => infieldIn ? "Chopper to third — fires home; runner out." : "Chopper to third — checks runner then to first; run scores.",
                4 => infieldIn ? "Hard grounder to second — throw home; runner out." : "Hard grounder to second — quick double play; run scores.",
                5 => infieldIn ? "Grounder back to pitcher — throws home; out at plate." : "Grounder back to pitcher — to first; run scores easily.",
                6 => infieldIn ? "Between 3B/SS — SS throws home; late, run scores." : "Between 3B/SS, deep — only play at first; run scores.",
                7 => infieldIn ? "Bouncer toward short — looks home, runner retreats." : "Bouncer toward short — feed to 2nd wide; one out, run scores.",
                8 => infieldIn ? "Grounder to first — throw home; runner out." : "Grounder to first — to 2nd return late; run scores.",
                9 => "Sharp grounder to third — throw home; runner out at plate.",
                10 => infieldIn ? "Chopper off mound — SS goes home; close play, runner out." : "Chopper off mound — SS to first; run scores.",
                _ => "Grounder; depth determines plate attempt outcome."
            };
        }

        public static string GetGroundoutR2R3Narrative(int roll, bool infieldIn, bool safe)
        {
            return roll switch
            {
                1 => infieldIn ? "Grounder to short — throw home; runner out; trail holds." : "Grounder to short — to first; run scores; trail to 3rd.",
                2 => infieldIn ? "Chopper to second — throw home; close play out." : "Chopper to second — to first; run scores, trail up one.",
                3 => infieldIn ? "Slow roller to third — fires home; runner out." : "Slow roller to third — throw across; run scores.",
                4 => infieldIn ? "Grounder up middle — SS throws home; late; run scores." : "Grounder up middle — to first; run scores.",
                5 => infieldIn ? "Hard one-hopper to first — throw home; possible out (treated out)." : "Hard one-hopper to first — unassisted; both advance, run scores.",
                6 => infieldIn ? "Grounder to short — throw home; tag play; may be out (treated out)." : "Grounder to short — only play at first; run scores.",
                7 => infieldIn ? "Bouncer to second — comes home; runner out." : "Bouncer to second — routine to first; run scores.",
                8 => infieldIn ? "Grounder to third — goes home; runner out." : "Grounder to third — across to first; run scores.",
                9 => infieldIn ? "Slow roller up line — pitcher checks then late home; run safe." : "Slow roller up line — flip to first; run scores; trail advances.",
                10 => infieldIn ? "Through right side — 2B from knees throws home; run scores." : "Through right side past dive — both advance safely, run scores.",
                _ => "Grounder with two in scoring position; drawn in alters result."
            };
        }

        public static string GetGroundoutBasesLoadedNarrative(int roll, bool infieldIn, bool safe)
        {
            return roll switch
            {
                1 => infieldIn ? "Grounder to short — force at home; relay not in time." : "Grounder to short — home for one, relay to first; double play; run prevented.",
                2 => infieldIn ? "Chopper to third — force at home; others advance." : "Chopper to third — steps on bag, fires to first; double play; run scores.",
                3 => infieldIn ? "Grounder to second — force at plate; no other play." : "Grounder to second — over to short and back to first; double play; run scores.",
                4 => infieldIn ? "Bouncer to first — throw home; out at plate." : "Bouncer to first — steps on bag, throw home late; one run scores.",
                5 => infieldIn ? "Hard up middle — SS throws home; out at plate." : "Hard up middle — relay wide; only one out, run scores.",
                6 => infieldIn ? "Grounder to third — force at plate; relay late; bases stay loaded." : "Grounder to third — force at plate; relay late; bases loaded.",
                7 => infieldIn ? "Soft roller to short — throw home; runner safe bang-bang." : "Soft roller to short — to second for one; relay late; one run scores.",
                8 => infieldIn ? "Back to pitcher — home for force, perfect relay to first; double play." : "Back to pitcher — throws home for force, relay to first; double play; run erased.",
                9 => infieldIn ? "Bouncer to second — throw home; out at plate." : "Bouncer to second — looks home then first; out; run scores.",
                10 => infieldIn ? "Through left side — SS dives; can’t get it — run scores." : "Through left side past diving SS — all advance safely, one run scores.",
                _ => "Bases loaded grounder; alignment dictates plate play outcome."
            };
        }

        // ==============================
        // Flyout Narratives (bases empty only for first extraction)
        // ==============================
        public static string GetFlyoutBasesEmptyNarrative(int roll) => roll switch
        {
            1 => "High fly ball to left field, routine play — caught for the out.",
            2 => "Lazy fly to center, CF settles under it and makes the catch.",
            3 => "Fly ball to right, RF drifts toward the line and squeezes it.",
            4 => "Towering fly in shallow left, shortstop drifts back and makes the catch on the grass.",
            5 => "Fly ball to straightaway center, high but playable — caught for out number one.",
            6 => "Soft fly toward right-center, RF calls off CF and makes the grab.",
            7 => "Lofted fly down the left-field line, LF runs it down near the chalk for the catch.",
            8 => "Routine fly to medium right, easy play for the outfielder.",
            9 => "High popup near second base, 2B takes charge and records the out.",
            10 => "Fly ball to deep left-center, CF tracks it down on the warning track for the out.",
            _ => "Routine fly, caught for the out."
        };

        // ==============================
        // Lineout Narratives (bases empty only)
        // ==============================
        public static string GetLineoutBasesEmptyNarrative(int roll) => roll switch
        {
            1 => "Line drive to shortstop, snagged on a reflex — one away.",
            2 => "Hard liner to center, CF barely moves and gloves it.",
            3 => "Scorcher to third, 3B snares it — batter out.",
            4 => "Line drive to right, RF squeezes it chest-high.",
            5 => "Rocket up the middle; pitcher ducks, SS catches behind him.",
            6 => "Liner to left, LF catches chest-high without moving.",
            7 => "Screaming liner toward first, 1B snares it — out.",
            8 => "Bullet to second base, quick reaction catch by 2B.",
            9 => "Line to deep left-center, CF runs it down on the move.",
            10 => "Laser to right-center, RF makes a running catch at the last moment.",
            _ => "Liner caught; batter is out."
        };

        // ==============================
        // Pop-out Narratives (bases empty only)
        // ==============================
        public static string GetPopoutBasesEmptyNarrative(int roll) => roll switch
        {
            1 => "High pop near first base — 1B squeezes it.",
            2 => "Skied on the infield toward short — routine catch.",
            3 => "Towering pop behind the plate — catcher settles under it.",
            4 => "Mile-high pop over second — 2B gloves it.",
            5 => "Pop toward third baseline — 3B drifts and makes the play.",
            6 => "Looper straight up — pitcher defers, shortstop takes it.",
            7 => "Shallow pop to left side — SS waves everyone off.",
            8 => "High spinner near mound — pitcher takes charge for the out.",
            9 => "Sky-high pop to third — easy out.",
            10 => "Infield pop — routine out recorded.",
            _ => "Infield pop — routine out."
        };
    }
}
