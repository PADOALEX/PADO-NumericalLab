# Primary source audit and preservation map

Actual input archives were inspected directly (not inferred from their names). Ignored virtual environments, binaries, caches and generated outputs. Each digest below is **SHA-256 of the unmodified source file bytes inside the original ZIP**, allowing reconstruction and comparison without bloating the public solution with duplicate app trees.

## Source fingerprints

| Archive | Original source | SHA-256 (first 16 chars) | Migrated component |
| --- | --- | --- | --- |
| `InterpolationLab_CSharp(2).zip` | `InterpolationLab_CSharp/Interpolation.cs` | `0994fb5a9e544b3c` | `Core` |
| `InterpolationLab_CSharp(2).zip` | `InterpolationLab_CSharp/ManualMath.cs` | `a11ecfbbcdd6b894` | `Core` |
| `InterpolationLab_CSharp(2).zip` | `InterpolationLab_CSharp/Program.cs` | `3cc4a8a6bc67ce0e` | `Core` |
| `InterpolationLab_CSharp(2).zip` | `InterpolationLab_CSharp/SvgGraph.cs` | `70dbcc8a124e4cd7` | `Cli.Reporting` |
| `InterpolationLab_CSharp(2).zip` | `InterpolationLab_CSharp/ReportWriter.cs` | `d93ffced58298623` | `Cli.Reporting` |
| `TrigInterpolationManual(1).zip` | `TrigInterpolationManual/TrigonometricInterpolator.cs` | `3a61c003071a9a22` | `Core` |
| `TrigInterpolationManual(1).zip` | `TrigInterpolationManual/ManualMath.cs` | `8269340e585fefea` | `Core` |
| `TrigInterpolationManual(1).zip` | `TrigInterpolationManual/SelfTests.cs` | `ca9eb7c721d69900` | `Core` |
| `TrigInterpolationManual(1).zip` | `TrigInterpolationManual/AnalysisHelper.cs` | `74a43876795a4d50` | `Core` |
| `C#_projects(1).zip` | `C#_projects/Program.cs` | `d815b94214bcc815` | `Cli.Tools` |
| `C#_projects(1).zip` | `C#_projects/Shapes/Rectangle.cs` | `d0087909efd9601c` | `Cli.Tools` |
| `C#_projects(1).zip` | `C#_projects/Shapes/Circle.cs` | `28a87a71a7510b77` | `Cli.Tools` |
| `PycharmProjects(1).zip` | `PycharmProjects/MatStat/main.py` | `126345cd6e748919` | `Core` |
| `PycharmProjects(1).zip` | `PycharmProjects/CMLA/main.py` | `cfb4b57e8958b952` | `Core` |
| `PycharmProjects(1).zip` | `PycharmProjects/CHMLA_2/main.py` | `5e776806876d0b41` | `Core` |
| `PycharmProjects(1).zip` | `PycharmProjects/CHMLA_3/main.py` | `fe6bc546c5e2688c` | `Core` |
| `PycharmProjects(1).zip` | `PycharmProjects/matrix/main.py` | `dc687182b36e16cb` | `Core` |
| `PycharmProjects(1).zip` | `PycharmProjects/Vector/vector.py` | `1d026c3d5a27ba87` | `Core` |
| `Python_Projects(1).zip` | `Python_Projects/matrix/main.py` | `bae6a4e617a1af18` | `Core` |
| `Python_Projects(1).zip` | `Python_Projects/matrix_class/main.py` | `a2f084d10209a3b8` | `Core` |
| `Python_Projects(1).zip` | `Python_Projects/Vector_class/vector.py` | `920788c63c93ab53` | `Core` |

## Observed issues and migration decisions

| Area | Observation | Treatment |
| --- | --- | --- |
| Original polynomial interpolation | Direct O(n²) Lagrange evaluation, divided differences in Newton, fixed 1001 plotting grid; original `Validate` rejects `graphNodes.Length <= nodes.Length`, an unnecessary coupling | Both formulas kept, node/grid coupling eliminated and equality-at-node handled to reduce arithmetic noise |
| Original natural logarithm | Reduction to `[1,2)` and atanh-style odd-power series with compensated sum | Preserved with explicit argument checks; independently tested against `Math.Log` |
| Original trigonometric interpolation | `N=2n+1` with manual sin/cos, Kahan sums; `n=7` intentional alias and `n>=15` harmonic representation | Same coefficient normalization, functions and historical controls preserved in xUnit |
| Original trig test harness | Self-tests executed at startup and mixed with UI/reporting | Recreated in xUnit tests; independent of console entrypoint |
| Original calculator | History limited to 100 entries, parsing permits special values via standard parse, math may overflow | Minimal non-interactive demo with finite operand/result guard; no artificial dependency from numerical core |
| Original geometry | Positive-only dimensions, but `NaN` silently passes `<=0` check | Preserve circle, rectangle, square inheritance with explicit finite positive constraints |
| Python algebra/stats | Missing shape validation, no matrix pivoting, unguarded statistics for sample size 1 | Pure C# numerical APIs, pivoting and well-defined error contracts |

## Original vs port

The original ZIPs remain the original source of record; the new repository contains the selected refactored C# implementation and **JSON derived from executing original Python functions**. The Python files themselves, Python virtual environments, unrelated games, network applications, IDE artifacts and temporary reports are not copied into the solution.

For all deliberate semantic changes, see [PYTHON_AUDIT.md](PYTHON_AUDIT.md) and [PRECISION.md](PRECISION.md).
