# Scientific Posture Review

Review status: public descriptive review. This document is not a certification,
legal conclusion, safety case, product validation, or implementation disclosure.

## Scope

This review evaluates the public scientific posture of the OAN public repository
and the private Sanctuary working program line at the level that can be safely
described in public.

The public repository is a release-controlled description surface. It does not
publish the private codebase, internal architecture, datasets, runtime payloads,
or build procedures. For that reason, this review does not claim empirical
validation of the private working system. It identifies the scientific posture,
evidence gaps, and review path that would be expected before stronger claims
could be made.

## Review Method

The review uses a public-evidence method:

- inspect the public repository as a disclosure and governance artifact;
- compare the public posture against established AI governance, software
  security, transparency, and assurance literature;
- separate descriptive claims from operational, empirical, or product-readiness
  claims;
- name evidence that would be needed before future stronger claims.

The review does not inspect private implementation bodies or convert private
engineering material into public instructions.

## Scientific Posture Summary

The current public posture is strongest as a governance and disclosure-control
surface. Its central public claim is modest and scientifically defensible:
advanced AI-assisted systems require explicit boundaries for authority, memory,
review, disclosure, safety, and human responsibility.

That posture is consistent with current governance literature and standards
that emphasize risk management, human accountability, transparency, security,
and evidence-limited claims. It is not yet a public empirical claim about system
performance, reliability, safety, or deployment readiness.

Academic review finding: the public repository is scientifically credible as a
claim-boundary and governance artifact, but it is not sufficient evidence for
technical validity. The private working line remains an internal research and
engineering body until supported by controlled evaluation, secure-development
records, data custody documentation, and an assurance argument that can be
reviewed without exposing private build capability.

## Maturity Classification

| Object Reviewed | Current Classification | Basis | Limitation |
| --- | --- | --- | --- |
| Public repository | Controlled descriptive epoch | Public documents define scope, non-claims, disclosure boundaries, contribution routing, and security intake | It does not contain implementation, runtime, dataset, or evaluation evidence |
| Private working line | Internal pre-public-validation program | Public posture acknowledges private research and engineering while withholding build capability | Public readers cannot validate architecture, runtime behavior, safety, or performance from this repository |
| Citizen science access posture | Education-and-stewardship proposal | Public materials frame public access as literacy, certification, approval, and bounded participation | It is not an approval to operate, deploy, modify, or reproduce the private system |
| Scientific claims | Evidence-limited conceptual claims | The repository supports governance and boundary claims | Empirical, safety, security, performance, and deployment claims require future evidence |

## Evidence Review Matrix

| Review Area | Current Public Evidence | Scientific Posture | Needed Before Stronger Claims |
| --- | --- | --- | --- |
| Governance | Public release boundary, disclosure model, contribution rules, security policy | Plausible governance frame | Full assurance case, role model, review receipts, and release decision records |
| Human accountability | Public non-claims and stewardship language | Aligned with human-centered AI principles | Operational responsibility model and incident handling evidence |
| Transparency | Public overview, non-executable examples, withheld-material notices | Appropriate for controlled disclosure | Structured system card, model/data documentation, and limitation reporting |
| Software security | Public repository excludes implementation-bearing material | Appropriate public surface control | Private secure-development evidence and vulnerability handling records |
| Safety and risk | Public materials emphasize review before expansion | Review-aware but not safety-validated | Hazard analysis, misuse analysis, evaluations, and mitigation evidence |
| Data and memory custody | Public repository withholds private corpus and runtime payloads | Conservative disclosure posture | Data provenance, consent, retention, and access-control records |
| Product readiness | Public materials make explicit non-claims | Not public-ready and not claimed | Reproducible test reports, operational limits, monitoring, and rollback plans |

## Alignment With External Literature

The public posture aligns most directly with:

- NIST AI Risk Management Framework 1.0, because the repository treats AI risk as
  a governance, mapping, measurement, and management problem rather than a single
  technical property.
- NIST AI 600-1, because generative AI systems require attention to risks that
  can be amplified by model behavior, misuse, information integrity, and
  deployment context.
