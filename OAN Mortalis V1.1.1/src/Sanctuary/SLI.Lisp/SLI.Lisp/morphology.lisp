(in-package :sli-core)

;; Morphology bridge module.
;; The first bridge pass keeps canonical law in C# while allowing
;; sentence, paragraph, and body formation programs to execute through
;; the LispBridge/interpreter path.

(defun morph-root (value)
  (list :morph-root value))

(defun morph-operator (token kind)
  (list :morph-operator token kind))

(defun morph-constructor (role root)
  (list :morph-constructor role root))

(defun morph-predicate-root (root)
  (list :morph-predicate-root root))

(defun morph-render (render)
  (list :morph-render render))

(defun morph-summary (summary)
  (list :morph-summary summary))

(defun morph-scalar (scalar)
  (list :morph-scalar scalar))

(defun morph-outcome (outcome)
  (list :morph-outcome outcome))

(defun morph-graph-edge (source target relation)
  (list :morph-graph-edge source target relation))

(defun morph-anchor (anchor)
  (list :morph-anchor anchor))

(defun morph-invariant (statement)
  (list :morph-invariant statement))

(defun morph-cluster-entry (entry)
  (list :morph-cluster-entry entry))

(defun morph-body-summary (summary)
  (list :morph-body-summary summary))
