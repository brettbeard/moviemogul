# Movie Mogul — Game Design & Data Provenance

This file is the single source of truth for game-balance numbers. Every
number used by the app is either **SOURCED** (pulled directly from the
original `MOGUL.EXE` data files / extracted strings) or **INVENTED**
(reasonable numbers made up for this recreation because the original
formula lives in unrecoverable BASIC p-code — see `MOGUL_ANALYSIS.md`:
the x86 disassembly bottoms out at a p-code interpreter handoff, so the
actual scoring/decay algorithms were never x86 and can't be decompiled
with normal tools).

## SOURCED data

### Movies (`MOVIES.DAT`)
All 12 titles, both summary lines, and all 3 role names per movie are
verbatim from `MOVIES.DAT` (a CRLF-delimited text file, not binary-packed).
See `Data/SeedData.Movies.cs`.

### Cast roster (`ACTOR.DAT` / `ACTRESS.DAT`)
Binary fixed-length records, 29 bytes each: 21-byte space-padded name +
an 8-digit numeric string. Decoded field layout (reverse-engineered by
inspecting all 76 actor / 64 actress records and checking value ranges
for plausibility):

| digits | field | notes |
|---|---|---|
| `[0]` | sex flag | literally `1` for every ACTOR.DAT record, `9` for every ACTRESS.DAT record — redundant with which file it's in, kept for fidelity |
| `[1]` | age bucket | one of `2/4/6/8` (young / adult / mature / senior) — correlates with real-world 1985 ages of the named performers (e.g. George Burns, Burt Lancaster, Kirk Douglas = `8`; Tom Cruise, Molly Ringwald = `2`), confirming the read |
| `[2:4]` | talent | 2-digit, range 12–99 across the roster |
| `[4:6]` | popularity | 2-digit, range 12–98 across the roster |
| `[6:8]` | salary tens | 2-digit, ×$10,000 → salary demand range $120,000–$950,000, a clean distribution consistent with the manual's "big star, big salary" |

This split isn't from documentation — it's inferred from the data's own
internal consistency (clean, plausible ranges; age bucket matches real
actor ages) — but confidence is high. All 76 actors and 64 actresses are
loaded verbatim (name + these 4 stats) into `Data/SeedData.Cast.cs`; the
game randomly draws 12 of them (mixed) per playthrough, per the manual's
"twelve available actors and actresses."

### Reviewers & verdicts (extracted strings)
Fixed reviewer list: The NY Times, Entertainment Tonight, Gene Siskel,
Roger Ebert, Sneak Previews, Rex Reed, Time Magazine, Newsweek, LA Times.
Verdict tiers: "Loved it!", "Liked it.", "Didn't like it.", "Hated it!".

### Random events (extracted strings)
Flavor-only (no direct $ cost): arrested for cocaine possession (bad
publicity), suing the National Enquirer (good publicity), stuntman killed
on set (bad publicity), dating a famous athlete (good publicity), dating a
famous singer (good publicity), published an autobiography (good
publicity).
Hard-cost events: injured in a car accident → **$200,000** delay cost; a
star hates the director, replacing them costs **$450,000**. Both dollar
figures are verbatim from the strings.

### MPAA ratings pool
PG, PG-13, R (verbatim).

### Budget overrun bands
On budget, +2%, +5%, +10%, +20%, +30% (verbatim band values; **probability
weights for each band are INVENTED**, see below).

### Hard limits
- Box office lifetime floor: **$200,000** (verbatim — a film that only
  hits this is a "bomb").
- Production spend ceiling: **$30,000,000** (verbatim, literal constant
  in the source, not per-movie).

### Per-movie production budget floor (interpreted)
`MOGUL_text.txt` embeds a flat list of 24 numbers, which cleanly splits
into 12 `(A, B)` pairs with `A < B` in every case, one pair per movie in
title order. I could not conclusively determine what `B` represents (the
"$30,000,000" ceiling shown in the input prompt is a separate literal
string, not this data — so `B` is something else, most likely a per-script
box-office scale/prestige factor). I use:
- `A` × 1,000 as each movie's **minimum production spend** (floor for the
  budget-entry step), range $250,000–$5,000,000.
- `B` × 1,000 as each movie's **box-office scale factor** (an internal
  multiplier on how big that script's revenue potential is — SPACE WARS
  ($30M), QUEST FOR HONOR ($27M), and DEMON DUSTERS ($26M) scale highest;
  BONKERS! ($7M) lowest), which is a reasonable and defensible reading but
  flagged here as an interpretation, not a certainty.

