global _V22internal_integer_powerxx_rx:function hidden
_V22internal_integer_powerxx_rx:
xor rax, rax ; Not implemented
ret

global _V12internal_cosd_rd
_V12internal_cosd_rd:
sub rsp, 8
movsd qword [rsp], xmm0
fld qword [rsp]
fcos
fstp qword [rsp]
movsd xmm0, qword [rsp]
add rsp, 8
ret

global _V12internal_sind_rd
_V12internal_sind_rd:
sub rsp, 8
movsd qword [rsp], xmm0
fld qword [rsp]
fsin
fstp qword [rsp]
movsd xmm0, qword [rsp]
add rsp, 8
ret