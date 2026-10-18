# Python → C# selection audit

Reviewed source archives: `PycharmProjects(1).zip` and `Python_Projects(1).zip`, after excluding virtual environments, `.git`, `__pycache__`, IDE files, compiled dependencies and output. Python source was **executed only during the migration audit to generate frozen golden reference values**; the delivered solution has no Python project/runtime dependency.

## Decisions before porting

| Original project / path | What it actually implements | Source quality / risk | Novelty compared with C# interpolation labs | Decision, with reason |
| --- | --- | --- | --- | --- |
| `PycharmProjects/MatStat/main.py` | Mean, median, mode(s), population and corrected sample variance, moments, skewness/kurtosis, Sturges-style histograms | **Medium**: useful formulas, little validation; sample variance divides by `n−1` unguarded | **High**: no descriptive statistics in initial C# labs | **PORT** base statistics and bins; isolates statistical math and enables useful quantitative analysis |
| `PycharmProjects/MatStat_2/main.py` | Midpoint quadrature for normal density, Poisson PMF, χ² goodness-of-fit and merging bins | **Low–medium**: interactive input in calculations; hardcoded approximate `e`, integration truncation, ad-hoc degrees of freedom, χ² table fallback to zero | Medium–high | **DO NOT PORT AS IS**: tests and formulas must be redesigned and independently justified before a scientifically credible χ² tool is added |
| `PycharmProjects/CMLA/main.py` | Householder QR reflections, solving square linear systems, transformation steps | **Medium**: clear algorithm, singular tolerance `1e−12` absolute and minimal shape checks | **High** | **PORT**: reliable linear-system solution is a valuable NumericalLab capability |
| `PycharmProjects/CHMLA_2/main.py` | Right sweep (Thomas recurrence), one finite-difference boundary-value example | **Medium**: original is tightly coupled to this particular ODE; lacks input validation | **High** | **PORT** a reusable tridiagonal solver + **preserve** original experiment as `SolveOriginalBoundaryProblem` |
| `PycharmProjects/CHMLA_3/main.py` | Matrix-vector multiplication, max-component normalized power iteration, residual display | **Medium–low**: stops when eigenvalue stops changing even if eigenvector is still far from converged | **High** | **PORT and correct** residual-based stopping; keep original eigenvalue as cross-language reference |
| `PycharmProjects/matrix/main.py` | Matrix addition/subtraction/multiplication/division via inverse | **Medium–low**: inverse has no pivoting, treats invertible matrices with zero diagonal as singular | High | **PORT** algebra; **fix** inverse by partial pivoting, verify against Python golden results on nonproblematic matrices |
| `Python_Projects/matrix/main.py`, `matrix_class/main.py` | Duplicated functional/OO matrix operations and inverse | **Medium–low**: broad `except`, missing shape guards, same no-pivot failure | High, but duplicate | **CONSOLIDATE**, one C# `Matrix` rather than duplicating the Python implementations |
| `PycharmProjects/Vector/vector.py`, `Python_Projects/Vector/vector.py`, `Vector_class/vector.py` | Componentwise vector add/subtract/multiply/divide, scalar operations and file-driven calculators | **Medium**: useful operators, little NaN checking, some bare `except` | High, but duplicate | **PORT ONCE** as pure `Vectors` with explicit size and zero-divisor rules |
| `PycharmProjects/Fibonacci_Durmanenko`, `Python_Projects/Fibonacci`, `PycharmProjects/PythonProject1` | Fibonacci sequence scripts | Low–medium | **Low**: belongs in algorithms-focused `AlgorithmLab` | **SKIP** NumericalLab; no portfolio value from duplication |
| `PycharmProjects/Linked`, `Vlad_ASD_PMA_31` | Linked-list ADTs / merging sorted lists | Medium | Low for mathematical computing | **SKIP**; those belong in `AlgorithmLab` |
| `PycharmProjects/Problem1`, `Problems_Solution` | Small exercises, several using NumPy | Variable; duplicated | Low | **SKIP** as independent modules: unneeded toy wrappers around standard functions |
| `PycharmProjects/Cryptology` | Ciphers, modular arithmetic, primality, Jacobi, signatures, secret sharing | Large, mixed UI/algorithms and many independent cryptographic concerns | Distinct domain | **SKIP**; if developed, deserves a separate audited cryptography project, not an interpolation toolkit |
| `PycharmProjects/AbstractBaseClass`, `PythonProject6` | OO geometry/factory and colors | Medium | Low; C# Shapes already cover this | **SKIP** Python geometry; preserve original C# shape demo |
| `SteamChecker`, `RestApi`, `Server`, `Server_Test`, `modul_LNU`, `Task_LNU_Server_*`, `mapping` | Steam API/Tkinter and web/distributed-system exercises | Non-numerical | None | **SKIP** NumericalLab; unrelated application domain |
| Remaining simple test/data-processing scripts | Introductory exercises or scaffolds | Variable | Negligible | **SKIP**; no unique numerical method |

**Total result:** one mathematical core library, one CLI/reporting host and one test project. No artificial package-per-algorithm subdivision.

## Python golden-reference data

`docs/reference/python-golden.json` was produced by calling function definitions from the **original source archives** on the exact inputs specified in the JSON file; *not* by predicting the answers from C#. The C# xUnit tests load the JSON and assert that the ported outputs agree (absolute tolerance `1e−10` for most selected examples). Entries include vector operations, matrix operations, statistics, Householder, right sweep, and dominant eigenvalue.

**Intentional corrections and non-parity:**

1. Python matrix inverse returns an empty result for `[[0,1],[1,0]]` although this matrix is nonsingular. C# uses partial pivoting and returns the correct inverse. A dedicated test verifies the fix.
2. The original Python power iteration returns the diagonal-matrix eigenpair after only two iterations because λ stays at `3`, even when the estimated eigenvector is `[1,1/9]` rather than `[1,0]`. C# compares the correct dominant eigenvalue **but deliberately does not reproduce the premature eigenvector**; its stopping test uses `||Av−λv||∞`.
3. The `MatStat` coefficient of variation of a zero-mean sample is printed as `0`, which can be misleading. C# reports it as **undefined (`null`)**. For constant samples, skewness/kurtosis are treated as zero **by explicit convention**, not a claim of statistical identifiability.
4. Unstable/insufficiently justified χ² grouped tests were not transplanted. The description of their omission is part of the audit, not a claim of statistical verification.

## Independent validation strategy

Use golden fixtures to test *legacy preservation*, analytic cases to test *mathematical correctness*, and stdlib references exclusively to test *hand-written elementary functions*. Compare both interpolation forms on the same exact input arrays; hold plot resolution fixed when changing `n`. Track max pointwise error separately from coefficient error, node interpolation residual, and linear-system residual. Do not treat matching the same code in two languages as an independent mathematical proof.
