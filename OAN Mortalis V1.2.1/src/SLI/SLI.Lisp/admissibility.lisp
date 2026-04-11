(in-package :sli-core)

;; Bounded admissible surface module.
;; Admissibility shapes completed transport into an inspectable surface
;; without granting governance, custody, or Sanctuary accountability.

(defun surface-begin (transport-handle)
  (list :surface-begin transport-handle))

(defun surface-source (transport-handle)
  (list :surface-source transport-handle))

(defun surface-class (surface-class identity-applicability)
  (list :surface-class surface-class identity-applicability))

(defun surface-reveal (posture)
  (list :surface-reveal posture))

(defun surface-boundary (boundary)
  (list :surface-boundary boundary))

(defun surface-evidence (detail)
  (list :surface-evidence detail))

(defun surface-residue (detail)
  (list :surface-residue detail))

(defun surface-status (status)
  (list :surface-status status))

;; Bounded composition catalog.
;; The bridge expands these forms deterministically and does not grant
;; Sanctuary intake, accountability, custody mutation, or governed identity.

(admissibility-composite admissible-surface-bounded (surface-begin $1) (surface-source $1) (surface-class $2 $3) (surface-reveal masked) (surface-boundary bounded) (surface-evidence witness-preserved-structure) (surface-evidence transport-residue-carried) (surface-status candidate))
