(in-package :sli-core)

;; Bounded accountability packet module.
;; Packet formation shapes a formed admissible surface into a review packet
;; without granting Sanctuary judgment, custody mutation, or identity promotion.

(defun packet-begin (surface-handle)
  (list :packet-begin surface-handle))

(defun packet-lineage (surface-handle)
  (list :packet-lineage surface-handle))

(defun packet-invariants (surface-handle)
  (list :packet-invariants surface-handle))

(defun packet-class (surface-handle)
  (list :packet-class surface-handle))

(defun packet-reveal (surface-handle)
  (list :packet-reveal surface-handle))

(defun packet-residue (detail)
  (list :packet-residue detail))

(defun packet-status (status)
  (list :packet-status status))

;; Bounded composition catalog for Sprint G1.
;; The bridge expands these forms deterministically and does not grant
;; Sanctuary intake, judgment, custody mutation, or governed identity.

(accountability-composite accountability-packet-bounded (packet-begin $1) (packet-lineage $1) (packet-invariants $1) (packet-class $1) (packet-reveal $1) (packet-status candidate))
