(in-package :sli-core)

;; Experimental fragment diagnostics vocabulary.
;; Lisp owns the symbolic names; the host mirrors them only for witness receipts.

(defparameter *diagnostic-roles*
  '(:origin :bridge :inversion :leap :reflection))

(defparameter *diagnostic-operators*
  '(:delta :reflect :rebind :bloom :fix))
