---
name: asset-analyzer
description: Use to scan this Unity project's Assets for oversized files and produce a prioritized optimization report. Finds textures, audio, models, and other assets over a size threshold (default 5MB) so build size and memory can be trimmed. Trigger on "analyze assets", "find large assets", "asset report", "what's bloating the build", or "asset-analyzer".
allowed-tools:
  - Bash
---

# Asset Analyzer (IdleRPG)

Scan `Assets/` for files at or above a size threshold, then report worst offenders with a
concrete optimization suggestion each. Skip Unity's `.meta` files and library caches.

## Run (default 5MB, sorted largest first)

```bash
cd "H:/Game/IdleRPG/NewRPG"
THRESH_MB=${1:-5}
find Assets -type f ! -name '*.meta' -size +$((THRESH_MB*1024))k 2>/dev/null \
  | while read -r f; do printf '%s\t%s\n' "$(du -m "$f" | cut -f1)" "$f"; done \
  | sort -rn | awk -F'\t' '{printf "%4d MB  %s\n", $1, $2}'
```

Change the threshold by passing MB as arg (e.g. `2` for 2MB). To also break down totals by
extension:

```bash
cd "H:/Game/IdleRPG/NewRPG"
find Assets -type f ! -name '*.meta' 2>/dev/null \
  | sed 's/.*\.//' | sort | uniq -c | sort -rn | head -20
```

## Report format

For each file at/over threshold, output: `size — path — suggested fix`. Prioritize by size.
Suggested fixes by type:
- **Textures** (`.png/.psd/.tga/.jpg`): enable Crunch compression, drop Max Size (2048→1024),
  disable Read/Write, mipmaps off for UI. `.psd` in-build is a red flag — flatten/export.
- **Audio** (`.wav`): compress to Vorbis; Streaming for BGM, Compressed-in-memory for SFX.
  Raw `.wav` shipped uncompressed is the most common idle-game bloat.
- **Models** (`.fbx`): strip unused meshes/animations, mesh compression on.
- **Uncompressed source** (`.psd/.tiff/.blend/.aiff`): shouldn't ship — Unity imports these
  raw. Move outside `Assets/` or export a compressed version.

End with the total count over threshold and combined MB reclaimable, so the user knows the
payoff before opening the Editor.
