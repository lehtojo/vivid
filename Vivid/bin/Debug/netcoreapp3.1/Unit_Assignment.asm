section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh

global _V10assignmentP6Holder
export _V10assignmentP6Holder
_V10assignmentP6Holder:
mov dword [rcx], 314159265
mov byte [rcx+4], 64
movsd xmm0, qword [rel _V10assignmentP6Holder_C0]
movsd qword [rcx+5], xmm0
mov rax, -2718281828459045
mov qword [rcx+13], rax
mov word [rcx+21], 12345
ret

_V4initv_rx:
mov rax, 1
ret

section .data

align 16
_V10assignmentP6Holder_C0 db 57, 180, 200, 118, 190, 159, 246, 63 ; 1.414