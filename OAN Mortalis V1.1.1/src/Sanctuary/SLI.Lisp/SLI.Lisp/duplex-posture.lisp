;; duplex-posture.lisp
;; First bounded RTME posture family for duplex field participation.

(defparameter *duplex-projection-postures*
  '(:hovering :rehearsing :braided :latent :ripening :unresolved))

(defparameter *duplex-posture-transition-law*
  '(:preserve-origin
    :preserve-bounded-standing
    :no-prime-closure
    :emit-bounded-snapshot))

(defun duplex-posture-module-profile ()
  "Return the symbolic profile for bounded duplex posture movement."
  '(:profile :rtme-duplex-posture
    :carrier :sli-lisp-symbolic-runtime
    :authority :projected-field-only
    :closure :membrane-receipt-required))
