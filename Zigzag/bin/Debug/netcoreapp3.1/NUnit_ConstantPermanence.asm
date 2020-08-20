section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V34constant_permanence_and_array_copyPhPS_
_V34constant_permanence_and_array_copyPhPS_:
xor rax, rax
cmp rax, 10
jge _V34constant_permanence_and_array_copyPhPS__L1
_V34constant_permanence_and_array_copyPhPS__L0:
lea rcx, [3+rax]
lea rdx, [3+rax]
movzx r8, byte [rdi+rdx]
mov byte [rsi+rcx], r8b
add rax, 1
cmp rax, 10
jl _V34constant_permanence_and_array_copyPhPS__L0
_V34constant_permanence_and_array_copyPhPS__L1:
ret

_V4initv_rx:
sub rsp, 8
mov rax, 1
add rsp, 8
ret
xor rdi, rdi
xor rsi, rsi
call _V34constant_permanence_and_array_copyPhPS_
ret