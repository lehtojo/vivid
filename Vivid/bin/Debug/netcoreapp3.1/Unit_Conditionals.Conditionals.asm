.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _V12conditionalsxx_rx
_V12conditionalsxx_rx:
cmp rcx, rdx
jl _V12conditionalsxx_rx_L1
mov rax, rcx
ret
mov rcx, rax
jmp _V12conditionalsxx_rx_L0
_V12conditionalsxx_rx_L1:
mov rax, rdx
ret
mov rdx, rax
_V12conditionalsxx_rx_L0:
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 1
mov rdx, 2
call _V12conditionalsxx_rx
ret

.section .data

