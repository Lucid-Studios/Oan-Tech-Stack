(in-package :sli-core)

;; Bounded witness and morphism candidacy module.
;; Witness compares lawful localities and branch variants without granting transport.

(defun witness-begin (left right)
  (list :witness-begin left right))

(defun witness-compare ()
  (list :witness-compare))

(defun witness-preserve (invariant)
  (list :witness-preserve invariant))

(defun witness-difference (detail)
  (list :witness-difference detail))

(defun witness-residue (detail)
  (list :witness-residue detail))

(defun glue-threshold (value)
  (list :glue-threshold value))

(defun morphism-candidate ()
  (list :morphism-candidate))

;; Bounded composition catalog.
;; The bridge expands these forms deterministically and does not grant
;; lawful transport, Sanctuary intake, or automatic gluing.

(witness-composite witness-locality-compare (witness-begin $1 $2) (witness-compare) (witness-preserve self-anchor-polarity) (witness-preserve other-anchor-polarity) (witness-preserve relation-anchor-polarity) (witness-preserve seal-posture-bound) (witness-preserve reveal-posture-bound) (witness-preserve participation-mode-limit) (witness-preserve identity-nonbinding) (glue-threshold 0.75) (morphism-candidate))
(witness-composite witness-branch-compare (witness-begin $1 $2) (witness-compare) (witness-preserve self-anchor-polarity) (witness-preserve other-anchor-polarity) (witness-preserve relation-anchor-polarity) (witness-preserve seal-posture-bound) (witness-preserve reveal-posture-bound) (witness-preserve participation-mode-limit) (witness-preserve identity-nonbinding) (witness-difference rehearsal-branch) (witness-difference substitution) (witness-residue branch-local-comparison) (glue-threshold 0.75) (morphism-candidate))
