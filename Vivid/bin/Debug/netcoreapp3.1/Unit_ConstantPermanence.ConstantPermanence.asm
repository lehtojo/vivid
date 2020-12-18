.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _V34constant_permanence_and_array_copyPhS_
_V34constant_permanence_and_array_copyPhS_:
xor r8, r8
cmp r8, 10
jge _V34constant_permanence_and_array_copyPhS__L1
_V34constant_permanence_and_array_copyPhS__L0:
lea r9, [3+r8]
movsx r10, byte ptr [rcx+r9]
lea r11, [3+r8]
mov byte ptr [rdx+r11], r10b
add r8, 1
cmp r8, 10
jl _V34constant_permanence_and_array_copyPhS__L0
_V34constant_permanence_and_array_copyPhS__L1:
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
xor rcx, rcx
xor rdx, rdx
call _V34constant_permanence_and_array_copyPhS_
ret

.section .data

