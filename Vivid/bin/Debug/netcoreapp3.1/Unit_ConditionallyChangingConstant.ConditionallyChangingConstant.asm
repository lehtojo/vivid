.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _V49conditionally_changing_constant_with_if_statementxx_rx
_V49conditionally_changing_constant_with_if_statementxx_rx:
mov rax, 7
cmp rcx, rdx
jle _V49conditionally_changing_constant_with_if_statementxx_rx_L0
mov rax, rcx
_V49conditionally_changing_constant_with_if_statementxx_rx_L0:
add rcx, rax
mov rax, rcx
ret

.global _V51conditionally_changing_constant_with_loop_statementxx_rx
_V51conditionally_changing_constant_with_loop_statementxx_rx:
mov rax, 100
cmp rcx, rdx
jge _V51conditionally_changing_constant_with_loop_statementxx_rx_L1
_V51conditionally_changing_constant_with_loop_statementxx_rx_L0:
add rax, 1
add rcx, 1
cmp rcx, rdx
jl _V51conditionally_changing_constant_with_loop_statementxx_rx_L0
_V51conditionally_changing_constant_with_loop_statementxx_rx_L1:
imul rdx, rax
mov rax, rdx
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 1
mov rdx, 1
call _V49conditionally_changing_constant_with_if_statementxx_rx
mov rcx, 1
mov rdx, 1
call _V51conditionally_changing_constant_with_loop_statementxx_rx
ret

.section .data

