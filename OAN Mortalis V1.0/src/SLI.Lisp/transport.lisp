(in-package :sli-core)

;; Bounded lawful transport module.
;; Transport carries only witness-preserved structure and does not imply admissibility or identity.

(defun transport-begin (witness-handle)
  (list :transport-begin witness-handle))

(defun transport-source (source)
  (list :transport-source source))

(defun transport-target (target)
  (list :transport-target target))

(defun transport-preserve (invariant)
  (list :transport-preserve invariant))

(defun transport-map (source target)
  (list :transport-map source target))

(defun transport-residue (detail)
  (list :transport-residue detail))

(defun transport-status (status)
  (list :transport-status status))

;; Bounded composition catalog for Sprint E.
;; The bridge expands these forms deterministically and does not grant
;; admissibility, Sanctuary intake, custody mutation, or accountability.

(transport-composite transport-bounded (transport-begin $1) (transport-source $2) (transport-target $3) (transport-preserve self-anchor-polarity) (transport-preserve other-anchor-polarity) (transport-preserve relation-anchor-polarity) (transport-preserve seal-posture-bound) (transport-preserve reveal-posture-bound) (transport-preserve participation-mode-limit) (transport-preserve identity-nonbinding) (transport-status candidate))
