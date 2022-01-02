.export _V22internal_integer_powerxx_rx
.export _V3powxx_rx
_V3powxx_rx:
_V22internal_integer_powerxx_rx:
# rcx: base
# rdx: exponent
mov qword [rsp+8], rcx
mov qword [rsp+16], rdx

fld1
fild qword [rsp+16]
fild qword [rsp+8]
# st0: base, st1: exponent, st2: 1.0 

fyl2x # Replace st1 with st1 ∗ log2(st0) and pop the register stack.
# st0: exponent ∗ log2(base), st1: 1.0

f2xm1 # Replace st0 with (2^st0 – 1)
# st0: 2^(exponent ∗ log2(base)) - 1.0 = base^exponent - 1, st1: 1.0 

faddp # Add st0 to st1, store result in st1, and pop the register stack.
# st0: base^exponent - 1.0 + 1.0 = base^exponent

fistp qword [rsp+8] # Load and pop base^exponent from the FPU-stack

mov rax, qword [rsp+8] # Load the result (base^exponent)
ret

.export _V22internal_decimal_powerdd_rd
.export _V3powdd_rd
_V3powdd_rd:
_V22internal_decimal_powerdd_rd:
# rcx: base
# rdx: exponent
movsd qword [rsp+8], xmm0
movsd qword [rsp+16], xmm1

fld1
fld qword [rsp+16]
fld qword [rsp+8]
# st0: base, st1: exponent, st2: 1.0 

fyl2x # Replace st1 with st1 ∗ log2(st0) and pop the register stack.
# st0: exponent ∗ log2(base), st1: 1.0

f2xm1 # Replace st0 with (2^st0 – 1)
# st0: 2^(exponent ∗ log2(base)) - 1.0 = base^exponent - 1, st1: 1.0 

faddp # Add st0 to st1, store result in st1, and pop the register stack.
# st0: base^exponent - 1.0 + 1.0 = base^exponent

fstp qword [rsp+8] # Load and pop base^exponent from the FPU-stack

movsd xmm0, qword [rsp+8] # Load the result (base^exponent)
ret

.export _V3cosd_rd
_V3cosd_rd:
movsd qword [rsp+8], xmm0
fld qword [rsp+8]
fcos
fstp qword [rsp+8]
movsd xmm0, qword [rsp+8]
ret

.export _V3sind_rd
_V3sind_rd:
movsd qword [rsp+8], xmm0
fld qword [rsp+8]
fsin
fstp qword [rsp+8]
movsd xmm0, qword [rsp+8]
ret

.export _V4sqrtd_rd
_V4sqrtd_rd:
sqrtsd xmm0, xmm0
ret
