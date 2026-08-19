---
name: unity-code-review
description: Use when reviewing Unity C# scripts in this project for bugs, leaks, and convention issues before committing or merging. Tailored to this IdleRPG codebase (Manager-pattern MonoBehaviours, DOTween, BreakInfinity big-numbers, JSON save system). Checks event-subscription leaks, DOTween cleanup, hot-path allocations, and save integrity. Trigger on "review this script", "check for leaks", "unity code review", or before finishing a script change.
allowed-tools:
  - Bash
  - Grep
  - Read
---

# Unity Code Review (IdleRPG)

Review the changed/target `.cs` files against the checklist below. Report findings as
`file:line — [severity] problem → fix`. Only report real hits; don't pad. Severity:
🔴 leak/crash/data-loss, 🟡 perf/correctness, 🟢 style.

## Headline checks (this repo's actual weak spots)

Baseline scan showed **121 files subscribe with `+=` but 0 unsubscribe with `-=`, and 0 use
`OnDestroy`**. Event/leak checks are the priority here.

1. 🔴 **Event subscription without unsubscribe.** Every `someEvent += Handler` (C# events,
   `Button.onClick`, `UnityAction`, static manager events) needs a matching `-= Handler` in
   `OnDestroy`/`OnDisable`. Missing = ghost callbacks firing on destroyed objects + leaks.
   - Grep: `grep -rnE '\+=\s*\w+;' <file>` then confirm a paired `-=` exists in the same class.
2. 🔴 **DOTween on objects that get destroyed.** A running tween on a destroyed transform
   throws / leaks. Any `.DOFade/.DOMove/.DOScale/transform.DO*` needs
   `transform.DOKill()` (or `.SetLink(gameObject)`) on destroy/disable.
3. 🔴 **Save integrity.** `PlayerPrefs`/JSON writes: is there a `.Save()`/flush, and is a
   corrupt/missing key handled on load (default value, not a crash)? Idle games lose progress here.
4. 🟡 **Hot-path allocation.** `GetComponent`, `Resources.Load`, `GameObject.Find`,
   `new`, LINQ, string concat inside `Update`/`FixedUpdate`/`OnGUI` or per-frame loops → cache it.
5. 🟡 **Big-number misuse.** BreakInfinity/`BigDouble` values must not be cast to
   `double`/`float`/`int` for gameplay math or compared with `==` — stays in BigDouble, formats only at display.
6. 🟡 **Null after scene load.** Serialized refs assumed non-null; `Instance` singletons used
   before Awake ordering is guaranteed.
7. 🟢 **Empty Unity messages** (empty `Update()`/`Start()`) — delete, they cost per-frame.
8. 🟢 Coroutines started but not stopped on disable; `InvokeRepeating` without `CancelInvoke`.

## How to run

```bash
# scope: files changed vs a ref, else pass explicit paths
git -C "H:/Game/IdleRPG/NewRPG" diff --name-only --diff-filter=d 2>/dev/null | grep '\.cs$'
```

Read each target file fully, walk the checklist, report the table. For check #1, the fast
signal is: does the class have any `+=` but no `OnDestroy`/`OnDisable` with `-=`? That alone
is a 🔴 in this codebase.
