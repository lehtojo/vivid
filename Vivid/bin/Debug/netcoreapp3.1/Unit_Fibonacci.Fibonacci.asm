.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V4copyPhxS_
.extern _V11offset_copyPhxS_x
.extern _V14internal_printPhx
.extern _V17internal_allocatex_rPh

.global _V9fibonaccix
_V9fibonaccix:
push rsi
push rbx
push rdi
push rbp
push r12
sub rsp, 32
xor rbx, rbx
mov rdi, 1
xor rbp, rbp
mov r12, rcx
cmp rbx, r12
jge _V9fibonaccix_L1
_V9fibonaccix_L0:
cmp rbx, 1
jg _V9fibonaccix_L4
mov rsi, rbx
jmp _V9fibonaccix_L3
_V9fibonaccix_L4:
lea rsi, [rbp+rdi]
mov rbp, rdi
mov rdi, rsi
_V9fibonaccix_L3:
mov rcx, rsi
call _V9to_stringx_rP6String
mov rcx, rax
call _V7printlnP6String
add rbx, 1
cmp rbx, r12
jl _V9fibonaccix_L0
_V9fibonaccix_L1:
add rsp, 32
pop r12
pop rbp
pop rdi
pop rbx
pop rsi
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
mov rcx, 10
call _V9fibonaccix
xor rax, rax
add rsp, 40
ret

.section .data

