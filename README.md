<div align="center">

# PADO NumericalLab

### Numerical Methods & Mathematical Computing in C#

**An educational .NET 10 numerical-computing toolkit** combining polynomial and trigonometric interpolation, hand-written elementary functions, linear algebra, descriptive statistics, automated experiments, and reproducible reports.

[![.NET 10 CI](https://github.com/PADOALEX/PADO-NumericalLab/actions/workflows/ci.yml/badge.svg)](https://github.com/PADOALEX/PADO-NumericalLab/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-Mathematical%20Computing-239120?logo=csharp)
![License](https://img.shields.io/badge/license-MIT-blue)

[Quick start](#quick-start) · [Mathematical methods](#mathematical-methods) · [Experiments](#experiments--reporting) · [Tests](#tests-and-precision) · [Python audit](docs/PYTHON_AUDIT.md)

</div>

---

## What this project demonstrates

| Area | Features | Source |
| --- | --- | --- |
| Polynomial interpolation | **Lagrange** direct basis and **Newton** divided differences | `InterpolationLab_CSharp(2).zip` |
| Trigonometric interpolation | Odd-node `N = 2n+1` Fourier coefficients `a_k`, `b_k`; periodic approximation and aliasing | `TrigInterpolationManual(1).zip` |
| Hand-written math | Taylor `sin`, `cos`, `exp`; argument-reduced natural logarithm; Kahan compensated summation | Original C# labs |
| Linear algebra | Matrix/vector arithmetic, pivoted inversion, **Householder** QR solve, **Thomas** right-sweep, dominant eigenpair by power iteration | Selected original Python labs |
| Statistics | Mean, median, multimodal samples, population and sample variance, standard deviation, skewness, kurtosis, grouped frequencies | `MatStat/main.py` |
| Reports | **CSV**, **SVG**, and responsive **HTML**, no plotting packages or Web API | Separate CLI reporting component |
| Standalone demos | Arithmetic calculator and OO circle/rectangle/square | `C#_projects(1).zip` |

Only the mathematical modules belong to the core library; the calculator and shape demo are independent console tools. The production solution contains **only C# projects**. Original Python outputs are stored as JSON regression fixtures, not Python runtime dependencies.

## Quick start

**Prerequisite:** .NET 10 SDK. No PostgreSQL, Docker, JavaScript, or API credentials.

```powershell
git clone https://github.com/PADOALEX/PADO-NumericalLab.git
cd PADO-NumericalLab

dotnet restore PADO.NumericalLab.sln
dotnet build PADO.NumericalLab.sln -c Release
dotnet test PADO.NumericalLab.sln -c Release

# Comparative polynomial experiments: n+1 nodes, independently 1001 plot points
dotnet run --project src/Pado.NumericalLab.Cli -c Release -- interpolate --n 5,10,15 --grid 1001

# Manual Fourier interpolation and historical aliasing examples
dotnet run --project src/Pado.NumericalLab.Cli -c Release -- trig --n 1,7,15 --grid 1001

# Independently executable demos
dotnet run --project src/Pado.NumericalLab.Cli -c Release -- stats
dotnet run --project src/Pado.NumericalLab.Cli -c Release -- algebra
dotnet run --project src/Pado.NumericalLab.Cli -c Release -- calc 2 pow 3
dotnet run --project src/Pado.NumericalLab.Cli -c Release -- shapes samples/shapes.txt
```

Generated reports are written to `results/polynomial/report.html` and `results/trig/report.html`. Output directories are ignored by Git; GitHub Actions also publishes CI-generated reports as downloadable artifacts.

## Mathematical methods

### Polynomial interpolation

For **distinct nodes** `(x_i, y_i)` with `i = 0, …, n`:

**Lagrange basis (original direct algorithm):**

$$L_n(x) = \sum_{i=0}^{n} y_i\prod_{\substack{j=0\\j\ne i}}^{n}\frac{x-x_j}{x_i-x_j}.$$

**Newton form (in-place divided-difference table, nested evaluation):**

$$N_n(x)=c_0+\sum_{k=1}^n c_k\prod_{j=0}^{k-1}(x-x_j),\qquad c_k=f[x_0,\ldots,x_k].$$

Both representations interpolate the same exact polynomial but differ in floating-point roundoff. Test functions inherited from the original coursework are **Runge's function** `f(x)=1/(1+25x²)` and `g(x)=ln(x+2)`, evaluated on `[-1,1]` at polynomial degrees `n = 5,10,15` by default. The original **1001-point graph/error grid is retained**. `n+1` interpolation nodes and `--grid` display samples are intentionally separate parameters.

### Trigonometric interpolation

With `N = 2n+1` nodes `t_j = 2πj/N` on `[0,2π)`:

$$a_k=\frac{2}{N}\sum_{j=0}^{N-1}f(t_j)\cos(kt_j),\qquad b_k=\frac{2}{N}\sum_{j=0}^{N-1}f(t_j)\sin(kt_j).$$

$$q_n(t)=\frac{a_0}{2}+\sum_{k=1}^n\left(a_k\cos(kt)+b_k\sin(kt)\right).$$

The two historical functions remain `exp(sin(t)+cos(t))` and `3cos(15t)`. The latter is a deliberately useful aliasing experiment: with `n=7`, `N=15` samples of `3cos(15t)` are identically `3`; when `n≥15`, the harmonic belongs to the basis and can be reproduced over the full interval. Coefficient, node, endpoint, and periodicity assertions are included in xUnit.

### Linear algebra and statistics

| Method | Implementation details |
| --- | --- |
| Matrix inverse | Gauss-Jordan elimination **with partial pivoting**; fixes the legacy false-singular result for an invertible matrix with a zero diagonal element |
| Householder | Reflections transform `Ax=b` into an upper-triangular system, then back-substitute |
| Thomas method | Solve tridiagonal systems; reproduce the original `CHMLA_2` boundary-value experiment `y(0)=1`, `y(1)=0.5` |
| Power iteration | Dominant eigenpair, **residual-based stopping** instead of the original premature eigenvalue-only stop |
| Descriptive statistics | Explicit distinction between population variance `Σ(x−μ)²/n` and sample variance `Σ(x−μ)²/(n−1)`; central moments, skewness and excess kurtosis |

For the Python selection criteria and documented deviations, see [the source audit](docs/PYTHON_AUDIT.md).

## Experiments & reporting

For each interpolation experiment the CLI exports:

- **CSV**: invariant-culture rows `x,exact,approximation,absolute_error` (one row per plot point).
- **SVG**: two contrasting curves (reference function and approximation), with zero external image libraries.
- **HTML**: a responsive summary with numerical error table, links to CSV, and embedded SVG plots.

Max absolute error is the maximum over the sampled grid, **not a mathematical proof of the maximum on the continuous interval**. Small differences between Lagrange and Newton are expected; higher polynomial degree with equally spaced Runge nodes may actually *worsen* error near the ends (Runge phenomenon).

Example values imported from *executed original Python methods*:

| Input | Reference result |
| --- | --- |
| `MatStat` sample `[1,2,2,3,4,4,4,5]` | mean **3.125**, median **3.5**, population variance **1.609375** |
| Householder, `A=[[4,1],[2,3]]`, `b=[9,8]` | solution **[1.9, 1.4]** |
| Right-sweep, `n=10`, `x=0.5` | `y≈0.6674997738352978` |
| Matrix multiplication `[[1,2],[3,4]]·[[2,0],[1,2]]` | `[[4,4],[10,8]]` |

## Tests and precision

Tests use **xUnit** and include parameterized mathematical invariants, interpolation at input nodes, trigonometric basis-coefficient checks, periodicity, singular matrices, zero denominators, nonfinite inputs, boundaries, report generation, and **golden-number comparisons with the actual Python source**.

See [PRECISION.md](docs/PRECISION.md) for absolute tolerance policies, known numerical limits, error taxonomy and intentionally changed legacy behavior. Standard library `Math.Sin`, `Math.Cos`, `Math.Log`, `Math.Exp` are used **only as independent references in manual-function tests**, not inside those manual implementations.

## Solution layout

```text
PADO.NumericalLab.sln
├── src/
│   ├── Pado.NumericalLab.Core/     # pure C# algorithms; no console or file I/O
│   │   ├── Manual/
│   │   ├── Interpolation/
│   │   ├── Trigonometry/
│   │   ├── LinearAlgebra/
│   │   ├── Statistics/
│   │   └── Experiments/
│   └── Pado.NumericalLab.Cli/      # commands, reporting, calculator, geometry
├── tests/Pado.NumericalLab.Tests/
├── docs/
│   ├── PYTHON_AUDIT.md
│   ├── SOURCE_AUDIT.md
│   ├── PRECISION.md
│   └── reference/python-golden.json
└── .github/workflows/ci.yml
```

## Project notes

- The repository is an **educational** numerical laboratory, not a validated scientific computation library.
- Manual trigonometric functions retain the original input-domain restriction `|t| ≤ 1,000,000 rad`; manual `exp` retains `[-2,2]`.
- Source Python experiments were audited, **not wholesale translated**. No `.venv`, site-packages, caches, `.exe`, NuGet output or original unrelated Python app code is shipped.
- This is a console-first library/CLI project by design: there is **no unnecessary ASP.NET Web API**.

## License

MIT — see [LICENSE](LICENSE). Author: [PADOALEX](https://github.com/PADOALEX).
