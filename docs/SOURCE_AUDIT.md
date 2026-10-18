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

## Original experiments, charts and reports compared

| Source | Original experimental workflow | What stays identical | Intentional presentation/architecture changes |
| --- | --- | --- | --- |
| Polynomial `Program.cs` + `ArrayFactory.cs` | Runge and shifted logarithm on an equidistant interpolation grid; `1001` plot points, absolute errors, Lagrange/Newton comparison | Functions, distinction between degree and node count, both polynomial forms, independent 1001-point evaluation grid | Common `ExperimentRunner` accepts degrees/graph samples explicitly; methods receive matching input arrays |
| Polynomial `SvgGraph.cs` + `ReportWriter.cs` | Self-contained SVG plots and tabular/HTML output | Dependency-free SVG/CSV/HTML outputs and pointwise absolute-error columns | `Cli.Reporting` owns serialization and SVG, never `Core`; SVG includes labeled axes, legend, reference and computed curves; HTML is encoded and linkable |
| Trigonometric `AnalysisHelper.cs` + `SelfTests.cs` | Comparison of `exp(sin t + cos t)` and `3cos(15t)`, aliasing and coefficients `a_k,b_k`; boundary cases | Fourier normalization, historical degrees `1,3,7,14,15,30,100`, node and periodicity tests | Independent xUnit suite and reusable experiments; original numerical values checked rather than blindly accepted |
| Source archive sample reports and graphs | Static generated outputs from coursework | Representative graph behaviors and error interpretation are documented | Published SVG examples are regenerated by the .NET CLI in CI; no stale legacy reports are mixed into source folders |
| Python numerical scripts | Interactive prints and selected tabular intermediate values | Fixed input/output numerical controls from actually executed original Python algorithms | Machine-readable JSON golden fixtures compare all migrated C# implementations, and HTML/SVG output is generated exclusively by C# |

**Important:** differences between exact mathematical results, floating-point approximation and grid-sampled error are explicitly distinguished. A small graph error is not a proof of uniform convergence, and no Python source code or runtime is needed in the public solution.

## Original vs port

The original ZIPs remain the original source of record; the new repository contains the selected refactored C# implementation and **JSON derived from executing original Python functions**. The Python files themselves, Python virtual environments, unrelated games, network applications, IDE artifacts and temporary reports are not copied into the solution.

For all deliberate semantic changes, see [PYTHON_AUDIT.md](PYTHON_AUDIT.md) and [PRECISION.md](PRECISION.md).
