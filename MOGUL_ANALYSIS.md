# MOGUL.EXE Reverse-Engineering Summary

Analysis of `MOGUL.EXE` found in this directory (part of the *Big Blue Disk #5* DOS shareware disk collection).

## What the program is

**"Movie Mogul"** — a movie-producer business simulation game.

- **Publisher / copyright:** Chiang Brothers Software, 1985
- **Original source:** written in Microsoft BASIC — the compiled binary embeds the original source filename `MOGUL.BAS`
- **Compiler / runtime:** compiled with the Microsoft BASIC Compiler (BASCOM), linked against the `BRUN20.EXE` runtime (Microsoft BASIC Compiler runtime, version 2.0) rather than compiled fully standalone. `BRUN20.EXE` **is present** in this directory, so the dependency is satisfied.
- **Distribution note (from the program's own help text):** *"Big Blue Disk is not public domain. Please respect the rights of the author."*

Gameplay, per the in-program instructions text: you're handed three movie scripts, pick one to produce, cast 3 of 12 available actors/actresses (balancing talent, popularity, and salary demands), and the game determines box-office results, awards, and a high-score outcome.

## File format

- Classic 16-bit DOS **MZ executable** (segmented real-mode format — not a Windows PE file)
- 512-byte header (32 paragraphs), 35,760-byte load module, 36,272 bytes total
- 7 relocation entries — consistent with a small number of far calls/segment references
- Entry point: `CS:IP = 081D:0000`

## Directory contents

| File | Role |
|---|---|
| `MOGUL.EXE` | The game itself |
| `BRUN20.EXE` | Required BASIC runtime the game links against |
| `ACTOR.DAT`, `ACTRESS.DAT` | Cast data (names, talent/salary stats) |
| `MOVIES.DAT`, `MOVIESST.DAT` | Script/movie data |
| `HMMHS.DAT`, `BAHMMHS.DAT` | High-score files (current and "backup/original" — see `RESET MOGUL SCORES` utility text below) |
| `STATUS.DAT` | Game state file |
| `PERIOD1-5.DAT`, `PERIODM.DAT`, `PERIODIC.EXE` | **Unrelated program** bundled on the same disk (not part of Movie Mogul) |
| `PLSINTRO.EXE`, `PREVIEW.EXE`, `RETURN.EXE`, `SURVEY1.EXE`, `CP.EXE`/`CP!.EXE` | Standard *Big Blue Disk* front-end/menu utilities shared across the whole disk issue (intro screen, program preview, "return to menu", reader survey) — not specific to Movie Mogul |

## Code structure (from disassembly)

Full linear disassembly (2,118 decoded instructions) saved separately; the reliable, hand-verified portion covers the program's startup path:

1. **`0x81D0`** — entry point. Immediately jumps over an embedded data table to `0x8264`.
2. **`0x81E1`–`0x8263`** — BASCOM/BRUN20 loader stub: relocates the program's segments in memory via block copies (`rep movsw`), based on a small type-tagged table (tags `1`/`2`/`9`) describing which segments need adjusting. This is boilerplate present in every BASCOM-compiled `.EXE`, not game-specific code.
3. **`0x8264`–`~0x8380`** — runtime initialization:
   - `INT 21h, AH=30h` — gets the DOS version (matches the embedded string `"Wrong version of runtime module"` — the game refuses to run under unsupported DOS versions)
   - Repeated calls to an internal allocator routine (`call 0x88dc` with varying `AX`/`CX`/`DX`) to carve out separate memory regions for BASIC's string space, file buffers, and heap
   - Copies the command-line/program-name data into a runtime area
4. **`~0x8380` onward** — the actual compiled BASIC program logic begins, interleaved with far calls out to `BRUN20.EXE` for `PRINT`, `INPUT`, file I/O, etc. This region mixes real code with large embedded data blocks (help text, the movie/actor tables' in-memory format, screen layout data), which is typical of BASCOM output.

**Note on "full decompilation":** recovering literal BASIC source isn't realistic here — there's no debug/symbol information, and BASCOM's compiled output doesn't map cleanly back to BASIC syntax. A pure linear-sweep disassembly (decoding every byte in file order) also desynchronizes once it runs into the large embedded data/string regions later in the file, since those bytes aren't code — so instruction-level detail is only trustworthy near the verified entry path above, not as an aggregate whole-file statistic.

## Recursive-descent disassembly (follow-up)

Ran a proper recursive-descent disassembly (Capstone, x86-16), starting at the entry point and following only real control-flow edges (direct `jmp`/`call`/`jcc` targets), instead of a naive linear sweep. Result:

- **825 instructions decoded, covering 1,977 of 35,760 load-module bytes (5.5%)**, address range `0x81d0`–`0x89c1`
- Every reachable path terminates cleanly in a `ret` or in the shared fatal-error trampoline at `0x89af` (prints an error string via `INT 21h AH=09h`, then exits via `INT 21h AH=4Ch`) — **no indirect `jmp`/`call` (register or memory operand) and no far calls exist anywhere in the reachable code.** The graph is fully closed; this isn't a case of the algorithm running out of leads.
- This confirms the region really is just BASCOM/BRUN20 boilerplate: segment relocation, DOS-version check, memory allocation, and a handful of low-level file I/O helpers (open/read/seek/close at `0x88dc`, `0x8913`, `0x8924`, `0x8936`, `0x8949`, `0x8956`) used to open the `.DAT` files and validate a loaded overlay.

**Key finding — why the game logic itself is unreachable this way:** at `0x8380`, the program does `INT 21h AH=3Fh` to read **exactly 6,224 bytes (`0xc28` words = `0x1850`)** into a fixed buffer, then at `0x83e9` immediately checks a field of that just-loaded block against the literal `0x1850` — i.e. it's validating a header/size field of a data overlay it just loaded, not decoding more x86 instructions. This is the load-and-validate step for BASIC **p-code**: `BRUN20.EXE` is Microsoft's p-code *interpreter* runtime (that's what "BRUN" — BASIC RUN-time — means for BASCOM), and MOGUL.EXE was compiled in p-code mode rather than native-code mode. Concretely, this means:

- The actual Movie Mogul game logic (script/casting/scoring code) is **not x86 machine code** anywhere in this file — it's a separate, proprietary Microsoft BASIC p-code bytecode stream, stored as data and executed by an interpreter loop inside `BRUN20.EXE`.
- No amount of x86 disassembly (linear-sweep or recursive-descent) can recover it, because it was never x86 to begin with. Recovering it would require a dedicated BASCOM p-code disassembler/decompiler (a niche, largely lost tool from the era) that understands Microsoft's p-code opcode table — not a generic x86 tool like Capstone/IDA/Ghidra.
- This is also *why* the recursive-descent graph is fully closed with no indirect jumps: the x86 code's only job is DOS-level bootstrapping (open files, load the p-code blob, sanity-check its size, then hand off into `BRUN20.EXE`'s interpreter) — it was never going to contain the game itself.

**Bottom line:** the earlier "linear sweep desyncs, recursive-descent is the next step" hypothesis has now been tested and answered — recursive-descent doesn't desync, it terminates naturally and completely, because there's genuinely no more reachable x86 code to find. Further "decompilation" of the actual gameplay logic is not an x86 reverse-engineering problem; it would require p-code-specific tooling. The extracted strings (`MOGUL_text.txt`) remain the most complete picture available of the program's actual content/behavior.

## Strings extracted (highlights)

Full list of 353 printable strings pulled from the binary is far more reliable than disassembly for understanding program behavior, since string scanning isn't affected by code/data misalignment. Highlights:

- **Title screen:** `M O V I E    M O G U L`, `Copyright 1985`, `Chiang Brothers Software`
- **Instructions text:** full in-game help (script selection, casting mechanics, Oscar/box-office scoring)
- **High score system:** `HIGH SCORES`, `PRESS ANY KEY TO CONTINUE`, `RESET MOGUL SCORES`, plus a note that resetting *"THE OLD SCORES ARE STILL INTACT"* — implying `BAHMMHS.DAT` is a backup copy of `HMMHS.DAT`
- **Data files referenced directly in code:** `movies.dat`, `moviesst.dat`, `ACTOR.DAT`, `ACTRESS.DAT`, `hmmhs.dat`, `BAHMMHS.dat`
- **Runtime/error strings:** `Wrong version of runtime module`, `Error in EXE file`, `$BRUN20.EXE`, `USERLIB.EXE`

---
*Generated via header parsing, `capstone`-based x86-16 disassembly, and ASCII string extraction of `MOGUL.EXE`.*