### `MOVIESST.DAT` (inconclusive — not used directly)
288 single digits (1–9), which cleanly group into 24-per-movie / 8-per-role
blocks (confirmed structurally: DEMON DUSTERS' three near-identical
"Demon Duster #1/2/3" roles produce three near-identical 8-digit blocks).
This is almost certainly a per-role casting-suitability weight table, but
without the p-code I can't recover which of the 8 digits maps to which
weight. Rather than guess field-by-field, I designed my own transparent
scoring formula (below) using the *confirmed* actor/actress stat fields
instead of this file.

## INVENTED design (documented here, implemented in `Data/GameConstants.cs`)

### Role sex restrictions
Not recoverable from data, so assigned by reading each role name (e.g.
"Princess", "Wife", "Femme Fatale" → female-only; "Rancher", "Husband",
"Villainous Warlord" → male-only; ensemble/ambiguous roles like "Space
Hero", "Detective", "Demon Duster #1/2/3" → either). Every movie has at
least one restricted and one open role, matching the manual's "some parts
are restricted by sex while others aren't. Experiment."

### Quality score
```
BudgetFactor  = clamp((spend - movieFloor) / (30,000,000 - movieFloor), 0, 1)
PopularityFactor = avg(cast popularity) / 100
TalentFactor      = avg(cast talent) / 100
QualityScore  = 100 * (0.45*BudgetFactor + 0.35*PopularityFactor + 0.20*TalentFactor)
                + random noise in [-8, +8]
```
Popularity is weighted higher than talent because the manual stresses
popularity drives box office; talent is weighted more heavily in the
separate Oscar-odds calculation below.

### Budget overrun probability bands
On budget 30%, +2% 25%, +5% 20%, +10% 15%, +20% 7%, +30% 3%.

### Random event odds
9 events total (6 flavor + 2 hard-cost + 1 "dating a singer" variant of
the "dating a celebrity" template), each equally likely (~11.1%) — one
always fires per playthrough, per spec.

### Reviews
Each reviewer's verdict is rolled independently, weighted by QualityScore:
```
p(Loved it!)        = clamp(QualityScore/100 - 0.10, 0.05, 0.6)
p(Liked it.)         = 0.35
p(Didn't like it.)   = 0.35
p(Hated it!)         = remainder
```
(weights re-normalized to sum to 1; higher quality shifts mass toward the
top tier).

### Box office
- Opening week gross scales primarily off **production spend** (not the per-script scale factor
  alone), so a cheap, badly-cast picture can plausibly bomb and an expensive, well-cast one can
  plausibly multiply its budget several times over:
  ```
  qualityMultiplier  = 0.05 + (QualityScore/100) * 2.45        // 0.05x..2.50x of spend
  scaleAdjustment    = 0.75 + 0.25 * (movie.ScaleFactor / 30,000,000)  // ~0.81x..1.00x, mild per-script flavor
  openingWeekGross   = spend * qualityMultiplier * scaleAdjustment * random(0.8, 1.2)
  ```
  A quality-0 picture opens at roughly 5% of its spend back; a quality-100 picture on full budget
  can open at up to ~2.5x spend. `scaleAdjustment` keeps SPACE WARS/QUEST FOR HONOR/DEMON DUSTERS
  scripts a bit stronger than BONKERS! at the same spend and quality, without swamping the budget
  and quality signal the way a raw multi-million-dollar scale factor would.
- Each film is randomly assigned a decay profile at release:
  - **Legs** (30% chance): week-over-week multiplier `random(0.80, 0.93)`.
  - **Spike-and-crash** (70% chance): week-over-week multiplier `random(0.35, 0.60)`.
- Run continues while weekly gross ≥ $50,000 cutoff, hard-capped at 20
  weeks regardless (guarantees termination).
- After the run, if lifetime total < $200,000, it's topped up to exactly
  $200,000 (the "bomb" floor).

### Academy Awards
3 categories (Best Actress, Best Actor, Best Picture), each with an
invented fictional presenter name (Marla Chevalier, Dexter Vance, and
"the previous year's Best Picture winner director," represented as a
generic in-universe stand-in — see `Data/GameConstants.cs`). Win
probability per category:
```
p(win) = clamp(0.15 + 0.5 * TalentFactor + 0.1 * (QualityScore/100), 0.05, 0.65)
```
rolled independently per category. Any win triggers a re-release adding
`0.15 * lifetimeGross * random(0.7, 1.3)` in extra revenue.

### High score qualification
A game qualifies for the (single, mixed) high score list if it's in the
current top 10 by net profit, **or** the bottom 5 by net profit (so both
big wins and spectacular bombs get immortalized, matching the manual's
"high or notably low" framing). Identical title+initials pairs get a
trailing marker (`II`, `III`, ...) appended, per the manual's own
disambiguation note.
