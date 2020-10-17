section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh

global _V34constant_permanence_and_array_copyPhPS_
export _V34constant_permanence_and_array_copyPhPS_
_V34constant_permanence_and_array_copyPhPS_:
xor rax, rax
cmp rax, 10
jge _V34constant_permanence_and_array_copyPhPS__L1
_V34constant_permanence_and_array_copyPhPS__L0:
lea r8, [3+rax]
lea r9, [3+rax]
mov r10b, [rcx+r9]
mov byte [rdx+r8], r10b
add rax, 1
cmp rax, 10
jl _V34constant_permanence_and_array_copyPhPS__L0
_V34constant_permanence_and_array_copyPhPS__L1:
ret

_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
xor rcx, rcx
xor rdx, rdx
call _V34constant_permanence_and_array_copyPhPS_
ret