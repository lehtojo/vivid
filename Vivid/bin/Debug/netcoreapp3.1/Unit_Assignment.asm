section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh

global _V10assignmentP6Holder
export _V10assignmentP6Holder
_V10assignmentP6Holder:
mov dword [rcx+8], 314159265
mov byte [rcx+12], 64
movsd xmm0, qword [rel _V10assignmentP6Holder_C0]
movsd qword [rcx+13], xmm0
mov rdx, -2718281828459045
mov qword [rcx+21], rdx
mov word [rcx+29], 12345
ret

_V4initv_rx:
mov rax, 1
ret

_VN6Holder4initEv_rPS_:
sub rsp, 40
mov rcx, 31
call _V8allocatex_rPh
add rsp, 40
ret

section .data

_VN6Holder_configuration:
dq _VN6Holder_descriptor

_VN6Holder_descriptor:
dq _VN6Holder_descriptor_0
dd 31
dd 0

_VN6Holder_descriptor_0:
db 'Holder', 0

align 16
_V10assignmentP6Holder_C0 db 57, 180, 200, 118, 190, 159, 246, 63 ; 1.414