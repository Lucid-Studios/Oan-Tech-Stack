(in-package :sli-core)

;; Bounded Dream/Game rehearsal module.
;; Rehearsal is exploratory, non-binding, pre-admissible, and explicitly invoked.

(defun rehearsal-begin (locality-state mode)
  (list :rehearsal-begin locality-state mode))

(defun rehearsal-branch (branch)
  (list :rehearsal-branch branch))

(defun rehearsal-substitute (source target)
  (list :rehearsal-substitute source target))

(defun rehearsal-analogy (source target)
  (list :rehearsal-analogy source target))

(defun rehearsal-seal (value)
  (list :rehearsal-seal value))

(defun rehearsal-residue (detail)
  (list :rehearsal-residue detail))

;; Bounded composition catalog for Sprint C.
;; The bridge expands these forms deterministically and does not grant
;; witness, morphism, transport, or admissibility machinery.

(rehearsal-composite rehearsal-bounded-exploration (rehearsal-begin $1 dream-game) (rehearsal-branch $2) (rehearsal-substitute $3 $4) (rehearsal-analogy $3 $4) (rehearsal-seal identity-sealed))
