;; duplex-braid.lisp
;; First bounded clustered/swarmed braid discipline for RTME duplex plurality.

(defparameter *duplex-braid-states*
  '(:dispersed :clustered :swarmed :coherent-braid :unstable-braid))

(defparameter *duplex-line-participation-kinds*
  '(:clustered :swarmed))

(defparameter *duplex-braid-invariants*
  '(:preserve-per-line-origin
    :preserve-per-line-posture
    :preserve-bounded-standing
    :no-self-authorized-closure
    :emit-bounded-braid-snapshot))

(defun duplex-braid-module-profile ()
  "Return the symbolic profile for bounded clustered/swarmed braid discipline."
  '(:profile :rtme-duplex-braid
    :carrier :sli-lisp-symbolic-runtime
    :plurality :clustered-and-swarmed
    :closure :advisory-return-only))
