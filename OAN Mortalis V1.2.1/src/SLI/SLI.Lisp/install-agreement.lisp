(in-package :sli-core)

;; Installed-disabled install-agreement extension surface.
;; This module forms localized agreement predicates and install footing without
;; granting service activation, runtime consequence, or Atlas mutation.

(defun install-agreement-begin (choice-matrix)
  (list :install-agreement-begin choice-matrix))

(defun install-agreement-predicate (lane assent-state)
  (list :install-agreement-predicate lane assent-state))

(defun install-agreement-identity (identity-state)
  (list :install-agreement-identity identity-state))

(defun install-agreement-posture (posture)
  (list :install-agreement-posture posture))

(transport-composite install-agreement-candidate
  (install-agreement-begin $1)
  (install-agreement-predicate $2 $3)
  (install-agreement-posture research-attached-default))
