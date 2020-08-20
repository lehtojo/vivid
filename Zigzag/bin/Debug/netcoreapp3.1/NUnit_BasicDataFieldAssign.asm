section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V23basic_data_field_assignP13BasicDataType
_V23basic_data_field_assignP13BasicDataType:
mov dword [rdi], 314159265
mov byte [rdi+4], 64
movsd xmm0, qword [rel _V23basic_data_field_assignP13BasicDataType_C0]
movsd qword [rdi+5], xmm0
mov rax, -2718281828459045
mov qword [rdi+13], rax
mov word [rdi+21], 12345
ret

_V4initv_rx:
sub rsp, 8
mov rax, 1
add rsp, 8
ret
mov rdi, 23
call _V8allocatex_rPh
mov rdi, rax
call _V23basic_data_field_assignP13BasicDataType
ret

section .data

_V23basic_data_field_assignP13BasicDataType_C0 dq 1.414