- NIST SP 800-218, because private implementation work should maintain secure
  development practices outside the public descriptive surface.
- ISO/IEC 42001, because the public framing treats responsible AI as a managed
  organizational process rather than only a model feature.
- Model cards and datasheets literature, because stronger public technical
  claims would need structured documentation of purpose, limitations, data,
  evaluation context, and intended use.
- Machine-learning assurance-case literature, because safety or reliability
  claims require an explicit evidence argument rather than broad assertion.

## Current Scientific Claim Boundary

The public repository may claim:

- the project maintains a controlled public disclosure posture;
- private build capability is intentionally withheld from the public repository;
- the public record is descriptive, non-executable, and review-oriented;
- future expansion should require evidence, review, and release gates;
- citizen science access should begin with education and stewardship, not
  uncontrolled build access.

The public repository should not claim:

- production readiness;
- validated safety;
- validated model performance;
- deployability;
- completeness of the private codebase;
- approval to operate the private working line;
- legal, identity, custody, or governance authority;
- publication of private datasets, runtime payloads, or implementation detail.

## Sanctuary Working Line Posture

The Sanctuary working program line should be presented publicly as private
research and engineering under review. The public posture can state that a
private working line exists, but it should not describe its file layout,
internal contracts, runtime payloads, build process, or operational sequence.

Scientifically, that places the working line in a pre-public-validation posture:
it may be an active internal research program, but public claims should remain
limited until the project produces reviewable evidence that can be safely
released.

## Recommended Evidence Path

The next public-safe academic evidence path is:

1. System description card: purpose, intended use, non-use, operators, limits,
   and known risks.
2. Data and memory documentation: provenance classes, consent posture, retention
   posture, access controls, and excluded data classes.
3. Evaluation plan: non-sensitive metrics, test boundaries, failure modes, and
   pass/fail criteria.
4. Security and misuse review: threat classes, disclosure routing, abuse
   resistance, and incident handling.
5. Assurance case: claim, argument, evidence, and residual risk for each future
   public technical claim.
6. Release review: rights, privacy, safety, security, dual-use risk,
   operational maturity, and claim accuracy.

## Working Line Review Questions

Before any stronger public technical derivative is released, the private working
line should be able to answer these questions with evidence:

- What narrow system claim is being made, and what evidence directly supports it?
- What data, memory, model, and runtime surfaces are excluded from public release?
- What secure-development practices govern the private implementation?
- What evaluation protocol would falsify or limit the claim?
- What misuse, failure, and custody risks remain after mitigation?
- What human review authority can stop, revise, or reject a release?

## Public Conclusion

The current public epoch is scientifically appropriate as a controlled
disclosure and governance posture. It is not yet an empirical publication of the
private system. The correct academic next step is to build a reviewable evidence
package that can support narrow, evidence-bound claims without exposing private
build capability.

## References

- NIST, [Artificial Intelligence Risk Management Framework (AI RMF 1.0)](https://www.nist.gov/publications/artificial-intelligence-risk-management-framework-ai-rmf-10).
- NIST, [Artificial Intelligence Risk Management Framework: Generative Artificial Intelligence Profile](https://www.nist.gov/publications/artificial-intelligence-risk-management-framework-generative-artificial-intelligence).
- NIST, [Secure Software Development Framework (SSDF) Version 1.1, SP 800-218](https://csrc.nist.gov/publications/detail/sp/800-218/final).
- ISO, [ISO/IEC 42001:2023 Artificial intelligence management system](https://www.iso.org/standard/42001).
- OECD, [OECD AI Principles](https://oecd.ai/ai-principles/).
- Mitchell et al., [Model Cards for Model Reporting](https://arxiv.org/abs/1810.03993).
- Gebru et al., [Datasheets for Datasets](https://arxiv.org/abs/1803.09010).
- Hawkins et al., [Guidance on the Assurance of Machine Learning in Autonomous Systems](https://arxiv.org/abs/2102.01564).
