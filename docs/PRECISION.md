# Accuracy, domains, and limitations

**Floating point:** all numerical kernels use binary64 (`double`). Published examples are educational accuracy checks, not proofs of correctly rounded results.

| Check | Policy / tolerance | Scope |
| --- | --- | --- |
| `sin`, `cos` manual vs `System.Math` | Absolute ≤ `1e−12` | Representative angles `[-2.8,500]` and special values |
| Manual `exp` vs `System.Math.Exp` | Absolute ≤ `1e−12` | Input `[-2,2]` only, as original |
| Manual logarithm vs `System.Math.Log` | Absolute ≤ `1e−12` | Positive finite sample points `1e−6 … 1e20` |
| Algebra/statistics vs original Python | Absolute ≤ `1e−10` | Fixed same-input fixture cases in JSON |
| Mixed harmonic reconstruction | Absolute ≤ `1e−12` | 1001 points over `[0,2π]` |
| Historical trig nodes and endpoint | Absolute ≤ `1e−9` | `n∈{1,3,7,14,15,30,100}` |
| `3cos(15t)` when `n≥15` | Max grid error ≤ `1e−9` | Representation in trigonometric polynomial basis |
| Dominant eigenvalue | Absolute ≤ `1e−8` from legacy | Legacy does *not* reliably converge the eigenvector |
| Power iteration termination | Residual infinity norm ≤ `tolerance·max(1, |λ|)` | Residual computed from the current approximate eigenvector |
| Polynomial interpolation at each node | Exact return of supplied `y_i` | Equality short-circuit; not a general off-node error guarantee |

Absolute error is `|f(t)−p(t)|`; reported max is over `--grid` samples, never a continuous-interval guarantee. Rounding increases when polynomial coefficients are ill-conditioned; no blanket promise that changing `n` improves accuracy. The default `n=5,10,15`, `1001` plot points come from the original polynomial experiment. The trig source used a `1000`-subinterval grid (`1001` points), and dynamic larger grids for high `n`; users can request higher `--grid` as needed.

## Stability mechanisms

- Reject `NaN`, infinity, invalid intervals, repeated interpolation nodes and incompatible matrix/vector dimensions before division.
- Retain Kahan compensation in Fourier coefficients/expansion and use it for the dot product.
- Preserve manual Taylor-series `sin`, `cos`, `exp` and logarithm, with their original formula families and documented domains.
- Use Gauss-Jordan **partial pivoting**; throw for singular or numerically singular matrices. A scaling threshold `pivotTolerance*||A||∞` is used, not an exact mathematical rank test.
- Householder uses an absolute small-pivot threshold; extreme rescaling of a nonsingular system may cause rejection. This is explicit, not hidden.
- Thomas algorithm is intended for well-behaved, non-pivoting tridiagonal systems; it rejects zero/nonfinite pivots but **does not guarantee stability for arbitrary tridiagonal matrices**.
- Power iteration needs a unique magnitude-dominant eigenvalue and a starting vector with a nonzero component in its eigendirection; nonconvergence throws rather than returning an unverified answer.
- `n` for Fourier interpolation is limited to `1..100`, polynomial CLI to `1..30`, and plot points to `2..100001`; these are usability and numerical guards, not universal theory limits.

## Original self-test controls

From `TrigInterpolationManual/SelfTests.cs`: trigonometric special values, exp at `-2,-1,0,1,2`, Pythagorean identity, 2001 Pythagorean identity samples, Fourier coefficients `a0=4`, `a3=1.5`, `b2=-.75`, `n=7` alias, degrees `1,3,7,14,15,30,100`, null/nonfinite cases and both ends of `[0,2π]`. These were ported into **independent xUnit assertions** with explicit tolerances. Report CSV files contain original computed errors rather than forcing them to zero when within tolerance.

## Reproducibility

The frozen JSON fixture is checked into version control. Run `dotnet test PADO.NumericalLab.sln -c Release`. The CI workflow also builds and runs the CLI, checks for reports, and exposes report artifacts; end-user generated `results/`, `bin/`, `obj/`, environments and caches are excluded from the repo.
