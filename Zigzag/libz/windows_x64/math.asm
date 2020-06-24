global integer_power
integer_power:
xor rax, rax ; Not implemented
ret

global internal_cos
internal_cos:
movsd qword [rsp+8], xmm0
fld qword [rsp+8]
fcos
fstp qword [rsp+8]
movsd xmm0, qword [rsp+8]
ret

global internal_sin
internal_sin:
movsd qword [rsp+8], xmm0
fld qword [rsp+8]
fsin
fstp qword [rsp+8]
movsd xmm0, qword [rsp+8]
ret