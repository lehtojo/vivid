global function_integer_power:function

; ebp+8: exponent
; ebp+4: base
function_integer_power:
fild dword [esp+4] ; st1: base
fild dword [esp+8] ; st0: exponent
; st0 = st1 * log2(st0)
fyl2x
; duplicate
fld st0
; st0 = round st0
frndint
; st1 = st1 - st0 => decimals
fsubr st1, st0
; st0 <-> st1
fxch st1
fchs
; st0 = 2^st0 - 1
f2xm1
; st0: +1.0
fld1 
; st0 = st1 (1.0) + st0 
faddp st1, st0
; st0 -> eax
fscale
fstp st1
sub esp, 4
fistp dword [esp]
mov eax, dword [esp]
add esp, 4
ret


